using Utils;
using Network;

namespace Protocol.Packets
{
  public static class PlayPacket
  {
    public static void SendKeepAlive(BinaryWriter writer, long id, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x0F);
      packet.Write(id);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }

    public static void SendClientSettings(BinaryWriter writer, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x08);
      McString.WriteString(packet, "pt_BR");
      packet.Write((byte)8);
      VarInt.WriteVarInt(packet, 0);
      packet.Write(true);
      packet.Write((byte)127);
      VarInt.WriteVarInt(packet, 1);
      packet.Write(false);
      packet.Write(true);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }

    public static void SendClientStatus(BinaryWriter writer, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x16);
      VarInt.WriteVarInt(packet, 0);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }

    public static void SendChatMessage(BinaryWriter writer, string message, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x01);
      McString.WriteString(packet, message);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }
  }
}