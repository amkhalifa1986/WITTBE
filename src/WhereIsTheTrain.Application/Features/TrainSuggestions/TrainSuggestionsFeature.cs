using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.TrainSuggestions;

public class TrainSuggestionDto
{
    public Guid Id { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? RouteDescriptionAr { get; set; }
    public string? RouteDescriptionEn { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
    public string SuggestedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// --- Suggest Train ---
public record SuggestTrainCommand(
    Guid UserId, string TrainNumber, string NameAr, string NameEn, string? DescriptionAr, string? DescriptionEn, string? RouteDescriptionAr, string? RouteDescriptionEn
) : IRequest<Result<TrainSuggestionDto>>;

public class SuggestTrainCommandHandler : IRequestHandler<SuggestTrainCommand, Result<TrainSuggestionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SuggestTrainCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<TrainSuggestionDto>> Handle(SuggestTrainCommand request, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user == null)
            return Result<TrainSuggestionDto>.Failure("User not found.", 404);

        var suggestion = new TrainSuggestion
        {
            SuggestedById = request.UserId,
            TrainNumber = request.TrainNumber,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            RouteDescriptionAr = request.RouteDescriptionAr,
            RouteDescriptionEn = request.RouteDescriptionEn
        };

        await _unitOfWork.Repository<TrainSuggestion>().AddAsync(suggestion, ct);
        await _unitOfWork.SaveChangesAsync(ct);

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
            SuggestedByName = user.DisplayName,
            CreatedAt = suggestion.CreatedAt
        }, 201);
    }
}

// --- Get My Suggestions ---
public record GetMySuggestionsQuery(Guid UserId) : IRequest<Result<List<TrainSuggestionDto>>>;

public class GetMySuggestionsQueryHandler : IRequestHandler<GetMySuggestionsQuery, Result<List<TrainSuggestionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMySuggestionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<TrainSuggestionDto>>> Handle(GetMySuggestionsQuery request, CancellationToken ct)
    {
        var suggestions = await _unitOfWork.Repository<TrainSuggestion>()
            .FindAsync(s => s.SuggestedById == request.UserId, ct);

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);

        var dtos = suggestions.OrderByDescending(s => s.CreatedAt).Select(s => new TrainSuggestionDto
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
        }).ToList();

        return Result<List<TrainSuggestionDto>>.Success(dtos);
    }
}
