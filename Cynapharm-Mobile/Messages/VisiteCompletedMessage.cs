namespace Cynapharm_Mobile.Messages;

/// <summary>
/// Sent by RapportViewModel after a rapport is successfully submitted.
/// VisitDetailViewModel listens for this message to flip IsCompleted=true
/// without requiring a round-trip API call.
/// </summary>
public sealed class VisiteCompletedMessage
{
    public int VisiteId { get; }

    public VisiteCompletedMessage(int visiteId) => VisiteId = visiteId;
}
