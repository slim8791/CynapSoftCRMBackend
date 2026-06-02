using Cynapharm_Mobile.Models.Orders;

namespace Cynapharm_Mobile.Tests.Models;

public class LigneCommandeTests
{
    [Fact]
    public void DisplayName_ReturnsProductNom_WhenNonEmpty()
    {
        var ligne = new LigneCommande { ProductId = 7, ProductNom = "Amoxicilline 515mg" };
        Assert.Equal("Amoxicilline 515mg", ligne.DisplayName);
    }

    [Fact]
    public void DisplayName_ReturnsFallbackWithId_WhenProductNomIsEmpty()
    {
        var ligne = new LigneCommande { ProductId = 42, ProductNom = string.Empty };
        Assert.Equal("Produit #42", ligne.DisplayName);
    }

    [Fact]
    public void SousTotal_IsQuantiteTimesUnitPrice()
    {
        var ligne = new LigneCommande { Quantite = 6, PrixUnitaire = 8.75m };
        Assert.Equal(52.50m, ligne.SousTotal);
    }
}
