using AutoMapper;
using CynapCRM.Services.DocAPI.Data;
using CynapCRM.Services.DocAPI.Models;
using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.DocAPI.Service
{
    public class BLService : IBLService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public BLService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<BonLivraisonDto?> GetBonLivraisonByIdAsync(int idBL)
        {
            var bonLivraison = await _db.BonsLivraisons
                .OfType<BonLivraison>().AsNoTracking()
                .FirstOrDefaultAsync(bl => bl.Numero_Doc == idBL);
            if (bonLivraison == null)
            {
                return null;
            }
            return _mapper.Map<BonLivraisonDto>(bonLivraison);
        }


        public async Task<IEnumerable<BonLivraisonDto>> GetBonsLivraisonByClientAsync(int idClient)
        {
            var bonsLivraison = await _db.BonsLivraisons
                    .Where(bl => bl.Id_Client == idClient).AsNoTracking()
                    .OrderByDescending(bl => bl.DateCreation)
                    .ToListAsync();

            if (!bonsLivraison.Any())
            {
                return Enumerable.Empty<BonLivraisonDto>();
            }

            return _mapper.Map<IEnumerable<BonLivraisonDto>>(bonsLivraison);
        }
        public async Task<BonLivraisonDto?> CreateOrUpdateBonLivraisonAsync(BonLivraisonDto blDto)
        {
            BonLivraison bl;

            if (blDto.Numero_Doc == 0)
            {
                bl = new BonLivraison
                {
                    // Fields inherited from Document
                    Nom_Doc = blDto.Nom_Doc,
                    Id_Commande = blDto.Id_Commande,
                    Id_Client = blDto.Id_Client,
                    TypeDocument = "BL",
                    DateCreation = DateTime.UtcNow
                };

                _db.BonsLivraisons.Add(bl);
            }

            else
            {
                bl = await _db.BonsLivraisons.FirstOrDefaultAsync(b => b.Numero_Doc == blDto.Numero_Doc);

                if (bl == null)
                    return null;

                // Modifiable fields
                bl.Nom_Doc = blDto.Nom_Doc;

            }

            await _db.SaveChangesAsync();
            return new BonLivraisonDto
            {
                Numero_Doc = bl.Numero_Doc,
                Nom_Doc = bl.Nom_Doc,
                DateCreation = bl.DateCreation,
                Id_Commande = bl.Id_Commande,
                Id_Client = bl.Id_Client,
                TypeDocument = "BL"
            };
        }

        public async Task<IEnumerable<BonLivraisonDto>> GetAllBonsLivraisonAsync(int pageNumber, int pageSize)
        {

            var bons = await _db.BonsLivraisons
                    .AsNoTracking()
                    .OrderByDescending(bl => bl.DateCreation)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            return _mapper.Map<IEnumerable<BonLivraisonDto>>(bons);
        }

        public async Task<IEnumerable<BonLivraisonDto>> GetBonsLivraisonByDateAsync(DateTime startDate, DateTime endDate)
        {

            var bons = await _db.BonsLivraisons
                    .AsNoTracking()
                    .Where(bl =>
                        bl.DateCreation >= startDate &&
                        bl.DateCreation <= endDate)
                    .OrderByDescending(bl => bl.DateCreation)
                    .ToListAsync();

            return _mapper.Map<IEnumerable<BonLivraisonDto>>(bons);
        }
    }
}
