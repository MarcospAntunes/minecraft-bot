using Utils;
using Network;
using Core;

namespace Protocol.Packets
{
  public class LoginPacket
  {
    public static void SendLoginStart(BinaryWriter writer, string username, int CompressionThreshold, Bot bot)
    {
      MemoryStream ms = new MemoryStream();
      BinaryWriter packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x00);
      McString.WriteString(packet, username);

      PacketWriter.SendPacket(writer, ms.ToArray(), CompressionThreshold);

      bot.SetState(State.Login);
    }
  }
}