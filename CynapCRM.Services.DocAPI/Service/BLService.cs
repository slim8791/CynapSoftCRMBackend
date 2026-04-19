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
                .FirstOrDefaultAsync(bl => bl.Id_BL == idBL);
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
            var entity = _mapper.Map<BonLivraison>(blDto);

            // Si le document existe déjà, on met à jour
            var existing = await _db.Documents
                .FirstOrDefaultAsync(d => d.Numero_Doc == blDto.Numero_Doc);

            if (existing == null)
            {
                _db.Documents.Add(entity);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<BonLivraisonDto>(entity);
        }

        public Task<IEnumerable<BonLivraisonDto>> GetAllBonsLivraisonAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BonLivraisonDto>> GetBonsLivraisonByDateAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }
    }
}
