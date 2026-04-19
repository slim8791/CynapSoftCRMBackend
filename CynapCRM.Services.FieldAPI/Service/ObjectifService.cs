using AutoMapper;
using CynapCRM.Services.FieldAPI.Data;
using CynapCRM.Services.FieldAPI.Models;
using CynapCRM.Services.FieldAPI.Models.Dto;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.FieldAPI.Service
{
    public class ObjectifService : IObjectifService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public ObjectifService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
    
    // ================================
        // 🔹 OBJECTIF
        // ================================

        public async Task<ObjectifDelegueDto?> CreateOrUpdateObjectifAsync(ObjectifDelegueDto dto)
        {

            Objectif_Delegue objectif;

            // ➕ Création
            if (dto.Id_Objectif == 0)
            {
                objectif = _mapper.Map<Objectif_Delegue>(dto);
                _db.Objectifs.Add(objectif);
            }
            // ✏️ Mise à jour
            else
            {
                objectif = await _db.Objectifs
                    .FirstOrDefaultAsync(o => o.Id_Objectif == dto.Id_Objectif);

                if (objectif == null)
                    return null;

                _mapper.Map(dto, objectif);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<ObjectifDelegueDto>(objectif);

        }
        
        public async Task<IEnumerable<ObjectifDelegueDto?>> GetObjectifsByDelegueAsync(int idDelegue)
        {

            var objectifs = await _db.Objectifs
                            .AsNoTracking()
                            .Where(o => o.Id_User_Delegue == idDelegue)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<ObjectifDelegueDto>>(objectifs);
        }

        public async Task<bool> DeleteObjectifAsync(int idObjectif)
        {

            var objectif = await _db.Objectifs.FirstOrDefaultAsync(o => o.Id_Objectif == idObjectif);
            if (objectif == null)
                return false;

            _db.Objectifs.Remove(objectif);
            await _db.SaveChangesAsync();
            return true;

        }

        public async Task<bool> UpdateObjectifValueAsync(int idObjectif, int nouvelleValeur)
        {
            var obj = await _db.Objectifs.FirstOrDefaultAsync(o => o.Id_Objectif == idObjectif);
            if (obj == null) return false;

            obj.ValeurCible = nouvelleValeur;
            await _db.SaveChangesAsync();
            return true;
        }

        
    }
}
