using Protocol.Packets;
using Core;

namespace Features
{
  public class CommandHandler
  {
    private readonly int maxExecutions;
    private int executedCount = 0;

    public CommandHandler(int maxExecutions = 1)
    {
      this.maxExecutions = maxExecutions;
    }

    /// <summary>
    /// Executa um comando enviado pelo usuário via chat
    /// </summary>
    public void ExecuteCommand(string command, Bot bot, int compression, BinaryWriter writer)
    {
      if (executedCount >= maxExecutions)
      {
        Console.WriteLine($">>> [COMMAND] Comando '{command}' já executado {executedCount}/{maxExecutions} vezes.");
        return;
      }

      executedCount++;

      Task.Run(async () =>
      {
        await Task.Delay(new Random().Next(500, 1500)); // Delay humano aleatório
        PlayPacket.SendChatMessage(writer, command, compression);
        Console.WriteLine($">>> [COMMAND] Comando enviado: '{command}' ({executedCount}/{maxExecutions})");
      });
    }

    /// <summary>
    /// Reseta a contagem de execuções
    /// </summary>
    public void Reset()
    {
      executedCount = 0;
    }
  }
}