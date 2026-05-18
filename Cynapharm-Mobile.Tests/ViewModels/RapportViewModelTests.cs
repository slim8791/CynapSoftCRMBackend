using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Rapports;

namespace Cynapharm_Mobile.Tests.ViewModels;

/// <summary>
/// Tests RapportViewModel validation logic and observable state.
/// LocalDatabaseService is passed as null: the constructor does not call it,
/// and it is only used in relay commands not exercised here.
/// </summary>
public class RapportViewModelTests
{
    public RapportViewModelTests()
    {
        // Suppress unobserved task exceptions from fire-and-forget background inits
        TaskScheduler.UnobservedTaskException += (_, e) => e.SetObserved();
    }

    private static RapportViewModel CreateViewModel()
    {
        var api            = new ApiService(new HttpClient());
        var visiteService  = new VisiteService(api);
        var productService = new ProductService(api);
        // null! is safe: RapportViewModel constructor does not call localDb
        return new RapportViewModel(visiteService, productService, null!);
    }

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void Title_IsSetOnConstruction()
    {
        var vm = CreateViewModel();
        Assert.Equal("Rapport de visite", vm.Title);
    }

    [Fact]
    public void IsBusy_IsFalseOnConstruction()
    {
        var vm = CreateViewModel();
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public void ResultatOptions_ContainsThreeExpectedValues()
    {
        var vm = CreateViewModel();
        Assert.Equal(new[] { "POSITIF", "NEGATIF", "EN_ATTENTE" }, vm.ResultatOptions);
    }

    [Fact]
    public void CanSubmit_IsTrueInitially_BeforeValidationRuns()
    {
        // Validation only triggers on property set, not on the initial field value.
        var vm = CreateViewModel();
        Assert.True(vm.CanSubmit);
    }

    // ── Validation — empty Contenu ────────────────────────────────────────────

    [Fact]
    public void ContenuError_ShowsRequiredMessage_WhenContenuSetToEmpty()
    {
        var vm = CreateViewModel();
        vm.Contenu = "some text";      // change from empty so setter fires
        vm.Contenu = string.Empty;     // triggers [Required] validation
        Assert.Equal("Le contenu du rapport est requis.", vm.ContenuError);
    }

    [Fact]
    public void CanSubmit_IsFalse_WhenContenuFailsValidation()
    {
        var vm = CreateViewModel();
        vm.Contenu = "some text";
        vm.Contenu = string.Empty;
        Assert.False(vm.CanSubmit);
    }

    // ── Validation — short Contenu ────────────────────────────────────────────

    [Fact]
    public void ContenuError_ShowsMinLengthMessage_WhenContenuIsTooShort()
    {
        var vm = CreateViewModel();
        vm.Contenu = "moins de 20"; // 11 chars < 20
        Assert.Equal("Le rapport doit contenir au moins 20 caractères.", vm.ContenuError);
    }

    // ── Validation — valid Contenu ────────────────────────────────────────────

    [Fact]
    public void ContenuError_IsEmpty_AndCanSubmit_WhenContenuMeetsMinLength()
    {
        var vm = CreateViewModel();
        vm.Contenu = "Contenu valide avec plus de vingt caractères."; // 46 chars
        Assert.Equal(string.Empty, vm.ContenuError);
        Assert.True(vm.CanSubmit);
    }
}
