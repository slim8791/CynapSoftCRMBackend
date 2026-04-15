using CynapCRM.Services.DocAPI.Models.Dto;

namespace CynapCRM.Services.DocAPI.Service.IService
{
    public interface IDocumentService
    {
        // ===============================
        // 📌 DOCUMENTS GÉNÉRIQUES
        // ===============================
        Task<DocumentDto?> CreateUpdateDocumentAsync(DocumentDto docDto);
        Task<DocumentDto?> GetDocumentByIdAsync(int numeroDoc);
        Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(int pageNumber, int pageSize);
        Task<IEnumerable<DocumentDto>> GetDocumentsByCommandeAsync(int idCommande);
        Task<IEnumerable<DocumentDto>> GetDocumentsByClientAsync(int idClient);
        Task<bool> DeleteDocumentAsync(int numeroDoc);

        // ===============================
        // 📌 FACTURES
        // ===============================
        Task<FactureDto?> CreateUpdateFactureAsync(FactureDto factureDto);
        Task<FactureDto?> GetFactureByIdAsync(int idFacture);
        Task<IEnumerable<FactureDto>> GetFacturesByClientAsync(int idClient);

        // ===============================
        // 📌 BONS DE COMMANDE
        // ===============================
        Task<BonCommandeDto?> CreateUpdateBonCommandeAsync(BonCommandeDto bcDto);
        Task<BonCommandeDto?> GetBonCommandeByIdAsync(int idBC);
        Task<IEnumerable<BonCommandeDto>> GetBonsCommandeByClientAsync(int idClient);

        // ===============================
        // 📌 BONS DE LIVRAISON
        // ===============================
        Task<BonLivraisonDto?> CreateUpdateBonLivraisonAsync(BonLivraisonDto blDto);
        Task<BonLivraisonDto?> GetBonLivraisonByIdAsync(int idBL);
        Task<IEnumerable<BonLivraisonDto>> GetBonsLivraisonByClientAsync(int idClient);
    }
}
