using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Routes
{

  class LogRoute
  {
    public static async Task<IResult> GetLastLog(LogDb db, Guid BotId)
    {
      var log = await db.Logs
        .Where(c => c.BotId == BotId)
        .OrderByDescending(c => c.CreatedAt)
        .FirstOrDefaultAsync();

      if(log == null) return Results.NotFound();
      return Results.Ok(log);
    }
  }

}