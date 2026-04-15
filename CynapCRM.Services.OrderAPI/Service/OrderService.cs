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
        //gestion des commandes 
        public async Task<IEnumerable<CommandeDto>> GetAllOrdersAsync()
        {
            var commandes = await _db.Commandes.Include(c => c.Lignes).AsNoTracking().ToListAsync();
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
            var order = _mapper.Map<Commande>(orderDto);
            order.DateCommande = DateTime.Now;
            if (orderDto.IsFinalValidation)
            {
                order.Statut = EtatCommande.EnAttente; // (1)
            }
            else
            {
                order.Statut = EtatCommande.Brouillon; // (0)
            }

            order.MontantTotalHT = order.Lignes.Sum(l =>

            (l.PrixUnitaire * l.Quantite) * (1 - (l.Remise / 100)));


            order.MontantTTC = order.MontantTotalHT * (1 + CreateOrderDto.TauxTVA);

            _db.Commandes.Add(order);
            await _db.SaveChangesAsync();
            return _mapper.Map<CommandeDto>(order);
        }
        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
        {
            var order = await _db.Commandes.FindAsync(dto.Id_Commande);

            if (order == null)
            {
                return false; 
            }

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
        public async Task<LigneCommandeDto?> CreateUpdateLigneCommandeAsync(LigneCommandeDto ligneDto)
        {
            // Mapper le DTO vers l'entité
            var ligne = _mapper.Map<LigneCommande>(ligneDto);

            if (ligne.Id_Ligne > 0) // Cas mise à jour
            {
                var existingLigne = await _db.LignesCommandes.FindAsync(ligne.Id_Ligne);
                if (existingLigne == null)
                {
                    return null; // Ligne inexistante
                }

                _mapper.Map(ligneDto, existingLigne);
                _db.LignesCommandes.Update(existingLigne);
            }
            else // Cas création
            {
                var order = await _db.Commandes.FindAsync(ligneDto.Id_Commande);
                if (order == null)
                {
                    return null; // Commande inexistante
                }

                order.Lignes.Add(ligne);
                _db.LignesCommandes.Add(ligne);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<LigneCommandeDto>(ligne);
        }

        public async Task<bool> RemoveLigneCommandeAsync(int ligneId)
        {
            var ligne = await _db.LignesCommandes.FindAsync(ligneId);
            if (ligne == null)
            {
                return false;
            }
            _db.LignesCommandes.Remove(ligne);
            await _db.SaveChangesAsync();
            return true;


        }
        public async Task<IEnumerable<ReclamationDto>> GetAllReclamationsAsync()
        {
            var reclamations = await _db.Reclamations
                .Include(r => r.Commande)
                .Include(r => r.LigneCommande)
                .AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<ReclamationDto>>(reclamations);
        }

        public async Task<IEnumerable<ReclamationDto>> GetReclamationsByOrderAsync(int orderId)
        {
            var reclamations = await _db.Reclamations
                .Where(r => r.Id_Commande == orderId)
                .Include(r => r.Commande)
                .Include(r => r.LigneCommande)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<ReclamationDto>>(reclamations);
        }
        public async Task<IEnumerable<ReclamationDto>> GetReclamationsByClientAsync(int idClient)
        {
            var reclamations = await _db.Reclamations
                .Where(r => r.Id_Client == idClient)
                .Include(r => r.Commande)
                .Include(r => r.LigneCommande)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<ReclamationDto>>(reclamations);

        }
        public async Task<ReclamationDto?> GetReclamationByIdAsync(int idReclamation)
        {
            var reclamation = await _db.Reclamations
                .Include(r => r.Commande)
                .Include(r => r.LigneCommande)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id_Rec == idReclamation);
            return _mapper.Map<ReclamationDto>(reclamation);

        }
        public async Task<ReclamationDto?> CreateUpdateReclamationAsync(ReclamationDto dto)
        {
            // Mapper le DTO vers l'entité
            var reclamation = _mapper.Map<Reclamation>(dto);

            if (reclamation.Id_Rec > 0) // Cas mise à jour
            {
                _db.Reclamations.Update(reclamation);
            }
            else // Cas création
            {
                // Vérifier que la commande existe
                var commande = await _db.Commandes.FindAsync(dto.Id_Commande);
                if (commande == null)
                {
                    return null;
                }

                // Vérifier que la ligne de commande existe
                var ligne = await _db.LignesCommandes.FindAsync(dto.Id_Ligne);
                if (ligne == null)
                {
                    return null;
                }

                _db.Reclamations.Add(reclamation);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<ReclamationDto>(reclamation);
        }
        public async Task<bool> UpdateReclamationStatusAsync(int reclamationId, string newStatus)
        {
            var reclamation = await _db.Reclamations.FindAsync(reclamationId);
            if (reclamation == null)
            {
                return false;
            }
            reclamation.Statut = newStatus;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReclamationAsync(int reclamationId)
        {
            var reclamation = await _db.Reclamations.FindAsync(reclamationId);
            if (reclamation == null)
            {
                return false;
            }
            _db.Reclamations.Remove(reclamation);
            await _db.SaveChangesAsync();
            return true;
        }

        
    }
}
