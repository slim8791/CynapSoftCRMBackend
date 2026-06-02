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
        public async Task<double> GetTauxConversionAsync(
    int idDelegue,
    DateTime debut,
    DateTime fin)
        {
            var totalVisites = await _db.Visites
                .CountAsync(v =>
                    v.Id_User_Delegue == idDelegue &&
                    v.DateVisite >= debut &&
                    v.DateVisite <= fin);

            if (totalVisites == 0) return 0;

            var visitePositives = await _db.Rapports
                .CountAsync(r =>
                    r.Id_User_Delegue == idDelegue &&
                    r.Resultat == "POSITIF" &&
                    r.DateRapport >= debut &&
                    r.DateRapport <= fin);

            return Math.Round((double)visitePositives / totalVisites * 100, 2);
        }
        public async Task<int> GetNombreVisitesAsync(int idDelegue,DateTime debut,DateTime fin)
        {

            if (debut > fin)
                return 0;

            return await _db.Visites
                .CountAsync(v =>
                    v.Id_User_Delegue == idDelegue &&
                    v.IsCompleted &&
                    v.DateVisite >= debut &&
                    v.DateVisite <= fin);
        }
        public async Task<bool> HasVisiteAtDateAsync(int idDelegue, DateTime date)
        {

            var start = date.Date;
            var end = start.AddDays(1);


            return await _db.Visites.AnyAsync(v =>
                    v.Id_User_Delegue == idDelegue &&
                    v.DateVisite >= start &&
                    v.DateVisite < end);

        }
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
                    Type = v.Type,
                    HasRapport = v.Rapport != null
                })
                .ToListAsync();
        }

        // Customer loyalty = number of visits to that customer (Medecin or pharmacien)
        public async Task<int> CalculateClientFideliteAsync(int idClient)
        {
            return await _db.Visites
                .CountAsync(v =>
                    v.Id_Medecin == idClient ||
                    v.Id_Pharmacien == idClient);
        }

        // Individual performance = percentage of goal achievement
        public async Task<IEnumerable<PerformanceDto>> CalculatePerformanceAsync(int idDelegue)
        {
            var objectifs = await _db.Objectifs
                .AsNoTracking()
                .Where(o => o.Id_User_Delegue == idDelegue)
                .ToListAsync();

            if (!objectifs.Any())
                return Enumerable.Empty<PerformanceDto>();

            // Calculer la période courante selon le type de période
            var now = DateTime.UtcNow;
            var result = new List<PerformanceDto>();

            foreach (var o in objectifs)
            {
                // Déterminer la plage de dates selon la période
                DateTime debut;
                DateTime fin;

                switch (o.Periode)
                {
                    case PeriodeObjectif.Mensuel:
                        debut = new DateTime(now.Year, now.Month, 1);
                        fin = debut.AddMonths(1).AddTicks(-1);
                        break;

                    case PeriodeObjectif.Trimestriel:
                        var trimestre = (now.Month - 1) / 3;
                        debut = new DateTime(now.Year, trimestre * 3 + 1, 1);
                        fin = debut.AddMonths(3).AddTicks(-1);
                        break;

                    case PeriodeObjectif.Annuel:
                        debut = new DateTime(now.Year, 1, 1);
                        fin = new DateTime(now.Year, 12, 31, 23, 59, 59);
                        break;

                    default:
                        debut = new DateTime(now.Year, now.Month, 1);
                        fin = debut.AddMonths(1).AddTicks(-1);
                        break;
                }

                // Recalcul dynamique selon le type d'objectif
                int valeurRealisee;

                switch (o.Type)
                {
                    // Type 1 — Visites complétées dans la période
                    case TypeObjectif.Visites:
                        valeurRealisee = await _db.Visites.CountAsync(v =>
                            v.Id_User_Delegue == idDelegue &&
                            v.IsCompleted &&
                            v.DateVisite >= debut &&
                            v.DateVisite <= fin);
                        break;

                    // Type 3 — Nouveaux clients visités pour la 1ère fois
                    case TypeObjectif.NouveauxClients:
                        // Médecins visités pour la première fois dans la période
                        var nouveauxMedecins = await _db.Visites
                            .Where(v =>
                                v.Id_User_Delegue == idDelegue &&
                                v.Id_Medecin != null &&
                                v.DateVisite >= debut &&
                                v.DateVisite <= fin)
                            .Select(v => v.Id_Medecin)
                            .Distinct()
                            .CountAsync();

                        // Pharmaciens visités pour la première fois dans la période
                        var nouveauxPharmaciens = await _db.Visites
                            .Where(v =>
                                v.Id_User_Delegue == idDelegue &&
                                v.Id_Pharmacien != null &&
                                v.DateVisite >= debut &&
                                v.DateVisite <= fin)
                            .Select(v => v.Id_Pharmacien)
                            .Distinct()
                            .CountAsync();

                        valeurRealisee = nouveauxMedecins + nouveauxPharmaciens;
                        break;

                    // Type 4 — Fidélisation = clients visités plusieurs fois
                    case TypeObjectif.Fidelisation:
                        valeurRealisee = await _db.Visites
                            .Where(v =>
                                v.Id_User_Delegue == idDelegue &&
                                v.IsCompleted &&
                                v.DateVisite >= debut &&
                                v.DateVisite <= fin)
                            .GroupBy(v => v.Id_Medecin ?? v.Id_Pharmacien)
                            .CountAsync(g => g.Count() > 1);
                        break;

                    // Type 2 — Chiffre d'affaires : valeur statique en base
                    // car le CA vient du service Commandes — pas accessible ici
                    case TypeObjectif.ChiffreAffaires:
                    default:
                        // Fallback sur la valeur stockée en base
                        valeurRealisee = o.ValeurRealisee;
                        break;
                }

                // Mettre à jour ValeurRealisee en base si différent
                // pour garder la cohérence avec UpdateObjectifValueAsync
                if (valeurRealisee != o.ValeurRealisee)
                {
                    var objectifToUpdate = await _db.Objectifs
                        .FirstOrDefaultAsync(obj => obj.Id_Objectif == o.Id_Objectif);

                    if (objectifToUpdate != null)
                    {
                        objectifToUpdate.ValeurRealisee = valeurRealisee;
                        await _db.SaveChangesAsync();
                    }
                }

                // Calculer le pourcentage
                var pourcentage = o.ValeurCible <= 0
                    ? 0
                    : (double)valeurRealisee / o.ValeurCible * 100;

                // Clamp entre 0 et 100
                pourcentage = Math.Min(100, Math.Max(0, pourcentage));

                result.Add(new PerformanceDto
                {
                    Type = o.Type,
                    ValeurCible = o.ValeurCible,
                    ValeurRealisee = valeurRealisee,
                    Pourcentage = Math.Round(pourcentage, 2)
                });
            }

            return result;
        }

        public async Task<double> GetPerformanceRateAsync(int idDelegue)
        {
            var performances = await CalculatePerformanceAsync(idDelegue);

            if (!performances.Any())
                return 0;

            return performances.Average(p => p.Pourcentage);
        }
    }

}

  