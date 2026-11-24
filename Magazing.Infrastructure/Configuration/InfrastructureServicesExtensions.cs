using Magazing.Application.Services;
using Magazing.Domain.Repositories;
using Magazing.Infrastructure.Adapters.Email;
using Magazing.Infrastructure.Adapters.Repositories;
using Magazing.Infrastructure.Adapters.Services;
using Magazing.Infrastructure.Adapters.MachineLearning;
using Magazing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Magazing.Infrastructure.Configuration;

public static class InfrastructureServicesExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");

        services.AddDbContext<MagazingDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddEmailServices(configuration);
        services.AddAssistantServices(configuration);
        services.AddSingleton<IGoogleTokenVerifier, GoogleTokenVerifier>();

        services.AddSingleton<IDemandForecaster, OnnxDemandForecaster>();
        services.AddScoped<IForecastReadService, ForecastReadService>();
        services.AddHostedService<ForecastSchedulerService>();

        return services;
    }

    private static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(EmailOptions.SectionName);
        services.Configure<EmailOptions>(section);

        if (string.IsNullOrWhiteSpace(section[nameof(EmailOptions.ResendApiKey)]))
        {
            services.AddScoped<IEmailSender, LogEmailSender>();
        }
        else
        {
            services.AddHttpClient<IEmailSender, ResendEmailSender>(client =>
            {
                client.BaseAddress = new Uri("https://api.resend.com/");
                client.Timeout = TimeSpan.FromSeconds(15);
            });
        }

        services.AddSingleton<EmailDispatcherService>();
        services.AddHostedService(provider => provider.GetRequiredService<EmailDispatcherService>());

        return services;
    }

    private static IServiceCollection AddAssistantServices(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Assistant");
        services.Configure<AssistantOptions>(section);

        services.AddHttpClient<IChatModel, OpenAiCompatibleChatModel>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(Math.Max(section.GetValue("TimeoutSeconds", 60), 10));
        });

        return services;
    }
}
