using AutoMapper;
using CynapCRM.Services.InventoryAPI.Data;
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Service
{
    public class InventoryBusinessService : IInventoryBusinessService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        private readonly IStockMovementService _stockMovementService;
        private readonly IDistributionService _distributionService;
        private readonly IStockDelegueService _stockDelegueService;

        public InventoryBusinessService(AppDbContext db, IMapper mapper, IStockMovementService stockMovementService, IDistributionService distributionService, IStockDelegueService stockDelegueService )
        {
            _db = db;
            _mapper = mapper;
            _stockMovementService = stockMovementService;
            _distributionService = distributionService;
            _stockDelegueService = stockDelegueService;
        }
        public async Task<bool> CheckStockAvailabilityAsync(int idStock, int quantite)
        {
            if (quantite <= 0)
            {
                return false;
            }
            var stock = await _db.StocksDelegues.AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.Id_stock == idStock &&
                    !s.IsDeleted);

            if (stock == null || stock.QteDisponible < quantite)
            {
                return false;

            }
            
            return true;


        }

        public async Task<bool> DistributeEchantillonAsync(int idDelegue, int idPharmacien, int idMedecin, int idStock, int qte)
        {
            var disponible = await CheckStockAvailabilityAsync(idStock, qte);
            if (!disponible)
                return false;

            var stock = await _stockDelegueService.GetStockByIdAsync(idStock);
            if (stock == null)
                return false;

            var decrementOk = await _stockMovementService.DecrementStockAsync(idStock, qte);

            if (!decrementOk)
                return false;

            var echantillon = new Echantillon
            {

                Id_Delegue = idDelegue,
                Id_Medecin = idMedecin,
                Id_Pharmacien = idPharmacien,
                Id_Stock = idStock,               
                Qte = qte,
                NumeroLot = stock.NumeroLot,
                DateDistribution = DateTime.UtcNow,
                IsDeleted = false

            };
            await _distributionService.CreateOrUpdateEchantillonAsync(echantillon);

            return true;

        }

        public async Task<bool> ApplyGratuiteAsync(int idStock, int quantiteAchetee, int seuilPromo)
        {
            if (quantiteAchetee < seuilPromo || seuilPromo <= 0) return false;

            var nbGratuites = quantiteAchetee / seuilPromo;
            return await _stockMovementService.DecrementStockAsync(idStock, nbGratuites);
        }
        public async Task<StockSummaryDto> GetStockSummaryByDelegueAsync(int idDelegue)
        {
            var stocks = await _db.StocksDelegues
                .AsNoTracking()
                .Where(s => s.Id_User_Delegue == idDelegue && !s.IsDeleted)
                .ToListAsync();

            var distributions = await _db.Echantillons
                .AsNoTracking()
                .Where(e => e.Id_Delegue == idDelegue && !e.IsDeleted)
                .ToListAsync();

            var dernierMouvement = await _db.StockMovements
                .AsNoTracking()
                .Where(m => stocks.Select(s => s.Id_stock).Contains(m.Id_Stock))
                .OrderByDescending(m => m.DateMovement)
                .Select(m => m.DateMovement)
                .FirstOrDefaultAsync();

            var baseStocks = stocks.Where(s => s.GetType() == typeof(Stock_Delegue)).ToList();
            var echantillons = stocks.OfType<Stock_Echantillon>().ToList();
            var gratuites = stocks.OfType<Stock_Gratuite>().ToList();

            var totalQte = baseStocks.Sum(s => s.QteDisponible)
                         + echantillons.Sum(e => e.QteEchantillon)
                         + gratuites.Sum(g => g.QteGratuite);

            var vides = baseStocks.Count(s => s.QteDisponible == 0)
                      + echantillons.Count(e => e.QteEchantillon == 0)
                      + gratuites.Count(g => g.QteGratuite == 0);

            var faibles = baseStocks.Count(s => s.QteDisponible > 0 && s.QteDisponible <= 5)
                        + echantillons.Count(e => e.QteEchantillon > 0 && e.QteEchantillon <= 5)
                        + gratuites.Count(g => g.QteGratuite > 0 && g.QteGratuite <= 5);

            return new StockSummaryDto
            {
                TotalProduits = stocks.Count,
                TotalQteDisponible = totalQte,
                StocksVides = vides,
                StocksFaibles = faibles,
                TotalDistributions = distributions.Count,
                TotalQteDistribuee = distributions.Sum(e => e.Qte),
                DernierMouvement = dernierMouvement == default ? null : dernierMouvement
            };
        }

        public async Task<bool> ReserveStockAsync(int idStock, int quantite)
        {

            var disponible = await CheckStockAvailabilityAsync(idStock, quantite);
            if (!disponible)
                return false;

            return await _stockMovementService.DecrementStockAsync(idStock, quantite);

        }
    }
}
