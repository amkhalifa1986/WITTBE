using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetTopologySuite.Geometries;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Trips.Commands;

// --- Commands ---

public record SubmitTelemetryCommand(
    Guid TripId,
    Guid UserId,
    double Latitude,
    double Longitude,
    double Speed
) : IRequest<Result<TelemetryResponseDto>>;

// --- Handlers ---

public class SubmitTelemetryCommandHandler : IRequestHandler<SubmitTelemetryCommand, Result<TelemetryResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TripNotificationHelper _notificationHelper;

    public SubmitTelemetryCommandHandler(IUnitOfWork unitOfWork, TripNotificationHelper notificationHelper)
    {
        _unitOfWork = unitOfWork;
        _notificationHelper = notificationHelper;
    }

    public async Task<Result<TelemetryResponseDto>> Handle(SubmitTelemetryCommand request, CancellationToken cancellationToken)
    {
        var trip = await _unitOfWork.Repository<Trip>().GetByIdAsync(request.TripId, cancellationToken);
        if (trip == null)
            return Result<TelemetryResponseDto>.Failure("Trip not found.", 404);

        // Automatically start the trip if it receives telemetry coordinates
        if (trip.Status == WhereIsTheTrain.Domain.Enums.TripStatus.Scheduled || trip.Status == WhereIsTheTrain.Domain.Enums.TripStatus.Delayed)
        {
            var oldStatus = trip.Status.ToString();
            trip.Status = WhereIsTheTrain.Domain.Enums.TripStatus.InTransit;
            await _unitOfWork.Repository<Trip>().UpdateAsync(trip, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationHelper.NotifyFollowersOfTripStatusAsync(trip.Id, oldStatus, trip.Status.ToString(), cancellationToken);
        }

        var train = await _unitOfWork.Trains.GetByIdAsync(trip.TrainId, cancellationToken);
        if (train == null)
            return Result<TelemetryResponseDto>.Failure("Train not found.", 404);

        // Load route stops and stop details
        var routeStopsList = new List<TrainRouteStop>();
        var stops = await _unitOfWork.Repository<TrainRouteStop>().FindAsync(rs => rs.TrainId == train.Id, cancellationToken);
        foreach (var rs in stops.OrderBy(rs => rs.StopOrder))
        {
            rs.Stop = await _unitOfWork.Repository<Stop>().GetByIdAsync(rs.StopId, cancellationToken);
            routeStopsList.Add(rs);
        }

        var railwayPaths = await _unitOfWork.RailwayPaths.GetAllWithStopsAsync(cancellationToken);
        var routePath = RoutePathBuilder.BuildRoutePath(routeStopsList, railwayPaths);

        // Snap coordinates
        var rawPoint = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        var (snappedCoord, segmentIndex) = GeoUtils.ProjectPointOnPolyline(routePath, rawPoint);

        // Snap distance check (filtering)
        double snapErrorDist = GeoUtils.HaversineDistance(request.Latitude, request.Longitude, snappedCoord.Y, snappedCoord.X);
        if (snapErrorDist > 150.0)
            return Result<TelemetryResponseDto>.Failure("Telemetry discarded: raw coordinate is too far from the track.", 400);

        // Speed check (filtering)
        if (request.Speed > 50.0) // 180 km/h in m/s
            return Result<TelemetryResponseDto>.Failure("Telemetry discarded: anomalous velocity.", 400);

        double distanceAlongRoute = GeoUtils.CalculateDistanceAlongPolyline(routePath, snappedCoord, segmentIndex);

        // Save telemetry record
        var telemetry = new TripTelemetry
        {
            TripId = request.TripId,
            UserId = request.UserId,
            RawLatitude = request.Latitude,
            RawLongitude = request.Longitude,
            SnappedLatitude = snappedCoord.Y,
            SnappedLongitude = snappedCoord.X,
            Speed = request.Speed,
            DistanceAlongRoute = distanceAlongRoute,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.Repository<TripTelemetry>().AddAsync(telemetry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Calculate moving average velocity (last 5 mins)
        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
        var recentTelemetry = await _unitOfWork.Repository<TripTelemetry>()
            .FindAsync(t => t.TripId == request.TripId && t.Timestamp >= fiveMinutesAgo, cancellationToken);

        double velocity = 0;
        if (recentTelemetry.Count >= 2)
        {
            var sorted = recentTelemetry.OrderBy(t => t.Timestamp).ToList();
            var first = sorted.First();
            var last = sorted.Last();
            double dt = (last.Timestamp - first.Timestamp).TotalSeconds;
            double dd = last.DistanceAlongRoute - first.DistanceAlongRoute;
            if (dt >= 10 && dd > 0)
            {
                velocity = dd / dt;
            }
        }

        if (velocity < 2.0) // Clamp / fallback if stopped or slow
        {
            var movingFeeds = recentTelemetry.Where(t => t.Speed > 2.0).ToList();
            if (movingFeeds.Any())
            {
                velocity = movingFeeds.Average(t => t.Speed);
            }
            else
            {
                velocity = 16.7; // default 60 km/h in m/s
            }
        }

        var response = new TelemetryResponseDto
        {
            TripId = request.TripId,
            SnappedLatitude = snappedCoord.Y,
            SnappedLongitude = snappedCoord.X,
            RawLatitude = request.Latitude,
            RawLongitude = request.Longitude,
            Speed = velocity * 3.6, // convert m/s to km/h
            DistanceAlongRoute = distanceAlongRoute
        };

        // Populate upcoming stops details
        var upcomingRouteStops = routeStopsList
            .Where(rs => rs.DistanceAlongRoute > distanceAlongRoute)
            .OrderBy(rs => rs.StopOrder)
            .ToList();

        foreach (var rs in upcomingRouteStops)
        {
            double distRemaining = rs.DistanceAlongRoute - distanceAlongRoute;
            double secondsToArrival = distRemaining / velocity;
            var eta = DateTime.UtcNow.AddSeconds(secondsToArrival);

            response.UpcomingStops.Add(new UpcomingStopTrackingDto
            {
                StopId = rs.StopId,
                StopNameAr = rs.Stop.NameAr,
                StopNameEn = rs.Stop.NameEn,
                StopCode = rs.Stop.Code,
                DistanceRemaining = distRemaining,
                EstimatedTimeOfArrival = eta
            });
        }

        return Result<TelemetryResponseDto>.Success(response);
    }
}

// --- Clear Telemetry Commands & Handlers ---

public record ClearEndedTripsTelemetryCommand() : IRequest<Result<int>>;
public record ClearTripTelemetryCommand(Guid TripId) : IRequest<Result<int>>;

public class ClearEndedTripsTelemetryCommandHandler : IRequestHandler<ClearEndedTripsTelemetryCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ClearEndedTripsTelemetryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(ClearEndedTripsTelemetryCommand request, CancellationToken cancellationToken)
    {
        var endedTrips = await _unitOfWork.Repository<Trip>()
            .FindAsync(t => t.Status == WhereIsTheTrain.Domain.Enums.TripStatus.Arrived || t.Status == WhereIsTheTrain.Domain.Enums.TripStatus.Cancelled, cancellationToken);
        
        if (!endedTrips.Any())
            return Result<int>.Success(0);

        var tripIds = endedTrips.Select(t => t.Id).ToList();

        var telemetryEntries = await _unitOfWork.Repository<TripTelemetry>()
            .FindAsync(t => tripIds.Contains(t.TripId), cancellationToken);

        int count = telemetryEntries.Count;
        foreach (var entry in telemetryEntries)
        {
            await _unitOfWork.Repository<TripTelemetry>().DeleteAsync(entry, cancellationToken);
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(count);
    }
}

public class ClearTripTelemetryCommandHandler : IRequestHandler<ClearTripTelemetryCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ClearTripTelemetryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(ClearTripTelemetryCommand request, CancellationToken cancellationToken)
    {
        var trip = await _unitOfWork.Repository<Trip>().GetByIdAsync(request.TripId, cancellationToken);
        if (trip == null)
            return Result<int>.Failure("Trip not found.", 404);

        var telemetryEntries = await _unitOfWork.Repository<TripTelemetry>()
            .FindAsync(t => t.TripId == request.TripId, cancellationToken);

        int count = telemetryEntries.Count;
        foreach (var entry in telemetryEntries)
        {
            await _unitOfWork.Repository<TripTelemetry>().DeleteAsync(entry, cancellationToken);
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(count);
    }
}
