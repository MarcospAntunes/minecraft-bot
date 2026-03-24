using Utils;
using Features;
using Core;
using Network;

namespace Protocol
{
  public class PacketHandler
  {
    private ChatHandler chatHandler = new ChatHandler();

    public void Handle(int packetId, BinaryReader packetReader, Bot bot, State state, int CompressionThreshold, BinaryWriter writer)
    {
      switch (state)
      {
        case State.Login: HandleLoginPackets(packetId, packetReader, bot); break;
        case State.Play: HandlePlayPackets(packetId, packetReader, bot, CompressionThreshold, writer); break;
      }
    }

    private void HandleLoginPackets(int packetId, BinaryReader reader, Bot bot)
    {
      switch (packetId)
      {
        case 0x03: bot.CompressionThreshold = VarInt.ReadVarInt(reader); break;
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
          if (bot.JoinCount == 2) bot.CompassUsed = false;
          break;

        case 0x2D: // Open Window
          byte windowId = reader.ReadByte();
          Console.WriteLine($">>> [MENU] Janela {windowId} aberta. Clicando no RankUP...");
          Task.Run(async () =>
          {
            await Task.Delay(3000);
            InventoryActions.SendClickWindow(bot.GetWriter(), windowId, 10, compression, bot);
            Console.WriteLine($">>> [SISTEMA] Clique enviado no Slot 10!");
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
            bwPos.Write(x); bwPos.Write(y); bwPos.Write(z);
            bwPos.Write(yaw); bwPos.Write(pitch);
            bwPos.Write(true);
            PacketWriter.SendPacket(bot.GetWriter(), msPos.ToArray(), compression);
          }
          break;

        case 0x02: // Chat Message
          string rawJson = McString.ReadString(reader);
          chatHandler.ListenChat(packetId, reader, bot, compression, writer, rawJson);
          break;

        case 0x30: // Window Items
          byte winId = reader.ReadByte();
          short count = reader.ReadInt16();
          if (winId == 0 && !bot.CompassUsed && bot.CanMove)
          {
            bot.CompassUsed = true;
            Task.Run(async () =>
            {
              Console.WriteLine(">>> [SISTEMA] Inventário recebido. Aguardando estabilização...");
              await Task.Delay(4000);
              for (int i = 0; i < 10; i++)
              {
                bot.PosX += 0.2;
                Movement.SendSmallMovement(bot.GetWriter(), bot.PosX, bot.PosY, bot.PosZ, compression);
                Console.WriteLine($">>> [BOT] Andando: {bot.PosX}");
                await Task.Delay(120);
              }
              Console.WriteLine(">>> [BOT] Clicando com o Direito (Slot padrão)...");
            });
          }
          if (reader.BaseStream.Position < reader.BaseStream.Length)
            reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
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