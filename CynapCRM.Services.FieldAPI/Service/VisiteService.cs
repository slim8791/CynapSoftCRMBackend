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
            var entity = _mapper.Map<Visite>(dto);

            var existing = await _db.Visites
                .FirstOrDefaultAsync(v => v.Id_Visite == dto.IdVisite);

            if (existing == null)
            {
                _db.Visites.Add(entity);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<VisiteDto>(entity);
        }

        public async Task<VisiteDto?> GetVisiteByIdAsync(int idVisite)
        {
            var visite = await _db.Visites
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
            .OrderByDescending(v => v.Date)
            .AsNoTracking()
            .ToListAsync();

            return _mapper.Map<IEnumerable<VisiteDto>>(visites);
        }

        public async Task<IEnumerable<VisiteDto>> GetVisitesByTourneeAsync(int idTournee)
        {
            var visites = await _db.Visites
            .Where(v => v.Id_Tournee == idTournee)
            .OrderByDescending(v => v.Date)
            .AsNoTracking()
            .ToListAsync();

            return _mapper.Map<IEnumerable<VisiteDto>>(visites);
        }

        public async Task<bool> DeleteVisiteAsync(int idVisite)
        {
            var visite = await _db.Visites.FindAsync(idVisite);
            if (visite == null)
            {
                return false;
            }

            _db.Visites.Remove(visite);
            await _db.SaveChangesAsync();
            return true;
        }

        // 🔥 logique métier
        public async Task<bool> AffectVisiteToTourneeAsync(int idVisite, int idTournee)
        {
            var visite = await _db.Visites.FirstOrDefaultAsync(v => v.Id_Visite == idVisite);
            var tournee = await _db.Tournees.FirstOrDefaultAsync(t => t.Id_Tournee == idTournee);

            if (visite == null || tournee == null)
                return false;

            // Vérifier que le délégué est le même
            if (visite.Id_User_Delegue != tournee.Id_User_Delegue)
                return false;

            // Vérifier que la date est la même
            if (visite.Date.Date != tournee.Date.Date)
                return false;

            // ✅ Affecter la tournée à la visite
            visite.Id_Tournee = tournee.Id_Tournee;

            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<bool> CompleteVisiteAsync(int idVisite)
        {
            var visite = await _db.Visites.FirstOrDefaultAsync(v => v.Id_Visite == idVisite);
            if (visite == null)
            {
                return false;
            }

            // Ici tu peux ajouter une logique métier : par exemple changer le type ou marquer comme "complétée"
            visite.IsCompleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsVisiteOwnedByDelegueAsync(int idVisite, int idDelegue)
        {
            var visite = await _db.Visites
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id_Visite == idVisite);

            if (visite == null)
                return false;

            // 🔥 Vérifie que le délégué est bien le propriétaire
            return visite.Id_User_Delegue == idDelegue;
        }
    }
}
