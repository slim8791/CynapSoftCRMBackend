using CynapCRM.Services.OrderAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace CynapCRM.Services.OrderAPI.Service.IService
{
    public interface IOrderService
    {

        // 1. GESTION DES COMMANDES (ORDER)
        // ==========================================

        // Récupérer toutes les commandes (Admin/Délégué)
        Task<IEnumerable<CommandeDto>> GetAllOrdersAsync();

        // Récupérer une commande spécifique avec ses lignes
        Task<CommandeDto> GetOrderByIdAsync(int orderId);

        // Récupérer l'historique des commandes d'un client spécifique
        Task<IEnumerable<CommandeDto>> GetOrdersByClientIdAsync(int clientId);

        // Créer une nouvelle commande (initialise le statut à 'Brouillon' ou 'EnAttente')
        Task<CommandeDto> CreateOrderAsync(CreateOrderDto orderDto);

        // Mettre à jour le statut (Valider, Expédier, Annuler)
        // C'est ici que l'Admin valide la commande du Délégué
        Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);

        // Supprimer une commande (Autorisé uniquement si Statut = Brouillon)
        Task<bool> DeleteOrderAsync(int idCommande);

        // 2. GESTION DES LIGNES (ORDER LINES)
        // ==========================================

        // Ajouter une nouvelle ligne à une commande existante
        Task<LigneCommandeDto?> CreateUpdateLigneCommandeAsync(LigneCommandeDto ligneDto);

        // MODIFICATION : Changer la quantité ou la remise d'une ligne
        // Important : Doit recalculer le MontantTotal de la commande parente

        // Supprimer une ligne de produit spécifique
        Task<bool> RemoveLigneCommandeAsync(int ligneId);
        // 3. GESTION DES RÉCLAMATIONS (CLAIMS)
        // ==========================================

        // Lister toutes les réclamations (pour le SAV / Admin)
        Task<IEnumerable<ReclamationDto>> GetAllReclamationsAsync();

        // Récupérer les réclamations liées à une commande précise
        Task<IEnumerable<ReclamationDto>> GetReclamationsByOrderAsync(int orderId);
        Task<IEnumerable<ReclamationDto>> GetReclamationsByClientAsync(int idClient);
        Task<ReclamationDto?> GetReclamationByIdAsync(int idReclamation);

        // Créer une réclamation (Client mécontent, produit cassé, etc.)
        Task<ReclamationDto?> CreateUpdateReclamationAsync(ReclamationDto dto);

        // Mettre à jour le statut d'une réclamation (Ouverte -> En cours -> Résolue)
        Task<bool> UpdateReclamationStatusAsync(int reclamationId, string newStatus);
        Task<bool> DeleteReclamationAsync(int reclamationId);
    }
}

