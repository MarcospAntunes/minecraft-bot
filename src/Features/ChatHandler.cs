using System.Text.RegularExpressions;
using Core;
namespace Features
{
  public class ChatHandler
  {
    private readonly CommandHandler _commandHandler = new();
    // Regex pré-compiladas (melhor performance) 
    private static readonly Regex ColorRegex = new(@"§[0-9a-fk-or]", RegexOptions.Compiled);
    private static readonly Regex TextRegex = new(@"text:([^,\]]+)", RegexOptions.Compiled);
    public void ListenChat(int packetId, BinaryReader reader, Bot bot, int compression, BinaryWriter writer, string rawJson)
    {
      if (packetId != 0x02) return;
      string cleanChat = CleanChat(rawJson);
      var parsed = ParseChat(cleanChat);
      if (parsed != null)
      {
        Console.WriteLine($">>> [CHAT] {parsed.Value.sender}: {parsed.Value.message}");

        _commandHandler.HandleAutoLogin(cleanChat, writer, compression);
        _commandHandler.HandleLoginSuccess(cleanChat, bot);
        _commandHandler.HandleAdminCommands(cleanChat, bot, writer, compression);
      }


    }
    private string CleanChat(string rawJson)
    {
      Regex ChatMessageRegex = new(@"text:([^:\]]+)\]\s*,?\s*text::\s*\]\s*,?\s*text:([^,\]]+)", RegexOptions.Compiled);
      string result = ColorRegex.Replace(rawJson, "").Replace("\"", "").Replace("{", "").Replace("}", "");
      result = ChatMessageRegex.Replace(result, "");

      return result;
    }

    private (string sender, string message)? ParseChat(string chat)
    {
      var matches = TextRegex.Matches(chat);

      var parts = matches
          .Select(m => m.Groups[1].Value.Trim())
          .Where(s => !string.IsNullOrWhiteSpace(s))
          .ToList();

      if (parts.Count < 2) return null;

      for (int i = 0; i < parts.Count - 1; i++)
      {
        if (parts[i + 1] == ":")
        {
          string sender = parts[i];

          // 🔥 Aqui está a correção: pega TUDO depois dos dois pontos
          var messageParts = parts.Skip(i + 2);

          string message = string.Join(" ", messageParts);

          return (sender, message);
        }
      }

      return null;
    }
  }
}