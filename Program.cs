using System;
using Core;

class Program
{
  static void Main()
  {
    string username = "WickAfk1";
    string playerName = "WickMode";
    string server = "jogar.gladmc.com";
    int port = 25565;

    Bot bot = new Bot(username, server, port, playerName);

    Console.WriteLine(">>> Iniciando bot...");
    bot.Start();

    Console.ReadLine();
  }
}