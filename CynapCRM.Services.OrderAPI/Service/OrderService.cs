using AutoMapper;
using CynapCRM.Services.OrderAPI.Data;
using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.OrderAPI.Service
{
    
    public class OrderService : IOrderService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        public OrderService(IMapper mapper, AppDbContext db)
        {
            _mapper = mapper;
            _db = db;
        }
        // Gestion des commandes 
        public async Task<IEnumerable<CommandeDto>> GetAllOrdersAsync(int page, int pageSize)
        {
            var commandes = await _db.Commandes
                .Include(c => c.Lignes)
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return _mapper.Map<IEnumerable<CommandeDto>>(commandes);
        }
        
        //une commande par son id
        public async Task<CommandeDto> GetOrderByIdAsync(int orderId)
        {
            var order = await _db.Commandes
                .Include(c => c.Lignes)
                .Include(c => c.Reclamations)
                .FirstOrDefaultAsync(u => u.Id_Commande == orderId);
            if (order == null)
            {
                return null;
            }
            return _mapper.Map<CommandeDto>(order);
        }
        //toutes les commandes d'un client
        public async Task<IEnumerable<CommandeDto>> GetOrdersByClientIdAsync(int clientId)
        {
            var orders = await _db.Commandes
                .Where(c => c.Id_Client == clientId)
                .Include(c => c.Lignes)
                .Include(c => c.Reclamations)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<CommandeDto>>(orders);
        }
        //creation d'une commande
        public async Task<CommandeDto> CreateOrderAsync(CreateOrderDto orderDto)
        {

            var order = new Commande
            {
                Id_Client = orderDto.Id_Client, 
                DateCommande = DateTime.UtcNow,
                Statut = orderDto.IsFinalValidation
                        ? EtatCommande.EnAttente
                        : EtatCommande.Brouillon,
                Lignes = new List<LigneCommande>()
            };

            foreach (var ligneDto in orderDto.Lignes)
            {
                var ligne = new LigneCommande
                {
                    Id_Produit = ligneDto.Id_Produit,
                    Quantite = ligneDto.Quantite,
                    PrixUnitaire = ligneDto.PrixUnitaire,
                    Remise = ligneDto.Remise,
                    NumeroLot = null,            
                    Commande = order             
                };

                order.Lignes.Add(ligne);
            }

            // calcul des montants

            order.MontantTotalHT = order.Lignes.Sum(l =>
            (l.PrixUnitaire * l.Quantite) * (1 - (l.Remise / 100)));


            order.MontantTTC = order.MontantTotalHT * (1 + CreateOrderDto.TauxTVA);

            _db.Commandes.Add(order);
            await _db.SaveChangesAsync();
            return _mapper.Map<CommandeDto>(order);
        }
        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
        {

            var order = await _db.Commandes
                            .FirstOrDefaultAsync(o => o.Id_Commande == dto.Id_Commande);

            if (order == null)
            {
                return false; 
            }

            if (order.Statut == EtatCommande.Annulee || order.Statut == EtatCommande.Livree)
                return false;


            order.Statut = dto.NouveauStatut;

            await _db.SaveChangesAsync();
            return true;


        }
        public async Task<bool> DeleteOrderAsync(int idCommande)
        {
            var order = await _db.Commandes.Include(c => c.Lignes)
                .Include(c => c.Reclamations)
                .FirstOrDefaultAsync(c => c.Id_Commande == idCommande);
            if (order == null)
            {
                return false;
            }
            _db.Commandes.Remove(order);
            await _db.SaveChangesAsync();
            return true;

        }
  
    }
}
