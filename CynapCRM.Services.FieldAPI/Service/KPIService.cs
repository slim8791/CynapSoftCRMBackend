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

        
        

       
        
        


        

        

        // 🔥 KPI
        public async Task<IEnumerable<PerformanceDto>> CalculatePerformanceAsync(int idDelegue)
        {
            var result = new List<PerformanceDto>();

            // 🔥 1. Récupérer les objectifs du délégué
            var objectifs = await _db.Objectifs
                .Where(o => o.Id_User_Delegue == idDelegue)
                .ToListAsync();

            foreach (var obj in objectifs)
            {
                int realise = 0;

                // 🔥 2. Calcul selon le type d'objectif
                switch (obj.Type.ToLower())
                {
                    case "visite":
                        realise = await _db.Visites
                            .CountAsync(v => v.Id_User_Delegue == idDelegue);
                        break;

                    case "rapport":
                        realise = await _db.Rapports
                            .CountAsync(r => r.Id_User_Delegue == idDelegue);
                        break;

                    case "tournee":
                        realise = await _db.Tournees
                            .CountAsync(t => t.Id_User_Delegue == idDelegue);
                        break;

                    default:
                        realise = 0;
                        break;
                }

                // 🔥 3. Calcul performance
                double pourcentage = obj.ValeurCible == 0
                    ? 0
                    : (double)realise / obj.ValeurCible * 100;

                // 🔥 4. Ajouter résultat
                result.Add(new PerformanceDto
                {
                    TypeObjectif = obj.Type,
                    ValeurCible = obj.ValeurCible,
                    ValeurRealisee = realise,
                    Pourcentage = Math.Round(pourcentage, 2)
                });
            }

            return result;
        }
        public async Task<double> GetPerformanceRateAsync(int idDelegue)
        {
            var tournees = await _db.Tournees
                .Include(t => t.Visites)
                .Where(t => t.Id_User_Delegue == idDelegue)
                .ToListAsync();

            if (!tournees.Any())
                return 0;

            // Calculer le taux de complétion pour chaque tournée
            var tauxList = tournees.Select(t =>
            {
                int total = t.Visites?.Count ?? 0;
                if (total == 0) return 0.0;

                int completees = t.Visites.Count(v => v.Rapport != null);
                return (double)completees / total * 100;
            });

            // Moyenne des taux
            double performance = tauxList.Average();

            return Math.Round(performance, 2);
        }

        

        

        public async Task<int> GetNombreVisitesAsync(int idDelegue, DateTime debut, DateTime fin)
        {
            return await _db.Visites
                        .CountAsync(v => v.Id_User_Delegue == idDelegue && v.Date >= debut && v.Date <= fin);
        }
        public async Task<IEnumerable<ActiviteHistoriqueDto>> GetHistoriqueActiviteAsync(int idDelegue)
        {
            var visites = await _db.Visites
                .AsNoTracking()
                .Where(v => v.Id_User_Delegue == idDelegue)
                .Include(v => v.Tournee)
                .Include(v => v.Rapport)
                .OrderByDescending(v => v.Date)
                .Select(v => new ActiviteHistoriqueDto
                {
                    Id_Visite = v.Id_Visite,
                    Date = v.Date,
                    Type = v.Type,
                    NomTournee = v.Tournee != null ? v.Tournee.Nom : string.Empty,
                    HasRapport = v.Rapport != null
                })
                .ToListAsync();

            return visites;
        }
        public async Task<int> CalculateClientFideliteAsync(int idClient)
        {
            var visites = await _db.Visites
                .AsNoTracking()
                .Where(v => v.Id_Medecin == idClient || v.Id_Pharmacien == idClient)
                .OrderBy(v => v.Date)
                .ToListAsync();

            if (!visites.Any())
                return 0;

            // Nombre total de visites
            int totalVisites = visites.Count;

            // Ancienneté du client (en mois)
            int dureeMois = Math.Max(1, (DateTime.Now.Year - visites.First().Date.Year) * 12
                + DateTime.Now.Month - visites.First().Date.Month);
            // Fréquence moyenne (visites par mois)
            double freq = dureeMois > 0 ? (double)totalVisites / dureeMois : totalVisites;

            // Score de fidélité (normalisé sur 100)
            int score = (int)Math.Min(100, freq * 20); // ex. 5 visites/mois ≈ 100

            return score;
        }
    }
}
  