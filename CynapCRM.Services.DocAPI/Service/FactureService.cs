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
                .OfType<Facture>()
                .AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    f.Numero_Doc == idFacture &&
                    !f.IsDeleted);

            return doc == null ? null : _mapper.Map<FactureDto>(doc);
        }

        public async Task<IEnumerable<FactureDto>> GetFacturesByClientAsync(
            int idClient)
        {
            var factures = await _db.Factures
                .Where(f => f.Id_Client == idClient && !f.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();

            return _mapper.Map<IEnumerable<FactureDto>>(factures);
        }

        // FIX: nouvelle méthode — factures par commande
        public async Task<IEnumerable<FactureDto>> GetFacturesByCommandeAsync(
            int idCommande)
        {
            var factures = await _db.Factures
                .Where(f => f.Id_Commande == idCommande && !f.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();

            return _mapper.Map<IEnumerable<FactureDto>>(factures);
        }

        public async Task<IEnumerable<FactureDto>> GetAllFacturesAsync(
            int pageNumber, int pageSize)
        {
            var factures = await _db.Factures
                .Where(f => !f.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(f => f.DateCreation)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<FactureDto>>(factures);
        }

        public async Task<IEnumerable<FactureDto>> GetFacturesByDateAsync(
            DateTime startDate, DateTime endDate)
        {
            var factures = await _db.Factures
                .Where(f =>
                    f.DateCreation >= startDate &&
                    f.DateCreation <= endDate &&
                    !f.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(f => f.DateCreation)
                .ToListAsync();

            return _mapper.Map<IEnumerable<FactureDto>>(factures);
        }

        public async Task<FactureDto?> CreateOrUpdateFactureAsync(FactureDto factureDto)
        {
            Facture facture;

            if (factureDto.Numero_Doc == 0)
            {
                facture = new Facture
                {
                    Nom_Doc = factureDto.Nom_Doc,
                    Id_Commande = factureDto.Id_Commande,
                    Id_Client = factureDto.Id_Client,
                    TypeDocument = "FACTURE",
                    DateCreation = DateTime.UtcNow,
                    MontantHT = factureDto.MontantHT,
                    MontantTTC = factureDto.MontantTTC,
                    DateFacture = factureDto.DateFacture,
                    CloudinaryUrl = factureDto.CloudinaryUrl
                };
                _db.Factures.Add(facture);
            }
            else
            {
                facture = await _db.Factures
                    .FirstOrDefaultAsync(f => f.Numero_Doc == factureDto.Numero_Doc);

                if (facture == null) return null;

                facture.Nom_Doc = factureDto.Nom_Doc;
                facture.MontantHT = factureDto.MontantHT;
                facture.MontantTTC = factureDto.MontantTTC;
                facture.DateFacture = factureDto.DateFacture;
                facture.CloudinaryUrl = factureDto.CloudinaryUrl;
            }

            await _db.SaveChangesAsync();

            return new FactureDto
            {
                Numero_Doc = facture.Numero_Doc,
                Nom_Doc = facture.Nom_Doc,
                DateCreation = facture.DateCreation,
                Id_Commande = facture.Id_Commande,
                Id_Client = facture.Id_Client,
                TypeDocument = "FACTURE",
                MontantHT = facture.MontantHT,
                MontantTTC = facture.MontantTTC,
                DateFacture = facture.DateFacture,
                CloudinaryUrl = facture.CloudinaryUrl
            };
        }

        // FIX: soft delete ajouté
        public async Task<bool> DeleteFactureAsync(int idFacture)
        {
            var facture = await _db.Factures.FindAsync(idFacture);
            if (facture == null) return false;

            facture.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
