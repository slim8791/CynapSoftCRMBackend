using AutoMapper;
using CynapCRM.Services.OrderAPI.Data;
using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.OrderAPI.Service
{
    public class ReclamationService : IReclamationService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        public ReclamationService(IMapper mapper, AppDbContext db)
        {
            _mapper = mapper;
            _db = db;
        }
        public async Task<IEnumerable<ReclamationDto>> GetAllReclamationsAsync()
        {
            var reclamations = await _db.Reclamations
                .Include(r => r.Commande)
                .Include(r => r.LigneCommande).OrderByDescending(r => r.DateReclamation)
                .AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<ReclamationDto>>(reclamations);
        }

        public async Task<IEnumerable<ReclamationDto>> GetReclamationsByOrderAsync(int orderId)
        {
            var reclamations = await _db.Reclamations
                .Where(r => r.Id_Commande == orderId)
                .Include(r => r.Commande)
                .Include(r => r.LigneCommande).OrderByDescending(r => r.DateReclamation)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<ReclamationDto>>(reclamations);
        }
        public async Task<IEnumerable<ReclamationDto>> GetReclamationsByClientAsync(int idClient)
        {
            var reclamations = await _db.Reclamations
                .Where(r => r.Id_Client == idClient)
                .Include(r => r.Commande)
                .Include(r => r.LigneCommande).OrderByDescending(r => r.DateReclamation)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<ReclamationDto>>(reclamations);

        }
        public async Task<ReclamationDto?> GetReclamationByIdAsync(int idReclamation)
        {
            var reclamation = await _db.Reclamations
                .Include(r => r.Commande)
                .Include(r => r.LigneCommande).OrderByDescending(r => r.DateReclamation)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id_Rec == idReclamation);
            return _mapper.Map<ReclamationDto>(reclamation);

        }
        public async Task<ReclamationDto?> CreateUpdateReclamationAsync(ReclamationDto dto)
        {

            var commandeExists = await _db.Commandes.AnyAsync(c => c.Id_Commande == dto.Id_Commande);

            if (!commandeExists)
                return null;


            Reclamation rec;

            if (dto.Id_Rec == 0)
            {
                rec = _mapper.Map<Reclamation>(dto);
                rec.DateReclamation = DateTime.UtcNow;

                rec.Statut = StatutReclamation.Ouverte;

                _db.Reclamations.Add(rec);
            }
            else
            {
                // ✏️ Mise à jour
                rec = await _db.Reclamations.FirstOrDefaultAsync(r => r.Id_Rec == dto.Id_Rec);

                if (rec == null)
                    return null;
                rec.Message = dto.Message;
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<ReclamationDto>(rec);
        }
        public async Task<bool> UpdateReclamationStatusAsync(int reclamationId, StatutReclamation newStatus)
        {
            var reclamation = await _db.Reclamations.FirstOrDefaultAsync(r => r.Id_Rec == reclamationId);
            if (reclamation == null)
            {
                return false;
            }

            switch (reclamation.Statut)
            {
                case StatutReclamation.Resolue:
                    //  Déjà résolue → aucune modification possible
                    return false;

                case StatutReclamation.Ouverte:
                    //  Une réclamation ouverte ne peut évoluer que vers EnCours
                    if (newStatus != StatutReclamation.EnCours)
                        return false;
                    break;

                case StatutReclamation.EnCours:
                    //  Une réclamation en cours ne peut évoluer que vers Résolue
                    if (newStatus != StatutReclamation.Resolue)
                        return false;
                    break;
            }

            reclamation.Statut = newStatus;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReclamationAsync(int reclamationId)
        {
            var reclamation = await _db.Reclamations.FirstOrDefaultAsync(r => r.Id_Rec == reclamationId);
            if (reclamation == null)
            {
                return false;
            }

            if (reclamation.Statut != StatutReclamation.Ouverte)
                return false;

            _db.Reclamations.Remove(reclamation);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
