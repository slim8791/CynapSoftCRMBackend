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

        public async Task<bool> CreateOrUpdateEchantillonAsync(Echantillon echantillon)
        {
            var distribution = await _db.Echantillons
                .FirstOrDefaultAsync(e => e.Id_Distribution == echantillon.Id_Distribution);

            if (distribution == null)
            {
                echantillon.DateDistribution = DateTime.UtcNow;
                echantillon.IsDeleted = false;
                _db.Echantillons.Add(echantillon); // ✅ une seule fois
            }
            else
            {
                _mapper.Map(echantillon, distribution); // ✅ update
            }

            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<EchantillonDto?> GetEchantillonByIdAsync(int idDistribution)
        {

            var distribution = await _db.Echantillons.AsNoTracking()
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
        public async Task<IEnumerable<EchantillonDto>> GetAllDistributionsAsync(
    int pageNumber, int pageSize)
        {
            var distributions = await _db.Echantillons
                .AsNoTracking()
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.DateDistribution)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<EchantillonDto>>(distributions);
        }
        public async Task<IEnumerable<EchantillonDto>> GetDistributionsByDelegueAsync(int idDelegue)
        {
            var distributions = await _db.Echantillons
                            .AsNoTracking()
                            .Where(e =>
                                        e.Id_Delegue == idDelegue && 
                                        !e.IsDeleted)
                            .OrderByDescending(e => e.DateDistribution)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<EchantillonDto>>(distributions);
        }
    }
}
