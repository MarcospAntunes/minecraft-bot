using System.IO.Compression;
using Utils;

namespace Network
{
  public class PacketWriter
  {
    public static void SendPacket(BinaryWriter writer, byte[] data, int compressionThreshold)
    {
      using MemoryStream packetMs = new MemoryStream();
      using BinaryWriter packetWriter = new BinaryWriter(packetMs);

      if (compressionThreshold >= 0)
      {
        if (data.Length >= compressionThreshold)
        {
          using MemoryStream compressed = new MemoryStream();

          using (var z = new ZLibStream(compressed, CompressionLevel.Fastest, true))
          {
            z.Write(data, 0, data.Length);
          }

          byte[] comp = compressed.ToArray();

          VarInt.WriteVarInt(packetWriter, VarInt.GetVarIntSize(data.Length) + comp.Length);
          VarInt.WriteVarInt(packetWriter, data.Length);
          packetWriter.Write(comp);
        }
        else
        {
          VarInt.WriteVarInt(packetWriter, VarInt.GetVarIntSize(0) + data.Length);
          VarInt.WriteVarInt(packetWriter, 0);
          packetWriter.Write(data);
        }
      }
      else
      {
        VarInt.WriteVarInt(packetWriter, data.Length);
        packetWriter.Write(data);
      }

      writer.BaseStream.Write(packetMs.ToArray(), 0, (int)packetMs.Length);
      writer.Flush();
    }
  }
}