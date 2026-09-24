using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Infrastructure.Authentication;
using StudentAPI.Infrastructure.Caching;
using StudentAPI.Infrastructure.Persistence;
using StudentAPI.Infrastructure.Persistence.Repositories;

namespace StudentAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            ));



        var redisConnectionString = configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Thiếu Redis connection string");

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "studentapi:";
        });
        // services.AddMemoryCache();
        // services.AddSingleton<ICacheService, InMemoryCacheService>();
        services.AddSingleton<ICacheService, RedisCacheService>();




        services.AddScoped<IAppDbContext>(provider =>
            (IAppDbContext)provider.GetRequiredService<AppDbContext>());

        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<SinhVienRepository>();
        services.AddScoped<ISinhVienRepository>(sp =>
            new CachedSinhVienRepository(
                sp.GetRequiredService<SinhVienRepository>(),
                sp.GetRequiredService<ICacheService>()));

        services.AddScoped<BoMonRepository>();
        services.AddScoped<IBoMonRepository>(sp =>
            new CachedBoMonRepository(
                sp.GetRequiredService<BoMonRepository>(),
                sp.GetRequiredService<ICacheService>()));

        services.AddScoped<LopHocRepository>();
        services.AddScoped<ILopHocRepository>(sp =>
            new CachedLopHocRepository(
                sp.GetRequiredService<LopHocRepository>(),
                sp.GetRequiredService<ICacheService>()));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();


        return services;
    }
}