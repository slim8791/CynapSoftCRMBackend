using AutoMapper;
using CynapCRM.Services.FieldAPI.Data;
using CynapCRM.Services.FieldAPI.Models;
using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.FieldAPI.Service
{
    public class VisiteService : IVisiteService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public VisiteService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        // ================================
        // 🔹 VISITE
        // ================================

        public async Task<VisiteDto?> CreateOrUpdateVisiteAsync(VisiteDto dto)
        {

            Visite visite;

            // ➕ Création
            if (dto.IdVisite == 0)
            {
                visite = _mapper.Map<Visite>(dto);
                visite.IsCompleted = false;

                _db.Visites.Add(visite);
            }
            // ✏️ Mise à jour
            else
            {
                visite = await _db.Visites.FirstOrDefaultAsync(v => v.Id_Visite == dto.IdVisite);

                if (visite == null || visite.IsCompleted)
                    return null;

                _mapper.Map(dto, visite);
            }


            await _db.SaveChangesAsync();
            return _mapper.Map<VisiteDto>(visite);
        }

        public async Task<VisiteDto?> GetVisiteByIdAsync(int idVisite)
        {
            var visite = await _db.Visites
            .Include(v => v.Rapport)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id_Visite == idVisite);

            if (visite == null)
            {
                return null;
            }

            return _mapper.Map<VisiteDto>(visite);
        }

        public async Task<IEnumerable<VisiteDto>> GetVisitesByDelegueAsync(int idDelegue)
        {
            var visites = await _db.Visites
            .Where(v => v.Id_User_Delegue == idDelegue)
            .OrderByDescending(v => v.DateVisite)
            .AsNoTracking()
            .ToListAsync();

            return _mapper.Map<IEnumerable<VisiteDto>>(visites);
        }

        public async Task<IEnumerable<VisiteDto>> GetVisitesByPlanningAsync(int idPlanning)
        {
            var visites = await _db.Visites
            .Where(v => v.Id_Planning == idPlanning)
            .OrderByDescending(v => v.DateVisite)
            .AsNoTracking()
            .ToListAsync();

            return _mapper.Map<IEnumerable<VisiteDto>>(visites);
        }

        public async Task<bool> DeleteVisiteAsync(int idVisite)
        {
            var visite = await _db.Visites.FirstOrDefaultAsync(v => v.Id_Visite == idVisite);
            if (visite == null || visite.IsCompleted)
            {
                return false;
            }

            _db.Visites.Remove(visite);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CompleteVisiteAsync(int idVisite)
        {
            var visite = await _db.Visites
                .Include(v => v.Rapport)
                .FirstOrDefaultAsync(v => v.Id_Visite == idVisite);
            if (visite == null)
            {
                return false;
            }

            if (visite.Rapport == null)
                return false;


            visite.IsCompleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
        // 🔥 logique métier
        public async Task<bool> AffectVisiteToPlanningAsync(int idVisite, int idPlanning)
        {

            // 1️⃣ Charger la visite
            var visite = await _db.Visites.FirstOrDefaultAsync(v => v.Id_Visite == idVisite);

            if (visite == null)
                return false;

            // ❌ Impossible si la visite est déjà complétée
            if (visite.IsCompleted)
                return false;

            // 2️⃣ Charger le planning
            var planning = await _db.Plannings
                .FirstOrDefaultAsync(p => p.Id_Planning == idPlanning);

            if (planning == null)
                return false;

            // 3️⃣ Sécurité métier : même délégué
            if (visite.Id_User_Delegue != planning.Id_User_Delegue)
                return false;

            // 4️⃣ Affectation
            visite.Id_Planning = idPlanning;

            await _db.SaveChangesAsync();
            return true;

        }

        public async Task<bool> IsVisiteOwnedByDelegueAsync(int idVisite, int idDelegue)
        {

            return await _db.Visites
                            .AnyAsync(v =>
                                v.Id_Visite == idVisite &&
                                v.Id_User_Delegue == idDelegue);
        }
    }
}
