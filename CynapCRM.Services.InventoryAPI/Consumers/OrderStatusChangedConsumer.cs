using CynapCRM.MessageBus.Events;
using CynapCRM.Services.InventoryAPI.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Consumers;

public class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderStatusChangedConsumer> _logger;

    public OrderStatusChangedConsumer(AppDbContext db, ILogger<OrderStatusChangedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "📥 Changement de statut reçu pour la commande #{OrderId} : {OldStatus} -> {NewStatus}",
            message.OrderId, message.OldStatus, message.NewStatus);

        foreach (var line in message.Lines)
        {
            // Attention : on utilise bien StocksDelegues avec un S !
            var stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_Produit == line.ProductId);

            if (stock != null)
            {
                // Cas 1 : La commande est annulée -> On remet le stock réservé à disposition
                if (message.NewStatus == "Annulee")
                {
                    stock.QteReservee = Math.Max(0, stock.QteReservee - line.Quantity);
                    _logger.LogInformation(
                        "↩️ Commande annulée : Stock libéré pour Produit #{ProductId}, Nouvelle Qté réservée = {Qty}",
                        line.ProductId, stock.QteReservee);
                }
                // Cas 2 : La commande est livrée -> On enlève définitivement du stock disponible et de la réservation
                else if (message.NewStatus == "Livree")
                {
                    stock.QteReservee = Math.Max(0, stock.QteReservee - line.Quantity);
                    stock.QteDisponible = Math.Max(0, stock.QteDisponible - line.Quantity);
                    _logger.LogInformation(
                        "🚚 Commande livrée : Stock déduit pour Produit #{ProductId}, Stock disponible restant = {Qty}",
                        line.ProductId, stock.QteDisponible);
                }
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ Aucun stock trouvé pour le produit #{ProductId}",
                    line.ProductId);
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("✅ Mise à jour du stock terminée pour commande #{OrderId}", message.OrderId);
    }
}
