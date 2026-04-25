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

        public async Task<IEnumerable<BonCommandeDto>> GetBonsCommandeByClientAsync(int idClient)
        {
            var bonsCommande = await _db.BonsCommandes
                    .Where(bl => bl.Id_Client == idClient).AsNoTracking()
                    .OrderByDescending(bl => bl.DateCreation)
                    .ToListAsync();

            if (!bonsCommande.Any())
            {
                return Enumerable.Empty<BonCommandeDto>();
            }

            return _mapper.Map<IEnumerable<BonCommandeDto>>(bonsCommande);
        }


        public async Task<BonCommandeDto?> GetBonCommandeByIdAsync(int idBC)
        {
            var bonCommande = await _db.BonsCommandes
                .OfType<BonCommande>().AsNoTracking()
                .FirstOrDefaultAsync(bc => bc.Numero_Doc == idBC);
            if (bonCommande == null)
            {
                return null;
            }
            return _mapper.Map<BonCommandeDto>(bonCommande);
        }
        public async Task<BonCommandeDto?> CreateOrUpdateBonCommandeAsync(BonCommandeDto bcDto)
        {
            BonCommande bc;

            if (bcDto.Numero_Doc == 0)
            {
                bc = new BonCommande
                {
                    // Fields inherited from Document
                    Nom_Doc = bcDto.Nom_Doc,
                    Id_Commande = bcDto.Id_Commande,
                    Id_Client = bcDto.Id_Client,
                    TypeDocument = "BC",
                    DateCreation = DateTime.UtcNow
                };

                _db.BonsCommandes.Add(bc);
            }
            else
            {
                bc = await _db.BonsCommandes.FirstOrDefaultAsync(b => b.Numero_Doc == bcDto.Numero_Doc);

                if (bc == null)
                    return null;
                // Modifiable fields
                bc.Nom_Doc = bcDto.Nom_Doc;

                
            }

            await _db.SaveChangesAsync();

            // manual dto return
            return new BonCommandeDto
            {
                Numero_Doc = bc.Numero_Doc,
                Nom_Doc = bc.Nom_Doc,
                DateCreation = bc.DateCreation,
                Id_Commande = bc.Id_Commande,
                Id_Client = bc.Id_Client,
                TypeDocument = "BC"
            };
        }
        public async Task<IEnumerable<BonCommandeDto>> GetAllBonsCommandeAsync(int pageNumber, int pageSize)
        {

            var bons = await _db.BonsCommandes
                    .AsNoTracking()
                    .OrderByDescending(bc => bc.DateCreation)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            return _mapper.Map<IEnumerable<BonCommandeDto>>(bons);
        }

        public async Task<IEnumerable<BonCommandeDto>> GetBonsCommandeByDateAsync(DateTime startDate, DateTime endDate)
        {

            var bons = await _db.BonsCommandes
                    .AsNoTracking()
                    .Where(bc =>
                        bc.DateCreation >= startDate &&
                        bc.DateCreation <= endDate)
                    .OrderByDescending(bc => bc.DateCreation)
                    .ToListAsync();
            return _mapper.Map<IEnumerable<BonCommandeDto>>(bons);
        }
    }
}
