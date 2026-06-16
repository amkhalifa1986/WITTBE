using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;
using WhereIsTheTrain.Application.Features.TrainSuggestions;
using WhereIsTheTrain.Application.Features.LostFound;

namespace WhereIsTheTrain.Application.Features.Admin;


// --- DTOs ---
public class StopDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Guid CityId { get; set; }
    public string? CityAr { get; set; }
    public string? CityEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public List<Guid> RailwayPathIds { get; set; } = new();
}

public class CityDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public Guid GovernorateId { get; set; }
    public string? GovernorateAr { get; set; }
    public string? GovernorateEn { get; set; }
}

public class GovernorateDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public record CreateCityCommand(string NameAr, string NameEn, Guid GovernorateId) : IRequest<Result<CityDto>>;
public record UpdateCityCommand(Guid Id, string NameAr, string NameEn, Guid GovernorateId) : IRequest<Result<CityDto>>;
public record DeleteCityCommand(Guid Id) : IRequest<Result<bool>>;
public record GetAllCitiesQuery() : IRequest<Result<List<CityDto>>>;

public record CreateGovernorateCommand(string NameAr, string NameEn) : IRequest<Result<GovernorateDto>>;
public record UpdateGovernorateCommand(Guid Id, string NameAr, string NameEn) : IRequest<Result<GovernorateDto>>;
public record DeleteGovernorateCommand(Guid Id) : IRequest<Result<bool>>;
public record GetAllGovernoratesQuery() : IRequest<Result<List<GovernorateDto>>>;

public class TrainRouteStopInput
{
    public Guid StopId { get; set; }
    public int StopOrder { get; set; }
    public TimeSpan? ScheduledArrival { get; set; }
    public TimeSpan? ScheduledDeparture { get; set; }
}

// ==========================================
// 📍 STOPS COMMANDS & QUERIES
// ==========================================

public record CreateStopCommand(
    string NameAr, string NameEn, string Code, double Latitude, double Longitude, Guid CityId, string? DescriptionAr, string? DescriptionEn, List<Guid> RailwayPathIds
) : IRequest<Result<StopDto>>;

public record UpdateStopCommand(
    Guid Id, string NameAr, string NameEn, string Code, double Latitude, double Longitude, Guid CityId, string? DescriptionAr, string? DescriptionEn, List<Guid> RailwayPathIds
) : IRequest<Result<StopDto>>;

public record DeleteStopCommand(Guid Id) : IRequest<Result<bool>>;

public record GetAllStopsQuery() : IRequest<Result<List<StopDto>>>;

// Handlers for Stops
public class StopsAdminHandlers :
    IRequestHandler<CreateStopCommand, Result<StopDto>>,
    IRequestHandler<UpdateStopCommand, Result<StopDto>>,
    IRequestHandler<DeleteStopCommand, Result<bool>>,
    IRequestHandler<GetAllStopsQuery, Result<List<StopDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public StopsAdminHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<StopDto>> Handle(CreateStopCommand request, CancellationToken ct)
    {
        // Check duplicate code
        var existing = await _unitOfWork.Repository<Stop>().FindAsync(s => s.Code.ToLower() == request.Code.ToLower(), ct);
        if (existing.Any())
            return Result<StopDto>.Failure($"Stop with code '{request.Code}' already exists.", 400);

        // Check if city exists
        var city = await _unitOfWork.Repository<City>().GetByIdAsync(request.CityId, ct);
        if (city == null)
            return Result<StopDto>.Failure("City not found.", 400);

        var stop = new Stop
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Code = request.Code.ToUpper(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CityId = request.CityId,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn
        };

        if (request.RailwayPathIds != null && request.RailwayPathIds.Any())
        {
            var paths = await _unitOfWork.RailwayPaths.FindAsync(rp => request.RailwayPathIds.Contains(rp.Id), ct);
            foreach (var path in paths)
            {
                stop.RailwayPaths.Add(path);
            }
        }

        await _unitOfWork.Repository<Stop>().AddAsync(stop, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = MapToDto(stop);
        dto.CityAr = city.NameAr;
        dto.CityEn = city.NameEn;

        return Result<StopDto>.Success(dto, 201);
    }

    public async Task<Result<StopDto>> Handle(UpdateStopCommand request, CancellationToken ct)
    {
        var stop = await _unitOfWork.Stops.GetWithRailwayPathsByIdAsync(request.Id, ct);
        if (stop == null)
            return Result<StopDto>.Failure("Stop not found.", 404);

        // Check duplicate code
        var existing = await _unitOfWork.Repository<Stop>().FindAsync(s => s.Code.ToLower() == request.Code.ToLower() && s.Id != request.Id, ct);
        if (existing.Any())
            return Result<StopDto>.Failure($"Another stop with code '{request.Code}' already exists.", 400);

        // Check if city exists
        var city = await _unitOfWork.Repository<City>().GetByIdAsync(request.CityId, ct);
        if (city == null)
            return Result<StopDto>.Failure("City not found.", 400);

        stop.NameAr = request.NameAr;
        stop.NameEn = request.NameEn;
        stop.Code = request.Code.ToUpper();
        stop.Latitude = request.Latitude;
        stop.Longitude = request.Longitude;
        stop.CityId = request.CityId;
        stop.DescriptionAr = request.DescriptionAr;
        stop.DescriptionEn = request.DescriptionEn;

        // Clear existing railway paths and add the new ones
        stop.RailwayPaths.Clear();
        if (request.RailwayPathIds != null && request.RailwayPathIds.Any())
        {
            var paths = await _unitOfWork.RailwayPaths.FindAsync(rp => request.RailwayPathIds.Contains(rp.Id), ct);
            foreach (var path in paths)
            {
                stop.RailwayPaths.Add(path);
            }
        }

        await _unitOfWork.Repository<Stop>().UpdateAsync(stop, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = MapToDto(stop);
        dto.CityAr = city.NameAr;
        dto.CityEn = city.NameEn;

        return Result<StopDto>.Success(dto);
    }

    public async Task<Result<bool>> Handle(DeleteStopCommand request, CancellationToken ct)
    {
        var stop = await _unitOfWork.Repository<Stop>().GetByIdAsync(request.Id, ct);
        if (stop == null)
            return Result<bool>.Failure("Stop not found.", 404);

        // Verify if used in active routes
        var routeStops = await _unitOfWork.Repository<TrainRouteStop>().FindAsync(rs => rs.StopId == request.Id, ct);
        if (routeStops.Any())
            return Result<bool>.Failure("Cannot delete stop because it is used in one or more train routes.", 400);

        await _unitOfWork.Repository<Stop>().DeleteAsync(stop, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<List<StopDto>>> Handle(GetAllStopsQuery request, CancellationToken ct)
    {
        var stops = await _unitOfWork.Stops.GetAllWithRailwayPathsAsync(ct);
        var cities = await _unitOfWork.Repository<City>().GetAllAsync(ct);
        var cityMap = cities.ToDictionary(c => c.Id);

        var dtos = stops.OrderBy(s => s.NameEn).Select(s => {
            var dto = MapToDto(s);
            if (cityMap.TryGetValue(s.CityId, out var city))
            {
                dto.CityAr = city.NameAr;
                dto.CityEn = city.NameEn;
            }
            return dto;
        }).ToList();

        return Result<List<StopDto>>.Success(dtos);
    }

    private static StopDto MapToDto(Stop stop) => new()
    {
        Id = stop.Id,
        NameAr = stop.NameAr,
        NameEn = stop.NameEn,
        Code = stop.Code,
        Latitude = stop.Latitude,
        Longitude = stop.Longitude,
        CityId = stop.CityId,
        DescriptionAr = stop.DescriptionAr,
        DescriptionEn = stop.DescriptionEn,
        RailwayPathIds = stop.RailwayPaths?.Select(rp => rp.Id).ToList() ?? new()
    };
}

public class CitiesAdminHandlers :
    IRequestHandler<CreateCityCommand, Result<CityDto>>,
    IRequestHandler<UpdateCityCommand, Result<CityDto>>,
    IRequestHandler<DeleteCityCommand, Result<bool>>,
    IRequestHandler<GetAllCitiesQuery, Result<List<CityDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CitiesAdminHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<CityDto>> Handle(CreateCityCommand request, CancellationToken ct)
    {
        var existing = await _unitOfWork.Repository<City>().FindAsync(c => c.NameEn.ToLower() == request.NameEn.ToLower() || c.NameAr == request.NameAr, ct);
        if (existing.Any())
            return Result<CityDto>.Failure("City with this name already exists.", 400);

        var gov = await _unitOfWork.Repository<Governorate>().GetByIdAsync(request.GovernorateId, ct);
        if (gov == null)
            return Result<CityDto>.Failure("Governorate not found.", 400);

        var city = new City
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            GovernorateId = request.GovernorateId
        };

        await _unitOfWork.Repository<City>().AddAsync(city, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CityDto>.Success(new CityDto
        {
            Id = city.Id,
            NameAr = city.NameAr,
            NameEn = city.NameEn,
            GovernorateId = city.GovernorateId,
            GovernorateAr = gov.NameAr,
            GovernorateEn = gov.NameEn
        }, 201);
    }

    public async Task<Result<CityDto>> Handle(UpdateCityCommand request, CancellationToken ct)
    {
        var city = await _unitOfWork.Repository<City>().GetByIdAsync(request.Id, ct);
        if (city == null)
            return Result<CityDto>.Failure("City not found.", 404);

        var existing = await _unitOfWork.Repository<City>().FindAsync(c => (c.NameEn.ToLower() == request.NameEn.ToLower() || c.NameAr == request.NameAr) && c.Id != request.Id, ct);
        if (existing.Any())
            return Result<CityDto>.Failure("Another city with this name already exists.", 400);

        var gov = await _unitOfWork.Repository<Governorate>().GetByIdAsync(request.GovernorateId, ct);
        if (gov == null)
            return Result<CityDto>.Failure("Governorate not found.", 400);

        city.NameAr = request.NameAr;
        city.NameEn = request.NameEn;
        city.GovernorateId = request.GovernorateId;

        await _unitOfWork.Repository<City>().UpdateAsync(city, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CityDto>.Success(new CityDto
        {
            Id = city.Id,
            NameAr = city.NameAr,
            NameEn = city.NameEn,
            GovernorateId = city.GovernorateId,
            GovernorateAr = gov.NameAr,
            GovernorateEn = gov.NameEn
        });
    }

    public async Task<Result<bool>> Handle(DeleteCityCommand request, CancellationToken ct)
    {
        var city = await _unitOfWork.Repository<City>().GetByIdAsync(request.Id, ct);
        if (city == null)
            return Result<bool>.Failure("City not found.", 404);

        // Check if any stops are associated with this city
        var stops = await _unitOfWork.Repository<Stop>().FindAsync(s => s.CityId == request.Id, ct);
        if (stops.Any())
            return Result<bool>.Failure("Cannot delete city because it has associated stops.", 400);

        await _unitOfWork.Repository<City>().DeleteAsync(city, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<List<CityDto>>> Handle(GetAllCitiesQuery request, CancellationToken ct)
    {
        var cities = await _unitOfWork.Repository<City>().GetAllAsync(ct);
        var govs = await _unitOfWork.Repository<Governorate>().GetAllAsync(ct);
        var govMap = govs.ToDictionary(g => g.Id);

        var dtos = cities.OrderBy(c => c.NameEn).Select(c => {
            var dto = new CityDto
            {
                Id = c.Id,
                NameAr = c.NameAr,
                NameEn = c.NameEn,
                GovernorateId = c.GovernorateId
            };
            if (govMap.TryGetValue(c.GovernorateId, out var g))
            {
                dto.GovernorateAr = g.NameAr;
                dto.GovernorateEn = g.NameEn;
            }
            return dto;
        }).ToList();

        return Result<List<CityDto>>.Success(dtos);
    }
}

public class GovernoratesAdminHandlers :
    IRequestHandler<CreateGovernorateCommand, Result<GovernorateDto>>,
    IRequestHandler<UpdateGovernorateCommand, Result<GovernorateDto>>,
    IRequestHandler<DeleteGovernorateCommand, Result<bool>>,
    IRequestHandler<GetAllGovernoratesQuery, Result<List<GovernorateDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GovernoratesAdminHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<GovernorateDto>> Handle(CreateGovernorateCommand request, CancellationToken ct)
    {
        var existing = await _unitOfWork.Repository<Governorate>().FindAsync(g => g.NameEn.ToLower() == request.NameEn.ToLower() || g.NameAr == request.NameAr, ct);
        if (existing.Any())
            return Result<GovernorateDto>.Failure("Governorate with this name already exists.", 400);

        var gov = new Governorate
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn
        };

        await _unitOfWork.Repository<Governorate>().AddAsync(gov, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<GovernorateDto>.Success(new GovernorateDto { Id = gov.Id, NameAr = gov.NameAr, NameEn = gov.NameEn }, 201);
    }

    public async Task<Result<GovernorateDto>> Handle(UpdateGovernorateCommand request, CancellationToken ct)
    {
        var gov = await _unitOfWork.Repository<Governorate>().GetByIdAsync(request.Id, ct);
        if (gov == null)
            return Result<GovernorateDto>.Failure("Governorate not found.", 404);

        var existing = await _unitOfWork.Repository<Governorate>().FindAsync(g => (g.NameEn.ToLower() == request.NameEn.ToLower() || g.NameAr == request.NameAr) && g.Id != request.Id, ct);
        if (existing.Any())
            return Result<GovernorateDto>.Failure("Another governorate with this name already exists.", 400);

        gov.NameAr = request.NameAr;
        gov.NameEn = request.NameEn;

        await _unitOfWork.Repository<Governorate>().UpdateAsync(gov, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<GovernorateDto>.Success(new GovernorateDto { Id = gov.Id, NameAr = gov.NameAr, NameEn = gov.NameEn });
    }

    public async Task<Result<bool>> Handle(DeleteGovernorateCommand request, CancellationToken ct)
    {
        var gov = await _unitOfWork.Repository<Governorate>().GetByIdAsync(request.Id, ct);
        if (gov == null)
            return Result<bool>.Failure("Governorate not found.", 404);

        // Check if any cities are associated with this governorate
        var cities = await _unitOfWork.Repository<City>().FindAsync(c => c.GovernorateId == request.Id, ct);
        if (cities.Any())
            return Result<bool>.Failure("Cannot delete governorate because it has associated cities.", 400);

        await _unitOfWork.Repository<Governorate>().DeleteAsync(gov, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<List<GovernorateDto>>> Handle(GetAllGovernoratesQuery request, CancellationToken ct)
    {
        var govs = await _unitOfWork.Repository<Governorate>().GetAllAsync(ct);
        var dtos = govs.OrderBy(g => g.NameEn).Select(g => new GovernorateDto
        {
            Id = g.Id,
            NameAr = g.NameAr,
            NameEn = g.NameEn
        }).ToList();

        return Result<List<GovernorateDto>>.Success(dtos);
    }
}

// ==========================================
// 🚂 TRAINS COMMANDS & QUERIES
// ==========================================

public record CreateTrainCommand(
    string TrainNumber, string NameAr, string NameEn, string? DescriptionAr, string? DescriptionEn, Guid? TrainTypeId, List<TrainRouteStopInput> RouteStops
) : IRequest<Result<TrainDto>>;

public record UpdateTrainCommand(
    Guid Id, string TrainNumber, string NameAr, string NameEn, string? DescriptionAr, string? DescriptionEn, Guid? TrainTypeId, List<TrainRouteStopInput> RouteStops
) : IRequest<Result<TrainDto>>;

public record DeleteTrainCommand(Guid Id) : IRequest<Result<bool>>;

public record GetAllTrainsQuery() : IRequest<Result<List<TrainDto>>>;

// Handlers for Trains
public class TrainsAdminHandlers :
    IRequestHandler<CreateTrainCommand, Result<TrainDto>>,
    IRequestHandler<UpdateTrainCommand, Result<TrainDto>>,
    IRequestHandler<DeleteTrainCommand, Result<bool>>,
    IRequestHandler<GetAllTrainsQuery, Result<List<TrainDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public TrainsAdminHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<TrainDto>> Handle(CreateTrainCommand request, CancellationToken ct)
    {
        // Check duplicate number
        var existing = await _unitOfWork.Trains.GetByTrainNumberAsync(request.TrainNumber, ct);
        if (existing != null)
            return Result<TrainDto>.Failure($"Train with number '{request.TrainNumber}' already exists.", 400);

        var train = new Train
        {
            TrainNumber = request.TrainNumber,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            TrainTypeId = request.TrainTypeId,
            IsActive = true
        };

        // Add stops in order
        foreach (var input in request.RouteStops.OrderBy(rs => rs.StopOrder))
        {
            var stopExists = await _unitOfWork.Repository<Stop>().ExistsAsync(input.StopId, ct);
            if (!stopExists)
                return Result<TrainDto>.Failure($"Stop with ID '{input.StopId}' does not exist.", 400);

            train.RouteStops.Add(new TrainRouteStop
            {
                StopId = input.StopId,
                StopOrder = input.StopOrder,
                ScheduledArrival = input.ScheduledArrival,
                ScheduledDeparture = input.ScheduledDeparture
            });
        }

        await _unitOfWork.Repository<Train>().AddAsync(train, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetTrainDtoAsync(train.Id, ct);
    }

    public async Task<Result<TrainDto>> Handle(UpdateTrainCommand request, CancellationToken ct)
    {
        var train = await _unitOfWork.Trains.GetByIdAsync(request.Id, ct);
        if (train == null)
            return Result<TrainDto>.Failure("Train not found.", 404);

        // Check duplicate number
        var existing = await _unitOfWork.Trains.GetByTrainNumberAsync(request.TrainNumber, ct);
        if (existing != null && existing.Id != request.Id)
            return Result<TrainDto>.Failure($"Another train with number '{request.TrainNumber}' already exists.", 400);

        train.TrainNumber = request.TrainNumber;
        train.NameAr = request.NameAr;
        train.NameEn = request.NameEn;
        train.DescriptionAr = request.DescriptionAr;
        train.DescriptionEn = request.DescriptionEn;
        train.TrainTypeId = request.TrainTypeId;

        // Clear existing route stops and add new ones (EF Core will track deletions)
        // Note: For Clean Architecture we need access to the RouteStops set to clear/update
        var existingRouteStops = await _unitOfWork.Repository<TrainRouteStop>().FindAsync(rs => rs.TrainId == request.Id, ct);
        foreach (var rs in existingRouteStops)
        {
            await _unitOfWork.Repository<TrainRouteStop>().DeleteAsync(rs, ct);
        }

        foreach (var input in request.RouteStops.OrderBy(rs => rs.StopOrder))
        {
            var stopExists = await _unitOfWork.Repository<Stop>().ExistsAsync(input.StopId, ct);
            if (!stopExists)
                return Result<TrainDto>.Failure($"Stop with ID '{input.StopId}' does not exist.", 400);

            train.RouteStops.Add(new TrainRouteStop
            {
                StopId = input.StopId,
                StopOrder = input.StopOrder,
                ScheduledArrival = input.ScheduledArrival,
                ScheduledDeparture = input.ScheduledDeparture
            });
        }

        await _unitOfWork.Trains.UpdateAsync(train, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetTrainDtoAsync(train.Id, ct);
    }

    public async Task<Result<bool>> Handle(DeleteTrainCommand request, CancellationToken ct)
    {
        var train = await _unitOfWork.Trains.GetByIdAsync(request.Id, ct);
        if (train == null)
            return Result<bool>.Failure("Train not found.", 404);

        // Check if has associated trips
        var trips = await _unitOfWork.Repository<Trip>().FindAsync(t => t.TrainId == request.Id, ct);
        if (trips.Any())
            return Result<bool>.Failure("Cannot delete train because it has associated trips. Delete trips first or mark train inactive.", 400);

        // Delete route stops first
        var routeStops = await _unitOfWork.Repository<TrainRouteStop>().FindAsync(rs => rs.TrainId == request.Id, ct);
        foreach (var rs in routeStops)
        {
            await _unitOfWork.Repository<TrainRouteStop>().DeleteAsync(rs, ct);
        }

        await _unitOfWork.Repository<Train>().DeleteAsync(train, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<List<TrainDto>>> Handle(GetAllTrainsQuery request, CancellationToken ct)
    {
        // Retrieve all trains. For stops mapping we will load them manually or via Repository.
        var trains = await _unitOfWork.Repository<Train>().GetAllAsync(ct);
        var trainTypes = await _unitOfWork.Repository<TrainType>().GetAllAsync(ct);
        var trainTypesDict = trainTypes.ToDictionary(t => t.Id);
        var followPlans = await _unitOfWork.Repository<TrainFollowPlan>().GetAllAsync(ct);
        var followPlansGrouped = followPlans.GroupBy(p => p.TrainId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.UserId).Distinct().Count());
        var railwayPaths = await _unitOfWork.RailwayPaths.GetAllWithStopsAsync(ct);
        var dtos = new List<TrainDto>();

        foreach (var train in trains)
        {
            var routeStops = await _unitOfWork.Repository<TrainRouteStop>().FindAsync(rs => rs.TrainId == train.Id, ct);
            var stopDtos = new List<RouteStopDto>();

            foreach (var rs in routeStops.OrderBy(x => x.StopOrder))
            {
                var stop = await _unitOfWork.Repository<Stop>().GetByIdAsync(rs.StopId, ct);
                if (stop != null)
                {
                    rs.Stop = stop;
                    var city = await _unitOfWork.Repository<City>().GetByIdAsync(stop.CityId, ct);
                    stopDtos.Add(new RouteStopDto
                    {
                        StopId = rs.StopId,
                        StopNameAr = stop.NameAr,
                        StopNameEn = stop.NameEn,
                        StopCode = stop.Code,
                        CityAr = city?.NameAr,
                        CityEn = city?.NameEn,
                        StopOrder = rs.StopOrder,
                        ScheduledArrival = rs.ScheduledArrival,
                        ScheduledDeparture = rs.ScheduledDeparture,
                        Latitude = stop.Latitude,
                        Longitude = stop.Longitude
                    });
                }
            }

            var followerCount = followPlansGrouped.TryGetValue(train.Id, out var fCount) ? fCount : 0;

            var coveringPaths = RoutePathBuilder.GetCoveringPaths(routeStops, railwayPaths);
            var typeInfo = train.TrainTypeId.HasValue && trainTypesDict.TryGetValue(train.TrainTypeId.Value, out var ttVal) ? ttVal : null;

            dtos.Add(new TrainDto
            {
                Id = train.Id,
                TrainNumber = train.TrainNumber,
                NameAr = train.NameAr,
                NameEn = train.NameEn,
                DescriptionAr = train.DescriptionAr,
                DescriptionEn = train.DescriptionEn,
                IsActive = train.IsActive,
                FollowerCount = followerCount,
                PathCode = string.Join(", ", coveringPaths.Select(p => p.Code)),
                PathNameAr = string.Join(" + ", coveringPaths.Select(p => p.NameAr)),
                PathNameEn = string.Join(" + ", coveringPaths.Select(p => p.NameEn)),
                RouteStops = stopDtos,
                TrainTypeId = train.TrainTypeId,
                TrainTypeNameAr = typeInfo?.NameAr,
                TrainTypeNameEn = typeInfo?.NameEn,
                MarkerPngUrl = typeInfo?.MarkerPngUrl
            });
        }

        return Result<List<TrainDto>>.Success(dtos.OrderBy(t => t.TrainNumber).ToList());
    }

    private async Task<Result<TrainDto>> GetTrainDtoAsync(Guid trainId, CancellationToken ct)
    {
        var train = await _unitOfWork.Trains.GetByIdAsync(trainId, ct);
        if (train == null)
            return Result<TrainDto>.Failure("Train not found.", 404);

        var routeStops = await _unitOfWork.Repository<TrainRouteStop>().FindAsync(rs => rs.TrainId == trainId, ct);
        var stopDtos = new List<RouteStopDto>();
        var railwayPaths = await _unitOfWork.RailwayPaths.GetAllWithStopsAsync(ct);

        foreach (var rs in routeStops.OrderBy(x => x.StopOrder))
        {
            var stop = await _unitOfWork.Repository<Stop>().GetByIdAsync(rs.StopId, ct);
            if (stop != null)
            {
                rs.Stop = stop;
                var city = await _unitOfWork.Repository<City>().GetByIdAsync(stop.CityId, ct);
                stopDtos.Add(new RouteStopDto
                {
                    StopId = rs.StopId,
                    StopNameAr = stop.NameAr,
                    StopNameEn = stop.NameEn,
                    StopCode = stop.Code,
                    CityAr = city?.NameAr,
                    CityEn = city?.NameEn,
                    StopOrder = rs.StopOrder,
                    ScheduledArrival = rs.ScheduledArrival,
                    ScheduledDeparture = rs.ScheduledDeparture,
                    Latitude = stop.Latitude,
                    Longitude = stop.Longitude
                });
            }
        }

        var followers = await _unitOfWork.Repository<TrainFollowPlan>().FindAsync(p => p.TrainId == trainId, ct);
        var followerCount = followers.Select(p => p.UserId).Distinct().Count();

        var coveringPaths = RoutePathBuilder.GetCoveringPaths(routeStops, railwayPaths);

        var trainType = train.TrainTypeId.HasValue
            ? await _unitOfWork.Repository<TrainType>().GetByIdAsync(train.TrainTypeId.Value, ct)
            : null;

        var dto = new TrainDto
        {
            Id = train.Id,
            TrainNumber = train.TrainNumber,
            NameAr = train.NameAr,
            NameEn = train.NameEn,
            DescriptionAr = train.DescriptionAr,
            DescriptionEn = train.DescriptionEn,
            IsActive = train.IsActive,
            FollowerCount = followerCount,
            PathCode = string.Join(", ", coveringPaths.Select(p => p.Code)),
            PathNameAr = string.Join(" + ", coveringPaths.Select(p => p.NameAr)),
            PathNameEn = string.Join(" + ", coveringPaths.Select(p => p.NameEn)),
            RouteStops = stopDtos,
            TrainTypeId = train.TrainTypeId,
            TrainTypeNameAr = trainType?.NameAr,
            TrainTypeNameEn = trainType?.NameEn,
            MarkerPngUrl = trainType?.MarkerPngUrl
        };
        return Result<TrainDto>.Success(dto);
    }
}

// ==========================================
// 📅 TRIPS COMMANDS & QUERIES
// ==========================================

public record CreateTripCommand(
    Guid TrainId, DateOnly TripDate, TripStatus Status
) : IRequest<Result<TripDto>>;

public record UpdateTripStatusCommand(
    Guid TripId, TripStatus Status, DateTime? ActualDeparture, DateTime? ActualArrival
) : IRequest<Result<TripDto>>;

public record DeleteTripCommand(Guid Id) : IRequest<Result<bool>>;

public record GetAllTripsQuery() : IRequest<Result<List<TripDto>>>;

// Handlers for Trips
public class TripsAdminHandlers :
    IRequestHandler<CreateTripCommand, Result<TripDto>>,
    IRequestHandler<UpdateTripStatusCommand, Result<TripDto>>,
    IRequestHandler<DeleteTripCommand, Result<bool>>,
    IRequestHandler<GetAllTripsQuery, Result<List<TripDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TripNotificationHelper _notificationHelper;

    public TripsAdminHandlers(IUnitOfWork unitOfWork, TripNotificationHelper notificationHelper)
    {
        _unitOfWork = unitOfWork;
        _notificationHelper = notificationHelper;
    }

    public async Task<Result<TripDto>> Handle(CreateTripCommand request, CancellationToken ct)
    {
        var train = await _unitOfWork.Trains.GetByIdAsync(request.TrainId, ct);
        if (train == null)
            return Result<TripDto>.Failure("Train not found.", 404);

        // Check if trip already exists for this train on this date
        var existing = await _unitOfWork.Repository<Trip>().FindAsync(t => t.TrainId == request.TrainId && t.TripDate == request.TripDate, ct);
        if (existing.Any())
            return Result<TripDto>.Failure($"A trip for train '{train.TrainNumber}' on {request.TripDate} already exists.", 400);

        var trip = new Trip
        {
            TrainId = request.TrainId,
            TripDate = request.TripDate,
            Status = request.Status
        };

        await _unitOfWork.Repository<Trip>().AddAsync(trip, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Auto-enroll users with matching active plans
        var tripDay = (int)request.TripDate.DayOfWeek;
        var plans = await _unitOfWork.Repository<TrainFollowPlan>()
            .FindAsync(p => p.TrainId == request.TrainId && p.DayOfWeek == tripDay, ct);

        var autoEnrolledCount = 0;

        foreach (var plan in plans)
        {
            var follower = new TripFollower
            {
                UserId = plan.UserId,
                TripId = trip.Id,
                PersonalStatus = PersonalTripStatus.Following,
                SourcePlanId = plan.Id
            };
            await _unitOfWork.Repository<TripFollower>().AddAsync(follower, ct);
            autoEnrolledCount++;
        }

        if (autoEnrolledCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var trainType = train.TrainTypeId.HasValue
            ? await _unitOfWork.Repository<TrainType>().GetByIdAsync(train.TrainTypeId.Value, ct)
            : null;

        return Result<TripDto>.Success(new TripDto
        {
            Id = trip.Id,
            TrainId = trip.TrainId,
            TrainNumber = train.TrainNumber,
            TrainNameAr = train.NameAr,
            TrainNameEn = train.NameEn,
            TripDate = trip.TripDate,
            Status = trip.Status.ToString(),
            FollowerCount = autoEnrolledCount,
            TrainTypeId = train.TrainTypeId,
            TrainTypeNameAr = trainType?.NameAr,
            TrainTypeNameEn = trainType?.NameEn,
            MarkerPngUrl = trainType?.MarkerPngUrl
        }, 201);
    }

    public async Task<Result<TripDto>> Handle(UpdateTripStatusCommand request, CancellationToken ct)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId, ct);
        if (trip == null)
            return Result<TripDto>.Failure("Trip not found.", 404);

        if (trip.Status == TripStatus.Arrived || trip.Status == TripStatus.Cancelled)
            return Result<TripDto>.Failure("Cannot modify a finished or cancelled trip.", 400);

        var oldStatus = trip.Status.ToString();
        trip.Status = request.Status;
        if (request.ActualDeparture.HasValue) trip.ActualDeparture = request.ActualDeparture;
        if (request.ActualArrival.HasValue) trip.ActualArrival = request.ActualArrival;

        // Auto set actuals on transition if not provided
        if ((request.Status == TripStatus.Departed || request.Status == TripStatus.InTransit) && !trip.ActualDeparture.HasValue)
            trip.ActualDeparture = DateTime.UtcNow;
        if (request.Status == TripStatus.Arrived && !trip.ActualArrival.HasValue)
            trip.ActualArrival = DateTime.UtcNow;

        await _unitOfWork.Repository<Trip>().UpdateAsync(trip, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        if (oldStatus != trip.Status.ToString())
        {
            await _notificationHelper.NotifyFollowersOfTripStatusAsync(trip.Id, oldStatus, trip.Status.ToString(), ct);
        }

        var train = await _unitOfWork.Trains.GetByIdAsync(trip.TrainId, ct);
        var followers = await _unitOfWork.Repository<TripFollower>().CountAsync(tf => tf.TripId == trip.Id, ct);
        var trainType = train?.TrainTypeId.HasValue == true
            ? await _unitOfWork.Repository<TrainType>().GetByIdAsync(train.TrainTypeId.Value, ct)
            : null;

        return Result<TripDto>.Success(new TripDto
        {
            Id = trip.Id,
            TrainId = trip.TrainId,
            TrainNumber = train?.TrainNumber ?? string.Empty,
            TrainNameAr = train?.NameAr ?? string.Empty,
            TrainNameEn = train?.NameEn ?? string.Empty,
            TripDate = trip.TripDate,
            Status = trip.Status.ToString(),
            ActualDeparture = trip.ActualDeparture,
            ActualArrival = trip.ActualArrival,
            FollowerCount = followers,
            TrainTypeId = train?.TrainTypeId,
            TrainTypeNameAr = trainType?.NameAr,
            TrainTypeNameEn = trainType?.NameEn,
            MarkerPngUrl = trainType?.MarkerPngUrl
        });
    }

    public async Task<Result<bool>> Handle(DeleteTripCommand request, CancellationToken ct)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(request.Id, ct);
        if (trip == null)
            return Result<bool>.Failure("Trip not found.", 404);

        // Delete associated followers, live updates first
        var followers = await _unitOfWork.Repository<TripFollower>().FindAsync(f => f.TripId == request.Id, ct);
        foreach (var follower in followers)
        {
            await _unitOfWork.Repository<TripFollower>().DeleteAsync(follower, ct);
        }

        var updates = await _unitOfWork.Repository<TripLiveUpdate>().FindAsync(u => u.TripId == request.Id, ct);
        foreach (var u in updates)
        {
            await _unitOfWork.Repository<TripLiveUpdate>().DeleteAsync(u, ct);
        }

        await _unitOfWork.Repository<Trip>().DeleteAsync(trip, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<List<TripDto>>> Handle(GetAllTripsQuery request, CancellationToken ct)
    {
        var trips = await _unitOfWork.Repository<Trip>().GetAllAsync(ct);
        var trainTypes = await _unitOfWork.Repository<TrainType>().GetAllAsync(ct);
        var trainTypesDict = trainTypes.ToDictionary(t => t.Id);
        var dtos = new List<TripDto>();

        foreach (var trip in trips)
        {
            var train = await _unitOfWork.Trains.GetByIdAsync(trip.TrainId, ct);
            var followers = await _unitOfWork.Repository<TripFollower>().CountAsync(tf => tf.TripId == trip.Id, ct);
            
            var trainType = train?.TrainTypeId.HasValue == true && trainTypesDict.TryGetValue(train.TrainTypeId.Value, out var tt)
                ? tt
                : null;

            dtos.Add(new TripDto
            {
                Id = trip.Id,
                TrainId = trip.TrainId,
                TrainNumber = train?.TrainNumber ?? string.Empty,
                TrainNameAr = train?.NameAr ?? string.Empty,
                TrainNameEn = train?.NameEn ?? string.Empty,
                TripDate = trip.TripDate,
                Status = trip.Status.ToString(),
                ActualDeparture = trip.ActualDeparture,
                ActualArrival = trip.ActualArrival,
                FollowerCount = followers,
                TrainTypeId = train?.TrainTypeId,
                TrainTypeNameAr = trainType?.NameAr,
                TrainTypeNameEn = trainType?.NameEn,
                MarkerPngUrl = trainType?.MarkerPngUrl
            });
        }

        return Result<List<TripDto>>.Success(dtos.OrderByDescending(t => t.TripDate).ThenBy(t => t.TrainNumber).ToList());
    }
}

// ==========================================
// 📩 SUGGESTIONS COMMANDS & QUERIES
// ==========================================

public record GetPendingTrainSuggestionsQuery() : IRequest<Result<List<TrainSuggestionDto>>>;

public record GetPendingStopSuggestionsQuery() : IRequest<Result<List<StopSuggestionDto>>>;

public record ReviewTrainSuggestionCommand(
    Guid SuggestionId,
    SuggestionStatus Status,
    string? AdminNotes,
    string TrainNumber,
    string NameAr,
    string NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string? RouteDescriptionEn
) : IRequest<Result<TrainSuggestionDto>>;

public record ReviewStopSuggestionCommand(
    Guid SuggestionId,
    SuggestionStatus Status,
    string? AdminNotes,
    string Code,
    string NameAr,
    string NameEn,
    Guid? CityId,
    string? NewCityNameAr,
    string? NewCityNameEn,
    Guid? NewCityGovernorateId,
    double? Latitude,
    double? Longitude,
    string? DescriptionAr,
    string? DescriptionEn
) : IRequest<Result<StopSuggestionDto>>;

public class SuggestionsAdminHandlers :
    IRequestHandler<GetPendingTrainSuggestionsQuery, Result<List<TrainSuggestionDto>>>,
    IRequestHandler<GetPendingStopSuggestionsQuery, Result<List<StopSuggestionDto>>>,
    IRequestHandler<ReviewTrainSuggestionCommand, Result<TrainSuggestionDto>>,
    IRequestHandler<ReviewStopSuggestionCommand, Result<StopSuggestionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SuggestionsAdminHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<TrainSuggestionDto>>> Handle(GetPendingTrainSuggestionsQuery request, CancellationToken ct)
    {
        var suggestions = await _unitOfWork.Repository<TrainSuggestion>()
            .FindAsync(s => s.Status == SuggestionStatus.Pending, ct);

        var dtos = new List<TrainSuggestionDto>();
        foreach (var s in suggestions)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(s.SuggestedById, ct);
            dtos.Add(new TrainSuggestionDto
            {
                Id = s.Id,
                TrainNumber = s.TrainNumber,
                NameAr = s.NameAr,
                NameEn = s.NameEn,
                DescriptionAr = s.DescriptionAr,
                DescriptionEn = s.DescriptionEn,
                RouteDescriptionAr = s.RouteDescriptionAr,
                RouteDescriptionEn = s.RouteDescriptionEn,
                Status = s.Status.ToString(),
                AdminNotes = s.AdminNotes,
                SuggestedByName = user?.DisplayName ?? "Unknown",
                CreatedAt = s.CreatedAt
            });
        }

        return Result<List<TrainSuggestionDto>>.Success(dtos.OrderByDescending(s => s.CreatedAt).ToList());
    }

    public async Task<Result<List<StopSuggestionDto>>> Handle(GetPendingStopSuggestionsQuery request, CancellationToken ct)
    {
        var suggestions = await _unitOfWork.Repository<StopSuggestion>()
            .FindAsync(s => s.Status == SuggestionStatus.Pending, ct);

        var dtos = new List<StopSuggestionDto>();
        foreach (var s in suggestions)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(s.SuggestedById, ct);
            City? city = null;
            if (s.CityId.HasValue)
            {
                city = await _unitOfWork.Repository<City>().GetByIdAsync(s.CityId.Value, ct);
            }
            Governorate? gov = null;
            if (s.NewCityGovernorateId.HasValue)
            {
                gov = await _unitOfWork.Repository<Governorate>().GetByIdAsync(s.NewCityGovernorateId.Value, ct);
            }

            dtos.Add(new StopSuggestionDto
            {
                Id = s.Id,
                Code = s.Code,
                NameAr = s.NameAr,
                NameEn = s.NameEn,
                CityId = s.CityId,
                CityNameAr = city?.NameAr,
                CityNameEn = city?.NameEn,
                NewCityNameAr = s.NewCityNameAr,
                NewCityNameEn = s.NewCityNameEn,
                NewCityGovernorateId = s.NewCityGovernorateId,
                NewCityGovernorateNameAr = gov?.NameAr,
                NewCityGovernorateNameEn = gov?.NameEn,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                DescriptionAr = s.DescriptionAr,
                DescriptionEn = s.DescriptionEn,
                Status = s.Status.ToString(),
                AdminNotes = s.AdminNotes,
                SuggestedByName = user?.DisplayName ?? "Unknown",
                CreatedAt = s.CreatedAt
            });
        }

        return Result<List<StopSuggestionDto>>.Success(dtos.OrderByDescending(s => s.CreatedAt).ToList());
    }

    public async Task<Result<TrainSuggestionDto>> Handle(ReviewTrainSuggestionCommand request, CancellationToken ct)
    {
        var suggestion = await _unitOfWork.Repository<TrainSuggestion>().GetByIdAsync(request.SuggestionId, ct);
        if (suggestion == null)
            return Result<TrainSuggestionDto>.Failure("Suggestion not found.", 404);

        if (suggestion.Status != SuggestionStatus.Pending)
            return Result<TrainSuggestionDto>.Failure("Suggestion has already been reviewed.", 400);

        suggestion.TrainNumber = request.TrainNumber;
        suggestion.NameAr = request.NameAr;
        suggestion.NameEn = request.NameEn;
        suggestion.DescriptionAr = request.DescriptionAr;
        suggestion.DescriptionEn = request.DescriptionEn;
        suggestion.RouteDescriptionEn = request.RouteDescriptionEn;
        suggestion.AdminNotes = request.AdminNotes;
        suggestion.Status = request.Status;

        if (request.Status == SuggestionStatus.Approved)
        {
            var existingTrains = await _unitOfWork.Repository<Train>().FindAsync(t => t.TrainNumber == request.TrainNumber.Trim(), ct);
            if (existingTrains.Any())
            {
                return Result<TrainSuggestionDto>.Failure($"A train with number {request.TrainNumber} already exists in the system.", 400);
            }

            var train = new Train
            {
                TrainNumber = request.TrainNumber.Trim(),
                NameAr = request.NameAr.Trim(),
                NameEn = request.NameEn.Trim(),
                DescriptionAr = request.DescriptionAr?.Trim(),
                DescriptionEn = request.DescriptionEn?.Trim(),
                IsActive = true,
                CreatedById = suggestion.SuggestedById
            };

            await _unitOfWork.Repository<Train>().AddAsync(train, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            if (!string.IsNullOrWhiteSpace(request.RouteDescriptionEn))
            {
                var stopNames = request.RouteDescriptionEn.Split(new[] { " -> " }, StringSplitOptions.RemoveEmptyEntries);
                int order = 1;
                foreach (var name in stopNames)
                {
                    var cleanName = name.Trim();
                    var stops = await _unitOfWork.Repository<Stop>().FindAsync(s => s.NameEn.ToLower() == cleanName.ToLower(), ct);
                    var stop = stops.FirstOrDefault();
                    if (stop != null)
                    {
                        var routeStop = new TrainRouteStop
                        {
                            TrainId = train.Id,
                            StopId = stop.Id,
                            StopOrder = order++,
                            DistanceAlongRoute = 0
                        };
                        await _unitOfWork.Repository<TrainRouteStop>().AddAsync(routeStop, ct);
                    }
                }
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }

        await _unitOfWork.Repository<TrainSuggestion>().UpdateAsync(suggestion, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var user = await _unitOfWork.Users.GetByIdAsync(suggestion.SuggestedById, ct);
        return Result<TrainSuggestionDto>.Success(new TrainSuggestionDto
        {
            Id = suggestion.Id,
            TrainNumber = suggestion.TrainNumber,
            NameAr = suggestion.NameAr,
            NameEn = suggestion.NameEn,
            DescriptionAr = suggestion.DescriptionAr,
            DescriptionEn = suggestion.DescriptionEn,
            RouteDescriptionAr = suggestion.RouteDescriptionAr,
            RouteDescriptionEn = suggestion.RouteDescriptionEn,
            Status = suggestion.Status.ToString(),
            AdminNotes = suggestion.AdminNotes,
            SuggestedByName = user?.DisplayName ?? "Unknown",
            CreatedAt = suggestion.CreatedAt
        });
    }

    public async Task<Result<StopSuggestionDto>> Handle(ReviewStopSuggestionCommand request, CancellationToken ct)
    {
        var suggestion = await _unitOfWork.Repository<StopSuggestion>().GetByIdAsync(request.SuggestionId, ct);
        if (suggestion == null)
            return Result<StopSuggestionDto>.Failure("Suggestion not found.", 404);

        if (suggestion.Status != SuggestionStatus.Pending)
            return Result<StopSuggestionDto>.Failure("Suggestion has already been reviewed.", 400);

        suggestion.Code = request.Code.Trim().ToUpper();
        suggestion.NameAr = request.NameAr.Trim();
        suggestion.NameEn = request.NameEn.Trim();
        suggestion.CityId = request.CityId;
        suggestion.NewCityNameAr = request.NewCityNameAr?.Trim();
        suggestion.NewCityNameEn = request.NewCityNameEn?.Trim();
        suggestion.NewCityGovernorateId = request.NewCityGovernorateId;
        suggestion.Latitude = request.Latitude;
        suggestion.Longitude = request.Longitude;
        suggestion.DescriptionAr = request.DescriptionAr?.Trim();
        suggestion.DescriptionEn = request.DescriptionEn?.Trim();
        suggestion.AdminNotes = request.AdminNotes;
        suggestion.Status = request.Status;

        City? cityObj = null;

        if (request.Status == SuggestionStatus.Approved)
        {
            var existingStops = await _unitOfWork.Repository<Stop>().FindAsync(s => s.Code.ToLower() == request.Code.Trim().ToLower(), ct);
            if (existingStops.Any())
            {
                return Result<StopSuggestionDto>.Failure($"A stop with code {request.Code} already exists in the system.", 400);
            }

            Guid actualCityId;
            if (request.CityId.HasValue)
            {
                actualCityId = request.CityId.Value;
                cityObj = await _unitOfWork.Repository<City>().GetByIdAsync(actualCityId, ct);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.NewCityNameEn) || string.IsNullOrWhiteSpace(request.NewCityNameAr) || !request.NewCityGovernorateId.HasValue)
                {
                    return Result<StopSuggestionDto>.Failure("For new city suggestions, new city names (Ar/En) and governorate must be filled.", 400);
                }

                var existingCities = await _unitOfWork.Repository<City>()
                    .FindAsync(c => c.NameEn.ToLower() == request.NewCityNameEn.Trim().ToLower(), ct);
                var existingCity = existingCities.FirstOrDefault();

                if (existingCity != null)
                {
                    actualCityId = existingCity.Id;
                    cityObj = existingCity;
                }
                else
                {
                    var newCity = new City
                    {
                        NameAr = request.NewCityNameAr.Trim(),
                        NameEn = request.NewCityNameEn.Trim(),
                        GovernorateId = request.NewCityGovernorateId.Value
                    };
                    await _unitOfWork.Repository<City>().AddAsync(newCity, ct);
                    await _unitOfWork.SaveChangesAsync(ct);
                    actualCityId = newCity.Id;
                    cityObj = newCity;
                }
            }

            var point = request.Longitude.HasValue && request.Latitude.HasValue
                ? new NetTopologySuite.Geometries.Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 }
                : null;

            var stop = new Stop
            {
                Code = request.Code.Trim().ToUpper(),
                NameAr = request.NameAr.Trim(),
                NameEn = request.NameEn.Trim(),
                CityId = actualCityId,
                Latitude = request.Latitude ?? 0,
                Longitude = request.Longitude ?? 0,
                DescriptionAr = request.DescriptionAr?.Trim(),
                DescriptionEn = request.DescriptionEn?.Trim(),
                Location = point
            };

            await _unitOfWork.Repository<Stop>().AddAsync(stop, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        await _unitOfWork.Repository<StopSuggestion>().UpdateAsync(suggestion, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var user = await _unitOfWork.Users.GetByIdAsync(suggestion.SuggestedById, ct);
        var govObj = suggestion.NewCityGovernorateId.HasValue 
            ? await _unitOfWork.Repository<Governorate>().GetByIdAsync(suggestion.NewCityGovernorateId.Value, ct)
            : null;

        return Result<StopSuggestionDto>.Success(new StopSuggestionDto
        {
            Id = suggestion.Id,
            Code = suggestion.Code,
            NameAr = suggestion.NameAr,
            NameEn = suggestion.NameEn,
            CityId = suggestion.CityId,
            CityNameAr = cityObj?.NameAr,
            CityNameEn = cityObj?.NameEn,
            NewCityNameAr = suggestion.NewCityNameAr,
            NewCityNameEn = suggestion.NewCityNameEn,
            NewCityGovernorateId = suggestion.NewCityGovernorateId,
            NewCityGovernorateNameAr = govObj?.NameAr,
            NewCityGovernorateNameEn = govObj?.NameEn,
            Latitude = suggestion.Latitude,
            Longitude = suggestion.Longitude,
            DescriptionAr = suggestion.DescriptionAr,
            DescriptionEn = suggestion.DescriptionEn,
            Status = suggestion.Status.ToString(),
            AdminNotes = suggestion.AdminNotes,
            SuggestedByName = user?.DisplayName ?? "Unknown",
            CreatedAt = suggestion.CreatedAt
        });
    }
}

// ==========================================
// 🔍 LOST & FOUND MODERATION COMMANDS & QUERIES
// ==========================================

public record GetAdminLostFoundPostsQuery() : IRequest<Result<List<LostFoundPostDto>>>;

public record UpdateLostFoundPostStatusCommand(Guid PostId, LostFoundStatus Status) : IRequest<Result<LostFoundPostDto>>;

public record AdminUpdateLostFoundPostCommand(Guid PostId, string Title, string Description, LostFoundType Type, string? TrainNumber, string? ContactInfo) : IRequest<Result<LostFoundPostDto>>;

public record AdminDeleteLostFoundPostCommand(Guid PostId) : IRequest<Result<bool>>;

public record AdminHideLostFoundCommentCommand(Guid CommentId, bool IsHidden) : IRequest<Result<LostFoundCommentDto>>;

public class AdminLostFoundHandlers :
    IRequestHandler<GetAdminLostFoundPostsQuery, Result<List<LostFoundPostDto>>>,
    IRequestHandler<UpdateLostFoundPostStatusCommand, Result<LostFoundPostDto>>,
    IRequestHandler<AdminUpdateLostFoundPostCommand, Result<LostFoundPostDto>>,
    IRequestHandler<AdminDeleteLostFoundPostCommand, Result<bool>>,
    IRequestHandler<AdminHideLostFoundCommentCommand, Result<LostFoundCommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminLostFoundHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<LostFoundPostDto>>> Handle(GetAdminLostFoundPostsQuery request, CancellationToken ct)
    {
        var posts = await _unitOfWork.Repository<LostFoundPost>().GetAllAsync(ct);

        // Load users to map names
        var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();
        var users = await _unitOfWork.Users.FindAsync(u => authorIds.Contains(u.Id), ct);
        var userDict = users.ToDictionary(u => u.Id);

        var dtos = new List<LostFoundPostDto>();
        foreach (var p in posts.OrderByDescending(p => p.CreatedAt))
        {
            // Load comments for each post (including hidden ones)
            var comments = await _unitOfWork.Repository<LostFoundComment>().FindAsync(c => c.PostId == p.Id, ct);
            var commentAuthorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
            var commentAuthors = await _unitOfWork.Users.FindAsync(u => commentAuthorIds.Contains(u.Id), ct);
            var commentAuthorDict = commentAuthors.ToDictionary(u => u.Id);

            var commentDtos = comments.OrderBy(c => c.CreatedAt).Select(c => new LostFoundCommentDto
            {
                Id = c.Id,
                PostId = c.PostId,
                AuthorId = c.AuthorId,
                AuthorName = commentAuthorDict.ContainsKey(c.AuthorId) ? commentAuthorDict[c.AuthorId].DisplayName : "Unknown",
                Content = c.Content,
                IsHidden = c.IsHidden,
                CreatedAt = c.CreatedAt
            }).ToList();

            dtos.Add(new LostFoundPostDto
            {
                Id = p.Id,
                AuthorId = p.AuthorId,
                AuthorName = userDict.ContainsKey(p.AuthorId) ? userDict[p.AuthorId].DisplayName : "Unknown",
                Title = p.Title,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                Type = p.Type.ToString(),
                TrainNumber = p.TrainNumber,
                ContactInfo = p.ContactInfo,
                IsResolved = p.IsResolved,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                Comments = commentDtos
            });
        }

        return Result<List<LostFoundPostDto>>.Success(dtos);
    }

    public async Task<Result<LostFoundPostDto>> Handle(UpdateLostFoundPostStatusCommand request, CancellationToken ct)
    {
        var post = await _unitOfWork.Repository<LostFoundPost>().GetByIdAsync(request.PostId, ct);
        if (post == null)
            return Result<LostFoundPostDto>.Failure("Post not found.", 404);

        post.Status = request.Status;
        if (request.Status == LostFoundStatus.Closed)
        {
            post.IsResolved = true;
        }
        else
        {
            post.IsResolved = false;
        }

        await _unitOfWork.Repository<LostFoundPost>().UpdateAsync(post, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var author = await _unitOfWork.Users.GetByIdAsync(post.AuthorId, ct);

        // Fetch comments
        var comments = await _unitOfWork.Repository<LostFoundComment>().FindAsync(c => c.PostId == post.Id, ct);
        var commentAuthorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
        var commentAuthors = await _unitOfWork.Users.FindAsync(u => commentAuthorIds.Contains(u.Id), ct);
        var commentAuthorDict = commentAuthors.ToDictionary(u => u.Id);

        var commentDtos = comments.OrderBy(c => c.CreatedAt).Select(c => new LostFoundCommentDto
        {
            Id = c.Id,
            PostId = c.PostId,
            AuthorId = c.AuthorId,
            AuthorName = commentAuthorDict.ContainsKey(c.AuthorId) ? commentAuthorDict[c.AuthorId].DisplayName : "Unknown",
            Content = c.Content,
            IsHidden = c.IsHidden,
            CreatedAt = c.CreatedAt
        }).ToList();

        return Result<LostFoundPostDto>.Success(new LostFoundPostDto
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            AuthorName = author?.DisplayName ?? "Unknown",
            Title = post.Title,
            Description = post.Description,
            ImageUrl = post.ImageUrl,
            Type = post.Type.ToString(),
            TrainNumber = post.TrainNumber,
            ContactInfo = post.ContactInfo,
            IsResolved = post.IsResolved,
            Status = post.Status.ToString(),
            CreatedAt = post.CreatedAt,
            Comments = commentDtos
        });
    }

    public async Task<Result<LostFoundPostDto>> Handle(AdminUpdateLostFoundPostCommand request, CancellationToken ct)
    {
        var post = await _unitOfWork.Repository<LostFoundPost>().GetByIdAsync(request.PostId, ct);
        if (post == null)
            return Result<LostFoundPostDto>.Failure("Post not found.", 404);

        post.Title = request.Title;
        post.Description = request.Description;
        post.Type = request.Type;
        post.TrainNumber = request.TrainNumber;
        post.ContactInfo = request.ContactInfo;

        await _unitOfWork.Repository<LostFoundPost>().UpdateAsync(post, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var author = await _unitOfWork.Users.GetByIdAsync(post.AuthorId, ct);

        return Result<LostFoundPostDto>.Success(new LostFoundPostDto
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            AuthorName = author?.DisplayName ?? "Unknown",
            Title = post.Title,
            Description = post.Description,
            ImageUrl = post.ImageUrl,
            Type = post.Type.ToString(),
            TrainNumber = post.TrainNumber,
            ContactInfo = post.ContactInfo,
            IsResolved = post.IsResolved,
            Status = post.Status.ToString(),
            CreatedAt = post.CreatedAt
        });
    }

    public async Task<Result<bool>> Handle(AdminDeleteLostFoundPostCommand request, CancellationToken ct)
    {
        var post = await _unitOfWork.Repository<LostFoundPost>().GetByIdAsync(request.PostId, ct);
        if (post == null)
            return Result<bool>.Failure("Post not found.", 404);

        await _unitOfWork.Repository<LostFoundPost>().DeleteAsync(post, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<LostFoundCommentDto>> Handle(AdminHideLostFoundCommentCommand request, CancellationToken ct)
    {
        var comment = await _unitOfWork.Repository<LostFoundComment>().GetByIdAsync(request.CommentId, ct);
        if (comment == null)
            return Result<LostFoundCommentDto>.Failure("Comment not found.", 404);

        comment.IsHidden = request.IsHidden;
        await _unitOfWork.Repository<LostFoundComment>().UpdateAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var author = await _unitOfWork.Users.GetByIdAsync(comment.AuthorId, ct);

        return Result<LostFoundCommentDto>.Success(new LostFoundCommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            AuthorName = author?.DisplayName ?? "Unknown",
            Content = comment.Content,
            IsHidden = comment.IsHidden,
            CreatedAt = comment.CreatedAt
        });
    }
}

public class PendingLiveUpdateDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public DateOnly TripDate { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? StatusTag { get; set; }
    public string? CrowdState { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record GetPendingLiveUpdatesQuery() : IRequest<Result<List<PendingLiveUpdateDto>>>;
public record GetRemovalRequestedLiveUpdatesQuery() : IRequest<Result<List<PendingLiveUpdateDto>>>;
public record ApproveLiveUpdateCommand(Guid Id) : IRequest<Result<bool>>;
public record DeleteLiveUpdateCommand(Guid Id) : IRequest<Result<bool>>;
public record DenyLiveUpdateRemovalCommand(Guid Id) : IRequest<Result<bool>>;

public class LiveUpdateModerationHandlers :
    IRequestHandler<GetPendingLiveUpdatesQuery, Result<List<PendingLiveUpdateDto>>>,
    IRequestHandler<GetRemovalRequestedLiveUpdatesQuery, Result<List<PendingLiveUpdateDto>>>,
    IRequestHandler<ApproveLiveUpdateCommand, Result<bool>>,
    IRequestHandler<DeleteLiveUpdateCommand, Result<bool>>,
    IRequestHandler<DenyLiveUpdateRemovalCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TripNotificationHelper _notificationHelper;

    public LiveUpdateModerationHandlers(IUnitOfWork unitOfWork, TripNotificationHelper notificationHelper)
    {
        _unitOfWork = unitOfWork;
        _notificationHelper = notificationHelper;
    }

    public async Task<Result<List<PendingLiveUpdateDto>>> Handle(GetPendingLiveUpdatesQuery request, CancellationToken ct)
    {
        var updates = await _unitOfWork.Repository<TripLiveUpdate>().FindAsync(u => !u.IsApproved, ct);
        var dtos = new List<PendingLiveUpdateDto>();

        foreach (var u in updates)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(u.TripId, ct);
            string trainNumber = string.Empty;
            DateOnly tripDate = default;
            if (trip != null)
            {
                tripDate = trip.TripDate;
                var train = await _unitOfWork.Trains.GetByIdAsync(trip.TrainId, ct);
                if (train != null)
                {
                    trainNumber = train.TrainNumber;
                }
            }
            var author = u.AuthorId.HasValue ? await _unitOfWork.Users.GetByIdAsync(u.AuthorId.Value, ct) : null;

            dtos.Add(new PendingLiveUpdateDto
            {
                Id = u.Id,
                TripId = u.TripId,
                TrainNumber = trainNumber,
                TripDate = tripDate,
                AuthorName = author?.DisplayName ?? "Unknown",
                Content = u.Content,
                StatusTag = u.StatusTag?.ToString(),
                CrowdState = u.CrowdState?.ToString(),
                Latitude = u.Latitude,
                Longitude = u.Longitude,
                CreatedAt = u.CreatedAt
            });
        }

        return Result<List<PendingLiveUpdateDto>>.Success(dtos.OrderByDescending(d => d.CreatedAt).ToList());
    }

    public async Task<Result<List<PendingLiveUpdateDto>>> Handle(GetRemovalRequestedLiveUpdatesQuery request, CancellationToken ct)
    {
        var updates = await _unitOfWork.Repository<TripLiveUpdate>().FindAsync(u => u.IsApproved && u.IsRemovalRequested, ct);
        var dtos = new List<PendingLiveUpdateDto>();

        foreach (var u in updates)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(u.TripId, ct);
            string trainNumber = string.Empty;
            DateOnly tripDate = default;
            if (trip != null)
            {
                tripDate = trip.TripDate;
                var train = await _unitOfWork.Trains.GetByIdAsync(trip.TrainId, ct);
                if (train != null)
                {
                    trainNumber = train.TrainNumber;
                }
            }
            var author = u.AuthorId.HasValue ? await _unitOfWork.Users.GetByIdAsync(u.AuthorId.Value, ct) : null;

            dtos.Add(new PendingLiveUpdateDto
            {
                Id = u.Id,
                TripId = u.TripId,
                TrainNumber = trainNumber,
                TripDate = tripDate,
                AuthorName = author?.DisplayName ?? "Unknown",
                Content = u.Content,
                StatusTag = u.StatusTag?.ToString(),
                CrowdState = u.CrowdState?.ToString(),
                Latitude = u.Latitude,
                Longitude = u.Longitude,
                CreatedAt = u.CreatedAt
            });
        }

        return Result<List<PendingLiveUpdateDto>>.Success(dtos.OrderByDescending(d => d.CreatedAt).ToList());
    }

    public async Task<Result<bool>> Handle(ApproveLiveUpdateCommand request, CancellationToken ct)
    {
        var update = await _unitOfWork.Repository<TripLiveUpdate>().GetByIdAsync(request.Id, ct);
        if (update == null)
            return Result<bool>.Failure("Live update not found.", 404);

        update.IsApproved = true;
        await _unitOfWork.Repository<TripLiveUpdate>().UpdateAsync(update, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _notificationHelper.NotifyFollowersOfNewReportAsync(update.TripId, update, ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> Handle(DeleteLiveUpdateCommand request, CancellationToken ct)
    {
        var update = await _unitOfWork.Repository<TripLiveUpdate>().GetByIdAsync(request.Id, ct);
        if (update == null)
            return Result<bool>.Failure("Live update not found.", 404);

        await _unitOfWork.Repository<TripLiveUpdate>().DeleteAsync(update, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> Handle(DenyLiveUpdateRemovalCommand request, CancellationToken ct)
    {
        var update = await _unitOfWork.Repository<TripLiveUpdate>().GetByIdAsync(request.Id, ct);
        if (update == null)
            return Result<bool>.Failure("Live update not found.", 404);

        update.IsRemovalRequested = false;
        await _unitOfWork.Repository<TripLiveUpdate>().UpdateAsync(update, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

