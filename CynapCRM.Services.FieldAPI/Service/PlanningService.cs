using AutoMapper;
using CynapCRM.Services.FieldAPI.Data;
using CynapCRM.Services.FieldAPI.Models;
using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.FieldAPI.Service
{
    public class PlanningService : IPlanningService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public PlanningService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // ================================
        // 🔹 PLANNING
        // ================================

        public async Task<PlanningVisiteDto?> CreateOrUpdatePlanningAsync(PlanningVisiteDto dto)
        {


            Planning_Visite planning;

            // ✅ Construire les DateTime complets à partir du DTO
            var debut = dto.Date.Date.Add(dto.HeureDebut);
            var fin = dto.Date.Date.Add(dto.HeureFin);

            // ✅ Vérifier la disponibilité du délégué
            var hasConflict = await CheckPlanningConflictAsync(
                dto.Id_User_Delegue,
                debut,
                fin);

            if (hasConflict)
                return null; // ❌ délégué non disponible

            // ➕ Création
            if (dto.Id_Planning == 0)
            {
                planning = _mapper.Map<Planning_Visite>(dto);
                planning.Etat = EtatPlanning.EnAttente;

                _db.Plannings.Add(planning);
            }
            else
            {
                // ✏️ Mise à jour
                planning = await _db.Plannings
                    .FirstOrDefaultAsync(p => p.Id_Planning == dto.Id_Planning);

                if (planning == null || planning.Etat == EtatPlanning.Confirme)
                    return null;

                _mapper.Map(dto, planning);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<PlanningVisiteDto>(planning);


        }

        public async Task<PlanningVisiteDto?> GetPlanningByIdAsync(int idPlanning)
        {
            var planningVisite = await _db.Plannings
                .AsNoTracking() // 🔥 optimisation
                .FirstOrDefaultAsync(p => p.Id_Planning == idPlanning);

            if (planningVisite == null)
            {
                return null;
            }

            return _mapper.Map<PlanningVisiteDto>(planningVisite);
        }

        public async Task<IEnumerable<PlanningVisiteDto>> GetPlanningByDelegueAsync(int idDelegue)
        {

            var plannings = await _db.Plannings
                            .AsNoTracking()
                            .Where(p => p.Id_User_Delegue == idDelegue)
                            .OrderBy(p => p.Date)
                            .ThenBy(p => p.HeureDebut)
                            .ToListAsync();


            return _mapper.Map<IEnumerable<PlanningVisiteDto>>(plannings);
        }

        public async Task<IEnumerable<PlanningVisiteDto>> GetPlanningsByDateRangeAsync(
                    int idDelegue,
                    DateTime startDate,
                    DateTime endDate)
        {
            var plannings = await _db.Plannings
                .AsNoTracking()
                .Where(p =>
                    p.Id_User_Delegue == idDelegue &&
                    p.Date >= startDate.Date &&
                    p.Date <= endDate.Date)
                .OrderBy(p => p.Date)
                .ThenBy(p => p.HeureDebut)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PlanningVisiteDto>>(plannings);
        }

        public async Task<IEnumerable<PlanningVisiteDto>> GetPlanningByDelegueAndDateAsync(
                    int idDelegue,
                    DateTime date)
        {
            var plannings = await _db.Plannings
                .AsNoTracking()
                .Where(p =>
                    p.Id_User_Delegue == idDelegue &&
                    p.Date == date.Date)
                .OrderBy(p => p.HeureDebut)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PlanningVisiteDto>>(plannings);
        }

        public async Task<bool> DeletePlanningAsync(int idPlanning)
        {

            var planning = await _db.Plannings.FirstOrDefaultAsync(p => p.Id_Planning == idPlanning);

            if (planning == null || planning.Etat != EtatPlanning.EnAttente)
                return false;

            _db.Plannings.Remove(planning);
            await _db.SaveChangesAsync();
            return true;

        }

        //  logique métier

        public async Task<bool> CheckPlanningConflictAsync(int idDelegue,DateTime debut,DateTime fin)
        {
            var date = debut.Date;
            var heureDebut = debut.TimeOfDay;
            var heureFin = fin.TimeOfDay;

            return await _db.Plannings.AnyAsync(p =>
                p.Id_User_Delegue == idDelegue &&
                p.Date == date &&
                heureDebut < p.HeureFin &&
                heureFin > p.HeureDebut
            );
        }

        public async Task<bool> ValidatePlanningAsync(int idPlanning)
        {
            var planning = await _db.Plannings
                .FirstOrDefaultAsync(p => p.Id_Planning == idPlanning);

            if (planning == null || planning.Etat != EtatPlanning.EnAttente)
                return false;

            planning.Etat = EtatPlanning.Confirme;
            await _db.SaveChangesAsync();
            return true;
        }


    }
}
