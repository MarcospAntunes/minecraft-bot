using Utils;
using Network;

namespace Protocol.Packets
{
  public static class HandshakePacket
  {
    public static void SendHandshake(BinaryWriter writer, string server, int port, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x00); // ID Handshake
      VarInt.WriteVarInt(packet, 47);   // Protocol 1.8/1.16.5

      McString.WriteString(packet, server);

      ushort portValue = (ushort)port;
      packet.Write((byte)(portValue >> 8));
      packet.Write((byte)(portValue & 0xFF));

      VarInt.WriteVarInt(packet, 2); // Next state = Login

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }
  }
}