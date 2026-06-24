using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.SystemLogs;

public class SystemLogDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string LogLevel { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
}

// --- Create System Log Command ---
public record CreateSystemLogCommand(
    string LogLevel,
    string Source,
    string Target,
    Guid? UserId,
    string? UserEmail,
    string Description,
    string? ErrorMessage = null,
    string? StackTrace = null
) : IRequest<Result<Guid>>;

public class CreateSystemLogCommandHandler : IRequestHandler<CreateSystemLogCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateSystemLogCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> Handle(CreateSystemLogCommand request, CancellationToken ct)
    {
        var log = new SystemLog
        {
            Timestamp = DateTime.UtcNow,
            LogLevel = request.LogLevel,
            Source = request.Source,
            Target = request.Target,
            UserId = request.UserId,
            UserEmail = request.UserEmail,
            Description = request.Description,
            ErrorMessage = request.ErrorMessage,
            StackTrace = request.StackTrace
        };

        await _unitOfWork.Repository<SystemLog>().AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(log.Id, 201);
    }
}

// --- Get System Logs Query ---
public record GetSystemLogsQuery(
    int Page = 1,
    int PageSize = 20,
    string? LogLevelFilter = null,
    string? SourceFilter = null,
    string? SearchTerm = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null
) : IRequest<Result<PagedResult<SystemLogDto>>>;

public class GetSystemLogsQueryHandler : IRequestHandler<GetSystemLogsQuery, Result<PagedResult<SystemLogDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSystemLogsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public Task<Result<PagedResult<SystemLogDto>>> Handle(GetSystemLogsQuery request, CancellationToken ct)
    {
        var queryable = _unitOfWork.Repository<SystemLog>().AsQueryable();

        // 1. Filter by Log Level
        if (!string.IsNullOrEmpty(request.LogLevelFilter))
        {
            queryable = queryable.Where(l => l.LogLevel == request.LogLevelFilter);
        }

        // 2. Filter by Source
        if (!string.IsNullOrEmpty(request.SourceFilter))
        {
            queryable = queryable.Where(l => l.Source == request.SourceFilter);
        }

        // 3. Filter by Search Term
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(l => 
                l.Description.ToLower().Contains(search) ||
                l.Target.ToLower().Contains(search) ||
                (l.ErrorMessage != null && l.ErrorMessage.ToLower().Contains(search)) ||
                (l.UserEmail != null && l.UserEmail.ToLower().Contains(search))
            );
        }

        // 4. Filter by Date From
        if (request.DateFrom.HasValue)
        {
            queryable = queryable.Where(l => l.Timestamp >= request.DateFrom.Value);
        }

        // 5. Filter by Date To
        if (request.DateTo.HasValue)
        {
            var dateToUtc = request.DateTo.Value.Date.AddDays(1).AddTicks(-1);
            queryable = queryable.Where(l => l.Timestamp <= dateToUtc);
        }

        // 6. Calculate Total Count
        var totalCount = queryable.Count();

        // 7. Order, Page and Project
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, request.PageSize);
        var skip = (page - 1) * pageSize;

        var items = queryable
            .OrderByDescending(l => l.Timestamp)
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        var dtos = items.Select(l => new SystemLogDto
        {
            Id = l.Id,
            Timestamp = l.Timestamp,
            LogLevel = l.LogLevel,
            Source = l.Source,
            Target = l.Target,
            UserId = l.UserId,
            UserEmail = l.UserEmail,
            Description = l.Description,
            ErrorMessage = l.ErrorMessage,
            StackTrace = l.StackTrace
        }).ToList();

        var pagedResult = new PagedResult<SystemLogDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Task.FromResult(Result<PagedResult<SystemLogDto>>.Success(pagedResult));
    }
}

// --- Clear All System Logs Command ---
public record ClearAllSystemLogsCommand(DateTime? DateFrom = null, DateTime? DateTo = null) : IRequest<Result<bool>>;

public class ClearAllSystemLogsCommandHandler : IRequestHandler<ClearAllSystemLogsCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ClearAllSystemLogsCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(ClearAllSystemLogsCommand request, CancellationToken ct)
    {
        var repository = _unitOfWork.Repository<SystemLog>();
        var queryable = repository.AsQueryable();

        if (request.DateFrom.HasValue)
        {
            queryable = queryable.Where(l => l.Timestamp >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            var dateToUtc = request.DateTo.Value.Date.AddDays(1).AddTicks(-1);
            queryable = queryable.Where(l => l.Timestamp <= dateToUtc);
        }

        var logs = queryable.ToList();
        foreach (var log in logs)
        {
            await repository.DeleteAsync(log, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true, 200);
    }
}

// --- Archive Unarchived System Logs Command ---
public record ArchiveUnarchivedLogsCommand() : IRequest<Result<int>>;

public class ArchiveUnarchivedLogsCommandHandler : IRequestHandler<ArchiveUnarchivedLogsCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveUnarchivedLogsCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<int>> Handle(ArchiveUnarchivedLogsCommand request, CancellationToken ct)
    {
        var repository = _unitOfWork.Repository<SystemLog>();
        
        // Fetch all unarchived logs
        var unarchivedLogs = repository.AsQueryable().Where(l => !l.IsArchived).ToList();
        
        if (unarchivedLogs.Count == 0)
        {
            return Result<int>.Success(0, 200);
        }

        var groupedLogs = unarchivedLogs.GroupBy(l => new { l.Timestamp.Year, l.Timestamp.Month });

        // Ensure directory exists
        var archiveDirectory = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "App_Data", "Archives", "SystemLogs");
        if (!System.IO.Directory.Exists(archiveDirectory))
        {
            System.IO.Directory.CreateDirectory(archiveDirectory);
        }

        foreach (var group in groupedLogs)
        {
            var fileName = $"Archive_{group.Key.Year}_{group.Key.Month:D2}.csv";
            var filePath = System.IO.Path.Combine(archiveDirectory, fileName);
            var fileExists = System.IO.File.Exists(filePath);

            using var writer = new System.IO.StreamWriter(filePath, append: true);
            
            // Write CSV header if file doesn't exist
            if (!fileExists)
            {
                writer.WriteLine("ID,Timestamp,LogLevel,Source,Target,UserId,UserEmail,Description,ErrorMessage,StackTrace");
            }

            foreach (var log in group)
            {
                var id = log.Id.ToString();
                var timestamp = log.Timestamp.ToString("o");
                var level = EscapeCsv(log.LogLevel);
                var source = EscapeCsv(log.Source);
                var target = EscapeCsv(log.Target);
                var userId = log.UserId?.ToString() ?? "";
                var userEmail = EscapeCsv(log.UserEmail);
                var desc = EscapeCsv(log.Description);
                var error = EscapeCsv(log.ErrorMessage);
                var stack = EscapeCsv(log.StackTrace);

                writer.WriteLine($"{id},{timestamp},{level},{source},{target},{userId},{userEmail},{desc},{error},{stack}");
                
                // Mark as archived
                log.IsArchived = true;
                await repository.UpdateAsync(log, ct);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<int>.Success(unarchivedLogs.Count, 200);
    }

    private string EscapeCsv(string? field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
