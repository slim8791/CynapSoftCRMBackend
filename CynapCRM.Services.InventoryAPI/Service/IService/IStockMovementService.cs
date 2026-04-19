using CynapCRM.Services.InventoryAPI.Models.Dto;

namespace CynapCRM.Services.InventoryAPI.Service.IService
{
    public interface IStockMovementService
    {
        //  MOUVEMENTS DE STOCK
        Task<bool> DecrementStockAsync(int idStock, int qte);
        Task<bool> IncrementStockAsync(int idStock, int qte);
        // Historique des mouvements 
        Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int idStock);
        Task<bool> TransferStockAsync(int idStockSource, int idStockDestination, int qte);
    }
}
