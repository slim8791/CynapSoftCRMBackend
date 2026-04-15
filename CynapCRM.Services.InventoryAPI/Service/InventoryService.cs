using AutoMapper;
using CynapCRM.Services.InventoryAPI.Data;
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Service
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public InventoryService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

        }
        public async Task<IEnumerable<StockDelegueDto>> GetAllStocksAsync(int pageNumber, int pageSize)
        {
            var stocks = await _db.StocksDelegues
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StockDelegueDto>>(stocks);
        }

        public async Task<StockDelegueDto?> CreateUpdateStockAsync(StockDelegueDto stockDto)
        {

            if (stockDto.Id_stock > 0)
            {
                var existingEntity = await _db.StocksDelegues.FindAsync(stockDto.Id_stock);
                if (existingEntity == null) return null;
                _mapper.Map(stockDto, existingEntity);

                // updated 
                _db.StocksDelegues.Update(existingEntity);
                await _db.SaveChangesAsync();
                return _mapper.Map<StockDelegueDto>(existingEntity);

            }
            else
            {
                var newEntity = _mapper.Map<Stock_Delegue>(stockDto);
                //added
                _db.StocksDelegues.Add(newEntity);
                await _db.SaveChangesAsync();
                return _mapper.Map<StockDelegueDto>(newEntity);

            }
                
          
                
        }
        public async Task<StockDelegueDto?> GetStockByIdAsync(int idStock)
        {
            var entity = await _db.StocksDelegues.FindAsync(idStock);
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<StockDelegueDto>(entity);
        }
        public async Task<IEnumerable<StockDelegueDto>> GetStocksByDelegueAsync(int idDelegue)
        {
            var stocks = await _db.StocksDelegues
                .Where(s => s.Id_User_Delegue == idDelegue)
                .ToListAsync();
            return _mapper.Map<IEnumerable<StockDelegueDto>>(stocks);
        }
        public async Task<IEnumerable<StockDelegueDto>> GetStockByProduitAsync(int idProduit)
        {
            var stocks = await _db.StocksDelegues
                .Where(s => s.Id_Produit == idProduit)
                .ToListAsync();
            return _mapper.Map<IEnumerable<StockDelegueDto>>(stocks);
        }
        public async Task<StockDelegueDto?> GetStockByLotAsync(string numeroLot)
        {
            var entity = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.NumeroLot == numeroLot);
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<StockDelegueDto>(entity);
        }
        public async Task<bool> DeleteStockAsync(int idStock, string type)
        {
            var stock = await _db.StocksDelegues.FindAsync(idStock);
            if (stock == null)
            {
                return false;   
            }
            switch (type.ToUpper())
            {
                case "DELEGUE":
                    if (stock is Stock_Delegue)
                        _db.StocksDelegues.Remove(stock);
                    break;

                case "GRATUITE":
                    if (stock is Stock_Gratuite)
                        _db.StocksDelegues.Remove(stock);
                    break;

                case "ECHANTILLON":
                    if (stock is Stock_Echantillon)
                        _db.StocksDelegues.Remove(stock);
                    break;

                default:
                    return false;
            }

            await _db.SaveChangesAsync();
            return true;
        }
        // Implémentations pour StockGratuite, StockEchantillon, Echantillon, et les méthodes de logique métier à suivre...
        public async Task<StockGratuiteDto?> CreateUpdateStockGratuiteAsync(StockGratuiteDto stockDto)
        {
            if (stockDto.Id_stock > 0)
            {
                var existing = await _db.StocksDelegues.FindAsync(stockDto.Id_stock);
                if (existing == null) return null;
                _mapper.Map(stockDto, existing);
                await _db.SaveChangesAsync();
                return _mapper.Map<StockGratuiteDto>(existing);
            }
            else
            {
                var entity = _mapper.Map<Stock_Gratuite>(stockDto);
                _db.StocksDelegues.Add(entity); // Table spécifique
                await _db.SaveChangesAsync();
                return _mapper.Map<StockGratuiteDto>(entity);
            }
        }
        public async Task<StockGratuiteDto?> GetStockGratuiteByIdAsync(int idStock)
        {
            var entity = await _db.StocksDelegues.FindAsync(idStock);
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<StockGratuiteDto>(entity);
        }
        public async Task<StockEchantillonDto?> CreateUpdateStockEchantillonAsync(StockEchantillonDto stockDto)
        {
            if (stockDto.Id_stock > 0)
            {
                var existing = await _db.StocksDelegues.FindAsync(stockDto.Id_stock);
                if (existing == null) return null;
                _mapper.Map(stockDto, existing);
                await _db.SaveChangesAsync();
                return _mapper.Map<StockEchantillonDto>(existing);
            }
            else
            {
                var entity = _mapper.Map<Stock_Gratuite>(stockDto);
                _db.StocksDelegues.Add(entity); // Table spécifique
                await _db.SaveChangesAsync();
                return _mapper.Map<StockEchantillonDto>(entity);
            }
        }
        public async Task<StockEchantillonDto?> GetStockEchantillonByIdAsync(int idStock)
        {
            var entity = await _db.StocksDelegues.FindAsync(idStock);
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<StockEchantillonDto>(entity);
        }
        public async Task<EchantillonDto?> CreateUpdateEchantillonAsync(EchantillonDto echantillonDto)
        {
            var entity = _mapper.Map<Echantillon>(echantillonDto);

            if (entity.Id_Distribution > 0)
                _db.Echantillons.Update(entity);
            else
                _db.Echantillons.Add(entity);

            await _db.SaveChangesAsync();
            return _mapper.Map<EchantillonDto>(entity);
        }
        public async Task<EchantillonDto?> GetEchantillonByIdAsync(int idDistribution)
        {
            var entity = await _db.Echantillons.FindAsync(idDistribution);
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<EchantillonDto>(entity);
        }
        public async Task<IEnumerable<EchantillonDto>> GetDistributionsByMedecinAsync(int idMedecin)
        {
            var distributions = await _db.Echantillons
                .Where(e => e.Id_Medecin == idMedecin)
                .ToListAsync();
            return _mapper.Map<IEnumerable<EchantillonDto>>(distributions);
        }
        public async Task<IEnumerable<EchantillonDto>> GetDistributionsByPharmacienAsync(int idPharmacien)
        {
            var distributions = await _db.Echantillons
                .Where(e => e.Id_Pharmacien == idPharmacien)
                .ToListAsync();
            return _mapper.Map<IEnumerable<EchantillonDto>>(distributions);
        }
        public async Task<bool> DeleteEchantillonAsync(int idDistribution)
        {
            var entity = await _db.Echantillons.FindAsync(idDistribution);
            if (entity == null)
            {
                return false;
            }
            _db.Echantillons.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DecrementStockAsync(int idStock, int qte)
        {
            var stock = await _db.StocksDelegues.FindAsync(idStock);
            if (stock == null || stock.QteDisponible < qte)
            {
                return false;
            }
            stock.QteDisponible -= qte;
            _db.StockMovements.Add(new StockMovement
            {
                IdStock = idStock,
                Quantity = -qte,
                DateMovement = DateTime.UtcNow,
                TypeMovement = "Decrement"
            });

            await _db.SaveChangesAsync();
            return true;

        }
        public async Task<bool> IncrementStockAsync(int idStock, int qte)
        {
            var stock = await _db.StocksDelegues.FindAsync(idStock);
            if (stock == null)
            {
                return false;
            }
            stock.QteDisponible += qte;
            _db.StockMovements.Add(new StockMovement
            {
                IdStock = idStock,
                Quantity = qte,
                DateMovement = DateTime.UtcNow,
                TypeMovement = "Increment"
            });
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> TransferStockAsync(int idStockSource, int idStockDestination, int qte)
        {
            var source = await _db.StocksDelegues.FindAsync(idStockSource);
            var destination = await _db.StocksDelegues.FindAsync(idStockDestination);
            if (source == null || destination == null)
            {
                return false;
            }
            if (source.QteDisponible < qte)
            {
                return false;
            }
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                source.QteDisponible -= qte;
                destination.QteDisponible += qte;

                _db.StockMovements.Add(new StockMovement
                {
                    IdStock = idStockSource,
                    TypeMovement = "Transfer-Out",
                    Quantity = qte,
                    DateMovement = DateTime.UtcNow
                });

                _db.StockMovements.Add(new StockMovement
                {
                    IdStock = idStockDestination,
                    TypeMovement = "Transfer-In",
                    Quantity = qte,
                    DateMovement = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<bool> CheckStockAvailabilityAsync(int idStock, int quantite)
        {
            var stock = await _db.StocksDelegues.FindAsync(idStock);
            if (stock == null)
            {
                return false;

            }
            if (stock.QteDisponible < quantite)
            {
                return false;
            }
            return true;


        }
        
        public async Task<bool> DistributeEchantillonAsync(int idDelegue, int idPharmacien, int idMedecin, int idStock, int qte)
        {
            if (qte <= 0)
            {
                return false;
            }
            var stock = await _db.StocksDelegues.FindAsync(idStock);
            if (stock == null)
            {
                return false;

            }
            if (stock.Id_User_Delegue != idDelegue)
            {
                return false;
            }
            if (stock.QteDisponible < qte)
            {
                return false;
            }
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                stock.QteDisponible -= qte;

                var distribution = new Echantillon
                {
                    Id_Pharmacien = idPharmacien,
                    Id_Medecin = idMedecin,
                    Id_Delegue = idDelegue,
                    Qte = qte,
                    DateDistribution = DateTime.UtcNow,
                    NumeroLot = stock.NumeroLot
                };
                await _db.Echantillons.AddAsync(distribution);
                _db.StockMovements.Add(new StockMovement
                {
                    IdStock = idStock,
                    Quantity = -qte,
                    TypeMovement = "DISTRIBUTION_ECHANTILLON",
                    DateMovement = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
            
        }

        public async Task<bool> ApplyGratuiteAsync(int idStock, int quantiteAchetee, int seuilPromo)
        {
            if (seuilPromo <= 0)
            {
                throw new ArgumentException("Le seuil doit être supérieur à zéro.");
            }
            var stock = await _db.StocksDelegues.FindAsync(idStock);
            if (stock == null)
            {
                return false;
            }
            int bonus = quantiteAchetee / seuilPromo;
            stock.QteDisponible += bonus;
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsLotExpiredAsync(string numeroLot)
        {
            var stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.NumeroLot == numeroLot);
            if (stock == null)
            {
                return false;
            }
            return stock.DateExpiration <= DateTime.UtcNow;
        }
        public async Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int idStock)
        {
            var movements = await _db.StockMovements
                .Where(m => m.IdStock == idStock)
                .OrderByDescending(m => m.DateMovement)
                .ToListAsync();
            return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
        }
        public async Task<bool> ReserveStockAsync(int idStock, int quantite)
        {
            if (quantite <= 0)
                return false;

            var stock = await _db.StocksDelegues.FindAsync(idStock);
            if (stock == null)
                return false;

            // 🔥 stock réel disponible
            int disponible = stock.QteDisponible - stock.QteReservee;

            if (disponible < quantite)
                return false;

            stock.QteReservee += quantite;

            // 🔥 Historique
            _db.StockMovements.Add(new StockMovement
            {
                IdStock = idStock,
                Quantity = quantite,
                TypeMovement = "RESERVE",
                DateMovement = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return true;
        }

        

        
    }
}
