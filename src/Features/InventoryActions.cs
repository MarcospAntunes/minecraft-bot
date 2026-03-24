using Utils;
using Network;
using Core;

namespace Features
{
  public class InventoryActions
  {
    public static void SendRightClick(BinaryWriter writer, int compression)
    {
      using MemoryStream ms = new MemoryStream();
      using BinaryWriter packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x08); // Player Block Placement

      // Coordenadas para clicar no "nada" na 1.8
      packet.Write((long)-1);  // Bloco: -1
      packet.Write((byte)255); // Direção: 255
      packet.Write((short)-1); // Item: -1 (usar item da mão atual)

      packet.Write((byte)0);   // Cursor X
      packet.Write((byte)0);   // Cursor Y
      packet.Write((byte)0);   // Cursor Z

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }

    public static void SendHeldItemChange(BinaryWriter writer, short slot, int CompressionThreshold)
    {
      using MemoryStream ms = new MemoryStream();
      using BinaryWriter packet = new BinaryWriter(ms);
      VarInt.WriteVarInt(packet, 0x09);
      packet.Write(slot); // Escreve 2 bytes (Int16)
      PacketWriter.SendPacket(writer, ms.ToArray(), CompressionThreshold);
    }

    public static void SendClickWindow(BinaryWriter writer, byte windowId, short slot, int compression, Bot bot)
    {
      using MemoryStream ms = new MemoryStream();
      using BinaryWriter packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x0E); // Click Window
      packet.Write(windowId);
      packet.Write(slot);
      packet.Write((byte)0); // Botão esquerdo

      // 🔥 Use o ActionNumber que o bot salvou do pacote 0x32 ou inicie em 1
      packet.Write((short)bot.ActionNumber);

      packet.Write((byte)0);   // Mode 0
      packet.Write((short)-1); // Item vazio

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }

  }
}