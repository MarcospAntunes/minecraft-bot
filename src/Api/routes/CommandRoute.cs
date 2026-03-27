using Data;
using Microsoft.AspNetCore.Http;
using Models;

namespace Routes
{
  class CommandRoute
  {
    public static async Task<IResult> addComand(string command, ChatDb db, Guid BotId)
    {
      Chat objCommand = new Chat {Message = command, BotId = BotId};

      db.Chats.Add(objCommand);
      await db.SaveChangesAsync();

      return Results.NoContent();
    }
  }
}