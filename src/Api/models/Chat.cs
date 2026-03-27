namespace Models
{
  public class Chat
  {
    public int Id { get; set; }
    public Guid BotId { get; set; }
    public required string? Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }

}
