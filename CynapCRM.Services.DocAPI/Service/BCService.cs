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
                .FirstOrDefaultAsync(bc => bc.Id_BC == idBC);
            if (bonCommande == null)
            {
                return null;
            }
            return _mapper.Map<BonCommandeDto>(bonCommande);
        }
        public async Task<BonCommandeDto?> CreateOrUpdateBonCommandeAsync(BonCommandeDto bcDto)
        {
            var entity = _mapper.Map<BonCommande>(bcDto);

            // Si le document existe déjà, on met à jour
            var existing = await _db.Documents
                .FirstOrDefaultAsync(d => d.Numero_Doc == bcDto.Numero_Doc);

            if (existing == null)
            {
                _db.Documents.Add(entity);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<BonCommandeDto>(entity);
        }

        public Task<IEnumerable<BonCommandeDto>> GetAllBonsCommandeAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BonCommandeDto>> GetBonsCommandeByDateAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }
    }
}
