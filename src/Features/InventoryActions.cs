using Utils;
using Network;
using Core;

namespace Features
{
  public static class InventoryActions
  {
    public static void SendRightClick(BinaryWriter writer, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x08); // Player Block Placement (1.8)

      // POSIÇÃO (-1, -1, -1)
      packet.Write((int)-1); // X
      packet.Write((byte)255); // Y (unsigned byte)
      packet.Write((int)-1); // Z

      // FACE
      packet.Write((byte)255);

      // HELD ITEM (precisa ser presente!)
      packet.Write((short)-1); // -1 = sem item (funciona pra bússola em muitos servidores)

      // CURSOR
      packet.Write((byte)0);
      packet.Write((byte)0);
      packet.Write((byte)0);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);

    }

    public static void SendHeldItemChange(BinaryWriter writer, short slot, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x09);
      packet.Write(slot);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }

    public static void SendClickWindow(BinaryWriter writer, byte windowId, short slot, int compression, Bot bot)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x0E);
      packet.Write(windowId);
      packet.Write(slot);
      packet.Write((byte)0); // Botão esquerdo
      packet.Write((short)bot.ActionNumber); // ActionNumber atual
      packet.Write((byte)0);   // Mode 0
      packet.Write((short)-1); // Item vazio

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);

      bot.ActionNumber++; 
      packet.Write(bot.ActionNumber);
    }
  }
}