using Cynapharm_Mobile.Models.Documents;

namespace Cynapharm_Mobile.Services;

public class DocumentService
{
    private readonly ApiService _api;
    public DocumentService(ApiService api) { _api = api; }

    public Task<List<Facture>?> GetFacturesAsync(int page = 1, int size = 20)
        => _api.GetAsync<List<Facture>>($"documents/factures?pageNumber={page}&pageSize={size}");

    public Task<Facture?> GetFactureByIdAsync(int id)
        => _api.GetAsync<Facture>($"documents/factures/{id}");

    public Task<List<BonCommande>?> GetBonsCommandeAsync(int page = 1, int size = 20)
        => _api.GetAsync<List<BonCommande>>($"documents/bons-commandes?pageNumber={page}&pageSize={size}");

    public Task<BonCommande?> GetBonCommandeByIdAsync(int id)
        => _api.GetAsync<BonCommande>($"documents/bons-commandes/{id}");

    public Task<List<BonLivraison>?> GetBonsLivraisonAsync(int page = 1, int size = 20)
        => _api.GetAsync<List<BonLivraison>>($"documents/bons-livraison?pageNumber={page}&pageSize={size}");

    public Task<BonLivraison?> GetBonLivraisonByIdAsync(int id)
        => _api.GetAsync<BonLivraison>($"documents/bons-livraison/{id}");

    // ── Unified client+type endpoint (replaces 3 separate per-type calls) ────

    /// <summary>
    /// GET documents/client/{idClient}/type/{type}
    /// type: "FACTURE", "BC", or "BL"
    /// </summary>
    public Task<List<DocumentSummary>?> GetDocumentsByClientAndTypeAsync(int idClient, string type)
        => _api.GetAsync<List<DocumentSummary>>(
            string.Format(ApiRoutes.Documents.DocumentsByClientAndType, idClient, type));

    // ── Documents linked to a specific order ─────────────────────────────────

    public Task<List<Facture>?> GetFacturesByCommandeAsync(int idCommande)
        => _api.GetAsync<List<Facture>>($"{ApiRoutes.Documents.FacturesByCommande}/{idCommande}");

    public Task<List<BonCommande>?> GetBCByCommandeAsync(int idCommande)
        => _api.GetAsync<List<BonCommande>>($"{ApiRoutes.Documents.BCByCommande}/{idCommande}");

    public Task<List<BonLivraison>?> GetBLByCommandeAsync(int idCommande)
        => _api.GetAsync<List<BonLivraison>>($"{ApiRoutes.Documents.BLByCommande}/{idCommande}");

    /// <summary>Downloads raw bytes from a Cloudinary (or any public) URL.</summary>
    public Task<byte[]?> DownloadFileAsync(string url)
        => _api.DownloadFileAsync(url);
}
