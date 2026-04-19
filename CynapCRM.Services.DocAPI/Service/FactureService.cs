using AutoMapper;
using CynapCRM.Services.DocAPI.Data;
using CynapCRM.Services.DocAPI.Models;
using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.DocAPI.Service
{
    public class FactureService : IFactureService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public FactureService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<FactureDto?> GetFactureByIdAsync(int idFacture)
        {
            var doc = await _db.Factures
                .OfType<Facture>().AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id_Facture == idFacture && !f.IsDeleted);
            if (doc == null)
            {
                return null;
            }
            return _mapper.Map<FactureDto>(doc);
        }

        public async Task<IEnumerable<FactureDto>> GetFacturesByClientAsync(int idClient)
        {
            var factures = await _db.Factures
                    .Where(f => f.Id_Client == idClient && !f.IsDeleted).AsNoTracking()
                    .OrderByDescending(f => f.DateCreation)
                    .ToListAsync();
            if (!factures.Any())
            {
                return Enumerable.Empty<FactureDto>();
            }

            return _mapper.Map<IEnumerable<FactureDto>>(factures);
        }
        public async Task<FactureDto?> CreateOrUpdateFactureAsync(FactureDto factureDto)
        {

            var facture = await _db.Factures
                            .FirstOrDefaultAsync(f => f.Id_Facture == factureDto.Id_Facture);

            if (facture == null)
            {
                facture = _mapper.Map<Facture>(factureDto);
                facture.DateCreation = DateTime.UtcNow;

                _db.Factures.Add(facture);
            }
            else
            {
                _mapper.Map(factureDto, facture);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<FactureDto>(facture);

        }

        public async Task<IEnumerable<FactureDto>> GetAllFacturesAsync(int pageNumber, int pageSize)
        {

            var factures = await _db.Factures
                            .AsNoTracking()
                            .Where(f => !f.IsDeleted)
                            .OrderByDescending(f => f.DateCreation)
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<FactureDto>>(factures);
        }

        public async Task<IEnumerable<FactureDto>> GetFacturesByDateAsync(DateTime startDate, DateTime endDate)
        {

            var factures = await _db.Factures
                            .AsNoTracking()
                            .Where(f =>
                                f.DateCreation >= startDate &&
                                f.DateCreation <= endDate &&
                                !f.IsDeleted)
                            .OrderByDescending(f => f.DateCreation)
                            .ToListAsync();

            return _mapper.Map<IEnumerable<FactureDto>>(factures);
        }
    }
}
