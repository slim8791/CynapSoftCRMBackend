using CynapCRM.MessageBus.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CynapCRM.Services.OrderAPI.Consumers;

public class VisiteCompletedConsumer : IConsumer<VisiteCompletedEvent>
{
    private readonly ILogger<VisiteCompletedConsumer> _logger;

    public VisiteCompletedConsumer(ILogger<VisiteCompletedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VisiteCompletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "📥 [OrderAPI - Commandes] Rapport de visite reçu du délégué #{DelegueId} (Visite #{VisiteId}) -> Résultat : {Resultat}",
            message.DelegueId, message.VisiteId, message.Resultat);

        if (message.Resultat == "Positif")
        {
            _logger.LogInformation("🎯 Visite positive ! Le système commercial est prêt pour une éventuelle prise de commande.");
        }

        await Task.CompletedTask;
    }
}