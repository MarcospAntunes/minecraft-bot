using Utils;
using Network;

namespace Protocol.Packets
{
  public class PlayPacket
  {
    public static void SendKeepAlive(BinaryWriter writer, long id, int CompressionThreshold)
    {
      using MemoryStream ms = new MemoryStream();
      using BinaryWriter packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x0F); // ID do Keep Alive Clientbound (ex: 1.16.5 é 0x0F)
      packet.Write(id); // Escreva como long (8 bytes), não VarInt!

      PacketWriter.SendPacket(writer, ms.ToArray(), CompressionThreshold);
    }

    public static void SendClientSettings(BinaryWriter writer, int compression)
    {
      using MemoryStream ms = new MemoryStream();
      using BinaryWriter packet = new BinaryWriter(ms);
      VarInt.WriteVarInt(packet, 0x08); // Client Settings (1.20.1)
      McString.WriteString(packet, "pt_BR");
      packet.Write((byte)8);
      VarInt.WriteVarInt(packet, 0);
      packet.Write(true);
      packet.Write((byte)127);
      VarInt.WriteVarInt(packet, 1);
      packet.Write(false); // Filtering
      packet.Write(true);  // Allow Server Listings
      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }


    public static void SendClientStatus(BinaryWriter writer, int CompressionThreshold)
    {
      using MemoryStream ms = new MemoryStream();
      using BinaryWriter packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x16);
      VarInt.WriteVarInt(packet, 0);

      PacketWriter.SendPacket(writer, ms.ToArray(), CompressionThreshold);
    }

    public static void SendChatMessage(BinaryWriter writer, string message, int CompressionThreshold)
    {
      using MemoryStream ms = new MemoryStream();
      using BinaryWriter packet = new BinaryWriter(ms);
      VarInt.WriteVarInt(packet, 0x01); // ID Chat Message na 1.8
      McString.WriteString(packet, message);
      PacketWriter.SendPacket(writer, ms.ToArray(), CompressionThreshold);
    }
  }
}