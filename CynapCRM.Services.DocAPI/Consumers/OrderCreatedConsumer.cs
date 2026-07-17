using CynapCRM.MessageBus.Events;
using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CynapCRM.Services.DocAPI.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IBCService _bcService;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IBCService bcService, ILogger<OrderCreatedConsumer> logger)
    {
        _bcService = bcService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "📥 [DocAPI - Documents] Nouvelle commande #{OrderId} interceptée pour le Client #{ClientId} -> Génération automatique du Bon de Commande (BC)...",
            message.OrderId, message.ClientId);

        try
        {
            var bcDto = new BonCommandeDto
            {
                Nom_Doc = $"BC_Commande_{message.OrderId}_{DateTime.UtcNow:yyyyMMdd}.pdf",
                DateCreation = DateTime.UtcNow,
                Id_Commande = message.OrderId,
                Id_Client = message.ClientId,
                TypeDocument = "BON_COMMANDE",
                CloudinaryUrl = $"https://cynapharm-crm.internal/docs/bc/{message.OrderId}.pdf"
            };

            var createdBc = await _bcService.CreateOrUpdateBonCommandeAsync(bcDto);
            if (createdBc != null)
            {
                _logger.LogInformation("✅ Bon de Commande (BC) généré et archivé avec succès pour la Commande #{OrderId} !", message.OrderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de la création automatique du Bon de Commande pour la commande #{OrderId}", message.OrderId);
        }
    }
}
