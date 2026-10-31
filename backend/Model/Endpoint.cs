namespace App.Model;

public class Endpoint
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  public string Name { get; set; } = "";
  public string Url { get; set; } = "";
}
