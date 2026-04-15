using AutoMapper;
using CynapCRM.Services.DocAPI.Data;
using CynapCRM.Services.DocAPI.Models;
using CynapCRM.Services.DocAPI.Models.Dto;
using CynapCRM.Services.DocAPI.Service.IService;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;

namespace CynapCRM.Services.DocAPI.Service
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public DocumentService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<DocumentDto?> GetDocumentByIdAsync(int numeroDoc)
        {
            var doc = await _db.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Numero_Doc == numeroDoc);
            if (doc == null)
            {
                return null;
            }
            return _mapper.Map<DocumentDto>(doc);
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByClientAsync(int idClient)
        {
            var doc = await _db.Documents
                        .Where(d => d.Id_Client == idClient)
                        .AsNoTracking()
                        .OrderByDescending(d => d.DateCreation)
                        .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(doc);
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByCommandeAsync(int idCommande)
        {
            var doc = await _db.Documents
                        .Where(d => d.Id_Commande == idCommande).AsNoTracking()
                        .OrderByDescending(d => d.DateCreation)
                        .ToListAsync();
            return _mapper.Map<IEnumerable<DocumentDto>>(doc);
        }
        

        public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(int pageNumber, int pageSize)
        {
            var docs = await _db.Documents
                        .OrderByDescending(d => d.DateCreation).AsNoTracking()
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(docs);
        }
        public async Task<DocumentDto?> CreateUpdateDocumentAsync(DocumentDto docDto)
        {
            var entity = _mapper.Map<Document>(docDto);

            // Si le document existe déjà, on met à jour
            var existing = await _db.Documents
                .FirstOrDefaultAsync(d => d.Numero_Doc == docDto.Numero_Doc);

            if (existing == null)
            {
                _db.Documents.Add(entity);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<DocumentDto>(entity);
        }
        public async Task<bool> DeleteDocumentAsync(int numeroDoc)
        {
            var doc = await _db.Documents.FindAsync(numeroDoc);
            if (doc == null)
            {
                return false;
            }

            _db.Documents.Remove(doc);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<FactureDto?> GetFactureByIdAsync(int idFacture)
        {
            var doc = await _db.Factures
                .OfType<Facture>().AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id_Facture == idFacture);
            if (doc == null)
            {
                return null;
            }
            return _mapper.Map<FactureDto>(doc);
        }

        public async Task<IEnumerable<FactureDto>> GetFacturesByClientAsync(int idClient)
        {
            var factures = await _db.Factures
                    .Where(f => f.Id_Client == idClient).AsNoTracking()
                    .OrderByDescending(f => f.DateCreation)
                    .ToListAsync();
            if (!factures.Any())
            {
                return Enumerable.Empty<FactureDto>();
            }

            return _mapper.Map<IEnumerable<FactureDto>>(factures);
        }
        public async Task<FactureDto?> CreateUpdateFactureAsync(FactureDto factureDto)
        {
            var entity = _mapper.Map<Facture>(factureDto);

            // Si le document existe déjà, on met à jour
            var existing = await _db.Documents
                .FirstOrDefaultAsync(d => d.Numero_Doc == factureDto.Numero_Doc);

            if (existing == null)
            {
                _db.Documents.Add(entity);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<FactureDto>(entity);
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
        public async Task<BonCommandeDto?> CreateUpdateBonCommandeAsync(BonCommandeDto bcDto)
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
        public async Task<BonLivraisonDto?> CreateUpdateBonLivraisonAsync(BonLivraisonDto blDto)
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

    }
}
