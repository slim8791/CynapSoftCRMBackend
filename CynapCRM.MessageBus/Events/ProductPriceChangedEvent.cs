namespace CynapCRM.MessageBus.Events;

public record ProductPriceChangedEvent
{
    public int ProductId { get; init; }
    public string NomProduit { get; init; } = string.Empty;
    public decimal NouveauPrix { get; init; }
    public DateTime DateModification { get; init; }
}
