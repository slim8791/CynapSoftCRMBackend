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
        public async Task<ObjectifDelegueDto?> CreateOrUpdateObjectifAsync(ObjectifDelegueDto dto)
        {
            if (dto.ValeurCible <= 0)
                return null;

            if (!Enum.IsDefined(typeof(TypeObjectif), dto.Type) ||
                !Enum.IsDefined(typeof(PeriodeObjectif), dto.Periode))
                return null;

            Objectif_Delegue objectif;
            if (dto.Id_Objectif == 0)
            {
                objectif = new Objectif_Delegue
                {
                    Type            = dto.Type,
                    ValeurCible     = dto.ValeurCible,
                    ValeurRealisee  = 0,
                    Periode         = dto.Periode,
                    Id_User_Delegue = dto.Id_User_Delegue,
                    DateDebut       = dto.DateDebut ?? DateTime.UtcNow,
                    DateFin         = dto.DateFin   ?? DateTime.UtcNow
                };

                _db.Objectifs.Add(objectif);
            }
            else
            {
                //  We are only changing ITS objectives (enhanced security)
                objectif = await _db.Objectifs
                    .FirstOrDefaultAsync(o =>
                        o.Id_Objectif == dto.Id_Objectif &&
                        o.Id_User_Delegue == dto.Id_User_Delegue);

                if (objectif == null)
                    return null;

                objectif.Type       = dto.Type;
                objectif.ValeurCible = dto.ValeurCible;
                objectif.Periode    = dto.Periode;
                if (dto.DateDebut.HasValue) objectif.DateDebut = dto.DateDebut.Value;
                if (dto.DateFin.HasValue)   objectif.DateFin   = dto.DateFin.Value;
            }
            await _db.SaveChangesAsync();
            return _mapper.Map<ObjectifDelegueDto>(objectif);
        }

        public async Task<IEnumerable<ObjectifDelegueDto?>> GetObjectifsByDelegueAsync(int idDelegue)
        {

            var objectifs = await _db.Objectifs
                            .AsNoTracking()
                            .Where(o => o.Id_User_Delegue == idDelegue)
                            .OrderBy(o => o.Periode)
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

            if (nouvelleValeur < 0)
                return false;

            var obj = await _db.Objectifs.FirstOrDefaultAsync(o => o.Id_Objectif == idObjectif);
            if (obj == null) return false;

            obj.ValeurRealisee = nouvelleValeur;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ObjectifDelegueDto>> GetAllObjectifsAsync()
        {

            var objectifs = await _db.Objectifs
                    .AsNoTracking()
                    .ToListAsync();

            return _mapper.Map<IEnumerable<ObjectifDelegueDto>>(objectifs);
        }

        public async Task<ObjectifDelegueDto?> GetObjectifsByIdAsync(int idObjectif)
        {
            var objectif = await _db.Objectifs
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id_Objectif == idObjectif);

            if (objectif == null)
                return null;

            return _mapper.Map<ObjectifDelegueDto>(objectif);
        }
    }
}
