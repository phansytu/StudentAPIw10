using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, InMemoryCacheService>();



        services.AddScoped<IAppDbContext>(provider =>
            (IAppDbContext)provider.GetRequiredService<AppDbContext>());

        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ISinhVienRepository, SinhVienRepository>();

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