using Protocol.Packets;
using Core;
using System.Diagnostics;
using System.Text.RegularExpressions;
namespace Features
{
  public class CommandHandler
  {
    private static readonly Regex LocationRegex = new(@"text:([^,\]]+)\](?!.*\])", RegexOptions.Compiled);
    private const string Senha = "Marquinhos_1212";  // ideal: mover pra config 
    private void ExecuteCommand(string command, int compression, BinaryWriter writer)
    {
      Task.Run(async () =>
      {
        await Task.Delay(new Random().Next(500, 1500)); // Delay humano aleatório 
        PlayPacket.SendChatMessage(writer, command, compression); Console.WriteLine($">>> [COMMAND] Comando enviado: '{command}'");
      });
    }

    public void HandleAutoLogin(string chat, BinaryWriter writer, int compression)
    {
      // 🔥 Registro primeiro
      if (chat.Contains("/register"))
      {
        Console.WriteLine(">>> [BOT] Detectei pedido de registro, enviando senha...");
        PlayPacket.SendChatMessage(writer, $"/register {Senha} {Senha}", compression);
        return;
      }

      // 🔥 Login depois
      if (chat.Contains("/login"))
      {
        Console.WriteLine(">>> [BOT] Detectei login, enviando senha...");
        PlayPacket.SendChatMessage(writer, $"/login {Senha}", compression);
      }
    }
    
    public void HandleLoginSuccess(string chat, Bot bot)
    {
      if (!(chat.Contains("logou com sucesso") || chat.Contains("bem-vindo"))) return;
      if (bot.CompassUsed) return;

      bot.CompassUsed = true;
      bot.CanMove = true;
      Console.WriteLine(">>> [CHAT] Login confirmado! Entrada liberada.");
    }
    public void HandleAdminCommands(string chat, Bot bot, BinaryWriter writer, int compression)
    {
      if (!chat.Contains(bot.playerName)) return;

      if (chat.Contains("/tpa"))
      {
        ExecuteCommand($"/tpa {bot.playerName}", compression, writer);
        return;
      }

      if (chat.Contains("/exit"))
      {
        Environment.Exit(0);
        return;
      }

      if (chat.Contains("/restart"))
      {
        RestartBot(); return;
      }

      if (chat.Contains("/pw") && chat.Contains("afk"))
      {
        HandlePwCommand(chat, bot, writer, compression);
      }
    }
    public void HandlePwCommand(string chat, Bot bot, BinaryWriter writer, int compression)
    {
      var match = LocationRegex.Match(chat);
      if (match.Success)
      {
        string location = match.Groups[1].Value.Trim();
        ExecuteCommand($"/pw {location}", compression, writer);
        Console.WriteLine($">>> [BOT] Palavra capturada: {location}");
      }
      else
      {
        ExecuteCommand($"/tell {bot.playerName} não entendi.", compression, writer);
      }
    }
    public void RestartBot()
    {
      string path = Environment.ProcessPath!;
      Process.Start(path);
      Environment.Exit(0);
    }
  }
}
