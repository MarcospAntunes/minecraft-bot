using Utils;
using Network;
using Core;

namespace Protocol.Packets
{
  public static class LoginPacket
  {
    public static void SendLoginStart(BinaryWriter writer, string username, int compression, Bot bot)
    {
      using var ms = new MemoryStream();
      using var packet = new BinaryWriter(ms);

      VarInt.WriteVarInt(packet, 0x00);
      McString.WriteString(packet, username);

      PacketWriter.SendPacket(writer, ms.ToArray(), compression);
      bot.SetState(State.Login);
    }
  }
}