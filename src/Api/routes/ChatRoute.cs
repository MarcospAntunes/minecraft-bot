using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Routes
{
  class ChatRoute
  {
    public static async Task<IResult> GetLastChat(ChatDb db, [FromQuery] Guid BotId)
    {
      var chat = await db.Chats
        .Where(c => c.BotId == BotId)
        .OrderByDescending(c => c.CreatedAt)
        .FirstOrDefaultAsync();

      if (chat == null) return Results.NotFound();
      return Results.Ok(chat);
    }

    public static async Task<IResult> AddChat(Guid BotId, string chat, ChatDb db)
    {
      Chat objChat = new Chat { Message = chat, BotId = BotId };
      db.Chats.Add(objChat);
      await db.SaveChangesAsync();
      return Results.NoContent();
    }

    public static async Task<IResult> DeleteAllChat(ChatDb db)
    {
      db.RemoveRange(db.Chats);
      await db.SaveChangesAsync();

      return Results.Ok();
    }
  }
}