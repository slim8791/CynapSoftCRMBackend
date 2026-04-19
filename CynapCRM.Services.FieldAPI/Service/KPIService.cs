using AutoMapper;
using CynapCRM.Services.FieldAPI.Data;
using CynapCRM.Services.FieldAPI.Models;
using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.FieldAPI.Service
{
    public class KPIService : IKPIService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public KPIService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // ==================================================
        // ✅ NOMBRE DE VISITES SUR UNE PÉRIODE
        // ==================================================
        public async Task<int> GetNombreVisitesAsync(
            int idDelegue,
            DateTime debut,
            DateTime fin)
        {
            return await _db.Visites
                .CountAsync(v =>
                    v.Id_User_Delegue == idDelegue &&
                    v.IsCompleted &&
                    v.DateVisite >= debut &&
                    v.DateVisite <= fin);
        }

        // ==================================================
        // ✅ EXISTE-T-IL UNE VISITE À UNE DATE ?
        // ==================================================
        public async Task<bool> HasVisiteAtDateAsync(int idDelegue, DateTime date)
        {
            var jour = date.Date;

            return await _db.Visites.AnyAsync(v =>
                v.Id_User_Delegue == idDelegue &&
                v.DateVisite.Date == jour);
        }

        // ==================================================
        // ✅ HISTORIQUE D’ACTIVITÉ DU DÉLÉGUÉ
        // ==================================================
        public async Task<IEnumerable<ActiviteHistoriqueDto>> GetHistoriqueActiviteAsync(
            int idDelegue)
        {
            return await _db.Visites
                .AsNoTracking()
                .Where(v => v.Id_User_Delegue == idDelegue)
                .OrderByDescending(v => v.DateVisite)
                .Select(v => new ActiviteHistoriqueDto
                {
                    Id_Visite = v.Id_Visite,
                    Date = v.DateVisite,
                    Type = v.Type.ToString(),
                    HasRapport = v.Rapport != null
                })
                .ToListAsync();
        }

        // ==================================================
        // ✅ KPI CLIENT : FIDÉLITÉ
        // ==================================================
        public async Task<int> CalculateClientFideliteAsync(int idClient)
        {
            return await _db.Visites
                .CountAsync(v =>
                    v.Id_Medecin == idClient ||
                    v.Id_Pharmacien == idClient);
        }

        // ==================================================
        // ✅ PERFORMANCE PAR OBJECTIF
        // ==================================================
        public async Task<IEnumerable<PerformanceDto>> CalculatePerformanceAsync(
            int idDelegue)
        {
            var objectifs = await _db.Objectifs
                .AsNoTracking()
                .Where(o => o.Id_User_Delegue == idDelegue)
                .ToListAsync();

            return objectifs.Select(o =>
            {
                var pourcentage = o.ValeurCible == 0
                    ? 0
                    : (double)o.ValeurCible / o.ValeurCible * 100;

                return new PerformanceDto
                {
                    TypeObjectif = o.Type,
                    ValeurCible = o.ValeurCible,
                    ValeurRealisee = o.ValeurCible,
                    Pourcentage = pourcentage
                };
            });
        }

        // ==================================================
        // ✅ KPI GLOBAL (MOYENNE)
        // ==================================================
        public async Task<double> GetPerformanceRateAsync(int idDelegue)
        {
            var performances = await CalculatePerformanceAsync(idDelegue);

            if (!performances.Any())
                return 0;

            return performances.Average(p => p.Pourcentage);
        }
    }

}

  