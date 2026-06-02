using Cynapharm_Mobile.Models.Orders;

namespace Cynapharm_Mobile.Tests.Models;

public class CartLineTests
{
    // ── HasPromo ──────────────────────────────────────────────────────────────

    [Fact]
    public void HasPromo_ReturnsFalse_WhenRemisePercentageIsZero()
    {
        var line = new CartLine { RemisePourcentage = 0m };
        Assert.False(line.HasPromo);
    }

    [Fact]
    public void HasPromo_ReturnsTrue_WhenRemisePercentageIsPositive()
    {
        var line = new CartLine { RemisePourcentage = 10m };
        Assert.True(line.HasPromo);
    }

    // ── SousTotal ─────────────────────────────────────────────────────────────

    [Fact]
    public void SousTotal_IsQuantiteTimesEffectiveUnitPrice()
    {
        var line = new CartLine { Quantite = 3, PrixUnitaire = 12.50m };
        Assert.Equal(37.50m, line.SousTotal);
    }

    [Fact]
    public void SousTotal_IsZero_WhenQuantiteIsZero()
    {
        var line = new CartLine { Quantite = 0, PrixUnitaire = 100m };
        Assert.Equal(0m, line.SousTotal);
    }

    // ── EconomieTotale ────────────────────────────────────────────────────────

    [Fact]
    public void EconomieTotale_IsQuantiteTimesDiscountAmount_WhenPromoActive()
    {
        // 20% off 50.00 → effective price 40.00 → saving per unit 10.00 × 5 = 50.00
        var line = new CartLine
        {
            Quantite          = 5,
            PrixOriginal      = 50m,
            PrixUnitaire      = 40m,
            RemisePourcentage = 20m
        };
        Assert.Equal(50m, line.EconomieTotale);
    }

    [Fact]
    public void EconomieTotale_IsZero_WhenNoPriceReduction()
    {
        var line = new CartLine { Quantite = 4, PrixOriginal = 25m, PrixUnitaire = 25m };
        Assert.Equal(0m, line.EconomieTotale);
    }
}
