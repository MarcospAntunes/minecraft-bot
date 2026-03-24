using System.IO.Compression;
using Utils;

namespace Network
{
  public static class PacketWriter
  {
    public static void SendPacket(BinaryWriter writer, byte[] data, int compressionThreshold)
    {
      using var packetMs = new MemoryStream();
      using var packetWriter = new BinaryWriter(packetMs);

      if (compressionThreshold >= 0)
      {
        if (data.Length >= compressionThreshold)
        {
          using var compressedStream = new MemoryStream();
          using (var z = new ZLibStream(compressedStream, CompressionLevel.Fastest, true))
            z.Write(data, 0, data.Length);

          byte[] compressed = compressedStream.ToArray();
          VarInt.WriteVarInt(packetWriter, VarInt.GetVarIntSize(data.Length) + compressed.Length);
          VarInt.WriteVarInt(packetWriter, data.Length);
          packetWriter.Write(compressed);
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