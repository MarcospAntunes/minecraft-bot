using System.Text.RegularExpressions;
using System.Diagnostics;
using Core;
using Protocol.Packets;

namespace Features
{
  public class ChatHandler
  {
    private CommandHandler commandHandler = new CommandHandler();

    public void ListenChat(int packetId, BinaryReader reader, Bot bot, int compression, BinaryWriter writer, string rawJson)
    {
      if (packetId != 0x02) return;

      string cleanChat = Regex.Replace(rawJson, "§[0-9a-fk-or]", "")
                              .Replace("\"", "").Replace("{", "").Replace("}", "");

      if (cleanChat.Contains(bot.playerName))
        Console.WriteLine($">>> [CHAT RAW] {cleanChat}");

      // Auto login
      if (cleanChat.Contains("/login"))
      {
        string senha = "Marquinhos_1212";
        Console.WriteLine(">>> [BOT] Detectei login, enviando senha...");
        PlayPacket.SendChatMessage(writer, $"/login {senha}", compression);
      }

      if (cleanChat.Contains("logou com sucesso") || cleanChat.Contains("bem-vindo"))
      {
        if (!bot.CompassUsed)
        {
          bot.CompassUsed = true;
          Console.WriteLine(">>> [CHAT] Login confirmado! Forçando entrada no Lobby Principal...");
          bot.CanMove = true;
        }
      }

      // Comandos privados
      if (cleanChat.Contains(bot.playerName))
      {
        if (cleanChat.Contains("/tpa"))
          commandHandler.ExecuteCommand($"/tpa {bot.playerName}", bot, compression, writer);
        else if (cleanChat.Contains("/exit"))
          Environment.Exit(0);
        else if (cleanChat.Contains("/pw") && cleanChat.Contains("afk"))
        {
          string regex = @"text:([^,\]]+)\](?!.*\])";
          Match match = Regex.Match(cleanChat, regex);

          if (match.Success)
          {
            string location = match.Groups[1].Value.Trim();
            commandHandler.ExecuteCommand($"/pw {location}", bot, compression, writer);
            Console.WriteLine("Palavra capturada: " + location);
          }
          else
          {
            commandHandler.ExecuteCommand($"/tell {bot.playerName} não entendi.", bot, compression, writer);
          }
        }
        else if (cleanChat.Contains("/restart"))
        {
          string path = Environment.ProcessPath;
          Process.Start(path);
          Environment.Exit(0);
        }
      }
    }
  }
}