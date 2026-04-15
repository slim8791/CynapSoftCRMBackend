using CynapCRM.Services.InventoryAPI.Models.Dto;

namespace CynapCRM.Services.InventoryAPI.Service.IService
{
    public interface IInventoryService
    {
        // ===============================
        // 📦 STOCK DÉLÉGUÉ (CRUD)
        // ===============================
        Task<IEnumerable<StockDelegueDto>> GetAllStocksAsync(int pageNumber, int pageSize);
        Task<StockDelegueDto?> CreateUpdateStockAsync(StockDelegueDto stockDto);
        Task<StockDelegueDto?> GetStockByIdAsync(int idStock);
        Task<IEnumerable<StockDelegueDto>> GetStocksByDelegueAsync(int idDelegue);
        Task<IEnumerable<StockDelegueDto>> GetStockByProduitAsync(int idProduit);
        Task<StockDelegueDto?> GetStockByLotAsync(string numeroLot);
        Task<bool> DeleteStockAsync(int idStock, string type);

        // ===============================
        // 🎁 STOCK GRATUITÉ
        // ===============================
        Task<StockGratuiteDto?> CreateUpdateStockGratuiteAsync(StockGratuiteDto stockDto);
        Task<StockGratuiteDto?> GetStockGratuiteByIdAsync(int idStock);

        // ===============================
        // 🧪 STOCK ÉCHANTILLON
        // ===============================
        Task<StockEchantillonDto?> CreateUpdateStockEchantillonAsync(StockEchantillonDto stockDto);
        Task<StockEchantillonDto?> GetStockEchantillonByIdAsync(int idStock);

        // ===============================
        // 🧪 DISTRIBUTION ÉCHANTILLONS
        // ===============================
        Task<EchantillonDto?> CreateUpdateEchantillonAsync(EchantillonDto echantillonDto);
        Task<EchantillonDto?> GetEchantillonByIdAsync(int idDistribution);
        Task<IEnumerable<EchantillonDto>> GetDistributionsByMedecinAsync(int idMedecin);
        Task<IEnumerable<EchantillonDto>> GetDistributionsByPharmacienAsync(int idPharmacien);
        Task<bool> DeleteEchantillonAsync(int idDistribution);

        // ===============================
        // 🔄 MOUVEMENTS DE STOCK
        // ===============================
        Task<bool> DecrementStockAsync(int idStock, int qte);
        Task<bool> IncrementStockAsync(int idStock, int qte);
        Task<bool> TransferStockAsync(int idStockSource, int idStockDestination, int qte);

        // ===============================
        // 🧠 LOGIQUE MÉTIER (IMPORTANT)
        // ===============================
        // Vérifier disponibilité du stock
        Task<bool> CheckStockAvailabilityAsync(int idStock, int quantite);

        // Distribution réelle d’un échantillon (avec décrément)
        Task<bool> DistributeEchantillonAsync(int idDelegue, int idPharmacien ,int idMedecin, int idStock, int qte);


        // Appliquer gratuité sur stock
        Task<bool> ApplyGratuiteAsync(int idStock, int quantiteAchetee, int seuilPromo);

        // Vérifier péremption d’un lot
        Task<bool> IsLotExpiredAsync(string numeroLot);

        // Historique des mouvements (niveau entreprise)
        Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int idStock);

        // Réservation de stock (avant commande)
        Task<bool> ReserveStockAsync(int idStock, int quantite);
    }
}
