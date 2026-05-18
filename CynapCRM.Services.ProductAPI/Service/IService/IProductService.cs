using CynapCRM.Services.ProductAPI.Models.Dto;

namespace CynapCRM.Services.ProductAPI.Service.IService
{
    /// <summary>
    /// Service métier responsable de la gestion du catalogue produit
    /// </summary>
    public interface IProductService
    {
        Task<IEnumerable<ProduitDto>> GetProductsWithExpiringLotsAsync(
            int daysThreshold = 30);
        Task<IEnumerable<ProduitDto>> GetProductsWithActivePromotionsAsync();
        //  Catalogue  consultation

        Task<IEnumerable<ProduitDto>> GetAllProductsAsync();
        Task<ProduitDto?> GetProductByIdAsync(int productId);

        /// <summary>
        /// Récupère les produits visibles pour les utilisateurs métier
        /// (actifs + non archivés)
        /// </summary>
        Task<IEnumerable<ProduitDto>> GetVisibleProductsAsync();

        //  Gestion du cycle de vie produit

        /// <summary>
        /// Crée ou met à jour un produit
        /// </summary>
        Task<ProduitDto> CreateOrUpdateProductAsync(ProduitDto produitDto);

        /// <summary>
        /// Archive un produit (suppression logique)
        /// </summary>
        Task<bool> ArchiveProductAsync(int productId);

        /// <summary>
        /// Désarchive un produit (inverse de ArchiveProductAsync)
        /// </summary>
        Task<bool> UnarchiveProductAsync(int productId);

        /// <summary>
        /// Active un produit (visible et vendable)
        /// </summary>
        Task<bool> ActivateProductAsync(int productId);

        /// <summary>
        /// Désactive un produit (non vendable)
        /// </summary>
        Task<bool> DeactivateProductAsync(int productId);

        /// <summary>
        /// Suppression physique — uniquement si le produit est archivé et son stock est à zéro.
        /// </summary>
        Task<bool> DeleteProductAsync(int productId);

        //🔹 Disponibilité / état produit

        /// <summary>
        /// Indique si un produit est vendable
        /// (actif + stock disponible)
        /// </summary>
        Task<bool> IsProductAvailableAsync(int productId);

        Task<IEnumerable<ProduitDto>> GetAvailableProductsAsync();
        Task<IEnumerable<ProduitDto>> GetUnavailableProductsAsync();

        //  Stock (lecture uniquement)

        /// <summary>
        /// Stock total calculé à partir des lots
        /// </summary>
        Task<int> GetTotalStockAsync(int productId);

        /// <summary>
        /// État global du stock (OK / faible / rupture)
        /// </summary>
        Task<IEnumerable<StockStatusDto>> GetStockStatusAsync();

        Task<IEnumerable<ProduitDto>> GetLowStockProductsAsync(int threshold);

        //  Recherche et navigation

        Task<IEnumerable<ProduitDto>> SearchProductsAsync(string keyword, bool isActive, bool allowArchived, int limit = 10);

        Task<IEnumerable<ProduitDto>> FilterProductsAsync(
            string? keyword,
            bool? isActive,
            bool? allowArchived,
            string? category,
            int page,
            int pageSize
        );

        //Task<IEnumerable<string>> GetSearchSuggestionsAsync(string keyword);

        //  Catégories (référentiel léger)

        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<ProduitDto>> GetProductsByCategoryAsync(string category);

        //  Règles métier  validation

        Task<bool> ProductExistsAsync(string productName);
        Task<bool> IsProductValidAsync(int productId);

        /// <summary>
        /// Vérifie si un produit peut être archivé ou supprimé
        /// (pas de stock, pas de commandes actives)
        /// </summary>
        Task<bool> CanArchiveProductAsync(int productId);

        //  Indicateurs // pilotage

        Task<IEnumerable<ProduitDto>> GetTopProductsAsync(int topN);
        Task<ProductDashboardDto> GetProductDashboardAsync();
    }
}