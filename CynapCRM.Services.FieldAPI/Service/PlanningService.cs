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
            var entity = _mapper.Map<PlanningVisite>(dto);

            var existing = await _db.Plannings
            .FirstOrDefaultAsync(p => p.Id_Planning == dto.Id_Planning);

            if (existing == null)
            {
                _db.Plannings.Add(entity);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<PlanningVisiteDto>(entity);
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
                     .Where(p => p.Id_User_Delegue == idDelegue)
                     .OrderByDescending(p => p.Date)
                     .AsNoTracking()
                     .ToListAsync();

            return _mapper.Map<IEnumerable<PlanningVisiteDto>>(plannings);
        }

        public async Task<bool> DeletePlanningAsync(int idPlanning)
        {
            var planning = await _db.Plannings.FindAsync(idPlanning);
            if (planning == null)
            {
                return false;
            }

            _db.Plannings.Remove(planning);
            await _db.SaveChangesAsync();
            return true;
        }

        // 🔥 logique métier
        public async Task<bool> ChangePlanningStatusAsync(int idPlanning, string statut)
        {
            var planning = await _db.Plannings.FirstOrDefaultAsync(p => p.Id_Planning == idPlanning);
            if (planning == null)
            {
                return false;
            }

            planning.Etat = statut;
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CheckPlanningConflictAsync(int idDelegue, DateTime debut, DateTime fin)
        {
            // Vérifier conflit avec plannings
            bool conflictPlanning = await _db.Plannings.AnyAsync(p =>
                p.Id_User_Delegue == idDelegue &&
                debut < p.HeureFin && fin > p.HeureDebut
            );

            if (conflictPlanning) return true;

            // Vérifier conflit avec visites
            bool conflictVisite = await _db.Visites.AnyAsync(v =>
                v.Id_User_Delegue == idDelegue &&
                v.Date >= debut && v.Date <= fin
            );

            return conflictVisite;
        }
        public async Task<bool> ValidatePlanningAsync(int idPlanning)
        {
            var planning = await _db.Plannings
                    .Include(p => p.Tournees)
                    .FirstOrDefaultAsync(p => p.Id_Planning == idPlanning);

            if (planning?.Tournees == null || !planning.Tournees.Any())
                return false;

            if (planning.HeureDebut >= planning.HeureFin)
                return false;

            if (planning.Tournees.Any(t => t.Id_User_Delegue != planning.Id_User_Delegue))
                return false;

            if (await CheckPlanningConflictAsync(planning.Id_User_Delegue, planning.HeureDebut, planning.HeureFin))
                return false;

            return true;
        }
        public async Task<bool> CheckDelegueAvailabilityAsync(int idDelegue, DateTime date)
        {
            // 1️⃣ Vérifier s’il existe un planning qui couvre cette date
            bool hasPlanning = await _db.Plannings.AnyAsync(p =>
                p.Id_User_Delegue == idDelegue &&
                date >= p.HeureDebut && date <= p.HeureFin
            );

            if (!hasPlanning) return false; // planning = indisponible

            // 2️⃣ Vérifier s’il existe une visite exactement à cette date/heure
            bool hasVisite = await _db.Visites.AnyAsync(v =>
                v.Id_User_Delegue == idDelegue &&
                v.Date == date
            );

            if (hasVisite) return false; // visite = indisponible

            // 3️⃣ Vérifier s’il existe une tournée active ce jour-là
            bool hasTournee = await _db.Tournees.AnyAsync(t =>
                t.Id_User_Delegue == idDelegue &&
                t.Date.Date == date.Date
            );

            if (hasTournee)
            {
                // ⚠️ Logique métier : une tournée peut contenir plusieurs visites
                // Donc on considère que le délégué est disponible pour ajouter une visite
                return true;
            }

            // ✅ Si aucun planning, aucune visite, et pas de tournée → disponible
            return true;
        }
    }
}
