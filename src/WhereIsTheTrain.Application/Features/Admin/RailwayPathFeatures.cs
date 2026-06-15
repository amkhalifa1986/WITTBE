using MediatR;
using NetTopologySuite.Geometries;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Admin;

// --- DTO ---
public class RailwayPathDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public Guid StartStationId { get; set; }
    public string StartStationNameEn { get; set; } = string.Empty;
    public string StartStationNameAr { get; set; } = string.Empty;
    public Guid EndStationId { get; set; }
    public string EndStationNameEn { get; set; } = string.Empty;
    public string EndStationNameAr { get; set; } = string.Empty;
    public List<double[]> RoutePath { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// --- Commands & Queries ---
public record CreateRailwayPathCommand(Guid StartStationId, Guid EndStationId, string Code, string GeoJsonContent) : IRequest<Result<RailwayPathDto>>;
public record UpdateRailwayPathCommand(Guid Id, string GeoJsonContent) : IRequest<Result<RailwayPathDto>>;
public record DeleteRailwayPathCommand(Guid Id) : IRequest<Result<bool>>;
public record GetRailwayPathByIdQuery(Guid Id) : IRequest<Result<RailwayPathDto>>;
public record GetAllRailwayPathsQuery() : IRequest<Result<List<RailwayPathDto>>>;

// --- Handlers ---
public class RailwayPathFeaturesHandlers :
    IRequestHandler<CreateRailwayPathCommand, Result<RailwayPathDto>>,
    IRequestHandler<UpdateRailwayPathCommand, Result<RailwayPathDto>>,
    IRequestHandler<DeleteRailwayPathCommand, Result<bool>>,
    IRequestHandler<GetRailwayPathByIdQuery, Result<RailwayPathDto>>,
    IRequestHandler<GetAllRailwayPathsQuery, Result<List<RailwayPathDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RailwayPathFeaturesHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<RailwayPathDto>> Handle(CreateRailwayPathCommand request, CancellationToken ct)
    {
        // 1. Validate stations are not identical
        if (request.StartStationId == request.EndStationId)
            return Result<RailwayPathDto>.Failure("Start station and End station cannot be identical.", 400);

        // 2. Validate stations exist
        var startStation = await _unitOfWork.Repository<Stop>().GetByIdAsync(request.StartStationId, ct);
        var endStation = await _unitOfWork.Repository<Stop>().GetByIdAsync(request.EndStationId, ct);

        if (startStation == null || endStation == null)
            return Result<RailwayPathDto>.Failure("One or both of the selected stations do not exist.", 400);

        // 3. Prevent duplicate paths for same station connection (in either direction)
        var existing = await _unitOfWork.RailwayPaths.FindAsync(rp => 
            (rp.StartStationId == request.StartStationId && rp.EndStationId == request.EndStationId) ||
            (rp.StartStationId == request.EndStationId && rp.EndStationId == request.StartStationId), ct);

        if (existing.Any())
            return Result<RailwayPathDto>.Failure("A railway path already exists between these stations. Please edit the existing path instead.", 400);

        // 4. Parse & Validate GeoJSON content
        LineString routePath;
        try
        {
            routePath = GeoJsonParser.ParseGeoJsonToLineString(request.GeoJsonContent);
        }
        catch (Exception ex)
        {
            return Result<RailwayPathDto>.Failure($"Invalid GeoJSON format: {ex.Message}", 400);
        }

        // 5. Create path
        var path = new RailwayPath
        {
            StartStationId = request.StartStationId,
            EndStationId = request.EndStationId,
            Code = request.Code,
            NameAr = $"{startStation.NameAr} - {endStation.NameAr}",
            NameEn = $"{startStation.NameEn} - {endStation.NameEn}",
            RoutePath = routePath
        };

        await _unitOfWork.RailwayPaths.AddAsync(path, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Map and return
        var dto = MapToDto(path);
        dto.StartStationNameAr = startStation.NameAr;
        dto.StartStationNameEn = startStation.NameEn;
        dto.EndStationNameAr = endStation.NameAr;
        dto.EndStationNameEn = endStation.NameEn;

        return Result<RailwayPathDto>.Success(dto, 201);
    }

    public async Task<Result<RailwayPathDto>> Handle(UpdateRailwayPathCommand request, CancellationToken ct)
    {
        // 1. Fetch existing path
        var path = await _unitOfWork.RailwayPaths.GetWithStationsByIdAsync(request.Id, ct);
        if (path == null)
            return Result<RailwayPathDto>.Failure("Railway path not found.", 404);

        // 2. Parse & Validate new GeoJSON
        LineString routePath;
        try
        {
            routePath = GeoJsonParser.ParseGeoJsonToLineString(request.GeoJsonContent);
        }
        catch (Exception ex)
        {
            return Result<RailwayPathDto>.Failure($"Invalid GeoJSON format: {ex.Message}", 400);
        }

        // 3. Update path & save (UpdatedAt timestamp is automatically set in ApplicationDbContext SaveChangesAsync)
        path.RoutePath = routePath;
        await _unitOfWork.RailwayPaths.UpdateAsync(path, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<RailwayPathDto>.Success(MapToDto(path));
    }

    public async Task<Result<bool>> Handle(DeleteRailwayPathCommand request, CancellationToken ct)
    {
        var path = await _unitOfWork.RailwayPaths.GetByIdAsync(request.Id, ct);
        if (path == null)
            return Result<bool>.Failure("Railway path not found.", 404);

        await _unitOfWork.RailwayPaths.DeleteAsync(path, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<RailwayPathDto>> Handle(GetRailwayPathByIdQuery request, CancellationToken ct)
    {
        var path = await _unitOfWork.RailwayPaths.GetWithStationsByIdAsync(request.Id, ct);
        if (path == null)
            return Result<RailwayPathDto>.Failure("Railway path not found.", 404);

        return Result<RailwayPathDto>.Success(MapToDto(path));
    }

    public async Task<Result<List<RailwayPathDto>>> Handle(GetAllRailwayPathsQuery request, CancellationToken ct)
    {
        var paths = await _unitOfWork.RailwayPaths.GetAllWithStationsAsync(ct);
        var dtos = paths.Select(MapToDto).ToList();
        return Result<List<RailwayPathDto>>.Success(dtos);
    }

    private static RailwayPathDto MapToDto(RailwayPath path) => new()
    {
        Id = path.Id,
        Code = path.Code,
        NameAr = path.NameAr,
        NameEn = path.NameEn,
        StartStationId = path.StartStationId,
        StartStationNameAr = path.StartStation?.NameAr ?? string.Empty,
        StartStationNameEn = path.StartStation?.NameEn ?? string.Empty,
        EndStationId = path.EndStationId,
        EndStationNameAr = path.EndStation?.NameAr ?? string.Empty,
        EndStationNameEn = path.EndStation?.NameEn ?? string.Empty,
        RoutePath = path.RoutePath?.Coordinates.Select(c => new double[] { c.Y, c.X }).ToList() ?? new(),
        CreatedAt = path.CreatedAt,
        UpdatedAt = path.UpdatedAt
    };
}
