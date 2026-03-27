using System.Text.Json;
using System.Text.RegularExpressions;
using Core;
using Models;

namespace Features
{
  public class ChatHandler
  {
    private readonly CommandHandler _commandHandler;
    private readonly ChatService _chatService;

    private static readonly Regex ColorRegex = new(@"§[0-9a-fk-or]", RegexOptions.Compiled);
    private static readonly Regex TextRegex = new(@"text:([^,\]]+)", RegexOptions.Compiled);
    private static readonly Regex ChatMessageRegex = new(@"text:([^:\]]+)\]\s*,?\s*text::\s*\]\s*,?\s*text:([^,\]]+)", RegexOptions.Compiled);

    private static readonly HttpClient http = new HttpClient
    {
      Timeout = TimeSpan.FromSeconds(5)
    };

    private int LastChatId = -1;
    private string _lastChat = "";

    public ChatHandler(ChatService chatService, LogService logService)
    {
      _chatService = chatService;
      _commandHandler = new CommandHandler(logService);
    }

    // =========================
    // CHAT VINDO DO PACKET
    // =========================
    public string HandlePacketChat(int packetId, BinaryReader reader, Bot bot, int compression, BinaryWriter writer, string rawJson)
    {
      if (packetId != 0x02) return "";

      string cleanChat = CleanChat(rawJson);
      var parsed = ParseChat(cleanChat);

      if (parsed != null)
        _lastChat = $">>> [CHAT] {parsed.Value.sender}: {parsed.Value.message}";
      else
        _lastChat = $">>> [CHAT] {cleanChat}";

      ExecuteCommands(_lastChat, bot, writer, compression);

      _ = _chatService.AddMessage(_lastChat, bot.Id);

      return _lastChat;
    }

    // =========================
    // CHAT VINDO DA API
    // =========================
    public string HandleExternalChat(Bot bot, BinaryWriter writer, string message)
    {
      _lastChat = $">>> [API] {message}";

      if (_lastChat.Contains("/pw")) _commandHandler.HandlePwCommand(_lastChat, bot, writer, bot.CompressionThreshold);
      else ExecuteCommands(_lastChat, bot, writer, bot.CompressionThreshold);

      _ = _chatService.AddMessage(_lastChat, bot.Id);

      return _lastChat;
    }

    // =========================
    // POLLING DA API
    // =========================
    public async Task StartChatPollingAsync(Bot bot, BinaryWriter writer)
    {
      var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

      while (true)
      {
        try
        {
          var response = await http.GetAsync($"http://localhost:5000/chat?BotId={bot.Id}");
          if (!response.IsSuccessStatusCode) { await Task.Delay(2000); continue; }

          var content = await response.Content.ReadAsStringAsync();
          var chat = JsonSerializer.Deserialize<Chat>(content, options);

          if (chat != null && chat.Id > LastChatId)
          {
            LastChatId = chat.Id;

            if (!chat.Message.StartsWith(">>>"))
            {
              HandleExternalChat(bot, writer, chat.Message);
            }
          }
        }
        catch (Exception ex) { Console.WriteLine($"Erro: {ex.Message}"); }
        await Task.Delay(2000);
      }
    }

    // =========================
    // LIMPEZA DE CHAT
    // =========================
    private string CleanChat(string rawJson)
    {
      string result = ColorRegex.Replace(rawJson, "")
        .Replace("\"", "")
        .Replace("{", "")
        .Replace("}", "");

      result = ChatMessageRegex.Replace(result, "");

      return result;
    }

    // =========================
    // PARSE DE CHAT
    // =========================
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
          var messageParts = parts.Skip(i + 2);

          string message = string.Join(" ", messageParts);

          return (sender, message);
        }
      }

      return null;
    }

    // =========================
    // EXECUÇÃO DE COMANDOS
    // =========================
    private void ExecuteCommands(string chat, Bot bot, BinaryWriter writer, int compression)
    {
      _commandHandler.HandleAutoLogin(chat, writer, compression, bot);
      _commandHandler.HandleLoginSuccess(chat, bot);
      _commandHandler.HandleAdminCommands(chat, bot, writer, compression);
    }
  }
}