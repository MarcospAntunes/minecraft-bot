using Data;
using Models;

public class ChatService
{
  private readonly ChatDb _db;

  public ChatService(ChatDb db)
  {
    _db = db;
  }

  public async Task AddMessage(string chat, Guid BotId)
  {
    _db.Chats.Add(new Chat { Message = chat, BotId = BotId });
    await _db.SaveChangesAsync();

  }
}