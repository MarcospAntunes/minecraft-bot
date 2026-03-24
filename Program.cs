using System;
using Core;

class Program
{
  static void Main()
  {
    Console.WriteLine("Escolha o nick do bot: ");
    string username = Console.ReadLine().Trim();

    Console.WriteLine("Qual nick do jogador que ele irá receber os comandos? ");
    string playerName = Console.ReadLine().Trim();
    
    string server = "jogar.gladmc.com";
    int port = 25565;

    Bot bot = new Bot(username, server, port, playerName);

    Console.WriteLine(">>> Iniciando bot...");
    bot.Start();

    Console.ReadLine();
  }
}