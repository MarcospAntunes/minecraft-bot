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

      VarInt.WriteVarInt(packet, 0x08); // Player Block Placement
      packet.Write((long)-1);  // Bloco: -1
      packet.Write((byte)255); // Direção: 255
      packet.Write((short)-1); // Item: -1 (usar item da mão atual)
      packet.Write((byte)0);   // Cursor X
      packet.Write((byte)0);   // Cursor Y
      packet.Write((byte)0);   // Cursor Z

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
    }
  }
}