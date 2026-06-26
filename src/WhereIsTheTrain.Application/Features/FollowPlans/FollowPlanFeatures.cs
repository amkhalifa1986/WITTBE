using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.FollowPlans;

// --- DTOs ---

public class FollowPlanDto
{
    public Guid Id { get; set; }
    public Guid TrainId { get; set; }
    public int DayOfWeek { get; set; }
    public string RoleType { get; set; } = string.Empty;
    public Guid TargetStopId { get; set; }
    public int AlertLeadTimeMinutes { get; set; }
}

public class UpcomingTripDto
{
    public Guid Id { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string TrainNameAr { get; set; } = string.Empty;
    public string TrainNameEn { get; set; } = string.Empty;
    public DateOnly TripDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RoleType { get; set; } = string.Empty;
    public string TargetStopNameAr { get; set; } = string.Empty;
    public string TargetStopNameEn { get; set; } = string.Empty;
    public TimeSpan? TargetStopScheduledArrival { get; set; }
    public int AlertLeadTimeMinutes { get; set; }
    public string PersonalStatus { get; set; } = string.Empty;
}

public class FollowedTrainPlanDto
{
    public Guid Id { get; set; }
    public Guid TrainId { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string TrainNameAr { get; set; } = string.Empty;
    public string TrainNameEn { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public string RoleType { get; set; } = string.Empty;
    public Guid TargetStopId { get; set; }
    public string TargetStopNameAr { get; set; } = string.Empty;
    public string TargetStopNameEn { get; set; } = string.Empty;
    public int AlertLeadTimeMinutes { get; set; }
}

public class FollowPlanDayConfig
{
    public int DayOfWeek { get; set; }
    public TrainFollowRole RoleType { get; set; }
    public Guid TargetStopId { get; set; }
    public int AlertLeadTimeMinutes { get; set; }
}

// --- Requests & Handlers ---

// 1. Get Follow Plans for Train
public record GetFollowPlanQuery(Guid TrainId, Guid UserId) : IRequest<Result<List<FollowPlanDto>>>;

public class GetFollowPlanQueryHandler : IRequestHandler<GetFollowPlanQuery, Result<List<FollowPlanDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFollowPlanQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<FollowPlanDto>>> Handle(GetFollowPlanQuery request, CancellationToken ct)
    {
        var plans = await _unitOfWork.Repository<TrainFollowPlan>()
            .FindAsync(p => p.UserId == request.UserId && p.TrainId == request.TrainId, ct);

        var dtos = plans.Select(plan => new FollowPlanDto
        {
            Id = plan.Id,
            TrainId = plan.TrainId,
            DayOfWeek = plan.DayOfWeek,
            RoleType = plan.RoleType.ToString(),
            TargetStopId = plan.TargetStopId,
            AlertLeadTimeMinutes = plan.AlertLeadTimeMinutes
        }).ToList();

        return Result<List<FollowPlanDto>>.Success(dtos);
    }
}

// 2. Create or Update Follow Plans
public record CreateOrUpdateFollowPlanCommand(
    Guid TrainId,
    Guid UserId,
    List<FollowPlanDayConfig> Configurations
) : IRequest<Result<List<FollowPlanDto>>>;

public class CreateOrUpdateFollowPlanCommandHandler : IRequestHandler<CreateOrUpdateFollowPlanCommand, Result<List<FollowPlanDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrUpdateFollowPlanCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<FollowPlanDto>>> Handle(CreateOrUpdateFollowPlanCommand request, CancellationToken ct)
    {
        var train = await _unitOfWork.Trains.GetByIdAsync(request.TrainId, ct);
        if (train == null)
            return Result<List<FollowPlanDto>>.Failure("Train not found.", 404);

        // Fetch existing plans for user & train
        var existingPlans = await _unitOfWork.Repository<TrainFollowPlan>()
            .FindAsync(p => p.UserId == request.UserId && p.TrainId == request.TrainId, ct);

        var savedPlans = new List<TrainFollowPlan>();
        var payloadDays = request.Configurations.Select(c => c.DayOfWeek).ToList();

        // 1. Process payload configurations
        foreach (var config in request.Configurations)
        {
            var stop = await _unitOfWork.Repository<Stop>().GetByIdAsync(config.TargetStopId, ct);
            if (stop == null)
                return Result<List<FollowPlanDto>>.Failure($"Target stop for day {config.DayOfWeek} not found.", 404);

            var plan = existingPlans.FirstOrDefault(p => p.DayOfWeek == config.DayOfWeek);
            bool isNew = plan == null;

            if (isNew)
            {
                plan = new TrainFollowPlan
                {
                    UserId = request.UserId,
                    TrainId = request.TrainId,
                    DayOfWeek = config.DayOfWeek
                };
            }

            plan.RoleType = config.RoleType;
            plan.TargetStopId = config.TargetStopId;
            plan.AlertLeadTimeMinutes = config.AlertLeadTimeMinutes;

            if (isNew)
            {
                await _unitOfWork.Repository<TrainFollowPlan>().AddAsync(plan, ct);
            }
            else
            {
                await _unitOfWork.Repository<TrainFollowPlan>().UpdateAsync(plan, ct);
            }

            savedPlans.Add(plan);
        }

        // 2. Delete existing plans for days not in the payload
        foreach (var plan in existingPlans)
        {
            if (!payloadDays.Contains(plan.DayOfWeek))
            {
                await _unitOfWork.Repository<TrainFollowPlan>().DeleteAsync(plan, ct);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // 3. Sync upcoming trips
        var today = WhereIsTheTrain.Domain.Common.DateHelper.GetEgyptToday();
        var upcomingTrips = await _unitOfWork.Repository<Trip>()
            .FindAsync(t => t.TrainId == request.TrainId && t.TripDate >= today, ct);

        foreach (var trip in upcomingTrips)
        {
            var tripDay = (int)trip.TripDate.DayOfWeek;
            var followers = await _unitOfWork.Repository<TripFollower>()
                .FindAsync(f => f.TripId == trip.Id && f.UserId == request.UserId, ct);
            var follower = followers.FirstOrDefault();

            var matchingPlan = savedPlans.FirstOrDefault(p => p.DayOfWeek == tripDay);

            if (matchingPlan != null)
            {
                if (follower == null)
                {
                    follower = new TripFollower
                    {
                        UserId = request.UserId,
                        TripId = trip.Id,
                        PersonalStatus = PersonalTripStatus.Following,
                        SourcePlanId = matchingPlan.Id
                    };
                    await _unitOfWork.Repository<TripFollower>().AddAsync(follower, ct);
                }
                else
                {
                    follower.SourcePlanId = matchingPlan.Id;
                    await _unitOfWork.Repository<TripFollower>().UpdateAsync(follower, ct);
                }
            }
            else
            {
                // Delete if they followed from a plan associated with this train but that day config is removed
                var isSourcePlanMatch = follower != null && existingPlans.Any(p => p.Id == follower.SourcePlanId);
                if (follower != null && isSourcePlanMatch && follower.PersonalStatus == PersonalTripStatus.Following)
                {
                    await _unitOfWork.Repository<TripFollower>().DeleteAsync(follower, ct);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var dtos = savedPlans.Select(plan => new FollowPlanDto
        {
            Id = plan.Id,
            TrainId = plan.TrainId,
            DayOfWeek = plan.DayOfWeek,
            RoleType = plan.RoleType.ToString(),
            TargetStopId = plan.TargetStopId,
            AlertLeadTimeMinutes = plan.AlertLeadTimeMinutes
        }).ToList();

        return Result<List<FollowPlanDto>>.Success(dtos);
    }
}

// 3. Delete Follow Plans (Global Unfollow)
public record DeleteFollowPlanCommand(Guid TrainId, Guid UserId) : IRequest<Result<bool>>;

public class DeleteFollowPlanCommandHandler : IRequestHandler<DeleteFollowPlanCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFollowPlanCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteFollowPlanCommand request, CancellationToken ct)
    {
        var plans = await _unitOfWork.Repository<TrainFollowPlan>()
            .FindAsync(p => p.UserId == request.UserId && p.TrainId == request.TrainId, ct);

        if (!plans.Any())
            return Result<bool>.Failure("Follow plans not found.", 404);

        // Cancel upcoming trip followers created by any of these plans
        var today = WhereIsTheTrain.Domain.Common.DateHelper.GetEgyptToday();
        var upcomingTrips = await _unitOfWork.Repository<Trip>()
            .FindAsync(t => t.TrainId == request.TrainId && t.TripDate >= today, ct);
        var upcomingTripIds = upcomingTrips.Select(t => t.Id).ToList();

        var planIds = plans.Select(p => p.Id).ToList();

        if (upcomingTripIds.Any())
        {
            var upcomingTripFollowers = await _unitOfWork.Repository<TripFollower>()
                .FindAsync(f => f.UserId == request.UserId && f.SourcePlanId != null && planIds.Contains(f.SourcePlanId.Value) && upcomingTripIds.Contains(f.TripId), ct);

            foreach (var tf in upcomingTripFollowers)
            {
                await _unitOfWork.Repository<TripFollower>().DeleteAsync(tf, ct);
            }
        }

        foreach (var plan in plans)
        {
            await _unitOfWork.Repository<TrainFollowPlan>().DeleteAsync(plan, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

// 4. Get Upcoming Followed Trips (Next 24 Hours)
public record GetUpcomingFollowedTripsQuery(Guid UserId) : IRequest<Result<List<UpcomingTripDto>>>;

public class GetUpcomingFollowedTripsQueryHandler : IRequestHandler<GetUpcomingFollowedTripsQuery, Result<List<UpcomingTripDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUpcomingFollowedTripsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<UpcomingTripDto>>> Handle(GetUpcomingFollowedTripsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = WhereIsTheTrain.Domain.Common.DateHelper.GetEgyptToday();
        var yesterday = today.AddDays(-1);

        var plans = await _unitOfWork.Repository<TrainFollowPlan>()
            .FindAsync(p => p.UserId == request.UserId, ct);

        var result = new List<UpcomingTripDto>();

        foreach (var plan in plans)
        {
            var train = await _unitOfWork.Trains.GetWithRouteAsync(plan.TrainId, ct);
            if (train == null) continue;

            var targetRouteStop = train.RouteStops.FirstOrDefault(rs => rs.StopId == plan.TargetStopId);
            if (targetRouteStop == null) continue;

            var trips = await _unitOfWork.Repository<Trip>()
                .FindAsync(t => t.TrainId == plan.TrainId && t.TripDate >= yesterday, ct);

            foreach (var trip in trips)
            {
                var followers = await _unitOfWork.Repository<TripFollower>()
                    .FindAsync(f => f.TripId == trip.Id && f.UserId == request.UserId && f.SourcePlanId == plan.Id, ct);
                var follower = followers.FirstOrDefault();
                if (follower == null) continue;

                var scheduledTime = targetRouteStop.ScheduledArrival ?? targetRouteStop.ScheduledDeparture ?? TimeSpan.Zero;
                var arrivalDateTime = trip.TripDate.ToDateTime(new TimeOnly(0, 0)).Add(scheduledTime);

                if (arrivalDateTime >= now && arrivalDateTime <= now.AddHours(24))
                {
                    result.Add(new UpcomingTripDto
                    {
                        Id = trip.Id,
                        TrainNumber = train.TrainNumber,
                        TrainNameAr = train.NameAr,
                        TrainNameEn = train.NameEn,
                        TripDate = trip.TripDate,
                        Status = trip.Status.ToString(),
                        RoleType = plan.RoleType.ToString(),
                        TargetStopNameAr = targetRouteStop.Stop.NameAr,
                        TargetStopNameEn = targetRouteStop.Stop.NameEn,
                        TargetStopScheduledArrival = scheduledTime,
                        AlertLeadTimeMinutes = plan.AlertLeadTimeMinutes,
                        PersonalStatus = follower.PersonalStatus.ToString()
                    });
                }
            }
        }

        var sortedResult = result.OrderBy(t => t.TripDate).ThenBy(t => t.TargetStopScheduledArrival).ToList();
        return Result<List<UpcomingTripDto>>.Success(sortedResult);
    }
}

// 5. Get Followed Trains (All Plans)
public record GetFollowedTrainsQuery(Guid UserId) : IRequest<Result<List<FollowedTrainPlanDto>>>;

public class GetFollowedTrainsQueryHandler : IRequestHandler<GetFollowedTrainsQuery, Result<List<FollowedTrainPlanDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFollowedTrainsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<FollowedTrainPlanDto>>> Handle(GetFollowedTrainsQuery request, CancellationToken ct)
    {
        var plans = await _unitOfWork.Repository<TrainFollowPlan>()
            .FindAsync(p => p.UserId == request.UserId, ct);

        var result = new List<FollowedTrainPlanDto>();

        foreach (var plan in plans)
        {
            var train = await _unitOfWork.Trains.GetByIdAsync(plan.TrainId, ct);
            if (train == null) continue;

            var stop = await _unitOfWork.Repository<Stop>().GetByIdAsync(plan.TargetStopId, ct);
            if (stop == null) continue;

            result.Add(new FollowedTrainPlanDto
            {
                Id = plan.Id,
                TrainId = plan.TrainId,
                TrainNumber = train.TrainNumber,
                TrainNameAr = train.NameAr,
                TrainNameEn = train.NameEn,
                DayOfWeek = plan.DayOfWeek,
                RoleType = plan.RoleType.ToString(),
                TargetStopId = plan.TargetStopId,
                TargetStopNameAr = stop.NameAr,
                TargetStopNameEn = stop.NameEn,
                AlertLeadTimeMinutes = plan.AlertLeadTimeMinutes
            });
        }

        return Result<List<FollowedTrainPlanDto>>.Success(result);
    }
}
