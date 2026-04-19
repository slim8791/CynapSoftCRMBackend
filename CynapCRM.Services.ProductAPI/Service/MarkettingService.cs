using AutoMapper;
using CynapCRM.Services.ProductAPI.Data;
using CynapCRM.Services.ProductAPI.Models;
using CynapCRM.Services.ProductAPI.Models.Dto;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.EntityFrameworkCore;
namespace CynapCRM.Services.ProductAPI.Service
{
    public class MarkettingService : IMarkettingService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public MarkettingService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // 🔹 Supports marketing

        public async Task<IEnumerable<SupportMarketingDto>> GetSupportsByProductAsync(int productId)
        {
            var supports = await _db.Support_Markettings
                .Where(s => s.Id_Produit == productId)
                .Include(s => s.Fichiers)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SupportMarketingDto>>(supports);
        }

        public async Task<SupportMarketingDto?> GetSupportByIdAsync(int supportId)
        {
            var support = await _db.Support_Markettings
                .Include(s => s.Fichiers)
                .FirstOrDefaultAsync(s => s.Id_SupportMarketting == supportId);

            return support == null ? null : _mapper.Map<SupportMarketingDto>(support);
        }

        public async Task<SupportMarketingDto> CreateOrUpdateSupportAsync(SupportMarketingDto supportDto)
        {

            var support = await _db.Support_Markettings
                            .FirstOrDefaultAsync(s => s.Id_SupportMarketting == supportDto.Id_SupportMarketting);

            if (support == null)
            {
                support = _mapper.Map<Support_Marketting>(supportDto);
                _db.Support_Markettings.Add(support);
            }
            else
            {
                _mapper.Map(supportDto, support);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<SupportMarketingDto>(support);

        }

        public async Task<bool> DisableSupportAsync(int supportId)
        {
            var support = await _db.Support_Markettings.FindAsync(supportId);
            if (support == null) return false;

            // Suppression logique (champ recommandé)
            support.IsActive = false;

            await _db.SaveChangesAsync();
            return true;
        }


        // 🔹 Fichiers marketing

        public async Task<FichierDto> AddFileToSupportAsync(FichierDto fichierDto)
        {
            var supportExists = await _db.Support_Markettings
                .AnyAsync(s => s.Id_SupportMarketting == fichierDto.Id_Support);

            if (!supportExists)
                throw new Exception("Support marketing introuvable.");

            var fichier = _mapper.Map<Fichier>(fichierDto);
            _db.Fichiers.Add(fichier);

            await _db.SaveChangesAsync();
            return _mapper.Map<FichierDto>(fichier);
        }


        public async Task<bool> DeleteFileAsync(int fichierId)
        {
            var fichier = await _db.Fichiers.FindAsync(fichierId);
            if (fichier == null) return false;

            _db.Fichiers.Remove(fichier);
            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<FichierDto>> GetFilesBySupportAsync(int supportId)
        {
            var fichiers = await _db.Fichiers
                .Where(f => f.Id_Support == supportId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<FichierDto>>(fichiers);
        }


        // 🔹 Visibilité & logique métier

        public async Task<bool> IsSupportActiveAsync(int supportId)
        {
            var support = await _db.Support_Markettings.FindAsync(supportId);
            return support != null && support.IsActive;
        }

        public async Task<IEnumerable<SupportMarketingDto>> GetVisibleSupportsByProductAsync(int productId)
        {
            var supports = await _db.Support_Markettings
                .Where(s =>
                    s.Id_Produit == productId &&
                    s.IsActive)
                .Include(s => s.Fichiers)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SupportMarketingDto>>(supports);
        }

        // ==================================================
        // 🔹 Campagnes marketing
        // ==================================================

        public async Task<IEnumerable<SupportMarketingDto>> GetSupportsByCampaignAsync(string campaignName)
        {
            var supports = await _db.Support_Markettings
                .Where(s => s.CampaignName == campaignName && s.IsActive)
                .Include(s => s.Fichiers)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SupportMarketingDto>>(supports);
        }

        public async Task<IEnumerable<string>> GetCampaignsAsync()
        {
            return await _db.Support_Markettings
                .Where(s => s.CampaignName != null)
                .Select(s => s.CampaignName!)
                .Distinct()
                .ToListAsync();
        }
    }
}
