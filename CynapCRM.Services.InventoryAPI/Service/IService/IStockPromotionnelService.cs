using CynapCRM.Services.InventoryAPI.Models.Dto;

namespace CynapCRM.Services.InventoryAPI.Service.IService
{
    public interface IStockPromotionnelService
    {
        // STOCK GRATUITÉ
        Task<StockGratuiteDto?> CreateUpdateStockGratuiteAsync(StockGratuiteDto stockDto);
        Task<StockGratuiteDto?> GetStockGratuiteByIdAsync(int idStock);
        Task<IEnumerable<StockGratuiteDto>> GetAllGratuiteAsync(int page, int size);

        //  STOCK ÉCHANTILLON
        Task<StockEchantillonDto?> CreateUpdateStockEchantillonAsync(StockEchantillonDto stockDto);
        Task<StockEchantillonDto?> GetStockEchantillonByIdAsync(int idStock);
        Task<IEnumerable<StockEchantillonDto>> GetAllEchantillonAsync(int page, int size);
    }
}
