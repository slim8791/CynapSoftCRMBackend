using CynapCRM.MessageBus.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CynapCRM.Services.OrderAPI.Consumers;

public class ProductPriceChangedConsumer : IConsumer<ProductPriceChangedEvent>
{
    private readonly ILogger<ProductPriceChangedConsumer> _logger;

    public ProductPriceChangedConsumer(ILogger<ProductPriceChangedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductPriceChangedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "📥 [OrderAPI - Commandes] Changement de tarif intercepté pour le produit #{ProductId} ({Nom}) -> Nouveau prix : {Prix} DA",
            message.ProductId, message.NomProduit, message.NouveauPrix);

        _logger.LogInformation("✅ Tarification mise à jour dans le système commercial pour Produit #{ProductId} !", message.ProductId);

        await Task.CompletedTask;
    }
}
