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
    }
    // ================================
        // 🔹 OBJECTIF
        // ================================

        public async Task<ObjectifDelegueDto?> CreateOrUpdateObjectifAsync(ObjectifDelegueDto dto)
        {
            var entity = _mapper.Map<Objectif_Delegue>(dto);

            var existing = await _db.Objectifs
                .FirstOrDefaultAsync(o => o.Id_Objectif == dto.Id_Objectif);

            if (existing == null)
            {
                _db.Objectifs.Add(entity);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<ObjectifDelegueDto>(entity);
        }

        public async Task<ObjectifDelegueDto?> GetObjectifByDelegueAsync(int idDelegue)
        {
            var objectif = await _db.Objectifs
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id_User_Delegue == idDelegue);

            if (objectif == null)
            {
                return null;
            }

            return _mapper.Map<ObjectifDelegueDto>(objectif);
        }

        public async Task<bool> DeleteObjectifAsync(int idObjectif)
        {
            var entity = await _db.Objectifs.FindAsync(idObjectif);
            if (entity == null) return false;

            _db.Objectifs.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateObjectifValueAsync(int idObjectif, int nouvelleValeur)
        {
            var obj = await _db.Objectifs.FindAsync(idObjectif);
            if (obj == null) return false;

            obj.ValeurCible = nouvelleValeur;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
