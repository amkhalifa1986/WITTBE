using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.Auth.DTOs;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Auth.Queries;

public record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCurrentUserQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserProfileDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            var adminUser = await _unitOfWork.AdminUsers.GetByIdAsync(request.UserId, cancellationToken);
            if (adminUser == null)
                return Result<UserProfileDto>.Failure("User not found.", 404);

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
