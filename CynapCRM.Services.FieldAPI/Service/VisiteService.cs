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
        public async Task<VisiteDto?> CreateOrUpdateVisiteAsync(CreateVisiteDto dto)
        {
            Visite visite;
            if (dto.IdVisite == 0)
            {
                visite = new Visite
                {
                    DateVisite = dto.DateVisite,
                    Type = dto.Type,

                    //  The delegate is valid via the JWT
                    Id_User_Delegue = dto.IdDelegue,
                    //  optional fields
                    Id_Medecin = dto.IdMedecin == 0 ? null : dto.IdMedecin,
                    Id_Pharmacien = dto.IdPharmacien == 0 ? null : dto.IdPharmacien,
                    Id_Planning = dto.IdPlanning == 0 ? null : dto.IdPlanning,

                    IsCompleted = false
                };

                _db.Visites.Add(visite);
            }
            else
            {
                visite = await _db.Visites.FirstOrDefaultAsync(v => v.Id_Visite == dto.IdVisite);

                if (visite == null || visite.IsCompleted)
                    return null;

                visite.DateVisite = dto.DateVisite;
                visite.Type = dto.Type;

                //  update optional fields
                visite.Id_Medecin = dto.IdMedecin == 0 ? null : dto.IdMedecin;
                visite.Id_Pharmacien = dto.IdPharmacien == 0 ? null : dto.IdPharmacien;
                visite.Id_Planning = dto.IdPlanning == 0 ? null : dto.IdPlanning;
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<VisiteDto>(visite);
        }
        public async Task<IEnumerable<VisiteDto>> GetAllVisitesAsync(
    DateTime? startDate = null,
    DateTime? endDate = null)
        {
            var query = _db.Visites
                .Include(v => v.Rapport)
                .AsNoTracking()
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(v => v.DateVisite >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(v => v.DateVisite <= endDate.Value);

            var visites = await query
                .OrderByDescending(v => v.DateVisite)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VisiteDto>>(visites);
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
            var visite = await _db.Visites
                .Include(v => v.Rapport) // ✅ inclure rapport
                .FirstOrDefaultAsync(v => v.Id_Visite == idVisite);

            if (visite == null || visite.IsCompleted)
                return false;

            // ✅ bloquer si rapport existe
            if (visite.Rapport != null)
                return false;

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
        public async Task<bool> AffectVisiteToPlanningAsync(int idVisite, int idPlanning)
        {
            var visite = await _db.Visites
                .FirstOrDefaultAsync(v => v.Id_Visite == idVisite);

            if (visite == null || visite.IsCompleted)
                return false;

            if (visite.Id_Planning != null)
                return false;

            var planning = await _db.Plannings
                .FirstOrDefaultAsync(p => p.Id_Planning == idPlanning);

            if (planning == null)
                return false;

            if (visite.Id_User_Delegue != planning.Id_User_Delegue)
                return false;

            if (planning.Etat == EtatPlanning.Confirme)
                return false;

            if (planning.Date.Date != visite.DateVisite.Date)
                return false;

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

        public async Task<VisiteDto?> StartVisiteAsync(int idVisite)
        {
            var visite = await _db.Visites
                .Include(v => v.Rapport)
                .FirstOrDefaultAsync(v => v.Id_Visite == idVisite);

            if (visite == null || visite.IsStarted || visite.IsCompleted)
                return null;

            visite.IsStarted  = true;
            visite.HeureDebut = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return _mapper.Map<VisiteDto>(visite);
        }
    }
}
