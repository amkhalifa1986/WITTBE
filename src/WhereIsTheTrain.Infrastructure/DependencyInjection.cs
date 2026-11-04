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

        // AutoDetect opens a connection, and the options callback below runs for every
        // DbContext (every request), so the detected version is cached after the first
        // success instead of adding a database round trip to each API call. A failed
        // detection is not cached, so the API picks MySQL up once it becomes reachable.
        ServerVersion? serverVersion = null;

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (serverVersion == null)
            {
                try
                {
                    serverVersion = ServerVersion.AutoDetect(connectionString);
                }
                catch
                {
                    // Database unreachable: fall back to the in-memory provider below.
                }
            }

            if (serverVersion != null)
            {
                options.UseMySql(
                    connectionString,
                    serverVersion,
                    x => x.UseNetTopologySuite());
            }
            else
            {
                options.UseInMemoryDatabase("WhereIsTheTrainDB");
            }

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

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
