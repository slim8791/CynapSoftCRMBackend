using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CynapCRM.MessageBus.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddCynapMessageBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        // 1. IMPORTANT : Ne pas bloquer le démarrage de l'API si RabbitMQ est lent ou bloqué par un pare-feu !
        services.Configure<MassTransitHostOptions>(options =>
        {
            options.WaitUntilStarted = false; // L'API démarre instantanément sans attendre RabbitMQ
            options.StartTimeout = TimeSpan.FromSeconds(5);
            options.StopTimeout = TimeSpan.FromSeconds(5);
        });

        services.AddMassTransit(x =>
        {
            // Enregistrer les consumers (si fournis)
            configureConsumers?.Invoke(x);

            // Configurer RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                // Sur CloudAMQP, le VirtualHost est généralement égal au Username (ou "/" par défaut en local)
                var vhost = configuration["RabbitMQ:VirtualHost"] ?? (host.Contains("cloudamqp.com") ? username : "/");

                cfg.Host(host, vhost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Configure automatiquement les endpoints pour chaque consumer
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
