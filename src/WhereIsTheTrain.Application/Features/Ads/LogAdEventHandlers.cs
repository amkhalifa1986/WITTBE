using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Ads;

public class LogAdEventHandlers : 
    IRequestHandler<LogAdImpressionCommand, Result<bool>>,
    IRequestHandler<LogAdClickCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogAdEventHandlers(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private DateTime GetEgyptTime()
    {
        TimeZoneInfo egyptTimeZone;
        try
        {
            egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        }
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
    }

    public async Task<Result<bool>> Handle(LogAdImpressionCommand request, CancellationToken cancellationToken)
    {
        var impression = new AdImpression
        {
            Id = Guid.NewGuid(),
            ScreenId = request.ScreenId,
            UserId = request.UserId,
            VisitorId = request.VisitorId,
            Timestamp = GetEgyptTime(),
            TrainNumber = request.TrainNumber
        };

        await _unitOfWork.Repository<AdImpression>().AddAsync(impression, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> Handle(LogAdClickCommand request, CancellationToken cancellationToken)
    {
        var click = new AdClick
        {
            Id = Guid.NewGuid(),
            ScreenId = request.ScreenId,
            UserId = request.UserId,
            VisitorId = request.VisitorId,
            Timestamp = GetEgyptTime(),
            TrainNumber = request.TrainNumber
        };

        await _unitOfWork.Repository<AdClick>().AddAsync(click, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
