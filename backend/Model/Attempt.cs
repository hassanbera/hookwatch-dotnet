namespace App.Model;

public class Attempt
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  public Guid DeliveryId { get; set; } = Guid.Empty;
  public int Number { get; set; } = 0;
  public int? StatusCode { get; set; } = null;
  public int DurationMs { get; set; } = 0;
  public string Error { get; set; } = "";
}
