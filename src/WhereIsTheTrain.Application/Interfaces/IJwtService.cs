using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, bool rememberMe = false);
    string GenerateAdminAccessToken(AdminUser admin, bool rememberMe = false);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}
