using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Routes
{
  class BotRoute
  {
    public static async Task<IResult> StartBot(IServiceProvider sp, string username, string playerName, string password)
    {
      IServiceScope scope = sp.CreateScope();
      var logService = scope.ServiceProvider.GetRequiredService<LogService>();
      var chatService = scope.ServiceProvider.GetRequiredService<ChatService>();

      string server = "jogar.gladmc.com";
      int port = 25565;
      Guid id = Guid.NewGuid();

      Bot bot = new Bot(id, username, server, port, playerName, password, logService, chatService);
      BotManager.Bots[id] = bot;

      _ = Task.Run(bot.Start);

      return Results.Ok(new { botId = id });
    }

    public static async Task<IResult> GetBotById(Guid id)
    {
      if (!BotManager.Bots.TryGetValue(id, out var bot)) return Results.NotFound();

      return Results.Ok(new { id, status = bot.GetState().ToString() });
    }
  }
}