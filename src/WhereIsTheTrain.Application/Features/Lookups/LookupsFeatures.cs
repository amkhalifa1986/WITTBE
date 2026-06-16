using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Lookups;

public class CityDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public Guid GovernorateId { get; set; }
    public string GovernorateNameAr { get; set; } = string.Empty;
    public string GovernorateNameEn { get; set; } = string.Empty;
}

public class GovernorateDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

// Queries
public record GetCitiesQuery() : IRequest<Result<List<CityDto>>>;
public record GetGovernoratesQuery() : IRequest<Result<List<GovernorateDto>>>;

// Handlers
public class GetCitiesQueryHandler : IRequestHandler<GetCitiesQuery, Result<List<CityDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetCitiesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<CityDto>>> Handle(GetCitiesQuery request, CancellationToken ct)
    {
        var cities = await _unitOfWork.Repository<City>().GetAllAsync(ct);
        var governorates = await _unitOfWork.Repository<Governorate>().GetAllAsync(ct);
        var govDict = governorates.ToDictionary(g => g.Id);

        var dtos = cities.Select(c => new CityDto
        {
            Id = c.Id,
            NameAr = c.NameAr,
            NameEn = c.NameEn,
            GovernorateId = c.GovernorateId,
            GovernorateNameAr = govDict.TryGetValue(c.GovernorateId, out var g) ? g.NameAr : "Unknown",
            GovernorateNameEn = govDict.TryGetValue(c.GovernorateId, out var gov) ? gov.NameEn : "Unknown"
        }).OrderBy(c => c.NameEn).ToList();

        return Result<List<CityDto>>.Success(dtos);
    }
}

public class GetGovernoratesQueryHandler : IRequestHandler<GetGovernoratesQuery, Result<List<GovernorateDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetGovernoratesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<GovernorateDto>>> Handle(GetGovernoratesQuery request, CancellationToken ct)
    {
        var governorates = await _unitOfWork.Repository<Governorate>().GetAllAsync(ct);
        var dtos = governorates.Select(g => new GovernorateDto
        {
            Id = g.Id,
            NameAr = g.NameAr,
            NameEn = g.NameEn
        }).OrderBy(g => g.NameEn).ToList();

        return Result<List<GovernorateDto>>.Success(dtos);
    }
}
