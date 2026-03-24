using Utils;
using Network;

namespace Protocol.Packets
{
  public class HandshakePacket
  {
    public static void SendHandshake(BinaryWriter writer, string server, int port, int CompressionThreshold)
    {
      MemoryStream ms = new MemoryStream();
      BinaryWriter packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x00);
      VarInt.WriteVarInt(packet, 47); // 1.16.5

      McString.WriteString(packet, server);

      ushort portValue = (ushort)port;
      packet.Write((byte)(portValue >> 8));
      packet.Write((byte)(portValue & 0xFF));

      VarInt.WriteVarInt(packet, 2);

      PacketWriter.SendPacket(writer, ms.ToArray(), CompressionThreshold);
    }
  }
}