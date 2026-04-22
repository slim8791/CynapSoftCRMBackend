using AutoMapper;
using CynapCRM.Services.InventoryAPI.Data;
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Service
{
    public class StockMovementService : IStockMovementService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public StockMovementService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<bool> DecrementStockAsync(int idStock, int qte)
        {
            if (qte <= 0)
            {
                return false;
            }

            var stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
            if (stock == null || stock.QteDisponible < qte)
            {
                return false;
            }
            stock.QteDisponible -= qte;
            _db.StockMovements.Add(new StockMovement
            {
                Id_Stock = idStock,
                Quantite = -qte,
                DateMovement = DateTime.UtcNow,
                TypeMovement = "Decrement"
            });

            await _db.SaveChangesAsync();
            return true;

        }
        public async Task<bool> IncrementStockAsync(int idStock, int qte)
        {
            if (qte <= 0)
            {
                return false;
            }

            var stock = await _db.StocksDelegues
                            .FirstOrDefaultAsync(s => s.Id_stock == idStock && !s.IsDeleted);
            if (stock == null)
            {
                return false;
            }
            stock.QteDisponible += qte;
            _db.StockMovements.Add(new StockMovement
            {
                Id_Stock = idStock,
                Quantite = qte,
                DateMovement = DateTime.UtcNow,
                TypeMovement = "Increment"
            });
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> TransferStockAsync(int idStockSource, int idStockDestination, int qte)
        {
            if (qte <= 0) 
            {
                return false;
            }

            var source = await _db.StocksDelegues
                            .FirstOrDefaultAsync(s => s.Id_stock == idStockSource && !s.IsDeleted);

            var destination = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_stock == idStockDestination && !s.IsDeleted);

            if (source == null || destination == null || source.QteDisponible < qte)
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
                    Id_Stock = idStockSource,
                    TypeMovement = "Transfer-Out",
                    Quantite = qte,
                    DateMovement = DateTime.UtcNow,
                    Description = $"Transfert vers stock {idStockDestination}"
                });

                _db.StockMovements.Add(new StockMovement
                {
                    Id_Stock = idStockDestination,
                    TypeMovement = "Transfer-In",
                    Quantite = qte,
                    DateMovement = DateTime.UtcNow,
                    Description = $"Transfert depuis stock {idStockSource}"
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
        public async Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int idStock)
        {

            return await _db.StockMovements
                            .AsNoTracking()
                            .Where(m => m.Id_Stock == idStock)
                            .OrderByDescending(m => m.DateMovement)
                            .Select(m => new StockMovementDto
                            {
                                Id_Movement = m.Id_Movement,
                                Id_Stock = m.Id_Stock,
                                Quantite = m.Quantite,
                                TypeMovement = m.TypeMovement,
                                DateMovement = m.DateMovement,
                                Description = m.Description
                            })
                            .ToListAsync();
        }
    }
}
