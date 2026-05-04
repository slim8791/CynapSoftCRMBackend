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

        // 🔹 Catalogue & consultation
        public async Task<IEnumerable<ProduitDto>> GetAllProductsAsync()
        {

            var products = await _db.Produits
                    .Include(p => p.Lots)
                        .ThenInclude(l => l.Promotions)
                    .Include(p => p.Supports)
                        .ThenInclude(s => s.Fichiers)
                    .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);

        }
        public async Task<ProduitDto> GetProductByIdAsync(int produitId)
        {

            var product = await _db.Produits
                    .Include(p => p.Lots)
                        .ThenInclude(l => l.Promotions)
                    .Include(p => p.Supports)
                        .ThenInclude(s => s.Fichiers)
                    .FirstOrDefaultAsync(p => p.Id_Produit == produitId);

            return product == null ? null : _mapper.Map<ProduitDto>(product);

        }
        public async Task<IEnumerable<ProduitDto>> GetVisibleProductsAsync()
        {
            var products = await _db.Produits
                .Where(p => p.IsActive && !p.IsArchived)
                .Include(p => p.Supports)
                    .ThenInclude(s => s.Fichiers)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        // 🔹 Cycle de vie produit

        public async Task<ProduitDto> CreateOrUpdateProductAsync(ProduitDto produitDto)
        {

            var product = await _db.Produits
                            .FirstOrDefaultAsync(p => p.Id_Produit == produitDto.Id_Produit);

            if (product == null)
            {
                product = _mapper.Map<Produit>(produitDto);
                _db.Produits.Add(product);
            }
            else
            {
                _mapper.Map(produitDto, product);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<ProduitDto>(product);

        }
        public async Task<bool> ArchiveProductAsync(int produitId)
        {

            var product = await _db.Produits.FindAsync(produitId);
            if (product == null) return false;

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

            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ActivateProductAsync(int produitId)
        {
            var produit = await _db.Produits.FindAsync(produitId);
            if (produit == null || produit.IsArchived) return false; // 🔥 règle métier
            produit.IsActive = true;
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeactivateProductAsync(int produitId)
        {
            var produit = await _db.Produits.FindAsync(produitId);
            if (produit == null || produit.IsArchived) return false; // 🔥 règle métier
            produit.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        //  Disponibilité // stock (lecture)

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
                    p.Lots!.Any(l => l.Quantite > 0 && l.DateExpiration > DateTime.UtcNow))
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
                    !p.Lots!.Any(l => l.Quantite > 0 && l.DateExpiration > DateTime.UtcNow))
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

        //  Recherche et navigation

        public async Task<IEnumerable<ProduitDto>> SearchProductsAsync(string keyword, int limit = 10)
        {

            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 3)
                return Enumerable.Empty<ProduitDto>();
            
            keyword = keyword.ToLower();

            var produits = await _db.Produits
                .AsNoTracking()
                .Where(p => p.Nom.ToLower().Contains(keyword) && !p.IsArchived) // 🔥 exclure archivés
                .OrderBy(p => p.Nom)
                .Take(limit) 
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(produits);
        }

        public async Task<IEnumerable<ProduitDto>> FilterProductsAsync(
                    string? keyword,
                    string? category,
                    bool? onlyAvailable,
                    int page,
                    int pageSize)
        {
            var query = _db.Produits.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p => p.Nom.Contains(keyword));

            if (onlyAvailable == true)
                query = query.Where(p => p.IsActive && !p.IsArchived);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        

        //  Catégories

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _db.Produits
                .Select(p => p.Description)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<ProduitDto>> GetProductsByCategoryAsync(string category)
        {
            var products = await _db.Produits
                .Where(p => p.Description == category && !p.IsArchived)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProduitDto>>(products);
        }

        //  Validation métier

        public async Task<bool> ProductExistsAsync(string productName)
        {
            return await _db.Produits.AnyAsync(p => p.Nom == productName);
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

        //  KPI /pilotage

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
                ActiveProducts = await _db.Produits.CountAsync(p => p.IsActive),
                ArchivedProducts = await _db.Produits.CountAsync(p => p.IsArchived)
            };
        }
    }
}
