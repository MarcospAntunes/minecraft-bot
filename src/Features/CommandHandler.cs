using Protocol.Packets;
using Core;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Models;
namespace Features
{
  public class CommandHandler
  {
    private static readonly Regex RawTextRegex = new(@"text:([^,\]]+)(?!.*text:)", RegexOptions.Compiled);
    private static readonly Regex PwCommandRegex = new(@"/pw\s+(\S+)", RegexOptions.Compiled);
    private readonly LogService _logService;

    public CommandHandler(LogService logService)
    {
      _logService = logService;
    }

    private void ExecuteCommand(string command, int compression, BinaryWriter writer, Bot bot)
    {
      Task.Run(async () =>
      {
        await Task.Delay(new Random().Next(500, 1500)); // Delay humano aleatório 
        PlayPacket.SendChatMessage(writer, command, compression);
        _ = _logService.AddMessage(new Log { Message = $">>> [COMMAND] Comando enviado: '{command}'" });
      });
    }

    public void HandleAutoLogin(string chat, BinaryWriter writer, int compression, Bot bot)
    {
      // 🔥 Registro primeiro
      if (chat.Contains("/register"))
      {
        _ = _logService.AddMessage(new Log { Message = ">>> [BOT] Detectei pedido de registro, enviando senha..." });

        PlayPacket.SendChatMessage(writer, $"/register {bot.GetPassword()} {bot.GetPassword()}", compression);
        bot.stopTryClickButton = false;
        return;
      }

      // 🔥 Login depois
      if (chat.Contains("/login"))
      {
        _ = _logService.AddMessage(new Log { Message = ">>> [BOT] Detectei login, enviando senha..." });
        PlayPacket.SendChatMessage(writer, $"/login {bot.GetPassword()}", compression);

        bot.stopTryClickButton = false;
      }
    }

    public void HandleLoginSuccess(string chat, Bot bot)
    {
      if (!(chat.Contains("logou com sucesso") || chat.Contains("bem-vindo")))
        return;

      // trava REAL (sem race condition)
      if (bot.LoginHandled)
        return;

      bot.LoginHandled = true;

      bot.CanMove = true;

      _ = _logService.AddMessage(new Log { Message = ">>> [CHAT] Login confirmado! Entrada liberada." });

      _ = Task.Run(async () =>
      {
        await Task.Delay(4000); // deixa o servidor te dar inventário

        _ = _logService.AddMessage(new Log { Message = ">>> [BOT] Selecionando bússola..." });

        InventoryActions.SendHeldItemChange(bot.GetWriter(), 0, bot.CompressionThreshold);

        await Task.Delay(500);

        _ = _logService.AddMessage(new Log { Message = ">>> [BOT] Usando bússola..." });
        InventoryActions.SendRightClick(bot.GetWriter(), bot.CompressionThreshold);

        await Task.Delay(2000);

        //_ = _logService.AddMessage(new Log {Message = ">>> [BOT] Andando para frente..."});
        //await Movement.WalkForward(bot, bot.CompressionThreshold, 2.0);
      });
    }

    public void HandleAdminCommands(string chat, Bot bot, BinaryWriter writer, int compression)
    {
      if (!chat.Contains(bot.playerName)) return;
      if (chat.Contains("/tpa"))
      {
        ExecuteCommand($"/tpa {bot.playerName}", compression, writer, bot);
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

      if (chat.Contains("/lobby"))
      {
        ExecuteCommand($"/lobby", compression, writer, bot);
        bot.stopTryClickButton = false;
      }
    }
    public void HandlePwCommand(string chat, Bot bot, BinaryWriter writer, int compression)
    {
      string? location = null;

      // 🔹 Caso 1: mensagem RAW
      if (chat.Contains("text:"))
      {
        var match = RawTextRegex.Match(chat);
        if (match.Success)
          location = match.Groups[1].Value.Trim();
      }
      // 🔹 Caso 2: mensagem normal
      else if (chat.Contains("/pw"))
      {
        var match = PwCommandRegex.Match(chat);
        if (match.Success)
          location = match.Groups[1].Value.Trim();
      }

      // 🔹 Execução final
      if (!string.IsNullOrWhiteSpace(location))
      {
        ExecuteCommand($"/pw {location}", compression, writer, bot);

        _ = _logService.AddMessage(new Log
        {
          Message = $">>> [BOT] Palavra capturada: {location}"
        });
      }
      else
      {
        ExecuteCommand($"/tell {bot.playerName} não entendi.", compression, writer, bot);
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
