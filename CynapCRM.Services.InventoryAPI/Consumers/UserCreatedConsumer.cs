using CynapCRM.MessageBus.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CynapCRM.Services.InventoryAPI.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "📥 [InventoryAPI - Stock] Nouvel utilisateur inscrit : {Name} (#{UserId}) - Rôle : {Role}",
            message.Name, message.UserId, message.Role);

        if (message.Role == "DELEGUE")
        {
            _logger.LogInformation("📦 Nouveau Délégué ! Le dépôt pharmaceutique lui ouvre un espace pour ses dotations en échantillons et promotions.");
        }

        await Task.CompletedTask;
    }
}
