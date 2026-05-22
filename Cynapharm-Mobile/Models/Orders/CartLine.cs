using CommunityToolkit.Mvvm.ComponentModel;

namespace Cynapharm_Mobile.Models.Orders;

public class CartLine : ObservableObject
{
    public int     ProductId  { get; set; }
    public string  ProductNom { get; set; } = string.Empty;

    private int _quantite;
    public int Quantite
    {
        get => _quantite;
        set
        {
            if (SetProperty(ref _quantite, value))
            {
                OnPropertyChanged(nameof(SousTotal));
                OnPropertyChanged(nameof(EconomieTotale));
            }
        }
    }

    public decimal  PrixOriginal       { get; set; }
    public decimal  PrixUnitaire       { get; set; }
    public decimal  RemisePourcentage  { get; set; }
    public string?  PromoTitre         { get; set; }

    public bool    HasPromo       => RemisePourcentage > 0;
    public decimal SousTotal      => Quantite * PrixUnitaire;
    public decimal EconomieTotale => Quantite * (PrixOriginal - PrixUnitaire);
}
