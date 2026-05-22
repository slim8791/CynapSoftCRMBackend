using Cynapharm_Mobile.Models.Products;

namespace Cynapharm_Mobile.Services;

public class ProductService
{
    private readonly ApiService _api;
    public ProductService(ApiService api) { _api = api; }

    private static readonly HashSet<string> _imageExts =
        new(StringComparer.OrdinalIgnoreCase) { "jpg", "png", "webp" };

    private static string? ExtractImageUrl(Product p) =>
        p.Supports?
            .FirstOrDefault(s => s.IsActive &&
                string.Equals(s.Type, "Image", StringComparison.OrdinalIgnoreCase))
            ?.Fichiers?
            .FirstOrDefault(f => _imageExts.Contains(f.Extension))
            ?.Url;

    // GET /api/products        — returns IEnumerable<ProduitDto> (no pagination)
    // GET /api/products/search — keyword search, respects isActive + limit
    //
    // The backend wraps both in ResponseDto { result, isSuccess, message }.
    // ApiService.HandleResponseAsync unwraps that automatically.

    /// <summary>
    /// Returns all active products when <paramref name="search"/> is null/empty,
    /// or the top <paramref name="limit"/> keyword matches otherwise.
    /// Both paths are accessible to every authenticated role.
    /// </summary>
    public async Task<List<Product>?> GetProductsAsync(string? search = null, int limit = 100)
    {
        List<Product>? result;
        if (!string.IsNullOrWhiteSpace(search))
            result = await _api.GetAsync<List<Product>>(
                $"products/search?keyword={Uri.EscapeDataString(search)}&isActive=true&limit={limit}");
        else
            result = await _api.GetAsync<List<Product>>("products");

        if (result != null)
            foreach (var p in result)
                p.ImageUrl ??= ExtractImageUrl(p);

        return result;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var product = await _api.GetAsync<Product>($"products/{id}");
        if (product != null)
            product.ImageUrl ??= ExtractImageUrl(product);
        return product;
    }

    public Task<List<Lot>?> GetLotsByProductAsync(int productId)
        => _api.GetAsync<List<Lot>>($"products/lots/{productId}");

    public Task<List<Promotion>?> GetPromotionsAsync(int? productId)
    {
        var url = "products/promos";
        if (productId.HasValue) url += $"?productId={productId.Value}";
        return _api.GetAsync<List<Promotion>>(url);
    }

    /// <summary>GET products/visible — active, non-archived products only (MEDECIN endpoint).</summary>
    public async Task<List<Product>?> GetVisibleProductsAsync()
    {
        var result = await _api.GetAsync<List<Product>>("products/visible");
        if (result != null)
            foreach (var p in result)
                p.ImageUrl ??= ExtractImageUrl(p);
        return result;
    }

    public Task<List<string>?> GetCategoriesAsync()
        => _api.GetAsync<List<string>>("products/categories");

    public Task<object?> GetMarketingAsync(int? productId)
    {
        var url = "products/marketing";
        if (productId.HasValue) url += $"?productId={productId.Value}";
        return _api.GetAsync<object>(url);
    }

    public Task<byte[]?> DownloadFileAsync(string url)
        => _api.DownloadFileAsync(url);
}
