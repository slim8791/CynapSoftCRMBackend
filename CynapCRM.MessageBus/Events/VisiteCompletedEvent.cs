namespace CynapCRM.MessageBus.Events;

public record VisiteCompletedEvent
{
    public int VisiteId { get; init; }
    public int RapportId { get; init; }
    public int DelegueId { get; init; }
    public string Resultat { get; init; } = string.Empty;
    public string? ProduitsDiscutes { get; init; }
    public DateTime DateRapport { get; init; }
}
