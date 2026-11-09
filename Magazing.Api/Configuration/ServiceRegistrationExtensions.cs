using Magazing.Application;
using Magazing.Infrastructure.Configuration;
using Microsoft.OpenApi;

namespace Magazing.Api.Configuration;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureServices(configuration);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Magazing API",
                Version = "v1",
                Description = "API de gestion de inventario, compras y ventas para pymes."
            });
        });

        services.AddHealthChecks();

        return services;
    }
}
