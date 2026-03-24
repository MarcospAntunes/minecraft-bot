using Protocol.Packets;
using Core;

namespace Features
{
  public class CommandHandler
  {
    /// <summary>
    /// Executa um comando enviado pelo usuário via chat
    /// </summary>
    public void ExecuteCommand(string command, Bot bot, int compression, BinaryWriter writer)
    {
      Task.Run(async () =>
      {
        await Task.Delay(new Random().Next(500, 1500)); // Delay humano aleatório
        PlayPacket.SendChatMessage(writer, command, compression);
        Console.WriteLine($">>> [COMMAND] Comando enviado: '{command}'");
      });
    }

    /// <summary>
    /// Reseta a contagem de execuções
    /// </summary>
  }
}