namespace CynapCRM.MessageBus.Events;

public record UserCreatedEvent
{
    public int UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public int? IdRegion { get; init; }
    public DateTime DateCreation { get; init; }
}
