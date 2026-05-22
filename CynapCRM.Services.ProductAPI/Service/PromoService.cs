using AutoMapper;
using CynapCRM.Services.ProductAPI.Data;
using CynapCRM.Services.ProductAPI.Models;
using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.ProductAPI.Service
{
    public class PromoService : IPromoService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public PromoService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // ─── Gestion des promotions ───────────────────────────────────────

        public async Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync()
        {
            var promotions = await _db.Promotions
                .Include(p => p.Lot)
                .Where(p => p.Lot != null
                         && p.NumeroLot != null
                         && p.DateDebut != null)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        }

        public async Task<PromotionDto?> GetPromotionByIdAsync(int promotionId)
        {
            var promo = await _db.Promotions
                .Include(p => p.Lot)
                .FirstOrDefaultAsync(p => p.Id_Promo == promotionId);

            return promo == null ? null : _mapper.Map<PromotionDto>(promo);
        }

        public async Task<PromotionDto?> CreateOrUpdatePromotionAsync(PromotionDto promotionDto)
        {
            // Vérifier que le lot existe avant de créer/modifier
            var lotExists = await _db.Lots
                .AnyAsync(l => l.NumeroLot == promotionDto.NumeroLot);

            if (!lotExists) return null;

            // FIX 8: block if this lot already has an active promotion (create path only)
            if (promotionDto.Id_Promo == 0)
            {
                var hasActivePromo = await _db.Promotions
                    .AnyAsync(p =>
                        p.NumeroLot == promotionDto.NumeroLot &&
                        p.EstActive);
                if (hasActivePromo) return null;
            }

            var promo = await _db.Promotions
                .FirstOrDefaultAsync(p => p.Id_Promo == promotionDto.Id_Promo);

            if (promo == null)
            {
                promo = _mapper.Map<Promotion>(promotionDto);
                _db.Promotions.Add(promo);
            }
            else
            {
                _mapper.Map(promotionDto, promo);
            }

            await _db.SaveChangesAsync();
            return promotionDto;
        }

        public async Task<bool> DeletePromotionAsync(int promotionId)
        {
            var promo = await _db.Promotions.FindAsync(promotionId);
            if (promo == null) return false;

            _db.Promotions.Remove(promo);
            await _db.SaveChangesAsync();
            return true;
        }

        // ─── Application des promotions ───────────────────────────────────

        public async Task<decimal> ApplyBestPromotionAsync(int productId, decimal initialPrice)
        {
            var today = DateTime.UtcNow;

            var promotions = await _db.Promotions
                .Include(p => p.Lot)
                .Where(p =>
                    p.EstActive &&
                    p.DateDebut != null &&
                    p.DateDebut <= today &&
                    p.DateExpiration >= today &&
                    p.Lot != null &&
                    p.Lot.Id_Produit == productId)
                .ToListAsync();

            if (!promotions.Any())
                return initialPrice;

            var bestPromo = promotions
                .Where(p => p.TypePromotion == TypePromotion.Pourcentage && p.Pourcentage.HasValue)
                .OrderByDescending(p => p.Pourcentage)
                .FirstOrDefault();

            if (bestPromo == null) return initialPrice;

            var discount = initialPrice * (decimal)(bestPromo.Pourcentage!.Value / 100);
            return initialPrice - discount;
        }

        public async Task<bool> IsProductInPromotionAsync(int productId)
        {
            var today = DateTime.UtcNow;

            return await _db.Promotions
                .Include(p => p.Lot)
                .AnyAsync(p =>
                    p.EstActive &&
                    p.DateDebut != null &&
                    p.DateDebut <= today &&
                    p.DateExpiration >= today &&
                    p.Lot != null &&
                    p.Lot.Id_Produit == productId);
        }

        // ─── Consultation par contexte ────────────────────────────────────

        public async Task<IEnumerable<PromotionDto>> GetPromotionsByProductAsync(int productId)
        {
            var promotions = await _db.Promotions
                .Include(p => p.Lot)
                .Where(p => p.Lot != null && p.Lot.Id_Produit == productId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        }

        public async Task<IEnumerable<PromotionDto>> GetPromotionsByLotAsync(string numeroLot)
        {
            var promotions = await _db.Promotions
                .Include(p => p.Lot)
                .Where(p => p.NumeroLot == numeroLot)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PromotionDto>>(promotions);
        }

        // ─── Validation métier ────────────────────────────────────────────

        public async Task<bool> IsPromotionValidAsync(int promotionId)
        {
            var today = DateTime.UtcNow;

            return await _db.Promotions.AnyAsync(p =>
                p.Id_Promo == promotionId &&
                p.EstActive &&
                p.DateDebut != null &&
                p.DateDebut <= today &&
                p.DateExpiration >= today);
        }

        public async Task<bool> IsPromotionApplicableAsync(int promotionId, DateTime referenceDate)
        {
            return await _db.Promotions.AnyAsync(p =>
                p.Id_Promo == promotionId &&
                p.EstActive &&
                p.DateDebut != null &&
                p.DateDebut <= referenceDate &&
                p.DateExpiration >= referenceDate);
        }

        // ─── Indicateurs / pilotage ───────────────────────────────────────

        public async Task<double> GetPromotionCoverageRateAsync()
        {
            var totalProducts = await _db.Produits.CountAsync();
            if (totalProducts == 0) return 0;

            var today = DateTime.UtcNow;

            var productsWithPromo = await _db.Promotions
                .Include(p => p.Lot)
                .Where(p =>
                    p.EstActive &&
                    p.DateDebut != null &&
                    p.DateDebut <= today &&
                    p.DateExpiration >= today &&
                    p.Lot != null)
                .Select(p => p.Lot!.Id_Produit)
                .Distinct()
                .CountAsync();

            return (double)productsWithPromo / totalProducts * 100;
        }

        public async Task<int> GetActivePromotionsCountAsync()
        {
            var today = DateTime.UtcNow;

            return await _db.Promotions.CountAsync(p =>
                p.EstActive &&
                p.DateDebut != null &&
                p.DateDebut <= today &&
                p.DateExpiration >= today);
        }
    }
}