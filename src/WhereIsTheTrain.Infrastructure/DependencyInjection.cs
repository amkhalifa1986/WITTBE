using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WhereIsTheTrain.Application.Interfaces;
using WhereIsTheTrain.Domain.Interfaces;
using WhereIsTheTrain.Infrastructure.Authentication;
using WhereIsTheTrain.Infrastructure.Email;
using WhereIsTheTrain.Infrastructure.Persistence;
using WhereIsTheTrain.Infrastructure.Persistence.Repositories;

namespace WhereIsTheTrain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                x => x.UseNetTopologySuite())
            // BE-06: Disable change tracking globally for all read queries.
            // Cuts memory allocations and CPU overhead significantly under load.
            // Entities returned by queries will NOT be tracked; writes via
            // UnitOfWork.SaveChangesAsync are unaffected (they use explicit Attach/Add).
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IAdminRoleRepository, AdminRoleRepository>();
        services.AddScoped<ITrainRepository, TrainRepository>();
        services.AddScoped<ITripRepository, TripRepository>();

        // JWT
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtService, JwtService>();

        // Email
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
