using Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Routes;

class Program
{
  static void Main()
  {
    WebApplicationBuilder builder = WebApplication.CreateBuilder();

    builder.Services.AddDbContext<ChatDb>(opt => opt.UseInMemoryDatabase("Chat"));
    builder.Services.AddDbContext<LogDb>(opt => opt.UseInMemoryDatabase("Log"));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddScoped<ChatService>();
    builder.Services.AddScoped<LogService>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApiDocument(config =>
    {
      config.DocumentName = "BotAPI";
      config.Title = "BotAPI v1";
      config.Version = "v1";
    });

    WebApplication app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
      app.UseOpenApi();
      app.UseSwaggerUi(config =>
      {
        config.DocumentTitle = "BotAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
      });
    }

    app.MapGet("/chat", ChatRoute.GetLastChat);
    app.MapGet("/log", LogRoute.GetLastLog);
    app.MapGet("bot/{id}", BotRoute.GetBotById);
    app.MapPost("/start", BotRoute.StartBot);
    app.MapPost("/chat", ChatRoute.AddChat);
    app.MapPost("/command", CommandRoute.addComand);
    app.MapDelete("/chat", ChatRoute.DeleteAllChat);

    app.Run();
  }
}