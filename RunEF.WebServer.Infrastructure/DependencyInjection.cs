using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunEF.WebServer.Application.Interfaces;
using RunEF.WebServer.Domain.Interfaces;
using RunEF.WebServer.Infrastructure.Data;
using RunEF.WebServer.Infrastructure.Repositories;
using RunEF.WebServer.Infrastructure.Services;

namespace RunEF.WebServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Redis Cache - Temporarily disabled to avoid connection issues
        // services.AddStackExchangeRedisCache(options =>
        // {
        //     options.Configuration = configuration.GetConnectionString("Redis");
        // });
        
        // Use in-memory cache instead
        services.AddMemoryCache();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientService, ClientService>();

        return services;
    }
}