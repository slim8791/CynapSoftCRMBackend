using CynapCRM.Services.DocAPI.Models.Dto;

namespace CynapCRM.Services.DocAPI.Service.IService
{
    public interface IDocumentService
    {
        Task<IEnumerable<DocumentDto>> GetDocumentsByClientAndTypeAsync(
            int idClient,
            string typeDocument);
        Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(
            string typeDocument,
            int pageNumber,
            int pageSize);
        Task<DocumentDto?> CreateOrUpdateDocumentAsync(DocumentDto docDto);
        Task<DocumentDto?> GetDocumentByIdAsync(int numeroDoc);
        Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<DocumentDto>> GetDocumentsByCommandeAsync(int idCommande);
        Task<IEnumerable<DocumentDto>> GetDocumentsByClientAsync(int idClient);
        Task<bool> DeleteDocumentAsync(int numeroDoc);

    }
}
