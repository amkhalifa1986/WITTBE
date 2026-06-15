using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Lookups;

public class StatusTagLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

// Queries / Commands
public record GetStatusTagsQuery() : IRequest<Result<List<StatusTagLookupDto>>>;

public record CreateStatusTagCommand(
    string Code,
    string NameAr,
    string NameEn
) : IRequest<Result<StatusTagLookupDto>>;

public record UpdateStatusTagCommand(
    Guid Id,
    string NameAr,
    string NameEn
) : IRequest<Result<StatusTagLookupDto>>;

public record DeleteStatusTagCommand(Guid Id) : IRequest<Result<string>>;

// Handlers
public class GetStatusTagsQueryHandler : IRequestHandler<GetStatusTagsQuery, Result<List<StatusTagLookupDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetStatusTagsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<StatusTagLookupDto>>> Handle(GetStatusTagsQuery request, CancellationToken ct)
    {
        var list = await _unitOfWork.Repository<StatusTagLookup>().GetAllAsync(ct);
        var dtos = list.Select(l => new StatusTagLookupDto
        {
            Id = l.Id,
            Code = l.Code,
            NameAr = l.NameAr,
            NameEn = l.NameEn
        }).OrderBy(l => l.Code).ToList();

        return Result<List<StatusTagLookupDto>>.Success(dtos);
    }
}

public class CreateStatusTagCommandHandler : IRequestHandler<CreateStatusTagCommand, Result<StatusTagLookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateStatusTagCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<StatusTagLookupDto>> Handle(CreateStatusTagCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return Result<StatusTagLookupDto>.Failure("Code is required.", 400);

        var trimmedCode = request.Code.Trim();

        // Check uniqueness
        var existing = await _unitOfWork.Repository<StatusTagLookup>()
            .FindAsync(l => l.Code.ToLower() == trimmedCode.ToLower(), ct);
        
        if (existing.Any())
            return Result<StatusTagLookupDto>.Failure("A status tag with this code already exists.", 400);

        var lookup = new StatusTagLookup
        {
            Code = trimmedCode,
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn.Trim()
        };

        await _unitOfWork.Repository<StatusTagLookup>().AddAsync(lookup, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<StatusTagLookupDto>.Success(new StatusTagLookupDto
        {
            Id = lookup.Id,
            Code = lookup.Code,
            NameAr = lookup.NameAr,
            NameEn = lookup.NameEn
        }, 201);
    }
}

public class UpdateStatusTagCommandHandler : IRequestHandler<UpdateStatusTagCommand, Result<StatusTagLookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateStatusTagCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<StatusTagLookupDto>> Handle(UpdateStatusTagCommand request, CancellationToken ct)
    {
        var lookup = await _unitOfWork.Repository<StatusTagLookup>().GetByIdAsync(request.Id, ct);
        if (lookup == null)
            return Result<StatusTagLookupDto>.Failure("Status tag not found.", 404);

        lookup.NameAr = request.NameAr.Trim();
        lookup.NameEn = request.NameEn.Trim();

        await _unitOfWork.Repository<StatusTagLookup>().UpdateAsync(lookup, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<StatusTagLookupDto>.Success(new StatusTagLookupDto
        {
            Id = lookup.Id,
            Code = lookup.Code,
            NameAr = lookup.NameAr,
            NameEn = lookup.NameEn
        });
    }
}

public class DeleteStatusTagCommandHandler : IRequestHandler<DeleteStatusTagCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteStatusTagCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<string>> Handle(DeleteStatusTagCommand request, CancellationToken ct)
    {
        var lookup = await _unitOfWork.Repository<StatusTagLookup>().GetByIdAsync(request.Id, ct);
        if (lookup == null)
            return Result<string>.Failure("Status tag not found.", 404);

        await _unitOfWork.Repository<StatusTagLookup>().DeleteAsync(lookup, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<string>.Success("Status tag deleted successfully.");
    }
}
