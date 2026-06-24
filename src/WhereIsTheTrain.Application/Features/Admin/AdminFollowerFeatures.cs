using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Admin;

// --- DTOs ---

public class AdminFollowedTrainDto
{
    public Guid TrainId { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string TrainNameAr { get; set; } = string.Empty;
    public string TrainNameEn { get; set; } = string.Empty;
    public List<int> DaysOfWeek { get; set; } = new();
}

public class AdminFollowedTripDto
{
    public Guid FollowerId { get; set; }
    public Guid TripId { get; set; }
    public DateOnly TripDate { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string TrainNameAr { get; set; } = string.Empty;
    public string TrainNameEn { get; set; } = string.Empty;
    public string PersonalStatus { get; set; } = string.Empty;
}

public class UserFollowingsDto
{
    public List<AdminFollowedTrainDto> FollowedTrains { get; set; } = new();
    public List<AdminFollowedTripDto> FollowedTrips { get; set; } = new();
}

public class TrainFollowerUserDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<int> DaysOfWeek { get; set; } = new();
}

public class TripFollowerUserDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid FollowerId { get; set; }
    public string PersonalStatus { get; set; } = string.Empty;
    public DateTime FollowedAt { get; set; }
}

// --- QUERIES & COMMANDS ---

public record GetUserFollowingsQuery(Guid UserId) : IRequest<Result<UserFollowingsDto>>;

public record AdminUnfollowTrainCommand(Guid UserId, Guid TrainId) : IRequest<Result<bool>>;

public record AdminUnfollowTripCommand(Guid UserId, Guid TripId) : IRequest<Result<bool>>;

public record GetTrainFollowersQuery(Guid TrainId) : IRequest<Result<List<TrainFollowerUserDto>>>;

public record DeleteTrainFollowersCommand(Guid TrainId, Guid? UserId) : IRequest<Result<bool>>;

public record GetTripFollowersQuery(Guid TripId) : IRequest<Result<List<TripFollowerUserDto>>>;

public record DeleteTripFollowersCommand(Guid TripId, Guid? UserId) : IRequest<Result<bool>>;


// --- HANDLERS ---

public class AdminFollowerHandlers :
    IRequestHandler<GetUserFollowingsQuery, Result<UserFollowingsDto>>,
    IRequestHandler<AdminUnfollowTrainCommand, Result<bool>>,
    IRequestHandler<AdminUnfollowTripCommand, Result<bool>>,
    IRequestHandler<GetTrainFollowersQuery, Result<List<TrainFollowerUserDto>>>,
    IRequestHandler<DeleteTrainFollowersCommand, Result<bool>>,
    IRequestHandler<GetTripFollowersQuery, Result<List<TripFollowerUserDto>>>,
    IRequestHandler<DeleteTripFollowersCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminFollowerHandlers(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    // 1. Get User Followings
    public async Task<Result<UserFollowingsDto>> Handle(GetUserFollowingsQuery request, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user == null)
            return Result<UserFollowingsDto>.Failure("User not found.", 404);

        var trainPlans = await _unitOfWork.Repository<TrainFollowPlan>().FindAsync(p => p.UserId == request.UserId, ct);
        var tripFollowers = await _unitOfWork.Repository<TripFollower>().FindAsync(p => p.UserId == request.UserId, ct);

        var followedTrains = new List<AdminFollowedTrainDto>();
        var groupedPlans = trainPlans.GroupBy(p => p.TrainId);

        foreach (var group in groupedPlans)
        {
            var train = await _unitOfWork.Trains.GetByIdAsync(group.Key, ct);
            if (train != null)
            {
                followedTrains.Add(new AdminFollowedTrainDto
                {
                    TrainId = train.Id,
                    TrainNumber = train.TrainNumber,
                    TrainNameAr = train.NameAr,
                    TrainNameEn = train.NameEn,
                    DaysOfWeek = group.Select(p => p.DayOfWeek).OrderBy(d => d).ToList()
                });
            }
        }

        var followedTrips = new List<AdminFollowedTripDto>();
        foreach (var tf in tripFollowers)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tf.TripId, ct);
            if (trip != null)
            {
                var train = await _unitOfWork.Trains.GetByIdAsync(trip.TrainId, ct);
                followedTrips.Add(new AdminFollowedTripDto
                {
                    FollowerId = tf.Id,
                    TripId = trip.Id,
                    TripDate = trip.TripDate,
                    TrainNumber = train?.TrainNumber ?? "Unknown",
                    TrainNameAr = train?.NameAr ?? "Unknown",
                    TrainNameEn = train?.NameEn ?? "Unknown",
                    PersonalStatus = tf.PersonalStatus.ToString()
                });
            }
        }

        return Result<UserFollowingsDto>.Success(new UserFollowingsDto
        {
            FollowedTrains = followedTrains.OrderBy(t => t.TrainNumber).ToList(),
            FollowedTrips = followedTrips.OrderByDescending(t => t.TripDate).ToList()
        });
    }

    // 2. Unfollow Train (Single User)
    public async Task<Result<bool>> Handle(AdminUnfollowTrainCommand request, CancellationToken ct)
    {
        var plans = await _unitOfWork.Repository<TrainFollowPlan>()
            .FindAsync(p => p.UserId == request.UserId && p.TrainId == request.TrainId, ct);

        if (!plans.Any())
            return Result<bool>.Failure("Follow plan not found.", 404);

        foreach (var plan in plans)
        {
            await _unitOfWork.Repository<TrainFollowPlan>().DeleteAsync(plan, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    // 3. Unfollow Trip (Single User)
    public async Task<Result<bool>> Handle(AdminUnfollowTripCommand request, CancellationToken ct)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId, ct);
        if (trip == null)
            return Result<bool>.Failure("Trip not found.", 404);

        if (trip.StatusId == TripStatuses.Arrived || trip.StatusId == TripStatuses.Cancelled)
            return Result<bool>.Failure("Cannot modify a finished or cancelled trip.", 400);

        var followers = await _unitOfWork.Repository<TripFollower>()
            .FindAsync(p => p.UserId == request.UserId && p.TripId == request.TripId, ct);

        if (!followers.Any())
            return Result<bool>.Failure("Trip follower record not found.", 404);

        foreach (var f in followers)
        {
            await _unitOfWork.Repository<TripFollower>().DeleteAsync(f, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    // 4. Get Train Followers
    public async Task<Result<List<TrainFollowerUserDto>>> Handle(GetTrainFollowersQuery request, CancellationToken ct)
    {
        var plans = await _unitOfWork.Repository<TrainFollowPlan>().FindAsync(p => p.TrainId == request.TrainId, ct);
        var userIds = plans.Select(p => p.UserId).Distinct().ToList();
        var users = await _unitOfWork.Users.FindAsync(u => userIds.Contains(u.Id), ct);
        var userDict = users.ToDictionary(u => u.Id);

        var result = new List<TrainFollowerUserDto>();
        var groupedByUsers = plans.GroupBy(p => p.UserId);

        foreach (var group in groupedByUsers)
        {
            if (userDict.TryGetValue(group.Key, out var user))
            {
                result.Add(new TrainFollowerUserDto
                {
                    UserId = user.Id,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    DaysOfWeek = group.Select(p => p.DayOfWeek).OrderBy(d => d).ToList()
                });
            }
        }

        return Result<List<TrainFollowerUserDto>>.Success(result.OrderBy(u => u.DisplayName).ToList());
    }

    // 5. Delete Train Followers (Single or All)
    public async Task<Result<bool>> Handle(DeleteTrainFollowersCommand request, CancellationToken ct)
    {
        IReadOnlyList<TrainFollowPlan> plans;
        if (request.UserId.HasValue)
        {
            plans = await _unitOfWork.Repository<TrainFollowPlan>()
                .FindAsync(p => p.TrainId == request.TrainId && p.UserId == request.UserId.Value, ct);
        }
        else
        {
            plans = await _unitOfWork.Repository<TrainFollowPlan>()
                .FindAsync(p => p.TrainId == request.TrainId, ct);
        }

        foreach (var plan in plans)
        {
            await _unitOfWork.Repository<TrainFollowPlan>().DeleteAsync(plan, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    // 6. Get Trip Followers
    public async Task<Result<List<TripFollowerUserDto>>> Handle(GetTripFollowersQuery request, CancellationToken ct)
    {
        var followers = await _unitOfWork.Repository<TripFollower>().FindAsync(p => p.TripId == request.TripId, ct);
        var userIds = followers.Select(f => f.UserId).Distinct().ToList();
        var users = await _unitOfWork.Users.FindAsync(u => userIds.Contains(u.Id), ct);
        var userDict = users.ToDictionary(u => u.Id);

        var result = new List<TripFollowerUserDto>();
        foreach (var f in followers)
        {
            if (userDict.TryGetValue(f.UserId, out var user))
            {
                result.Add(new TripFollowerUserDto
                {
                    UserId = user.Id,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    FollowerId = f.Id,
                    PersonalStatus = f.PersonalStatus.ToString(),
                    FollowedAt = f.FollowedAt
                });
            }
        }

        return Result<List<TripFollowerUserDto>>.Success(result.OrderBy(u => u.DisplayName).ToList());
    }

    // 7. Delete Trip Followers (Single or All)
    public async Task<Result<bool>> Handle(DeleteTripFollowersCommand request, CancellationToken ct)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId, ct);
        if (trip == null)
            return Result<bool>.Failure("Trip not found.", 404);

        if (trip.StatusId == TripStatuses.Arrived || trip.StatusId == TripStatuses.Cancelled)
            return Result<bool>.Failure("Cannot modify a finished or cancelled trip.", 400);

        IReadOnlyList<TripFollower> followers;
        if (request.UserId.HasValue)
        {
            followers = await _unitOfWork.Repository<TripFollower>()
                .FindAsync(p => p.TripId == request.TripId && p.UserId == request.UserId.Value, ct);
        }
        else
        {
            followers = await _unitOfWork.Repository<TripFollower>()
                .FindAsync(p => p.TripId == request.TripId, ct);
        }

        foreach (var f in followers)
        {
            await _unitOfWork.Repository<TripFollower>().DeleteAsync(f, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
