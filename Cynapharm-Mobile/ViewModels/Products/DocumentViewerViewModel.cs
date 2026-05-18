using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Base;

namespace Cynapharm_Mobile.ViewModels.Products;

public partial class DocumentViewerViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ProductService _productService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsImage), nameof(PdfViewerUrl))]
    private string? _url;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsImage), nameof(PdfViewerUrl))]
    private string? _extension;

    [ObservableProperty] private string? _nomFichier;

    public bool IsImage =>
        Extension?.ToLowerInvariant() is "jpg" or "jpeg" or "png" or "webp" or "gif";

    public string PdfViewerUrl
    {
        get
        {
            if (IsImage || string.IsNullOrWhiteSpace(Url)) return string.Empty;
            return $"https://docs.google.com/viewer?url={Uri.EscapeDataString(Url)}&embedded=true";
        }
    }

    public DocumentViewerViewModel(ProductService productService)
    {
        _productService = productService;
        Title = "Document";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Url        = query.TryGetValue("url",        out var u) ? Uri.UnescapeDataString(u?.ToString() ?? "") : null;
        NomFichier = query.TryGetValue("nomFichier", out var n) ? Uri.UnescapeDataString(n?.ToString() ?? "") : null;
        Extension  = query.TryGetValue("extension",  out var e) ? Uri.UnescapeDataString(e?.ToString() ?? "") : null;

        if (!string.IsNullOrWhiteSpace(NomFichier))
            Title = NomFichier;
    }

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task OpenExternalAsync()
    {
        if (string.IsNullOrWhiteSpace(Url)) return;
        try
        {
            var downloadUrl = Url.Contains("/raw/upload/")
                ? Url.Replace("/raw/upload/", "/raw/upload/fl_attachment/")
                : Url;

            var bytes = await _productService.DownloadFileAsync(downloadUrl);
            if (bytes == null || bytes.Length == 0) return;

            var fileName = string.IsNullOrWhiteSpace(NomFichier) ? "document" : NomFichier;
            var localPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(localPath, bytes);
            await Launcher.OpenAsync(
                new OpenFileRequest(fileName, new ReadOnlyFile(localPath, MimeTypeFor(fileName))));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DocumentViewer] OpenExternal: {ex.Message}");
        }
    }

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
}
