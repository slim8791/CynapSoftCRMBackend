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
        public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(
            int pageNumber, int pageSize)
        {
            // FIX: ajout filtre IsDeleted
            var docs = await _db.Documents
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.DateCreation)
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(docs);
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(int numeroDoc)
        {
            var doc = await _db.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.Numero_Doc == numeroDoc &&
                    !d.IsDeleted);

            return doc == null ? null : _mapper.Map<DocumentDto>(doc);
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByClientAsync(
            int idClient)
        {
            var docs = await _db.Documents
                .Where(d => d.Id_Client == idClient && !d.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(docs);
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByCommandeAsync(
            int idCommande)
        {
            var docs = await _db.Documents
                .Where(d => d.Id_Commande == idCommande && !d.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(docs);
        }

        // FIX: nouvelle méthode — filtre par type
        public async Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(
            string typeDocument,
            int pageNumber,
            int pageSize)
        {
            var docs = await _db.Documents
                .Where(d => d.TypeDocument == typeDocument && !d.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(d => d.DateCreation)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(docs);
        }

        // FIX: nouvelle méthode — filtre par client + type
        public async Task<IEnumerable<DocumentDto>> GetDocumentsByClientAndTypeAsync(
            int idClient,
            string typeDocument)
        {
            var docs = await _db.Documents
                .Where(d =>
                    d.Id_Client == idClient &&
                    d.TypeDocument == typeDocument &&
                    !d.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(docs);
        }

        public async Task<DocumentDto?> CreateOrUpdateDocumentAsync(DocumentDto dto)
        {
            Document document;

            if (dto.Numero_Doc == 0)
            {
                document = new Document
                {
                    Nom_Doc = dto.Nom_Doc,
                    Id_Commande = dto.Id_Commande,
                    Id_Client = dto.Id_Client,
                    TypeDocument = "GENERIC",
                    DateCreation = DateTime.UtcNow
                };
                _db.Documents.Add(document);
            }
            else
            {
                document = await _db.Documents
                    .FirstOrDefaultAsync(d => d.Numero_Doc == dto.Numero_Doc);

                if (document == null) return null;

                document.Nom_Doc = dto.Nom_Doc;
            }

            await _db.SaveChangesAsync();

            return new DocumentDto
            {
                Numero_Doc = document.Numero_Doc,
                Nom_Doc = document.Nom_Doc,
                DateCreation = document.DateCreation,
                Id_Commande = document.Id_Commande,
                Id_Client = document.Id_Client,
                TypeDocument = document.TypeDocument
            };
        }

        // FIX: soft delete au lieu de suppression physique
        public async Task<bool> DeleteDocumentAsync(int numeroDoc)
        {
            var doc = await _db.Documents.FindAsync(numeroDoc);
            if (doc == null) return false;

            doc.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}