using CynapCRM.Services.InventoryAPI.Models.Dto;

namespace CynapCRM.Services.InventoryAPI.Service.IService
{
    public interface IInventoryBusinessService
    {

        // Vérifier disponibilité du stock
        Task<bool> CheckStockAvailabilityAsync(int idStock, int quantite);

        // Distribution réelle d’un échantillon (avec décrément)
        Task<bool> DistributeEchantillonAsync(int idDelegue, int idPharmacien, int idMedecin, int idStock, int qte);
        Task<StockSummaryDto> GetStockSummaryByDelegueAsync(int idDelegue);

        // Appliquer gratuité sur stock
        Task<bool> ApplyGratuiteAsync(int idStock, int quantiteAchetee, int seuilPromo);

        // Réservation de stock (avant commande)
        Task<bool> ReserveStockAsync(int idStock, int quantite);
    }
}
