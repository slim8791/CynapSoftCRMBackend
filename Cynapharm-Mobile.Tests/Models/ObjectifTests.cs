using Cynapharm_Mobile.Models.Field;

namespace Cynapharm_Mobile.Tests.Models;

public class ObjectifTests
{
    // ── TypeObjectif ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, "Visites")]
    [InlineData(2, "Chiffre d'affaires")]
    [InlineData(3, "Nouveaux clients")]
    [InlineData(4, "Fidélisation")]
    public void TypeObjectif_ReturnsExpectedLabel_ForKnownCodes(int code, string expected)
    {
        var obj = new Objectif { TypeCode = code };
        Assert.Equal(expected, obj.TypeObjectif);
    }

    [Fact]
    public void TypeObjectif_ReturnsGenericLabel_ForUnknownPositiveCode()
    {
        var obj = new Objectif { TypeCode = 99 };
        Assert.Equal("Type 99", obj.TypeObjectif);
    }

    [Fact]
    public void TypeObjectif_ReturnsEmpty_WhenCodeIsZero()
    {
        var obj = new Objectif { TypeCode = 0 };
        Assert.Equal(string.Empty, obj.TypeObjectif);
    }

    // ── Periode ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, "Mensuel")]
    [InlineData(2, "Trimestriel")]
    [InlineData(3, "Annuel")]
    public void Periode_ReturnsExpectedLabel_ForKnownCodes(int code, string expected)
    {
        var obj = new Objectif { PeriodeCode = code };
        Assert.Equal(expected, obj.Periode);
    }

    [Fact]
    public void Periode_ReturnsGenericLabel_ForUnknownPositiveCode()
    {
        var obj = new Objectif { PeriodeCode = 5 };
        Assert.Equal("Période 5", obj.Periode);
    }

    [Fact]
    public void Periode_ReturnsEmpty_WhenCodeIsZero()
    {
        var obj = new Objectif { PeriodeCode = 0 };
        Assert.Equal(string.Empty, obj.Periode);
    }

    // ── ProgressValue ─────────────────────────────────────────────────────────

    [Fact]
    public void ProgressValue_IsZero_WhenValeurCibleIsZero()
    {
        var obj = new Objectif { ValeurCible = 0, ValeurActuelle = 50 };
        Assert.Equal(0, obj.ProgressValue);
    }

    [Fact]
    public void ProgressValue_IsZero_WhenValeurActuelleIsNull()
    {
        var obj = new Objectif { ValeurCible = 100, ValeurActuelle = null };
        Assert.Equal(0, obj.ProgressValue);
    }

    [Fact]
    public void ProgressValue_IsCorrectRatio_WhenPartiallyAchieved()
    {
        var obj = new Objectif { ValeurCible = 200, ValeurActuelle = 50 };
        Assert.Equal(0.25, obj.ProgressValue, precision: 10);
    }

    [Fact]
    public void ProgressValue_IsClampedAt1_WhenValeurActuelleExceedsTarget()
    {
        var obj = new Objectif { ValeurCible = 100, ValeurActuelle = 150 };
        Assert.Equal(1.0, obj.ProgressValue);
    }
}
