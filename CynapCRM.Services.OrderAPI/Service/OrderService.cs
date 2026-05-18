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
        public async Task<IEnumerable<CommandeDto>> GetAllOrdersAsync(
    int page,
    int pageSize,
    EtatCommande? statut = null,
    DateTime? startDate = null,
    DateTime? endDate = null)
        {
            var query = _db.Commandes
                .Where(c => !c.IsDeleted) // FIX: filtre IsDeleted
                .Include(c => c.Lignes)
                .AsNoTracking()
                .AsQueryable();

            // Filtre optionnel par statut
            if (statut.HasValue)
                query = query.Where(c => c.Statut == statut.Value);

            // Filtre optionnel par plage de dates
            if (startDate.HasValue)
                query = query.Where(c => c.DateCommande >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.DateCommande <= endDate.Value);

            var commandes = await query
                .OrderByDescending(c => c.DateCommande) // FIX: tri par date
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CommandeDto>>(commandes);
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
        public async Task<IEnumerable<CommandeDto>> GetOrdersByClientIdAsync(
    int clientId, int page = 1, int pageSize = 20)
        {
            var orders = await _db.Commandes
                .Where(c => c.Id_Client == clientId)
                .Include(c => c.Lignes)
                .Include(c => c.Reclamations)
                .AsNoTracking()
                .OrderByDescending(c => c.DateCommande) // ✅ tri par date
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
        public async Task<IEnumerable<CommandeDto>> GetOrdersByDateRangeAsync(
    DateTime startDate, DateTime endDate,
    int page = 1, int pageSize = 20)
        {
            var orders = await _db.Commandes
                .Where(c =>
                    c.DateCommande >= startDate &&
                    c.DateCommande <= endDate)
                .Include(c => c.Lignes)
                .AsNoTracking()
                .OrderByDescending(c => c.DateCommande)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CommandeDto>>(orders);
        }
        // Méthode dédiée plus claire que UpdateOrderStatus
        public async Task<bool> CancelOrderAsync(int idCommande, string motif)
        {
            var order = await _db.Commandes
                .FirstOrDefaultAsync(o => o.Id_Commande == idCommande);
            if (order == null) return false;

            // Seules Brouillon, EnAttente et Confirmee sont annulables
            if (order.Statut == EtatCommande.Livree ||
                order.Statut == EtatCommande.Annulee)
                return false;

            order.Statut = EtatCommande.Annulee;
            order.MotifAnnulation = motif; // si le champ existe
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<OrderDashboardDto> GetOrdersDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var debutMois = new DateTime(now.Year, now.Month, 1);
            var debutJour = now.Date;

            return new OrderDashboardDto
            {
                // Volumes par statut
                TotalCommandes = await _db.Commandes.CountAsync(c => !c.IsDeleted),
                EnAttente = await _db.Commandes.CountAsync(c => !c.IsDeleted && c.Statut == EtatCommande.EnAttente),
                Confirmees = await _db.Commandes.CountAsync(c => !c.IsDeleted && c.Statut == EtatCommande.Confirmee),
                EnPreparation = await _db.Commandes.CountAsync(c => !c.IsDeleted && c.Statut == EtatCommande.EnPreparation),
                Expediees = await _db.Commandes.CountAsync(c => !c.IsDeleted && c.Statut == EtatCommande.Expediee),
                Livrees = await _db.Commandes.CountAsync(c => !c.IsDeleted && c.Statut == EtatCommande.Livree),
                Annulees = await _db.Commandes.CountAsync(c => !c.IsDeleted && c.Statut == EtatCommande.Annulee),

                // Financier — uniquement commandes livrées
                MontantTotalHT = await _db.Commandes
                                        .Where(c => !c.IsDeleted && c.Statut == EtatCommande.Livree)
                                        .SumAsync(c => c.MontantTotalHT),
                MontantTotalTTC = await _db.Commandes
                                        .Where(c => !c.IsDeleted && c.Statut == EtatCommande.Livree)
                                        .SumAsync(c => c.MontantTTC),

                // Réclamations
                ReclamationsOuvertes = await _db.Reclamations
                                        .CountAsync(r => r.Statut == StatutReclamation.Ouverte),
                ReclamationsEnCours = await _db.Reclamations
                                        .CountAsync(r => r.Statut == StatutReclamation.EnCours),
                ReclamationsResolues = await _db.Reclamations
                                        .CountAsync(r => r.Statut == StatutReclamation.Resolue),

                // Tendance
                CommandesAujourdHui = await _db.Commandes
                                        .CountAsync(c => !c.IsDeleted &&
                                            c.DateCommande >= debutJour),
                CommandesCeMois = await _db.Commandes
                                        .CountAsync(c => !c.IsDeleted &&
                                            c.DateCommande >= debutMois)
            };
        }
        public async Task<IEnumerable<CommandeDto>> GetOrdersByStatusAsync(
    EtatCommande statut, int page = 1, int pageSize = 20)
        {
            var orders = await _db.Commandes
                .Where(c => c.Statut == statut)
                .Include(c => c.Lignes)
                .AsNoTracking()
                .OrderByDescending(c => c.DateCommande)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CommandeDto>>(orders);
        }
        private static readonly Dictionary<EtatCommande, List<EtatCommande>>
    _transitionsAutorisees = new()
{
    { EtatCommande.Brouillon,     new() { EtatCommande.EnAttente,     EtatCommande.Annulee } },
    { EtatCommande.EnAttente,     new() { EtatCommande.Confirmee,     EtatCommande.Annulee } },
    { EtatCommande.Confirmee,     new() { EtatCommande.EnPreparation, EtatCommande.Annulee } },
    { EtatCommande.EnPreparation, new() { EtatCommande.Expediee,      EtatCommande.Annulee } },
    { EtatCommande.Expediee,      new() { EtatCommande.Livree } },
    { EtatCommande.Livree,        new() { } }, // terminal
    { EtatCommande.Annulee,       new() { } }  // terminal
};

        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
        {
            var order = await _db.Commandes
                .FirstOrDefaultAsync(o =>
                    o.Id_Commande == dto.Id_Commande &&
                    !o.IsDeleted);

            if (order == null) return false;

            if (!_transitionsAutorisees.TryGetValue(order.Statut, out var autorisees))
                return false;

            if (!autorisees.Contains(dto.NouveauStatut))
                return false;

            order.Statut = dto.NouveauStatut;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOrderAsync(int idCommande)
        {
            var order = await _db.Commandes
                .FirstOrDefaultAsync(c => c.Id_Commande == idCommande);
            if (order == null) return false;

            // Règle métier : ne peut supprimer que Brouillon ou Annulée
            if (order.Statut != EtatCommande.Brouillon &&
                order.Statut != EtatCommande.Annulee)
                return false;

            // Soft delete
            order.IsDeleted = true; // si le champ existe
                                    // ou suppression physique si IsDeleted n'existe pas
            _db.Commandes.Remove(order);
            await _db.SaveChangesAsync();
            return true;
        }

    }
}
