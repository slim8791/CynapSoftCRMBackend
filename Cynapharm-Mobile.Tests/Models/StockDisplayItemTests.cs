using Cynapharm_Mobile.Models.Inventory;

namespace Cynapharm_Mobile.Tests.Models;

public class StockDisplayItemTests
{
    // ── HasExpiry ─────────────────────────────────────────────────────────────

    [Fact]
    public void HasExpiry_ReturnsTrue_WhenExpiryLabelIsNotNull()
    {
        var item = new StockDisplayItem { ExpiryLabel = "31/12/2025" };
        Assert.True(item.HasExpiry);
    }

    [Fact]
    public void HasExpiry_ReturnsFalse_WhenExpiryLabelIsNull()
    {
        var item = new StockDisplayItem { ExpiryLabel = null };
        Assert.False(item.HasExpiry);
    }

    // ── CanDistribute ─────────────────────────────────────────────────────────

    [Fact]
    public void CanDistribute_ReturnsTrue_WhenIsEchantillonAndStockPositive()
    {
        var item = new StockDisplayItem { IsEchantillon = true, QuantiteRestante = 5 };
        Assert.True(item.CanDistribute);
    }

    [Fact]
    public void CanDistribute_ReturnsFalse_WhenNotEchantillon()
    {
        var item = new StockDisplayItem { IsEchantillon = false, QuantiteRestante = 10 };
        Assert.False(item.CanDistribute);
    }

    [Fact]
    public void CanDistribute_ReturnsFalse_WhenEchantillonButZeroStock()
    {
        var item = new StockDisplayItem { IsEchantillon = true, QuantiteRestante = 0 };
        Assert.False(item.CanDistribute);
    }

    [Fact]
    public void CanDistribute_ReturnsFalse_WhenEchantillonButNegativeStock()
    {
        // Defensive: negative stock should never distribute
        var item = new StockDisplayItem { IsEchantillon = true, QuantiteRestante = -1 };
        Assert.False(item.CanDistribute);
    }
}
