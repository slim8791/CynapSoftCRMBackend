using AutoMapper;
using CynapCRM.Services.FieldAPI.Data;
using CynapCRM.Services.FieldAPI.Models;
using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.FieldAPI.Service
{
    public class TourneeService : ITourneeService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public TourneeService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        // ================================
        // 🔹 TOURNÉE
        // ================================

        public async Task<TourneeDto?> CreateOrUpdateTourneeAsync(TourneeDto dto)
        {
            var entity = _mapper.Map<Tournee>(dto);

            var existing = await _db.Tournees
                .FirstOrDefaultAsync(t => t.Id_Tournee == dto.Id_Tournee);

            if (existing == null)
            {
                _db.Tournees.Add(entity);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<TourneeDto>(entity);
        }

        public async Task<TourneeDto?> GetTourneeByIdAsync(int idTournee)
        {
            var tournee = await _db.Tournees
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id_Tournee == idTournee);

            if (tournee == null)
            {
                return null;
            }

            return _mapper.Map<TourneeDto>(tournee);
        }

        public async Task<IEnumerable<TourneeDto>> GetTourneesByPlanningAsync(int idPlanning)
        {
            var tournees = await _db.Tournees
                        .Where(t => t.Id_Planning == idPlanning)
                        .OrderByDescending(t => t.Date)
                        .AsNoTracking()
                        .ToListAsync();

            return _mapper.Map<IEnumerable<TourneeDto>>(tournees);
        }

        public async Task<bool> DeleteTourneeAsync(int idTournee)
        {
            var tournee = await _db.Tournees.FindAsync(idTournee);
            if (tournee == null)
            {
                return false;
            }

            _db.Tournees.Remove(tournee);
            await _db.SaveChangesAsync();
            return true;
        }

        // 🔥 logique métier
        public async Task<bool> StartTourneeAsync(int idTournee)
        {
            var tournee = await _db.Tournees.FirstOrDefaultAsync(t => t.Id_Tournee == idTournee);
            if (tournee == null)
            {
                return false;
            }

            tournee.Statut = "En cours";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EndTourneeAsync(int idTournee)
        {
            var tournee = await _db.Tournees.FirstOrDefaultAsync(t => t.Id_Tournee == idTournee);
            if (tournee == null)
            {
                return false;
            }

            tournee.Statut = "Terminée";
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ValidateTourneeAsync(int idTournee)
        {
            var tournee = await _db.Tournees
                    .Include(t => t.Visites)
                    .FirstOrDefaultAsync(t => t.Id_Tournee == idTournee);

            if (tournee?.Visites == null || !tournee.Visites.Any())
                return false;

            // Toutes les visites doivent appartenir au même délégué
            if (tournee.Visites.Any(v => v.Id_User_Delegue != tournee.Id_User_Delegue))
                return false;

            // Toutes les visites doivent être le même jour que la tournée
            if (tournee.Visites.Any(v => v.Date.Date != tournee.Date.Date))
                return false;

            // Pas de doublons (même client, même date)
            bool hasDuplicates = tournee.Visites
                .GroupBy(v => new { v.Date, v.Id_Medecin, v.Id_Pharmacien })
                .Any(g => g.Count() > 1);

            if (hasDuplicates) return false;

            return true;
        }
        public async Task<double> GetTourneeCompletionRateAsync(int idTournee)
        {
            var tournee = await _db.Tournees
                    .Include(t => t.Visites)
                    .FirstOrDefaultAsync(t => t.Id_Tournee == idTournee);

            if (tournee?.Visites == null || !tournee.Visites.Any())
                return 0;

            int total = tournee.Visites.Count;
            int completees = tournee.Visites.Count(v => v.Rapport != null);

            return Math.Round((double)completees / total * 100, 2);

        }
    }
}
