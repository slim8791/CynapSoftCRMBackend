using AutoMapper;
using CynapCRM.Services.DocAPI.Data;
using CynapCRM.Services.DocAPI.Models;
using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.DocAPI.Service
{
    public class BCService : IBCService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public BCService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<BonCommandeDto?> GetBonCommandeByIdAsync(int idBC)
        {
            var bc = await _db.BonsCommandes
                .OfType<BonCommande>()
                .AsNoTracking()
                // FIX: ajout filtre IsDeleted
                .FirstOrDefaultAsync(bc => bc.Numero_Doc == idBC && !bc.IsDeleted);

            return bc == null ? null : _mapper.Map<BonCommandeDto>(bc);
        }

        public async Task<IEnumerable<BonCommandeDto>> GetBonsCommandeByClientAsync(
            int idClient)
        {
            var bons = await _db.BonsCommandes
                .OfType<BonCommande>()
                .Where(bc => bc.Id_Client == idClient && !bc.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(bc => bc.DateCreation)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BonCommandeDto>>(bons);
        }

        // FIX: nouvelle méthode — BC par commande
        public async Task<IEnumerable<BonCommandeDto>> GetBonsCommandeByCommandeAsync(
            int idCommande)
        {
            var bons = await _db.BonsCommandes
                .Where(bc => bc.Id_Commande == idCommande && !bc.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(bc => bc.DateCreation)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BonCommandeDto>>(bons);
        }

        public async Task<IEnumerable<BonCommandeDto>> GetAllBonsCommandeAsync(
            int pageNumber, int pageSize)
        {
            // FIX: ajout filtre IsDeleted
            var bons = await _db.BonsCommandes
                .Where(bc => !bc.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(bc => bc.DateCreation)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BonCommandeDto>>(bons);
        }

        public async Task<IEnumerable<BonCommandeDto>> GetBonsCommandeByDateAsync(
            DateTime startDate, DateTime endDate)
        {
            // FIX: ajout filtre IsDeleted
            var bons = await _db.BonsCommandes
                .Where(bc =>
                    bc.DateCreation >= startDate &&
                    bc.DateCreation <= endDate &&
                    !bc.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(bc => bc.DateCreation)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BonCommandeDto>>(bons);
        }

        public async Task<BonCommandeDto?> CreateOrUpdateBonCommandeAsync(
            BonCommandeDto bcDto)
        {
            BonCommande bc;

            if (bcDto.Numero_Doc == 0)
            {
                bc = new BonCommande
                {
                    Nom_Doc = bcDto.Nom_Doc,
                    Id_Commande = bcDto.Id_Commande,
                    Id_Client = bcDto.Id_Client,
                    TypeDocument = "BC",
                    DateCreation = DateTime.UtcNow,
                    CloudinaryUrl = bcDto.CloudinaryUrl
                };
                _db.BonsCommandes.Add(bc);
            }
            else
            {
                bc = await _db.BonsCommandes
                    .FirstOrDefaultAsync(b => b.Numero_Doc == bcDto.Numero_Doc);

                if (bc == null) return null;

                bc.Nom_Doc = bcDto.Nom_Doc;
                bc.CloudinaryUrl = bcDto.CloudinaryUrl;
            }

            await _db.SaveChangesAsync();

            return new BonCommandeDto
            {
                Numero_Doc = bc.Numero_Doc,
                Nom_Doc = bc.Nom_Doc,
                DateCreation = bc.DateCreation,
                Id_Commande = bc.Id_Commande,
                Id_Client = bc.Id_Client,
                TypeDocument = "BC",
                CloudinaryUrl = bc.CloudinaryUrl
            };
        }

        // FIX: soft delete ajouté
        public async Task<bool> DeleteBonCommandeAsync(int idBC)
        {
            var bc = await _db.BonsCommandes.FindAsync(idBC);
            if (bc == null) return false;

            bc.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}