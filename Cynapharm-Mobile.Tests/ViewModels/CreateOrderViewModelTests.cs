using Cynapharm_Mobile.Models.Orders;
using Cynapharm_Mobile.Services;
using Cynapharm_Mobile.ViewModels.Orders;

namespace Cynapharm_Mobile.Tests.ViewModels;

/// <summary>
/// Tests CreateOrderViewModel pure business logic: cart arithmetic, step navigation,
/// and line management. LocalDatabaseService is passed as null because the constructor
/// does not call it; its usage is confined to relay commands not exercised here.
/// The background InitializeAsync fires Preferences/Connectivity accesses that will
/// silently fail in a headless runner — UnobservedTaskException is suppressed.
/// </summary>
public class CreateOrderViewModelTests
{
    public CreateOrderViewModelTests()
    {
        // Suppress unobserved task exceptions from fire-and-forget background inits
        TaskScheduler.UnobservedTaskException += (_, e) => e.SetObserved();
    }

    private static CreateOrderViewModel CreateViewModel()
    {
        var api            = new ApiService(new HttpClient());
        var orderService   = new OrderService(api);
        var productService = new ProductService(api);
        // null! is safe: constructor stores the reference but does not call it
        return new CreateOrderViewModel(orderService, productService, null!);
    }

    // ── Cart totals ───────────────────────────────────────────────────────────

    [Fact]
    public void CartTotal_IsZero_WhenCartLinesIsEmpty()
    {
        var vm = CreateViewModel();
        Assert.Equal(0m, vm.CartTotal);
    }

    [Fact]
    public void CartTotal_SumsSousTotal_AcrossAllLines()
    {
        var vm = CreateViewModel();
        vm.CartLines.Add(new CartLine { Quantite = 2, PrixUnitaire = 10m });
        vm.CartLines.Add(new CartLine { Quantite = 3, PrixUnitaire = 5m });
        Assert.Equal(35m, vm.CartTotal);
    }

    [Fact]
    public void CartSavings_IsZero_WhenNoPromoApplied()
    {
        var vm = CreateViewModel();
        vm.CartLines.Add(new CartLine { Quantite = 2, PrixOriginal = 10m, PrixUnitaire = 10m });
        Assert.Equal(0m, vm.CartSavings);
    }

    [Fact]
    public void CartSavings_SumsEconomieTotale_AcrossPromoLines()
    {
        var vm = CreateViewModel();
        // 10% off 100 → save 10 per unit × 2 = 20
        vm.CartLines.Add(new CartLine
        {
            Quantite          = 2,
            PrixOriginal      = 100m,
            PrixUnitaire      = 90m,
            RemisePourcentage = 10m
        });
        Assert.Equal(20m, vm.CartSavings);
    }

    [Fact]
    public void HasCartSavings_IsFalse_WhenCartSavingsIsZero()
    {
        var vm = CreateViewModel();
        Assert.False(vm.HasCartSavings);
    }

    [Fact]
    public void HasCartSavings_IsTrue_WhenCartSavingsIsPositive()
    {
        var vm = CreateViewModel();
        vm.CartLines.Add(new CartLine { Quantite = 1, PrixOriginal = 50m, PrixUnitaire = 40m, RemisePourcentage = 20m });
        Assert.True(vm.HasCartSavings);
    }

    // ── Step navigation ───────────────────────────────────────────────────────

    [Fact]
    public void IsStep1_IsTrue_OnInitialStep()
    {
        var vm = CreateViewModel();
        Assert.True(vm.IsStep1);
        Assert.False(vm.IsStep2);
        Assert.False(vm.IsStep3);
    }

    [Fact]
    public void NextStep_SetsErrorMessage_WhenCartIsEmptyOnStep1()
    {
        var vm = CreateViewModel();
        vm.NextStepCommand.Execute(null);
        Assert.Equal("Ajoutez au moins un produit.", vm.ErrorMessage);
        Assert.Equal(1, vm.CurrentStep);
    }

    [Fact]
    public void NextStep_AdvancesToStep2_WhenCartHasLines()
    {
        var vm = CreateViewModel();
        vm.CartLines.Add(new CartLine { Quantite = 1, PrixUnitaire = 10m });
        vm.NextStepCommand.Execute(null);
        Assert.Equal(2, vm.CurrentStep);
        Assert.True(vm.IsStep2);
    }

    [Fact]
    public void PreviousStep_DoesNothing_WhenAlreadyOnStep1()
    {
        var vm = CreateViewModel();
        vm.PreviousStepCommand.Execute(null);
        Assert.Equal(1, vm.CurrentStep);
    }

    [Fact]
    public void PreviousStep_DecrementsStep_WhenBeyondStep1()
    {
        var vm = CreateViewModel();
        vm.CartLines.Add(new CartLine { Quantite = 1, PrixUnitaire = 5m });
        vm.NextStepCommand.Execute(null); // → step 2
        vm.PreviousStepCommand.Execute(null); // → step 1
        Assert.Equal(1, vm.CurrentStep);
    }

    // ── RemoveLine ────────────────────────────────────────────────────────────

    [Fact]
    public void RemoveLine_RemovesLineFromCart()
    {
        var vm   = CreateViewModel();
        var line = new CartLine { Quantite = 1, PrixUnitaire = 20m };
        vm.CartLines.Add(line);

        vm.RemoveLineCommand.Execute(line);

        Assert.Empty(vm.CartLines);
        Assert.Equal(0m, vm.CartTotal);
    }

    [Fact]
    public void RemoveLine_DoesNothing_WhenLineIsNull()
    {
        var vm = CreateViewModel();
        vm.CartLines.Add(new CartLine { Quantite = 1, PrixUnitaire = 10m });

        vm.RemoveLineCommand.Execute(null);

        Assert.Single(vm.CartLines);
    }
}
