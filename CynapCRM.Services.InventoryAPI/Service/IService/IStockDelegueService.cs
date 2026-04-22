using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;

namespace CynapCRM.Services.InventoryAPI.Service.IService
{
    public interface IStockDelegueService
    {

        Task<IEnumerable<StockDelegueDto>> GetAllStocksAsync(int pageNumber, int pageSize);
        Task<StockDelegueDto?> CreateUpdateStockAsync(StockDelegueDto stockDto);
        Task<StockDelegueDto?> GetStockByIdAsync(int idStock);
        Task<IEnumerable<StockDelegueDto>> GetStocksByDelegueAsync(int idDelegue);
        Task<IEnumerable<StockDelegueDto>> GetStockByProduitAsync(int idProduit);
        Task<StockDelegueDto?> GetStockByLotAsync(string numeroLot);

        Task<bool> DeleteStockAsync(int idStock, StockType type);
    }
}
