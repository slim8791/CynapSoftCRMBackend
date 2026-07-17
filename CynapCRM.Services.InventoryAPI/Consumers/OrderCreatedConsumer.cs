using CynapCRM.MessageBus.Events;
using CynapCRM.Services.InventoryAPI.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(AppDbContext db, ILogger<OrderCreatedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "📥 Message reçu : Commande #{OrderId} créée par client #{ClientId}",
            message.OrderId, message.ClientId);

        foreach (var line in message.Lines)
        {
            var stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_Produit == line.ProductId);

            if (stock != null)
            {
                stock.QteReservee += line.Quantity;
                _logger.LogInformation(
                    " Stock réservé : Produit #{ProductId}, Qté réservée = {Qty}",
                    line.ProductId, stock.QteReservee);
            }
            else
            {
                _logger.LogWarning(
                    " Aucun stock trouvé pour le produit #{ProductId}",
                    line.ProductId);
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation(" Réservation de stock terminée pour commande #{OrderId}", message.OrderId);
    }
}