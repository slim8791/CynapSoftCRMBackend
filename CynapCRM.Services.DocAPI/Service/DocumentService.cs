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
        public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(int pageNumber, int pageSize)
        {
            var docs = await _db.Documents
                        .OrderByDescending(d => d.DateCreation).AsNoTracking()
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(docs);
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(int numeroDoc)
        {
            var doc = await _db.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Numero_Doc == numeroDoc && !d.IsDeleted);
            if (doc == null)
            {
                return null;
            }
            return _mapper.Map<DocumentDto>(doc);
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByClientAsync(int idClient)
        {
            var doc = await _db.Documents
                        .Where(d => d.Id_Client == idClient && !d.IsDeleted)
                        .AsNoTracking()
                        .OrderByDescending(d => d.DateCreation)
                        .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(doc);
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByCommandeAsync(int idCommande)
        {
            var doc = await _db.Documents
                        .Where(d => d.Id_Commande == idCommande && !d.IsDeleted).AsNoTracking()
                        .OrderByDescending(d => d.DateCreation)
                        .ToListAsync();
            return _mapper.Map<IEnumerable<DocumentDto>>(doc);
        }
        

        
        public async Task<DocumentDto?> CreateOrUpdateDocumentAsync(DocumentDto docDto)
        {

            var document = await _db.Documents
                            .FirstOrDefaultAsync(d => d.Numero_Doc == docDto.Numero_Doc);

            if (document == null)
            {
                document = _mapper.Map<Document>(docDto);
                document.DateCreation = DateTime.UtcNow;

                _db.Documents.Add(document);
            }
            else
            {
                _mapper.Map(docDto, document);
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<DocumentDto>(document);

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
        


        
        

        

    }
}
