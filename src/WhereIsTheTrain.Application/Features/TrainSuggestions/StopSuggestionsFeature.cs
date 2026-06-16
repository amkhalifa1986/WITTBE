using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.TrainSuggestions;

public class StopSuggestionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    
    public Guid? CityId { get; set; }
    public string? CityNameAr { get; set; }
    public string? CityNameEn { get; set; }
    
    public string? NewCityNameAr { get; set; }
    public string? NewCityNameEn { get; set; }
    public Guid? NewCityGovernorateId { get; set; }
    public string? NewCityGovernorateNameAr { get; set; }
    public string? NewCityGovernorateNameEn { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
    public string SuggestedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// --- Suggest Stop ---
public record SuggestStopCommand(
    Guid UserId, 
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

public class SuggestStopCommandHandler : IRequestHandler<SuggestStopCommand, Result<StopSuggestionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SuggestStopCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<StopSuggestionDto>> Handle(SuggestStopCommand request, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user == null)
            return Result<StopSuggestionDto>.Failure("User not found.", 404);

        if (string.IsNullOrWhiteSpace(request.Code))
            return Result<StopSuggestionDto>.Failure("Stop code is required.", 400);

        if (string.IsNullOrWhiteSpace(request.NameAr) || string.IsNullOrWhiteSpace(request.NameEn))
            return Result<StopSuggestionDto>.Failure("Arabic and English names are required.", 400);

        if (!request.CityId.HasValue && (string.IsNullOrWhiteSpace(request.NewCityNameAr) || string.IsNullOrWhiteSpace(request.NewCityNameEn) || !request.NewCityGovernorateId.HasValue))
        {
            return Result<StopSuggestionDto>.Failure("Please specify an existing city or fill the new city details (Arabic, English, and Governorate).", 400);
        }

        var suggestion = new StopSuggestion
        {
            SuggestedById = request.UserId,
            Code = request.Code.Trim(),
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn.Trim(),
            CityId = request.CityId,
            NewCityNameAr = request.NewCityNameAr?.Trim(),
            NewCityNameEn = request.NewCityNameEn?.Trim(),
            NewCityGovernorateId = request.NewCityGovernorateId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            DescriptionAr = request.DescriptionAr?.Trim(),
            DescriptionEn = request.DescriptionEn?.Trim()
        };

        await _unitOfWork.Repository<StopSuggestion>().AddAsync(suggestion, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Map City info for response
        string? cityNameAr = null;
        string? cityNameEn = null;
        if (suggestion.CityId.HasValue)
        {
            var city = await _unitOfWork.Repository<City>().GetByIdAsync(suggestion.CityId.Value, ct);
            cityNameAr = city?.NameAr;
            cityNameEn = city?.NameEn;
        }

        string? govNameAr = null;
        string? govNameEn = null;
        if (suggestion.NewCityGovernorateId.HasValue)
        {
            var gov = await _unitOfWork.Repository<Governorate>().GetByIdAsync(suggestion.NewCityGovernorateId.Value, ct);
            govNameAr = gov?.NameAr;
            govNameEn = gov?.NameEn;
        }

        return Result<StopSuggestionDto>.Success(new StopSuggestionDto
        {
            Id = suggestion.Id,
            Code = suggestion.Code,
            NameAr = suggestion.NameAr,
            NameEn = suggestion.NameEn,
            CityId = suggestion.CityId,
            CityNameAr = cityNameAr,
            CityNameEn = cityNameEn,
            NewCityNameAr = suggestion.NewCityNameAr,
            NewCityNameEn = suggestion.NewCityNameEn,
            NewCityGovernorateId = suggestion.NewCityGovernorateId,
            NewCityGovernorateNameAr = govNameAr,
            NewCityGovernorateNameEn = govNameEn,
            Latitude = suggestion.Latitude,
            Longitude = suggestion.Longitude,
            DescriptionAr = suggestion.DescriptionAr,
            DescriptionEn = suggestion.DescriptionEn,
            Status = suggestion.Status.ToString(),
            SuggestedByName = user.DisplayName,
            CreatedAt = suggestion.CreatedAt
        }, 201);
    }
}

// --- Get My Stop Suggestions ---
public record GetMyStopSuggestionsQuery(Guid UserId) : IRequest<Result<List<StopSuggestionDto>>>;

public class GetMyStopSuggestionsQueryHandler : IRequestHandler<GetMyStopSuggestionsQuery, Result<List<StopSuggestionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyStopSuggestionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<StopSuggestionDto>>> Handle(GetMyStopSuggestionsQuery request, CancellationToken ct)
    {
        var suggestions = await _unitOfWork.Repository<StopSuggestion>()
            .FindAsync(s => s.SuggestedById == request.UserId, ct);

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        
        var cities = await _unitOfWork.Repository<City>().GetAllAsync(ct);
        var citiesDict = cities.ToDictionary(c => c.Id);

        var govs = await _unitOfWork.Repository<Governorate>().GetAllAsync(ct);
        var govsDict = govs.ToDictionary(g => g.Id);

        var dtos = suggestions.OrderByDescending(s => s.CreatedAt).Select(s => {
            string? cityNameAr = s.CityId.HasValue && citiesDict.TryGetValue(s.CityId.Value, out var c) ? c.NameAr : null;
            string? cityNameEn = s.CityId.HasValue && citiesDict.TryGetValue(s.CityId.Value, out var c2) ? c2.NameEn : null;
            
            string? govNameAr = s.NewCityGovernorateId.HasValue && govsDict.TryGetValue(s.NewCityGovernorateId.Value, out var g) ? g.NameAr : null;
            string? govNameEn = s.NewCityGovernorateId.HasValue && govsDict.TryGetValue(s.NewCityGovernorateId.Value, out var g2) ? g2.NameEn : null;

            return new StopSuggestionDto
            {
                Id = s.Id,
                Code = s.Code,
                NameAr = s.NameAr,
                NameEn = s.NameEn,
                CityId = s.CityId,
                CityNameAr = cityNameAr,
                CityNameEn = cityNameEn,
                NewCityNameAr = s.NewCityNameAr,
                NewCityNameEn = s.NewCityNameEn,
                NewCityGovernorateId = s.NewCityGovernorateId,
                NewCityGovernorateNameAr = govNameAr,
                NewCityGovernorateNameEn = govNameEn,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                DescriptionAr = s.DescriptionAr,
                DescriptionEn = s.DescriptionEn,
                Status = s.Status.ToString(),
                AdminNotes = s.AdminNotes,
                SuggestedByName = user?.DisplayName ?? "Unknown",
                CreatedAt = s.CreatedAt
            };
        }).ToList();

        return Result<List<StopSuggestionDto>>.Success(dtos);
    }
}
