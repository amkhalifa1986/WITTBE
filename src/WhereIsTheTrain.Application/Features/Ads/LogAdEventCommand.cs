using MediatR;
using WhereIsTheTrain.Application.Common;

namespace WhereIsTheTrain.Application.Features.Ads;

public record LogAdImpressionCommand(string ScreenId, string VisitorId, Guid? UserId, string? TrainNumber = null) : IRequest<Result<bool>>;

public record LogAdClickCommand(string ScreenId, string VisitorId, Guid? UserId, string? TrainNumber = null) : IRequest<Result<bool>>;
