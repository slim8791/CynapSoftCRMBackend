using CynapCRM.MessageBus.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CynapCRM.Services.FieldAPI.Consumers;

public class StockDistributedConsumer : IConsumer<StockDistributedEvent>
{
    private readonly ILogger<StockDistributedConsumer> _logger;

    public StockDistributedConsumer(ILogger<StockDistributedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<StockDistributedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "📥 [FieldAPI - Terrain] Distribution d'échantillons interceptée : Délégué #{DelegueId} a distribué {Qty} boîtes du Stock #{StockId} (Lot : {Lot}) au Médecin #{MedecinId} / Pharmacien #{PharmId}",
            message.DelegueId, message.Quantite, message.StockId, message.NumeroLot, message.MedecinId, message.PharmacienId);

        _logger.LogInformation("✅ Activité terrain du délégué #{DelegueId} mise à jour avec succès !", message.DelegueId);

        await Task.CompletedTask;
    }
}