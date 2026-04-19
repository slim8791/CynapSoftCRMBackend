using AutoMapper;
using CynapCRM.Services.InventoryAPI.Data;
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;
using CynapCRM.Services.InventoryAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Service
{
    public class DistributionService : IDistributionService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public DistributionService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;

        }

        public async Task<EchantillonDto?> CreateOrUpdateEchantillonAsync(EchantillonDto echantillonDto)
        {

            var distribution = await _db.Echantillons
                            .FirstOrDefaultAsync(e =>
                                e.Id_Distribution == echantillonDto.Id_Distribution);

            if (distribution == null)
            {
                distribution = _mapper.Map<Echantillon>(echantillonDto);
                distribution.DateDistribution = DateTime.UtcNow;
                distribution.IsDeleted = false;

                _db.Echantillons.Add(distribution);
            }
            else
            {
                _mapper.Map(echantillonDto, distribution);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<EchantillonDto>(distribution);

        }
        public async Task<EchantillonDto?> GetEchantillonByIdAsync(int idDistribution)
        {

            var distribution = await _db.Echantillons
                            .AsNoTracking()
                            .FirstOrDefaultAsync(e =>
                                e.Id_Distribution == idDistribution && !e.IsDeleted);
            if (distribution == null)
            {
                return null;
            }
            return _mapper.Map<EchantillonDto>(distribution);
        }
        public async Task<IEnumerable<EchantillonDto>> GetDistributionsByMedecinAsync(int idMedecin)
        {

            var distributions = await _db.Echantillons
                            .AsNoTracking()
                            .Where(e =>
                                e.Id_Medecin == idMedecin &&
                                !e.IsDeleted)
                            .OrderByDescending(e => e.DateDistribution)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<EchantillonDto>>(distributions);
        }
        public async Task<IEnumerable<EchantillonDto>> GetDistributionsByPharmacienAsync(int idPharmacien)
        {

            var distributions = await _db.Echantillons
                            .AsNoTracking()
                            .Where(e =>
                                e.Id_Pharmacien == idPharmacien &&
                                !e.IsDeleted)
                            .OrderByDescending(e => e.DateDistribution)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<EchantillonDto>>(distributions);
        }
        public async Task<bool> DeleteEchantillonAsync(int idDistribution)
        {

            var distribution = await _db.Echantillons
                            .FirstOrDefaultAsync(e => e.Id_Distribution == idDistribution);
            if (distribution == null)
            {
                return false;
            }

            distribution.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
