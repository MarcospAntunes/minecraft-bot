using System;
using System.IO;
using System.Threading.Tasks;
using Utils;
using Core;
using Protocol.Packets;

namespace Features
{
    public class CommandHandler
    {
        // Limite de execuções por comando (opcional)
        private readonly int maxExecutions;
        private int executedCount = 0;

        public CommandHandler(int maxExecutions = 1)
        {
            this.maxExecutions = maxExecutions;
        }

        /// <summary>
        /// Executa um comando enviado pelo usuário via chat
        /// </summary>
        public void ExecuteCommand(string command, Bot bot, int CompressionThreshold, BinaryWriter writer)
        {
            if (executedCount >= maxExecutions)
            {
                Console.WriteLine($">>> [COMMAND] Comando '{command}' já executado {executedCount}/{maxExecutions} vezes.");
                return;
            }

            executedCount++;

            Task.Run(async () =>
            {
                // Delay humano aleatório entre 0,5 e 1,5 segundos
                await Task.Delay(new Random().Next(500, 1500));

                PlayPacket.SendChatMessage(writer, command, CompressionThreshold);
                Console.WriteLine($">>> [COMMAND] Comando enviado: '{command}' ({executedCount}/{maxExecutions})");
            });
        }

        /// <summary>
        /// Reseta a contagem de execuções para poder enviar novamente
        /// </summary>
        public void Reset()
        {
            executedCount = 0;
        }
    }
}