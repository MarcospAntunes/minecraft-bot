using Utils;
using Core;
using Protocol.Packets;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Features
{
  public class ChatHandler
  {
    private CommandHandler commandHandler = new CommandHandler();
    public void ListenChat(int packetId, BinaryReader packetReader, Bot bot, int CompressionThreshold, BinaryWriter writer, string rawJson)
    {
      if (packetId == 0x02)
      {
        // Limpeza de cores e caracteres especiais
        string cleanChat = Regex.Replace(rawJson, "§[0-9a-fk-or]", "");
        cleanChat = cleanChat.Replace("\"", "").Replace("{", "").Replace("}", "");

        if (cleanChat.Contains(bot.playerName))
        {
          Console.WriteLine($">>> [CHAT RAW] {cleanChat}");

        }
        // Login / lobby

        if (cleanChat.Contains("/login"))
        {
          string senha = "Marquinhos_1212";
          Console.WriteLine(">>> [BOT] Detectei login, enviando senha...");
          PlayPacket.SendChatMessage(writer, $"/login {senha}", CompressionThreshold);
        }
        if (cleanChat.Contains("logou com sucesso") || cleanChat.Contains("bem-vindo"))
        {
          if (!bot.CompassUsed)
          {
            bot.CompassUsed = true;
            Console.WriteLine(">>> [CHAT] Login Confirmado! Forçando entrada no Lobby Principal...");
            bot.CanMove = true;
          }
        }

        // Detecta mensagem privada /pw afk
        if (cleanChat.Contains(bot.playerName) && cleanChat.Contains("/tpa") && cleanChat.Contains(bot.playerName))
        {
          commandHandler.ExecuteCommand("/tpa WickMode", bot, CompressionThreshold, writer);
        }
        else if (cleanChat.Contains(bot.playerName) && cleanChat.Contains("/exit"))
        {
          Environment.Exit(0);
        }
        else if (cleanChat.Contains(bot.playerName) && cleanChat.Contains("/pw") && cleanChat.Contains("afk"))
        {
          string regex =  @"text:([^,\]]+)\](?!.*\])";
          Match match = Regex.Match(cleanChat, regex);

          if (match.Success)
          {
            // Armazena o resultado em uma variável
            string location = match.Groups[1].Value.Trim();
            commandHandler.ExecuteCommand($"/pw {location}", bot, CompressionThreshold, writer);
            Console.WriteLine("Palavra capturada: " + location);
          }
          else
          {
            Console.WriteLine("Palavra capturada: " + match.Groups[1].Value.Trim());
            commandHandler.ExecuteCommand($"/tell {bot.playerName} não entendi.", bot, CompressionThreshold, writer);
          }
        }
        else if(cleanChat.Contains(bot.playerName) && cleanChat.Contains("/restart"))
        {
          string path = Environment.ProcessPath;

          Process.Start(path);
          Environment.Exit(0);
        }
      }
    }
  }
}