using MediatR;
using NetTopologySuite.Geometries;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Trips.Queries;

public record GetTripTrackingQuery(Guid TripId) : IRequest<Result<TripTrackingDto>>;

public class TripTrackingDto
{
    public Guid TripId { get; set; }
    public double SnappedLatitude { get; set; }
    public double SnappedLongitude { get; set; }
    public double? RawLatitude { get; set; }
    public double? RawLongitude { get; set; }
    public double Speed { get; set; } // m/s
    public double SpeedKmh => Speed * 3.6;
    public double AverageVelocity { get; set; } // m/s
    public double AverageVelocityKmh => AverageVelocity * 3.6;
    public List<StopTrackingDto> UpcomingStops { get; set; } = new();
}

public class StopTrackingDto
{
    public Guid StopId { get; set; }
    public string StopCode { get; set; } = string.Empty;
    public string StopNameAr { get; set; } = string.Empty;
    public string StopNameEn { get; set; } = string.Empty;
    public int StopOrder { get; set; }
    public double DistanceRemaining { get; set; } // Meters along track
    public DateTime? EstimatedTimeOfArrival { get; set; }
    public double EtaMinutes { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class GetTripTrackingQueryHandler : IRequestHandler<GetTripTrackingQuery, Result<TripTrackingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTripTrackingQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<TripTrackingDto>> Handle(GetTripTrackingQuery request, CancellationToken ct)
    {
        var trip = await _unitOfWork.Trips.GetWithDetailsAsync(request.TripId, ct);
        if (trip == null)
            return Result<TripTrackingDto>.Failure("Trip not found.", 404);

        var railwayPaths = await _unitOfWork.RailwayPaths.GetAllWithStopsAsync(ct);
        var routePath = RoutePathBuilder.BuildRoutePath(trip.Train.RouteStops, railwayPaths);
        bool hasRoutePath = routePath.Coordinates.Length >= 2;

        // Fetch telemetries in the last 10 minutes
        var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);
        var telemetries = await _unitOfWork.Repository<TripTelemetry>()
            .FindAsync(t => t.TripId == request.TripId && t.Timestamp >= tenMinutesAgo, ct);

        double snappedLat = 0;
        double snappedLon = 0;
        double currentSpeed = 0;
        double currentDistance = 0;
        double velocity = 16.67; // Default 60 km/h in m/s

        var latestTelemetry = telemetries.OrderByDescending(t => t.Timestamp).FirstOrDefault();
        if (latestTelemetry == null)
        {
            // Fallback: Check if there's any telemetry ever
            var anyTelemetry = (await _unitOfWork.Repository<TripTelemetry>()
                .FindAsync(t => t.TripId == request.TripId, ct))
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefault();

            if (anyTelemetry != null)
            {
                latestTelemetry = anyTelemetry;
            }
        }

        if (latestTelemetry != null)
        {
            snappedLat = latestTelemetry.SnappedLatitude;
            snappedLon = latestTelemetry.SnappedLongitude;
            currentSpeed = latestTelemetry.Speed;
            currentDistance = latestTelemetry.DistanceAlongRoute;
        }
        else
        {
            // Fallback to train's first route stop coordinates
            var firstStop = trip.Train.RouteStops.OrderBy(rs => rs.StopOrder).FirstOrDefault();
            if (firstStop != null)
            {
                snappedLat = firstStop.Stop.Latitude;
                snappedLon = firstStop.Stop.Longitude;
            }
        }

        // Calculate average velocity over the delta of the sliding 10-minute window
        if (telemetries.Count >= 2)
        {
            var sorted = telemetries.OrderBy(t => t.Timestamp).ToList();
            var first = sorted.First();
            var last = sorted.Last();

            double deltaD = last.DistanceAlongRoute - first.DistanceAlongRoute;
            double deltaT = (last.Timestamp - first.Timestamp).TotalSeconds;

            if (deltaT > 0 && deltaD > 0)
            {
                velocity = deltaD / deltaT;
            }
            else if (latestTelemetry != null && latestTelemetry.Speed > 2.0)
            {
                velocity = latestTelemetry.Speed;
            }
        }
        else if (latestTelemetry != null && latestTelemetry.Speed > 2.0)
        {
            velocity = latestTelemetry.Speed;
        }

        // Keep velocity within reasonable bounds (min 5 m/s = 18 km/h, max 45 m/s = 162 km/h) to avoid extreme ETAs
        velocity = Math.Clamp(velocity, 5.0, 45.0);

        var remainingStops = new List<StopTrackingDto>();
        if (hasRoutePath)
        {
            foreach (var rs in trip.Train.RouteStops.OrderBy(rs => rs.StopOrder))
            {
                var point = new Coordinate(rs.Stop.Longitude, rs.Stop.Latitude);
                var (_, stopDistance, _) = GeometrySnappingHelper.ProjectPointOntoPolyline(routePath, point);

                double remainingDistance = Math.Max(0.0, stopDistance - currentDistance);

                // If remaining distance is > 0, it means it's an upcoming stop (or if it is the last stop and we haven't arrived)
                if (remainingDistance > 0)
                {
                    double etaSeconds = remainingDistance / velocity;
                    var etaTime = DateTime.UtcNow.AddSeconds(etaSeconds);
                    double etaMins = Math.Round(etaSeconds / 60.0, 1);

                    remainingStops.Add(new StopTrackingDto
                    {
                        StopId = rs.StopId,
                        StopCode = rs.Stop.Code,
                        StopNameAr = rs.Stop.NameAr,
                        StopNameEn = rs.Stop.NameEn,
                        StopOrder = rs.StopOrder,
                        DistanceRemaining = Math.Round(remainingDistance, 1),
                        EstimatedTimeOfArrival = etaTime,
                        EtaMinutes = etaMins,
                        Latitude = rs.Stop.Latitude,
                        Longitude = rs.Stop.Longitude
                    });
                }
            }
        }
        else
        {
            // Without route path, return all stops without distance/ETA calculations
            foreach (var rs in trip.Train.RouteStops.OrderBy(rs => rs.StopOrder))
            {
                remainingStops.Add(new StopTrackingDto
                {
                    StopId = rs.StopId,
                    StopCode = rs.Stop.Code,
                    StopNameAr = rs.Stop.NameAr,
                    StopNameEn = rs.Stop.NameEn,
                    StopOrder = rs.StopOrder,
                    Latitude = rs.Stop.Latitude,
                    Longitude = rs.Stop.Longitude
                });
            }
        }

        var trackingDto = new TripTrackingDto
        {
            TripId = request.TripId,
            SnappedLatitude = snappedLat,
            SnappedLongitude = snappedLon,
            RawLatitude = latestTelemetry?.RawLatitude,
            RawLongitude = latestTelemetry?.RawLongitude,
            Speed = currentSpeed * 3.6, // Convert m/s to km/h
            AverageVelocity = velocity,
            UpcomingStops = remainingStops
        };

        return Result<TripTrackingDto>.Success(trackingDto);
    }
}
