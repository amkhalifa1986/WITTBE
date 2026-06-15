using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Lookups;

public class CrowdLevelLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

// Queries / Commands
public record GetCrowdLevelsQuery() : IRequest<Result<List<CrowdLevelLookupDto>>>;

public record CreateCrowdLevelCommand(
    string Code,
    string NameAr,
    string NameEn
) : IRequest<Result<CrowdLevelLookupDto>>;

public record UpdateCrowdLevelCommand(
    Guid Id,
    string NameAr,
    string NameEn
) : IRequest<Result<CrowdLevelLookupDto>>;

public record DeleteCrowdLevelCommand(Guid Id) : IRequest<Result<string>>;

// Handlers
public class GetCrowdLevelsQueryHandler : IRequestHandler<GetCrowdLevelsQuery, Result<List<CrowdLevelLookupDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetCrowdLevelsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<CrowdLevelLookupDto>>> Handle(GetCrowdLevelsQuery request, CancellationToken ct)
    {
        var list = await _unitOfWork.Repository<CrowdLevelLookup>().GetAllAsync(ct);
        var dtos = list.Select(l => new CrowdLevelLookupDto
        {
            Id = l.Id,
            Code = l.Code,
            NameAr = l.NameAr,
            NameEn = l.NameEn
        }).OrderBy(l => l.Code).ToList();

        return Result<List<CrowdLevelLookupDto>>.Success(dtos);
    }
}

public class CreateCrowdLevelCommandHandler : IRequestHandler<CreateCrowdLevelCommand, Result<CrowdLevelLookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateCrowdLevelCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<CrowdLevelLookupDto>> Handle(CreateCrowdLevelCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return Result<CrowdLevelLookupDto>.Failure("Code is required.", 400);

        var trimmedCode = request.Code.Trim();

        // Check uniqueness
        var existing = await _unitOfWork.Repository<CrowdLevelLookup>()
            .FindAsync(l => l.Code.ToLower() == trimmedCode.ToLower(), ct);
        
        if (existing.Any())
            return Result<CrowdLevelLookupDto>.Failure("A crowd level with this code already exists.", 400);

        var lookup = new CrowdLevelLookup
        {
            Code = trimmedCode,
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn.Trim()
        };

        await _unitOfWork.Repository<CrowdLevelLookup>().AddAsync(lookup, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CrowdLevelLookupDto>.Success(new CrowdLevelLookupDto
        {
            Id = lookup.Id,
            Code = lookup.Code,
            NameAr = lookup.NameAr,
            NameEn = lookup.NameEn
        }, 201);
    }
}

public class UpdateCrowdLevelCommandHandler : IRequestHandler<UpdateCrowdLevelCommand, Result<CrowdLevelLookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateCrowdLevelCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<CrowdLevelLookupDto>> Handle(UpdateCrowdLevelCommand request, CancellationToken ct)
    {
        var lookup = await _unitOfWork.Repository<CrowdLevelLookup>().GetByIdAsync(request.Id, ct);
        if (lookup == null)
            return Result<CrowdLevelLookupDto>.Failure("Crowd level not found.", 404);

        lookup.NameAr = request.NameAr.Trim();
        lookup.NameEn = request.NameEn.Trim();

        await _unitOfWork.Repository<CrowdLevelLookup>().UpdateAsync(lookup, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CrowdLevelLookupDto>.Success(new CrowdLevelLookupDto
        {
            Id = lookup.Id,
            Code = lookup.Code,
            NameAr = lookup.NameAr,
            NameEn = lookup.NameEn
        });
    }
}

public class DeleteCrowdLevelCommandHandler : IRequestHandler<DeleteCrowdLevelCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteCrowdLevelCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<string>> Handle(DeleteCrowdLevelCommand request, CancellationToken ct)
    {
        var lookup = await _unitOfWork.Repository<CrowdLevelLookup>().GetByIdAsync(request.Id, ct);
        if (lookup == null)
            return Result<string>.Failure("Crowd level not found.", 404);

        await _unitOfWork.Repository<CrowdLevelLookup>().DeleteAsync(lookup, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<string>.Success("Crowd level deleted successfully.");
    }
}
