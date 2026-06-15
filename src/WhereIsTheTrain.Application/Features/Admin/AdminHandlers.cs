using MediatR;
using System.Linq.Expressions;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Admin;

// --- DTOs ---
public class UserDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsSuspended { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ServiceDisruptionDto
{
    public Guid Id { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string? AffectedLine { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// --- COMMANDS & QUERIES ---

// User Management
public record GetUsersQuery() : IRequest<Result<List<UserDto>>>;
public record ToggleUserSuspensionCommand(Guid UserId, bool IsSuspended) : IRequest<Result<bool>>;
public record ChangeUserRoleCommand(Guid UserId, UserRole Role) : IRequest<Result<bool>>;

// CSV Bulk Import
public record ImportStopsCommand(string CsvContent, bool IgnoreDuplicates = false) : IRequest<Result<int>>;
public record ImportTrainsCommand(string CsvContent, bool IgnoreDuplicates = false) : IRequest<Result<int>>;

// Disruptions
public record GetDisruptionsQuery() : IRequest<Result<List<ServiceDisruptionDto>>>;
public record CreateDisruptionCommand(
    string TitleAr, string TitleEn, string DescriptionAr, string DescriptionEn, string? AffectedLine
) : IRequest<Result<ServiceDisruptionDto>>;
public record DeactivateDisruptionCommand(Guid Id) : IRequest<Result<bool>>;

// --- HANDLERS ---
public class AdminHandlers :
    IRequestHandler<GetUsersQuery, Result<List<UserDto>>>,
    IRequestHandler<ToggleUserSuspensionCommand, Result<bool>>,
    IRequestHandler<ChangeUserRoleCommand, Result<bool>>,
    IRequestHandler<ImportStopsCommand, Result<int>>,
    IRequestHandler<ImportTrainsCommand, Result<int>>,
    IRequestHandler<GetDisruptionsQuery, Result<List<ServiceDisruptionDto>>>,
    IRequestHandler<CreateDisruptionCommand, Result<ServiceDisruptionDto>>,
    IRequestHandler<DeactivateDisruptionCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    // User Management
    public async Task<Result<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var users = await _unitOfWork.Users.GetAllAsync(ct);
        var dtos = users
            .Where(u => u.Role != UserRole.Admin) // Filter out admins from standard users list
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                Email = u.Email,
                Role = u.Role,
                IsSuspended = u.IsSuspended,
                CreatedAt = u.CreatedAt
            }).ToList();

        return Result<List<UserDto>>.Success(dtos);
    }

    public async Task<Result<bool>> Handle(ToggleUserSuspensionCommand request, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user == null)
            return Result<bool>.Failure("User not found.", 404);

        user.IsSuspended = request.IsSuspended;
        await _unitOfWork.Users.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> Handle(ChangeUserRoleCommand request, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user == null)
            return Result<bool>.Failure("User not found.", 404);

        if (request.Role == UserRole.Admin)
            return Result<bool>.Failure("Standard users cannot be converted to admins.", 400);

        user.Role = request.Role;
        await _unitOfWork.Users.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // CSV Bulk Import
    public async Task<Result<int>> Handle(ImportStopsCommand request, CancellationToken ct)
    {
        var rows = ParseCsv(request.CsvContent);
        if (rows.Count <= 1)
            return Result<int>.Failure("CSV content is empty or invalid.", 400);

        var dataRows = rows.Skip(1).ToList();
        int importedCount = 0;

        foreach (var row in dataRows)
        {
            if (row.Length < 7) continue;

            var code = row[0].ToUpper();
            var nameAr = row[1];
            var nameEn = row[2];
            var cityAr = row[3];
            var cityEn = row[4];
            
            if (!double.TryParse(row[5], out var lat) || !double.TryParse(row[6], out var lng))
                continue;

            var descAr = row.Length > 7 ? row[7] : "";
            var descEn = row.Length > 8 ? row[8] : "";

            // Check if stop exists
            var existing = await _unitOfWork.Repository<Stop>().FindAsync(s => s.Code.ToLower() == code.ToLower(), ct);
            
            // Lookup or create city
            var city = (await _unitOfWork.Repository<City>().FindAsync(c => c.NameEn.ToLower() == cityEn.ToLower() || c.NameAr == cityAr, ct)).FirstOrDefault();
            if (city == null)
            {
                var gov = (await _unitOfWork.Repository<Governorate>().FindAsync(g => g.NameEn.ToLower() == cityEn.ToLower() || g.NameAr == cityAr, ct)).FirstOrDefault();
                if (gov == null)
                {
                    gov = (await _unitOfWork.Repository<Governorate>().FindAsync(g => g.NameEn.ToLower() == "cairo", ct)).FirstOrDefault()
                          ?? (await _unitOfWork.Repository<Governorate>().GetAllAsync(ct)).FirstOrDefault();
                }
                if (gov == null)
                {
                    gov = new Governorate { NameAr = "القاهرة", NameEn = "Cairo" };
                    await _unitOfWork.Repository<Governorate>().AddAsync(gov, ct);
                    await _unitOfWork.SaveChangesAsync(ct);
                }

                city = new City { NameAr = cityAr, NameEn = cityEn, GovernorateId = gov.Id };
                await _unitOfWork.Repository<City>().AddAsync(city, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            if (existing.Any())
            {
                if (request.IgnoreDuplicates) continue;

                var stop = existing.First();
                stop.NameAr = nameAr;
                stop.NameEn = nameEn;
                stop.CityId = city.Id;
                stop.Latitude = lat;
                stop.Longitude = lng;
                stop.DescriptionAr = descAr;
                stop.DescriptionEn = descEn;
                await _unitOfWork.Repository<Stop>().UpdateAsync(stop, ct);
            }
            else
            {
                var stop = new Stop
                {
                    Code = code,
                    NameAr = nameAr,
                    NameEn = nameEn,
                    CityId = city.Id,
                    Latitude = lat,
                    Longitude = lng,
                    DescriptionAr = descAr,
                    DescriptionEn = descEn
                };
                await _unitOfWork.Repository<Stop>().AddAsync(stop, ct);
            }
            importedCount++;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Success(importedCount);
    }

    public async Task<Result<int>> Handle(ImportTrainsCommand request, CancellationToken ct)
    {
        var rows = ParseCsv(request.CsvContent);
        if (rows.Count <= 1)
            return Result<int>.Failure("CSV content is empty or invalid.", 400);

        var dataRows = rows.Skip(1).ToList();
        var trainsGrouped = dataRows.GroupBy(r => r[0]);
        int importedCount = 0;

        foreach (var group in trainsGrouped)
        {
            var trainNumber = group.Key;
            var firstRow = group.First();
            if (firstRow.Length < 7) continue;

            var nameAr = firstRow[1];
            var nameEn = firstRow[2];
            var descAr = firstRow.Length > 3 ? firstRow[3] : "";
            var descEn = firstRow.Length > 4 ? firstRow[4] : "";

            var existing = await _unitOfWork.Trains.FindAsync(t => t.TrainNumber.ToLower() == trainNumber.ToLower(), ct);
            Train train;
            if (existing.Any())
            {
                if (request.IgnoreDuplicates) continue;

                train = existing.First();
                train.NameAr = nameAr;
                train.NameEn = nameEn;
                train.DescriptionAr = descAr;
                train.DescriptionEn = descEn;
                await _unitOfWork.Trains.UpdateAsync(train, ct);

                // Remove existing route stops to rebuild
                var routeStops = await _unitOfWork.Repository<TrainRouteStop>().FindAsync(rs => rs.TrainId == train.Id, ct);
                foreach (var rs in routeStops)
                {
                    await _unitOfWork.Repository<TrainRouteStop>().DeleteAsync(rs, ct);
                }
            }
            else
            {
                train = new Train
                {
                    TrainNumber = trainNumber,
                    NameAr = nameAr,
                    NameEn = nameEn,
                    DescriptionAr = descAr,
                    DescriptionEn = descEn,
                    IsActive = true
                };
                await _unitOfWork.Trains.AddAsync(train, ct);
            }

            foreach (var row in group)
            {
                if (row.Length < 7) continue;

                var stopCode = row[5];
                if (!int.TryParse(row[6], out var stopOrder)) continue;

                TimeSpan? arrival = null;
                if (row.Length > 7 && TimeSpan.TryParse(row[7], out var arrTime)) arrival = arrTime;

                TimeSpan? departure = null;
                if (row.Length > 8 && TimeSpan.TryParse(row[8], out var depTime)) departure = depTime;

                var stops = await _unitOfWork.Repository<Stop>().FindAsync(s => s.Code.ToLower() == stopCode.ToLower(), ct);
                if (!stops.Any())
                {
                    return Result<int>.Failure($"Stop with code '{stopCode}' was not found. Please import stops first.", 400);
                }

                var stop = stops.First();
                var routeStop = new TrainRouteStop
                {
                    TrainId = train.Id,
                    StopId = stop.Id,
                    StopOrder = stopOrder,
                    ScheduledArrival = arrival,
                    ScheduledDeparture = departure
                };
                await _unitOfWork.Repository<TrainRouteStop>().AddAsync(routeStop, ct);
            }

            importedCount++;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Success(importedCount);
    }

    // Disruptions
    public async Task<Result<List<ServiceDisruptionDto>>> Handle(GetDisruptionsQuery request, CancellationToken ct)
    {
        var alerts = await _unitOfWork.Repository<ServiceDisruption>().GetAllAsync(ct);
        var dtos = alerts.OrderByDescending(a => a.CreatedAt)
            .Select(a => new ServiceDisruptionDto
            {
                Id = a.Id,
                TitleAr = a.TitleAr,
                TitleEn = a.TitleEn,
                DescriptionAr = a.DescriptionAr,
                DescriptionEn = a.DescriptionEn,
                AffectedLine = a.AffectedLine,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            }).ToList();

        return Result<List<ServiceDisruptionDto>>.Success(dtos);
    }

    public async Task<Result<ServiceDisruptionDto>> Handle(CreateDisruptionCommand request, CancellationToken ct)
    {
        var alert = new ServiceDisruption
        {
            TitleAr = request.TitleAr,
            TitleEn = request.TitleEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            AffectedLine = request.AffectedLine,
            IsActive = true
        };

        await _unitOfWork.Repository<ServiceDisruption>().AddAsync(alert, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = new ServiceDisruptionDto
        {
            Id = alert.Id,
            TitleAr = alert.TitleAr,
            TitleEn = alert.TitleEn,
            DescriptionAr = alert.DescriptionAr,
            DescriptionEn = alert.DescriptionEn,
            AffectedLine = alert.AffectedLine,
            IsActive = alert.IsActive,
            CreatedAt = alert.CreatedAt
        };

        return Result<ServiceDisruptionDto>.Success(dto, 201);
    }

    public async Task<Result<bool>> Handle(DeactivateDisruptionCommand request, CancellationToken ct)
    {
        var alert = await _unitOfWork.Repository<ServiceDisruption>().GetByIdAsync(request.Id, ct);
        if (alert == null)
            return Result<bool>.Failure("Alert not found.", 404);

        alert.IsActive = false;
        await _unitOfWork.Repository<ServiceDisruption>().UpdateAsync(alert, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // Helper method for simple CSV splitting
    private static List<string[]> ParseCsv(string content)
    {
        var result = new List<string[]>();
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            result.Add(parts.Select(p => p.Trim().Trim('"')).ToArray());
        }
        return result;
    }
}
