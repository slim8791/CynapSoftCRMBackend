using CynapCRM.Services.ProductAPI.Models.Dto;

namespace CynapCRM.Services.ProductAPI.Service.IService
{
    public interface IProductService
    {
        // 🔹 CRUD
        Task<IEnumerable<ProduitDto>> GetProductsAsync();
        Task<ProduitDto?> GetProductByIdAsync(int produitId);
        Task<ProduitDto> CreateUpdateProductAsync(ProduitDto produitDto);
        Task<bool> ArchiveProductAsync(int produitId);

        // 🔹 Recherche & filtre
        Task<IEnumerable<ProduitDto>> SearchProductsAsync(string keyword, int limit = 10);
        Task<IEnumerable<ProduitDto>> FilterProductsAsync(
            string? keyword,
            string? category,
            bool? isAvailable,
            int page = 1,
            int pageSize = 20
        );

        // 🔹 Activation
        Task<bool> ActivateProductAsync(int produitId);
        Task<bool> DeactivateProductAsync(int produitId);

        // 🔹 Disponibilité
        Task<bool> IsProductAvailableAsync(int productId);
        Task<IEnumerable<ProduitDto>> GetAvailableProductsAsync();
        Task<IEnumerable<ProduitDto>> GetOutOfStockProductsAsync();
        Task<IEnumerable<ProduitDto>> GetLowStockProductsAsync(int threshold);

        // 🔹 Catégories
        Task<bool> CategoryExistsAsync(string category);
        Task<IEnumerable<ProduitDto>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<string>> GetCategoriesAsync();

        // 🔹 Stock (lecture uniquement)
        Task<int> GetTotalStockByProductAsync(int productId);
        Task<IEnumerable<StockStatusDto>> GetStockStatusAsync();

        // 🔹 Validation métier
        Task<bool> ProductExistsAsync(string nomProduit);
        Task<bool> IsProductValidAsync(int productId);
        Task<bool> CanDeleteProductAsync(int productId);

        // 🔹 KPI
        Task<IEnumerable<ProduitDto>> GetTopProductsAsync(int topN);
        Task<ProductDashboardDto> GetProductDashboardAsync();

        // 🔹 UX
        Task<IEnumerable<string>> GetSearchSuggestionsAsync(string keyword);
    }
}
