using Utils;
using Network;

namespace Features
{
  public static class Movement
  {
    public static void SendPositionLook(BinaryWriter writer, double x, double y, double z, float yaw, float pitch, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x06);
      packet.Write(x);
      packet.Write(y);
      packet.Write(z);
      packet.Write(yaw);
      packet.Write(pitch);
      packet.Write(true);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }

    public static void SendLook(BinaryWriter writer, float yaw, float pitch, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x05);
      packet.Write(yaw);
      packet.Write(pitch);
      packet.Write(true);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }

    public static void SendSmallMovement(BinaryWriter writer, double x, double y, double z, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x04);
      packet.Write(x);
      packet.Write(y);
      packet.Write(z);
      packet.Write(true);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }

    public static void SendTeleportConfirm(BinaryWriter writer, int teleportId, int compression)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x00);
      VarInt.WriteVarInt(packet, teleportId);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
    }
  }
}