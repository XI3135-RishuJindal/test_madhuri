namespace UserManagementService.Models.Events;

/// <summary>
/// Event published when a new user is created.
/// TODO: Confirm Kafka event publishing requirements with domain experts.
/// </summary>
public class UserCreatedEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string EventType { get; set; } = "UserCreated";
    public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;
}
