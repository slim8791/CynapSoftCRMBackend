using System.Collections.ObjectModel;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Models.Products;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Products;

[QueryProperty(nameof(ProductId), "productId")]
public partial class ProductDetailViewModel : BaseViewModel
{
    private readonly ProductService _productService;

    [ObservableProperty] private int      _productId;
    [ObservableProperty] private Product? _product;
    [ObservableProperty] private bool     _canSeePrices = true;

    public ObservableCollection<Lot>            Lots       { get; } = new();
    public ObservableCollection<Promotion>      Promotions { get; } = new();
    public ObservableCollection<SupportMarketing> Supports { get; } = new();

    public bool HasSupports => Supports.Count > 0;

    public ProductDetailViewModel(ProductService productService)
    {
        _productService = productService;
        Title = "Produit";
        Supports.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasSupports));
    }

    partial void OnProductIdChanged(int value)
    {
        if (value > 0) _ = InitAsync();
    }

    private async Task InitAsync()
    {
        var role = await SecureStorage.GetAsync(StorageKeys.UserRole) ?? string.Empty;
        CanSeePrices = role is not "MEDECIN";
        await LoadAsync();
    }

    [RelayCommand]
    private Task LoadAsync() => ExecuteAsync(async () =>
    {
        if (!await CheckConnectivityAsync()) return;

        // Product load — errors here propagate to ErrorBanner as intended.
        var product = await _productService.GetProductByIdAsync(ProductId);
        if (product != null)
        {
            Product = product;
            Title   = product.Nom;

            Supports.Clear();
            if (product.Supports != null)
                foreach (var s in product.Supports.Where(s =>
                    s.IsActive &&
                    !string.Equals(s.Type, "Image", StringComparison.OrdinalIgnoreCase)))
                    Supports.Add(s);
        }

        // Lots and promotions: 404 for MEDECIN — silently skip, no error banner.
        try
        {
            var lots = await _productService.GetLotsByProductAsync(ProductId);
            Lots.Clear();
            if (lots != null) foreach (var l in lots) Lots.Add(l);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound
                                   || ex.StatusCode == HttpStatusCode.Forbidden) { }
        catch (Exception) { }

        try
        {
            var promos = await _productService.GetPromotionsAsync(ProductId);
            Promotions.Clear();
            if (promos != null) foreach (var p in promos) Promotions.Add(p);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound
                                   || ex.StatusCode == HttpStatusCode.Forbidden) { }
        catch (Exception) { }
    });

    protected override Task RetryAsync() => LoadAsync();

    [RelayCommand]
    private static Task GoBackAsync() => Shell.Current.GoToAsync("..");

    private static string MimeTypeFor(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf"  => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"  => "image/png",
            ".mp4"  => "video/mp4",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".ppt"  => "application/vnd.ms-powerpoint",
            _       => "application/octet-stream"
        };

    [RelayCommand]
    private async Task OpenDocumentAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        try
        {
            var downloadUrl = url.Contains("/raw/upload/")
                ? url.Replace("/raw/upload/", "/raw/upload/fl_attachment/")
                : url;

            var bytes = await _productService.DownloadFileAsync(downloadUrl);
            if (bytes == null || bytes.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("[OpenDoc] Download returned empty — check URL/auth");
                return;
            }

            var fileName = Path.GetFileName(new Uri(url).LocalPath);
            if (string.IsNullOrWhiteSpace(fileName)) fileName = "document";

            var localPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(localPath, bytes);

            // OpenFileRequest with an explicit MIME type sends Intent.ACTION_VIEW on
            // Android, which launches the correct viewer directly (no share sheet).
            await Launcher.OpenAsync(
                new OpenFileRequest(fileName, new ReadOnlyFile(localPath, MimeTypeFor(fileName))));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OpenDoc] {ex.GetType().Name}: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewDocumentAsync(Fichier? fichier)
    {
        if (fichier == null) return;
        await Shell.Current.GoToAsync(
            $"///products/detail/viewer" +
            $"?url={Uri.EscapeDataString(fichier.Url)}" +
            $"&nomFichier={Uri.EscapeDataString(fichier.NomFichier)}" +
            $"&extension={Uri.EscapeDataString(fichier.Extension)}");
    }

    [RelayCommand]
    private async Task AddToOrderAsync()
        => await Shell.Current.GoToAsync($"//orders/create?productId={ProductId}");
}
