using AutoMapper;
using CynapCRM.Services.FieldAPI.Data;
using CynapCRM.Services.FieldAPI.Models;
using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.FieldAPI.Service
{
    public class RapportService : IRapportService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public RapportService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // ================================
        // 🔹 RAPPORT
        // ================================

        public async Task<RapportVisiteDto?> CreateRapportAsync(RapportVisiteDto dto)
        {
            // 🔥 règle métier : 1 visite = 1 rapport
            var exists = await _db.Rapports
                .AnyAsync(r => r.Id_Visite == dto.Id_Visite);

            if (exists) return null;

            var entity = _mapper.Map<Rapport_visite>(dto);

            _db.Rapports.Add(entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<RapportVisiteDto>(entity);
        }

        public async Task<RapportVisiteDto?> GetRapportByVisiteAsync(int idVisite)
        {
            var rapport = await _db.Rapports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id_Visite == idVisite);

            if (rapport == null)
            {
                return null;
            }

            return _mapper.Map<RapportVisiteDto>(rapport);
        }

        public async Task<bool> DeleteRapportAsync(int idRapport)
        {
            var entity = await _db.Rapports.FindAsync(idRapport);
            if (entity == null) return false;

            _db.Rapports.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateRapportAsync(int idRapport, int idSuperviseur)
        {
            var rapport = await _db.Rapports.FindAsync(idRapport);
            if (rapport == null)
            {
                return false;
            }

            rapport.Resultat = $"Validé par superviseur {idSuperviseur}";
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CanCreateRapportAsync(int idVisite)
        {
            // Charger la visite avec son rapport
            var visite = await _db.Visites
                .AsNoTracking()
                .Include(v => v.Rapport)
                .FirstOrDefaultAsync(v => v.Id_Visite == idVisite);

            // Si la visite n’existe pas → impossible
            if (visite == null)
                return false;

            // Si la visite a déjà un rapport → impossible
            if (visite.Rapport != null)
                return false;

            // Sinon → autorisé
            return true;
        }
        public async Task<bool> HasRapportAsync(int idVisite)
        {
            var rapport = await _db.Rapports
                        .FirstOrDefaultAsync(r => r.Id_Visite == idVisite);

            return rapport != null;
        }

    }
}
