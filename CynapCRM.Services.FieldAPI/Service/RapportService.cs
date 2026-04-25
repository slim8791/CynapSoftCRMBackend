using AutoMapper;
using CynapCRM.Services.FieldAPI.Data;
using CynapCRM.Services.FieldAPI.Models;
using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

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

        public async Task<IEnumerable<RapportVisiteDto>> GetAllRapportsAsync()
        {
            var rapports = await _db.Rapports
                .AsNoTracking()
                .OrderByDescending(r => r.DateRapport)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RapportVisiteDto>>(rapports);
        }

        public async Task<RapportVisiteDto?> GetRapportByIdAsync(int idRapport)
        {
            var rapport = await _db.Rapports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id_Rapport == idRapport);

            if (rapport == null)
                return null;

            return _mapper.Map<RapportVisiteDto>(rapport);
        }

        public async Task<RapportVisiteDto?> CreateOrUpdateRapportAsync(RapportVisiteDto dto)
        {
            var visite = await _db.Visites
                .Include(v => v.Rapport)
                .FirstOrDefaultAsync(v => v.Id_Visite == dto.Id_Visite);

            if (visite == null)
                return null;
            if (string.IsNullOrWhiteSpace(dto.Commentaire))
                return null;
            if (string.IsNullOrWhiteSpace(dto.Resultat))
                return null;
            if (visite.IsCompleted)
                return null;

            // OWNERSHIP: the delegate must be the owner of the tour
            if (visite.Id_User_Delegue != dto.Id_User_Delegue)
                return null;

            Rapport_Visite rapport;
            if (dto.Id_Rapport == 0)
            {
                if (visite.Rapport != null)
                    return null;

                rapport = new Rapport_Visite
                {
                    Id_Visite = dto.Id_Visite,
                    Commentaire = dto.Commentaire,
                    Resultat = dto.Resultat,
                    DateRapport = DateTime.UtcNow,

                    // injected from the JWT
                    Id_User_Delegue = dto.Id_User_Delegue
                };

                _db.Rapports.Add(rapport);
            }
            else
            {
                rapport = await _db.Rapports.FirstOrDefaultAsync(r =>
                        r.Id_Rapport == dto.Id_Rapport &&
                        r.Id_Visite == dto.Id_Visite &&
                        r.Id_User_Delegue == dto.Id_User_Delegue);

                if (rapport == null)
                    return null;

                rapport.Commentaire = dto.Commentaire;
                rapport.Resultat = dto.Resultat;
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<RapportVisiteDto>(rapport);
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
            var rapport = await _db.Rapports
                .Include(r => r.Visite)
                .FirstOrDefaultAsync(r => r.Id_Rapport == idRapport);

            if (rapport == null) return false;

            if (rapport.Visite != null && rapport.Visite.IsCompleted)
                return false;

            _db.Rapports.Remove(rapport);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateRapportAsync(int idRapport, int idSuperviseur)
        {
            var rapport = await _db.Rapports
                .Include(r => r.Visite)
                .FirstOrDefaultAsync(r => r.Id_Rapport == idRapport);

            if (rapport == null || rapport.Visite == null)
            {
                return false;
            }

            rapport.Visite.IsCompleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CanCreateRapportAsync(int idVisite)
        {
            var visite = await _db.Visites
                .AsNoTracking()
                .Include(v => v.Rapport)
                .FirstOrDefaultAsync(v => v.Id_Visite == idVisite);
            if ( visite == null)
            {
                return false;
            }

            if (visite.IsCompleted || visite.Rapport != null)
            {
                return false;
            }
            return true;

        }
        public async Task<bool> HasRapportAsync(int idVisite)
        {

            return await _db.Rapports
                            .AnyAsync(r => r.Id_Visite == idVisite);

        }

    }
}
