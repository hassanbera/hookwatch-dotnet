namespace App.Model;

public class Delivery
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  public Guid EndpointId { get; set; } = Guid.Empty;
  public string EventType { get; set; } = "";
  public string Payload { get; set; } = "{}";
  public string Status { get; set; } = "QUEUED";
  public int AttemptCount { get; set; } = 0;
  public int CycleAttempts { get; set; } = 0;
  public DateTimeOffset NextAttemptAt { get; set; } = DateTimeOffset.UtcNow;
}
