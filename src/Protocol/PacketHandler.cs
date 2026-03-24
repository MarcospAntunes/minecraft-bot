using Utils;
using Features;
using Core;
using Network;

namespace Protocol
{
  public class PacketHandler
  {
    private readonly ChatHandler chatHandler = new ChatHandler();

    public void Handle(int packetId, BinaryReader packetReader, Bot bot, State state, int compression, BinaryWriter writer)
    {
      switch (state)
      {
        case State.Login:
          HandleLoginPackets(packetId, packetReader, bot);
          break;

        case State.Play:
          HandlePlayPackets(packetId, packetReader, bot, compression, writer);
          break;
      }
    }

    private void HandleLoginPackets(int packetId, BinaryReader reader, Bot bot)
    {
      switch (packetId)
      {
        case 0x03:
          bot.CompressionThreshold = VarInt.ReadVarInt(reader);
          break;

        case 0x02:
          string uuid = McString.ReadString(reader);
          string name = McString.ReadString(reader);

          bot.SetState(State.Play);
          bot.Logged = true;

          Console.WriteLine($">>> [LOGIN 1.8] Logado como: {name}");
          break;
      }
    }

    private void HandlePlayPackets(int packetId, BinaryReader reader, Bot bot, int compression, BinaryWriter writer)
    {
      switch (packetId)
      {
        case 0x01: // Join Game
          bot.JoinCount++;
          Console.WriteLine($">>> [MUNDO] Servidor #{bot.JoinCount} conectado.");

          if (bot.JoinCount == 2)
            bot.CompassUsed = false;

          break;

        case 0x2D: // Open Window (menu abriu)
          byte windowId = reader.ReadByte();
          // Pula o resto do cabeçalho do Open Window (Inventory Type, Title, Slot Count, etc)
          // Se não ler o resto, o próximo pacote no buffer ficará corrompido
          McString.ReadString(reader); // Pula o Title
          reader.ReadByte(); // Pula o Inventory Type
          reader.ReadByte(); // Pula o Slot Count

          Console.WriteLine($">>> [MENU] Janela {windowId} aberta.");

          if (bot.MenuHandled) return;
          bot.MenuHandled = true;

          _ = Task.Run(async () =>
          {
            await Task.Delay(1000);
            Console.WriteLine($">>> [BOT] Iniciando varredura de slots...");

            for (short slot = 0; slot <= 1000; slot++)
            {
              if (bot.stopTryClickButton)
              {
                Console.WriteLine(">>> [BOT] Parando cliques: Condição de parada atingida.");
                break;
              }

              // Incrementa o ActionNumber ANTES de enviar, para bater com o que o servidor espera
              bot.ActionNumber++;

              InventoryActions.SendClickWindow(bot.GetWriter(), windowId, slot, compression, bot);
              Console.WriteLine($"Clicando slot: {slot} | Action: {bot.ActionNumber}");

              // Delay um pouco maior (75-100ms) é mais seguro contra AntiCheats de inventário
              await Task.Delay(75);
            }
          });
          break;

        case 0x00: // Keep Alive
          {
            int kaId = VarInt.ReadVarInt(reader);

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            VarInt.WriteVarInt(bw, 0x00);
            VarInt.WriteVarInt(bw, kaId);

            PacketWriter.SendPacket(bot.GetWriter(), ms.ToArray(), compression);
          }
          break;

        case 0x08: // Position and Look
          {
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();
            double z = reader.ReadDouble();
            float yaw = reader.ReadSingle();
            float pitch = reader.ReadSingle();
            byte flags = reader.ReadByte();

            using var msPos = new MemoryStream();
            using var bwPos = new BinaryWriter(msPos);

            VarInt.WriteVarInt(bwPos, 0x06);
            bwPos.Write(x);
            bwPos.Write(y);
            bwPos.Write(z);
            bwPos.Write(yaw);
            bwPos.Write(pitch);
            bwPos.Write(true);

            bot.PosX = x;
            bot.PosY = y;
            bot.PosZ = z;
            bot.Yaw = yaw;
            bot.Pitch = pitch;

            PacketWriter.SendPacket(bot.GetWriter(), msPos.ToArray(), compression);
          }
          break;

        case 0x02: // Chat Message
          string rawJson = McString.ReadString(reader);
          // Chame apenas uma vez e guarde o resultado
          string processedMessage = chatHandler.ListenChat(packetId, reader, bot, compression, writer, rawJson);

          if (processedMessage.Contains("Carregando seus dados..."))
          {
            bot.stopTryClickButton = true;
          }
          break;

        case 0x30: // Window Items
          {
            byte winId = reader.ReadByte();
            short count = reader.ReadInt16();

            // limpa o resto do pacote
            if (reader.BaseStream.Position < reader.BaseStream.Length)
              reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
          }
          break;

        case 0x32: // Confirm Transaction
          {
            byte tid_winId = reader.ReadByte();
            short actionId = reader.ReadInt16();
            bool accepted = reader.ReadBoolean();

            bot.ActionNumber = actionId;

            using var msT = new MemoryStream();
            using var bwT = new BinaryWriter(msT);

            VarInt.WriteVarInt(bwT, 0x0F);
            bwT.Write(tid_winId);
            bwT.Write(actionId);
            bwT.Write(accepted);

            PacketWriter.SendPacket(bot.GetWriter(), msT.ToArray(), compression);
          }
          break;
      }
    }
  }
}