using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.Auth.DTOs;
using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Profile;

// --- Update Profile ---
public record UpdateProfileCommand(Guid UserId, string? DisplayName, string? Bio, string? AvatarUrl) : IRequest<Result<UserProfileDto>>;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user == null)
        {
            var adminUser = await _unitOfWork.AdminUsers.GetByIdAsync(request.UserId, ct);
            if (adminUser == null)
                return Result<UserProfileDto>.Failure("User not found.", 404);

            if (!string.IsNullOrWhiteSpace(request.DisplayName))
                adminUser.DisplayName = request.DisplayName;
            if (request.Bio != null)
                adminUser.Bio = request.Bio;
            if (request.AvatarUrl != null)
                adminUser.AvatarUrl = request.AvatarUrl;

            await _unitOfWork.AdminUsers.UpdateAsync(adminUser, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<UserProfileDto>.Success(new UserProfileDto
            {
                Id = adminUser.Id,
                DisplayName = adminUser.DisplayName,
                Email = adminUser.Email,
                AvatarUrl = adminUser.AvatarUrl,
                Bio = adminUser.Bio,
                Role = adminUser.IsSuperAdmin ? "SuperAdmin" : "Admin",
                CreatedAt = adminUser.CreatedAt
            });
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            user.DisplayName = request.DisplayName;
        if (request.Bio != null)
            user.Bio = request.Bio;
        if (request.AvatarUrl != null)
            user.AvatarUrl = request.AvatarUrl;

        await _unitOfWork.Users.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<UserProfileDto>.Success(new UserProfileDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        });
    }
}

// --- Get Trip History ---
public record GetTripHistoryQuery(Guid UserId) : IRequest<Result<List<TripDto>>>;

public class GetTripHistoryQueryHandler : IRequestHandler<GetTripHistoryQuery, Result<List<TripDto>>>
{
    private readonly ITripRepository _tripRepo;

    public GetTripHistoryQueryHandler(ITripRepository tripRepo) => _tripRepo = tripRepo;

    public async Task<Result<List<TripDto>>> Handle(GetTripHistoryQuery request, CancellationToken ct)
    {
        var trips = await _tripRepo.GetUserTripHistoryAsync(request.UserId, ct);
        var dtos = trips.Select(t => new TripDto
        {
            Id = t.Id,
            TrainNumber = t.Train.TrainNumber,
            TrainNameAr = t.Train.NameAr,
            TrainNameEn = t.Train.NameEn,
            TripDate = t.TripDate,
            Status = t.Status.ToString(),
            ActualDeparture = t.ActualDeparture,
            ActualArrival = t.ActualArrival,
            FollowerCount = t.Followers?.Count ?? 0,
            IsFollowedByCurrentUser = true
        }).ToList();

        return Result<List<TripDto>>.Success(dtos);
    }
}
