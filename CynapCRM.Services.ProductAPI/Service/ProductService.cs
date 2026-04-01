using AutoMapper;
using CynapCRM.Services.ProductAPI.Data;
using CynapCRM.Services.ProductAPI.Models;
using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
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
        // 1. Gestion des produits
        public async Task<IEnumerable<ProduitDto>> GetProductsAsync()
        {
            var produits = await _db.Produits.Include(u => u.Lots).Include(u => u.Supports).ToListAsync();
            return _mapper.Map<IEnumerable<ProduitDto>>(produits);
        }
        public async Task<ProduitDto> GetProductByIdAsync(int produitId)
        {
            var produit = await _db.Produits
                .Include(u => u.Lots).ThenInclude(l => l.Promotions)
                .Include(u => u.Supports).ThenInclude(s => s.Fichiers)
                .FirstOrDefaultAsync(u => u.Id_Produit == produitId);
            return _mapper.Map<ProduitDto>(produit);
        }
        public async Task<ProduitDto> CreateUpdateProductAsync(ProduitDto produitDto)
        {
            Produit produit = _mapper.Map<Produit>(produitDto);
            if (produit.Id_Produit > 0)
            {
                _db.Produits.Update(produit);
            }
            else
            {
                _db.Produits.Add(produit);
            }
            await _db.SaveChangesAsync();
            return _mapper.Map<ProduitDto>(produit);
        }
        public async Task<bool> DeleteProductAsync(int produitId)
        {
            var produit = await _db.Produits.FirstOrDefaultAsync(u => u.Id_Produit == produitId);
            if (produit == null)
            {
                return false;
            }
            _db.Produits.Remove(produit);
            await _db.SaveChangesAsync();
            return true;
        }
        // 2. Gestion des lots

        public async Task<IEnumerable<LotDto>> GetLotsByProductIdAsync(int productId)
        {
            var lots = await _db.Lots.Include(l => l.Promotions)
                .Where(l =>l.Id_Produit == productId).ToListAsync();
            return _mapper.Map<IEnumerable<LotDto>>(lots);
        }

        public async Task<LotDto> CreateUpdateLotAsync(LotDto lotDto)
        {
            Lot lot = _mapper.Map<Lot>(lotDto);
            var existingLot = await _db.Lots.AsNoTracking().FirstOrDefaultAsync(l => l.Numero == lot.Numero);
            if (existingLot != null) 
            { 
                _db.Lots.Update(lot);
            }
            else 
            { 
                _db.Lots.Add(lot); 
            }
            await _db.SaveChangesAsync();
            return _mapper.Map<LotDto>(lot);

        }
        public async Task<bool> DeleteLotAsync(string numeroLot)
        {
            var lots = await _db.Lots.FirstOrDefaultAsync(l => l.Numero == numeroLot);
            if (lots == null)
            {
                return false;
            }
            _db.Lots.Remove(lots);
            await _db.SaveChangesAsync();
            return true;
        }
        // 3. Gestion des promotions

        public async Task<IEnumerable<PromotionDto>> GetPromotionsAsync()
        {
            var promos = await _db.Promotions.ToListAsync();
            return _mapper.Map<IEnumerable<PromotionDto>>(promos);
        }


        public async Task<PromotionDto> CreateUpdatePromotionAsync(PromotionDto promotionDto)
        {
            try
            {
                Promotion promo = _mapper.Map<Promotion>(promotionDto);
                if (promo.Id_Promo > 0)
                {
                    _db.Promotions.Update(promo);
                }
                else
                {
                    _db.Promotions.Add(promo);
                }

                await _db.SaveChangesAsync();
                return _mapper.Map<PromotionDto>(promo);
            }
            catch(Exception ex)
            {
                var realError = ex.InnerException?.Message ?? ex.Message;
                throw new Exception(realError);

            }
            
        }
        public async Task<bool> DeletePromotionAsync(int promotionId)
        {
            var promo = await _db.Promotions.FirstOrDefaultAsync(p => p.Id_Promo == promotionId);
            if (promo == null)
            {
                return false;
            }
            _db.Promotions.Remove(promo);
            await _db.SaveChangesAsync();
            return true;
        }


        // 4. Gestion des supports marketing

        public async Task<IEnumerable<SupportMarketingDto>> GetSupportsByProductIdAsync(int productId)
        {
            var supports = await _db.Support_Markettings.Include(s => s.Fichiers)
                .Where(s => s.Id_Produit == productId).ToListAsync();
            return _mapper.Map<IEnumerable<SupportMarketingDto>>(supports);
        }
        public async Task<SupportMarketingDto> CreateUpdateSupportAsync(SupportMarketingDto supportDto)
        {
            Support_Marketting support = _mapper.Map<Support_Marketting>(supportDto);
            if (support.Id_SupportMarketting > 0)
            {
                _db.Support_Markettings.Update(support);
            }
            else
            {
                _db.Support_Markettings.Add(support);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<SupportMarketingDto>(support);
        }
        
        public async Task<FichierDto> AddFichierToSupportAsync(FichierDto fichierDto)
        {
            Fichier fichier = _mapper.Map<Fichier>(fichierDto);
            _db.Fichiers.Add(fichier);
            await _db.SaveChangesAsync();
            return _mapper.Map<FichierDto>(fichier);
        }
        public async Task<bool> DeleteFichierAsync(int fichierId)
        {
            var fichier = await _db.Fichiers.FirstOrDefaultAsync(f => f.Id_Fichier == fichierId);
            if (fichier == null)
            {
                return false;
            }
            _db.Fichiers.Remove(fichier);
            await _db.SaveChangesAsync();
            return true;

        }
    }
}
