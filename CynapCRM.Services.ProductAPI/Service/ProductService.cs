using AutoMapper;
using CynapCRM.Services.ProductAPI.Data;
using CynapCRM.Services.ProductAPI.Models;
using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.ProductAPI.Service
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public ProductService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // ═══════════════════════════════════════
        // Catalogue & consultation
        // ═══════════════════════════════════════

        public async Task<IEnumerable<ProduitDto>> GetAllProductsAsync()
        {
            var products = await _db.Produits
                .Where(p => !p.IsArchived)
                .Include(p => p.Lots)
                    .ThenInclude(l => l.Promotions)
                .Include(p => p.Supports)
                    .ThenInclude(s => s.Fichiers)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        public async Task<ProduitDto?> GetProductByIdAsync(int produitId)
        {
            var product = await _db.Produits
                .Include(p => p.Lots)
                    .ThenInclude(l => l.Promotions)
                .Include(p => p.Supports)
                    .ThenInclude(s => s.Fichiers)
                .FirstOrDefaultAsync(p =>
                    p.Id_Produit == produitId &&
                    !p.IsArchived);

            return product == null ? null : _mapper.Map<ProduitDto>(product);
        }

        public async Task<IEnumerable<ProduitDto>> GetVisibleProductsAsync()
        {
            // FIX: ajout Lots + Promotions manquants
            var products = await _db.Produits
                .Where(p => p.IsActive && !p.IsArchived)
                .Include(p => p.Lots)
                    .ThenInclude(l => l.Promotions)
                .Include(p => p.Supports)
                    .ThenInclude(s => s.Fichiers)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        // ═══════════════════════════════════════
        // Cycle de vie produit
        // ═══════════════════════════════════════

        public async Task<ProduitDto> CreateOrUpdateProductAsync(ProduitDto produitDto)
        {
            // FIX 7: price must be > 0
            if (produitDto.PrixVente <= 0)
                return null!;

            var product = await _db.Produits
                .FirstOrDefaultAsync(p => p.Id_Produit == produitDto.Id_Produit);

            if (product == null)
            {
                product = _mapper.Map<Produit>(produitDto);
                _db.Produits.Add(product);
            }
            else
            {
                if (product.IsArchived) return null!;
                _mapper.Map(produitDto, product);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<ProduitDto>(product);
        }

       

        public async Task<bool> ArchiveProductAsync(int produitId)
        {
            var product = await _db.Produits.FindAsync(produitId);
            if (product == null) return false;

            // FIX 5: block archiving if stock > 0
            var canArchive = await CanArchiveProductAsync(produitId);
            if (!canArchive) return false;

            product.IsArchived = true;
            product.IsActive = false;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnarchiveProductAsync(int produitId)
        {
            var product = await _db.Produits.FindAsync(produitId);
            if (product == null) return false;

            product.IsArchived = false;
            product.IsActive   = false; // FIX 6: stays inactive — ADMIN must activate explicitly
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateProductAsync(int produitId)
        {
            var produit = await _db.Produits.FindAsync(produitId);
            if (produit == null || produit.IsArchived) return false;

            produit.IsActive = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateProductAsync(int produitId)
        {
            var produit = await _db.Produits.FindAsync(produitId);
            if (produit == null || produit.IsArchived) return false;

            produit.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        // FIX: suppression physique sécurisée
        public async Task<bool> DeleteProductAsync(int produitId)
        {
            var product = await _db.Produits.FindAsync(produitId);
            if (product == null) return false;

            // Seulement si archivé et stock = 0
            if (!product.IsArchived) return false;
            var totalStock = await GetTotalStockAsync(produitId);
            if (totalStock > 0) return false;

            _db.Produits.Remove(product);
            await _db.SaveChangesAsync();
            return true;
        }

        // ═══════════════════════════════════════
        // Disponibilité & stock
        // ═══════════════════════════════════════

        public async Task<bool> IsProductAvailableAsync(int productId)
        {
            var product = await _db.Produits
                .Include(p => p.Lots)
                .FirstOrDefaultAsync(p => p.Id_Produit == productId);

            if (product == null || !product.IsActive || product.IsArchived)
                return false;

            return product.Lots!.Any(l =>
                l.Quantite > 0 &&
                l.DateExpiration > DateTime.UtcNow);
        }

        public async Task<IEnumerable<ProduitDto>> GetAvailableProductsAsync()
        {
            var products = await _db.Produits
                .Include(p => p.Lots)
                .Where(p =>
                    p.IsActive &&
                    !p.IsArchived &&
                    p.Lots!.Any(l =>
                        l.Quantite > 0 &&
                        l.DateExpiration > DateTime.UtcNow))
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        public async Task<IEnumerable<ProduitDto>> GetUnavailableProductsAsync()
        {
            var products = await _db.Produits
                .Include(p => p.Lots)
                .Where(p =>
                    !p.IsActive ||
                    p.IsArchived ||
                    !p.Lots!.Any(l =>
                        l.Quantite > 0 &&
                        l.DateExpiration > DateTime.UtcNow))
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        public async Task<int> GetTotalStockAsync(int productId)
        {
            return await _db.Lots
                .Where(l => l.Id_Produit == productId)
                .SumAsync(l => l.Quantite);
        }

        public async Task<IEnumerable<StockStatusDto>> GetStockStatusAsync()
        {
            return await _db.Produits
                .Select(p => new StockStatusDto
                {
                    ProductId = p.Id_Produit,
                    ProductName = p.Nom,
                    TotalStock = p.Lots!.Sum(l => l.Quantite)
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ProduitDto>> GetLowStockProductsAsync(int threshold)
        {
            var products = await _db.Produits
                .Include(p => p.Lots)
                .Where(p => p.Lots!.Sum(l => l.Quantite) <= threshold)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        // FIX: lots bientôt expirés
        public async Task<IEnumerable<ProduitDto>> GetProductsWithExpiringLotsAsync(
            int daysThreshold = 30)
        {
            var limit = DateTime.UtcNow.AddDays(daysThreshold);
            var products = await _db.Produits
                .Include(p => p.Lots)
                .Where(p => p.Lots!.Any(l =>
                    l.DateExpiration <= limit &&
                    l.DateExpiration > DateTime.UtcNow &&
                    l.Quantite > 0))
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        // ═══════════════════════════════════════
        // Recherche et navigation
        // ═══════════════════════════════════════

        public async Task<IEnumerable<ProduitDto>> SearchProductsAsync(
            string keyword,
            bool isActive,
            bool allowArchived,
            int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 3)
                return Enumerable.Empty<ProduitDto>();

            keyword = keyword.ToLower();

            // FIX: ajout Categorie dans la recherche
            var produits = await _db.Produits
                .AsNoTracking()
                .Where(p =>
                    (p.Nom.ToLower().Contains(keyword) ||
                     p.Description.ToLower().Contains(keyword) ||
                     p.Categorie.ToLower().Contains(keyword) ||
                     p.PrixVente.ToString().ToLower().Contains(keyword)) &&
                    p.IsArchived == allowArchived &&
                    p.IsActive == isActive)
                .OrderBy(p => p.Nom)
                .Take(limit)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(produits);
        }

        public async Task<IEnumerable<ProduitDto>> FilterProductsAsync(
            string? keyword,
            bool? isActive,
            bool? allowArchived,
            string? category,
            int page,
            int pageSize)
        {
            var query = _db.Produits.AsQueryable();

            // FIX: filtre archivé/actif
            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            if (allowArchived.HasValue)
                query = query.Where(p => p.IsArchived == allowArchived.Value);

            // FIX: filtre keyword
            if (!string.IsNullOrEmpty(keyword))
            {
                var kw = keyword.ToLower();
                query = query.Where(p =>
                    p.Nom.ToLower().Contains(kw) ||
                    p.Description.ToLower().Contains(kw) ||
                    p.Categorie.ToLower().Contains(kw));
            }

            // FIX: filtre category maintenant appliqué
            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Categorie == category);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        // ═══════════════════════════════════════
        // Catégories
        // ═══════════════════════════════════════

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            // FIX: null-forgiving operator
            return await _db.Produits
                .Where(p => !p.IsArchived &&
                            !string.IsNullOrEmpty(p.Categorie))
                .Select(p => p.Categorie!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProduitDto>> GetProductsByCategoryAsync(
            string category)
        {
            var products = await _db.Produits
                .Where(p => p.Categorie == category && !p.IsArchived)
                .Include(p => p.Lots)
                    .ThenInclude(l => l.Promotions)
                .Include(p => p.Supports)
                    .ThenInclude(s => s.Fichiers)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        // ═══════════════════════════════════════
        // Promotions
        // ═══════════════════════════════════════

        // FIX: nouvelle méthode produits avec promos actives
        public async Task<IEnumerable<ProduitDto>> GetProductsWithActivePromotionsAsync()
        {
            var now = DateTime.UtcNow;
            var products = await _db.Produits
                .Include(p => p.Lots)
                    .ThenInclude(l => l.Promotions)
                .Where(p =>
                    !p.IsArchived &&
                    p.IsActive &&
                    p.Lots!.Any(l =>
                        l.Promotions.Any(pr =>
                            pr.EstActive &&
                            pr.DateDebut <= now &&
                            pr.DateExpiration >= now)))
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        // ═══════════════════════════════════════
        // Validation métier
        // ═══════════════════════════════════════

        public async Task<bool> ProductExistsAsync(string productName)
        {
            return await _db.Produits
                .AnyAsync(p => p.Nom == productName);
        }

        public async Task<bool> IsProductValidAsync(int productId)
        {
            return await _db.Produits.AnyAsync(p =>
                p.Id_Produit == productId &&
                p.IsActive &&
                !p.IsArchived);
        }

        public async Task<bool> CanArchiveProductAsync(int productId)
        {
            var totalStock = await GetTotalStockAsync(productId);
            return totalStock == 0;
        }

        // ═══════════════════════════════════════
        // KPI & pilotage
        // ═══════════════════════════════════════

        public async Task<IEnumerable<ProduitDto>> GetTopProductsAsync(int topN)
        {
            var products = await _db.Produits
                .Include(p => p.Lots)
                .OrderByDescending(p => p.Lots!.Sum(l => l.Quantite))
                .Take(topN)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        public async Task<ProductDashboardDto> GetProductDashboardAsync()
        {
            return new ProductDashboardDto
            {
                TotalProducts = await _db.Produits.CountAsync(),
                ActiveProducts = await _db.Produits
                    .CountAsync(p => p.IsActive && !p.IsArchived),
                ArchivedProducts = await _db.Produits
                    .CountAsync(p => p.IsArchived)
            };
        }
    }
}