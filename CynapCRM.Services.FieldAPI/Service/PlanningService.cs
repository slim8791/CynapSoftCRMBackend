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
        public async Task<IEnumerable<PlanningVisiteDto>> GetAllPlanningsAsync(
    DateTime? startDate = null,
    DateTime? endDate = null)
        {
            var query = _db.Plannings.AsNoTracking().AsQueryable();

            if (startDate.HasValue)
                query = query.Where(p => p.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(p => p.Date <= endDate.Value.Date);

            var plannings = await query
                .OrderBy(p => p.Date)
                .ThenBy(p => p.HeureDebut)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PlanningVisiteDto>>(plannings);
        }
        public async Task<PlanningVisiteDto?> CreateOrUpdatePlanningAsync(PlanningVisiteDto dto)
        {
            if (dto.HeureDebut >= dto.HeureFin)
                return null;

            var debut = dto.Date.Date.Add(dto.HeureDebut);
            var fin = dto.Date.Date.Add(dto.HeureFin);

            var hasConflict = await _db.Plannings.AnyAsync(p =>
                p.Id_User_Delegue == dto.Id_User_Delegue &&
                p.Date == dto.Date &&
                p.Id_Planning != dto.Id_Planning &&
                debut.TimeOfDay < p.HeureFin &&
                fin.TimeOfDay > p.HeureDebut
            );

            if (hasConflict)
                return null;

            Planning_Visite planning;

            if (dto.Id_Planning == 0)
            {
                planning = new Planning_Visite
                {
                    Date = dto.Date,
                    HeureDebut = dto.HeureDebut,
                    HeureFin = dto.HeureFin,
                    Etat = EtatPlanning.EnAttente,
                    Id_User_Delegue = dto.Id_User_Delegue
                };

                _db.Plannings.Add(planning);
            }
            else
            {
                planning = await _db.Plannings
                    .FirstOrDefaultAsync(p => p.Id_Planning == dto.Id_Planning);

                if (planning == null || planning.Etat == EtatPlanning.Confirme)
                    return null;

                planning.Date = dto.Date;
                planning.HeureDebut = dto.HeureDebut;
                planning.HeureFin = dto.HeureFin;
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<PlanningVisiteDto>(planning);
        }

        public async Task<PlanningVisiteDto?> GetPlanningByIdAsync(int idPlanning)
        {
            var planningVisite = await _db.Plannings
                .AsNoTracking() // optimization
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

        public async Task<IEnumerable<PlanningVisiteDto>> GetPlanningsByDateRangeAsync(int idDelegue,
                                                                        DateTime startDate,
                                                                        DateTime endDate)
        {
            if (startDate.Date > endDate.Date)
                return Enumerable.Empty<PlanningVisiteDto>();

            var plannings = await _db.Plannings
                .AsNoTracking()
                .Where(p =>
                    p.Id_User_Delegue == idDelegue &&
                    p.Date >= startDate.Date &&
                    p.Date <= endDate.Date )
                .OrderBy(p => p.Date)
                .ThenBy(p => p.HeureDebut)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PlanningVisiteDto>>(plannings);
        }

        public async Task<IEnumerable<PlanningVisiteDto>> GetPlanningByDelegueAndDateAsync(int idDelegue,
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
        public async Task<bool> CheckPlanningConflictAsync(int idDelegue,DateTime debut,DateTime fin,
            int? excludePlanningId = null)
        {
            var date = debut.Date;
            var heureDebut = debut.TimeOfDay;
            var heureFin = fin.TimeOfDay;

            return await _db.Plannings.AnyAsync(p =>
                p.Id_User_Delegue == idDelegue &&
                p.Date == date &&
                (excludePlanningId == null || p.Id_Planning != excludePlanningId) &&
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
