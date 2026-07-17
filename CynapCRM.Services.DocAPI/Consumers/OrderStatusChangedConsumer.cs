using CynapCRM.MessageBus.Events;
using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CynapCRM.Services.DocAPI.Consumers;

public class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly IBLService _blService;
    private readonly IFactureService _factureService;
    private readonly ILogger<OrderStatusChangedConsumer> _logger;

    public OrderStatusChangedConsumer(IBLService blService, IFactureService factureService, ILogger<OrderStatusChangedConsumer> logger)
    {
        _blService = blService;
        _factureService = factureService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "📥 [DocAPI - Documents] Statut commande #{OrderId} changé : {OldStatus} -> {NewStatus}",
            message.OrderId, message.OldStatus, message.NewStatus);

        // Lorsque la commande est validée, confirmée ou livrée, on génère le BL et la Facture
        if (message.NewStatus == "LIVREE" || message.NewStatus == "CONFIRMEE" || message.NewStatus == "VALIDEE")
        {
            try
            {
                _logger.LogInformation("📦 Génération automatique du Bon de Livraison (BL) et de la Facture pour la Commande #{OrderId}...", message.OrderId);

                var blDto = new BonLivraisonDto
                {
                    Nom_Doc = $"BL_Commande_{message.OrderId}_{DateTime.UtcNow:yyyyMMdd}.pdf",
                    DateCreation = DateTime.UtcNow,
                    Id_Commande = message.OrderId,
                    Id_Client = message.ClientId,
                    TypeDocument = "BON_LIVRAISON",
                    CloudinaryUrl = $"https://cynapharm-crm.internal/docs/bl/{message.OrderId}.pdf"
                };
                await _blService.CreateOrUpdateBonLivraisonAsync(blDto);

                var factureDto = new FactureDto
                {
                    Nom_Doc = $"Facture_Commande_{message.OrderId}_{DateTime.UtcNow:yyyyMMdd}.pdf",
                    DateCreation = DateTime.UtcNow,
                    Id_Commande = message.OrderId,
                    Id_Client = message.ClientId,
                    TypeDocument = "FACTURE",
                    DateFacture = DateTime.UtcNow,
                    MontantHT = 0, // Sera mis à jour par le service comptabilité ou calculé
                    MontantTTC = 0,
                    CloudinaryUrl = $"https://cynapharm-crm.internal/docs/factures/{message.OrderId}.pdf"
                };
                await _factureService.CreateOrUpdateFactureAsync(factureDto);

                _logger.LogInformation("✅ BL et Facture générés et archivés avec succès pour la Commande #{OrderId} !", message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la génération des documents de livraison pour la commande #{OrderId}", message.OrderId);
            }
        }
    }
}
