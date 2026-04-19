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

        public InventoryBusinessService(AppDbContext db, IMapper mapper, IStockMovementService stockMovementService, IDistributionService distributionService)
        {
            _db = db;
            _mapper = mapper;
            _stockMovementService = stockMovementService;
            _distributionService = distributionService;
        }
        public async Task<bool> CheckStockAvailabilityAsync(int idStock, int quantite)
        {
            if (quantite <= 0)
            {
                return false;
            }
            var stock = await _db.StocksDelegues
                .AsNoTracking()
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

            // 1️⃣ Vérifier disponibilité
            var disponible = await CheckStockAvailabilityAsync(idStock, qte);
            if (!disponible)
                return false;

            // 2️⃣ Décrémenter le stock (technique)
            var decrementOk = await _stockMovementService
                .DecrementStockAsync(idStock, qte);

            if (!decrementOk)
                return false;

            // 3️⃣ Tracer la distribution
            var distributionOk = await _distributionService
                .CreateOrUpdateEchantillonAsync(new Models.Dto.EchantillonDto
                {
                    Id_Delegue = idDelegue,
                    Id_Pharmacien = idPharmacien,
                    Id_Medecin = idMedecin,
                    Id_Stock = idStock,
                    Qte = qte
                });

            return distributionOk != null;


        }

        public async Task<bool> ApplyGratuiteAsync(int idStock, int quantiteAchetee, int seuilPromo)
        {

            if (quantiteAchetee < seuilPromo)
                return false;

            // règle simple : 1 unité offerte
            return await _stockMovementService
                .DecrementStockAsync(idStock, 1);

        }


        public async Task<bool> ReserveStockAsync(int idStock, int quantite)
        {

            var disponible = await CheckStockAvailabilityAsync(idStock, quantite);
            if (!disponible)
                return false;

            return await _stockMovementService
                .DecrementStockAsync(idStock, quantite);

        }
    }
}
