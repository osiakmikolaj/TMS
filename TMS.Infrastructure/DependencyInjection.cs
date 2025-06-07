using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMS.Application.Common.Interfaces;
using TMS.Infrastructure.Persistence;
using TMS.Infrastructure.Services;

namespace TMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("TMSDb"));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitializer>();

        services.AddTransient<IDateTime, DateTimeService>();

        return services;
    }
}