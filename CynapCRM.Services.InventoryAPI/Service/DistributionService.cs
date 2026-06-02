using AutoMapper;
using CynapCRM.Services.InventoryAPI.Data;
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Service
{
    public class DistributionService : IDistributionService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public DistributionService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

        }

        public async Task<bool> CreateOrUpdateEchantillonAsync(Echantillon echantillon)
        {
            var distribution = await _db.Echantillons
                .FirstOrDefaultAsync(e => e.Id_Distribution == echantillon.Id_Distribution);

            if (distribution == null)
            {
                // FIX 4: at least one recipient required
                if (echantillon.Id_Medecin == null && echantillon.Id_Pharmacien == null)
                    return false;

                // STEP 1 — log inputs
                Console.WriteLine($"[DIST] Id_Stock={echantillon.Id_Stock} Qte={echantillon.Qte}");

                // STEP 2 — regular query (applies TPH discriminator filter)
                var stock = await _db.StocksDelegues
                    .FirstOrDefaultAsync(s =>
                        s.Id_stock == echantillon.Id_Stock &&
                        !s.IsDeleted);

                Console.WriteLine(
                    $"[DIST] Stock found: {stock != null}" +
                    $" QteDisponible={stock?.QteDisponible}" +
                    $" IsDeleted={stock?.IsDeleted}");

                // STEP 3 — same query without TPH discriminator filter (detects TPH mismatch)
                var stockRaw = await _db.StocksDelegues
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.Id_stock == echantillon.Id_Stock);

                Console.WriteLine(
                    $"[DIST] IgnoreFilters: {stockRaw != null}" +
                    $" QteDisponible={stockRaw?.QteDisponible}");

                if (stock == null)
                {
                    Console.WriteLine("[DIST] BLOCKED: stock null");
                    return false;
                }

                if (stock.QteDisponible < echantillon.Qte)
                {
                    Console.WriteLine(
                        $"[DIST] BLOCKED: {stock.QteDisponible} < {echantillon.Qte}");
                    return false;
                }

                Console.WriteLine("[DIST] PASSED all validations");

                // lot expiration check
                if (stock.DateExpiration != default(DateTime) &&
                    stock.DateExpiration.Date < DateTime.UtcNow.Date)
                    return false;

                echantillon.DateDistribution = DateTime.UtcNow;
                echantillon.IsDeleted = false;
                _db.Echantillons.Add(echantillon);

                // FIX 1: decrement available stock
                stock.QteDisponible -= echantillon.Qte;

                // FIX 5: record distribution movement
                _db.StockMovements.Add(new StockMovement
                {
                    Id_Stock     = stock.Id_stock,
                    Quantite     = -echantillon.Qte,
                    TypeMovement = "Distribution",
                    DateMovement = DateTime.UtcNow,
                    Description  = $"Distribution vers médecin#{echantillon.Id_Medecin}/pharmacien#{echantillon.Id_Pharmacien}"
                });
            }
            else
            {
                // FIX 4: apply qty delta to stock when updating
                var stock = await _db.StocksDelegues
                    .FirstOrDefaultAsync(s => s.Id_stock == distribution.Id_Stock && !s.IsDeleted);

                if (stock != null)
                {
                    int delta = echantillon.Qte - distribution.Qte;
                    if (delta > 0 && stock.QteDisponible < delta)
                        return false;
                    stock.QteDisponible -= delta;
                }

                _mapper.Map(echantillon, distribution);
            }

            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<EchantillonDto?> GetEchantillonByIdAsync(int idDistribution)
        {

            var distribution = await _db.Echantillons.AsNoTracking()
                                .FirstOrDefaultAsync(e =>
                                e.Id_Distribution == idDistribution && !e.IsDeleted);
            if (distribution == null)
            {
                return null;
            }
            return _mapper.Map<EchantillonDto>(distribution);
        }
        public async Task<IEnumerable<EchantillonDto>> GetDistributionsByMedecinAsync(int idMedecin)
        {

            var distributions = await _db.Echantillons
                            .AsNoTracking()
                            .Where(e =>
                                e.Id_Medecin == idMedecin &&
                                !e.IsDeleted)
                            .OrderByDescending(e => e.DateDistribution)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<EchantillonDto>>(distributions);
        }
        public async Task<IEnumerable<EchantillonDto>> GetDistributionsByPharmacienAsync(int idPharmacien)
        {

            var distributions = await _db.Echantillons
                            .AsNoTracking()
                            .Where(e =>
                                e.Id_Pharmacien == idPharmacien &&
                                !e.IsDeleted)
                            .OrderByDescending(e => e.DateDistribution)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<EchantillonDto>>(distributions);
        }
        public async Task<bool> DeleteEchantillonAsync(int idDistribution)
        {
            var distribution = await _db.Echantillons
                .FirstOrDefaultAsync(e => e.Id_Distribution == idDistribution);
            if (distribution == null || distribution.IsDeleted) return false;

            // FIX 5: reincrement stock when distribution is deleted
            var stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_stock == distribution.Id_Stock && !s.IsDeleted);
            if (stock != null)
            {
                stock.QteDisponible += distribution.Qte;

                _db.StockMovements.Add(new StockMovement
                {
                    Id_Stock     = stock.Id_stock,
                    Quantite     = distribution.Qte,
                    TypeMovement = "Distribution-Annulée",
                    DateMovement = DateTime.UtcNow,
                    Description  = $"Annulation distribution #{idDistribution}"
                });
            }

            distribution.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<EchantillonDto>> GetAllDistributionsAsync(
    int pageNumber, int pageSize)
        {
            var distributions = await _db.Echantillons
                .AsNoTracking()
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.DateDistribution)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<EchantillonDto>>(distributions);
        }
        public async Task<IEnumerable<EchantillonDto>> GetDistributionsByDelegueAsync(int idDelegue)
        {
            var distributions = await _db.Echantillons
                            .AsNoTracking()
                            .Where(e =>
                                        e.Id_Delegue == idDelegue && 
                                        !e.IsDeleted)
                            .OrderByDescending(e => e.DateDistribution)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<EchantillonDto>>(distributions);
        }
    }
}
