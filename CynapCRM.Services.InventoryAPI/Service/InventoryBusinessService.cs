using AutoMapper;
using CynapCRM.Services.InventoryAPI.Data;
using CynapCRM.Services.InventoryAPI.Models;
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

            if (quantiteAchetee < seuilPromo)
                return false;

            return await _stockMovementService.DecrementStockAsync(idStock, 1);

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
