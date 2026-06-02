namespace Cynapharm_Mobile.Models.Products;

/// <summary>
/// A selectable product item used in the rapport multi-select list.
/// </summary>
public class ProductCheckItem
{
    public int ProductId { get; set; }
    public string ProductNom { get; set; } = string.Empty;
    public string ProductReference { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}
