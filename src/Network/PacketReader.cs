using System.IO.Compression;
using Utils;

namespace Network
{
  public static class PacketReader
  {
    public static byte[]? ReadPacket(BinaryReader reader, int compressionThreshold)
    {
      int packetLength = VarInt.ReadVarInt(reader);
      if (packetLength <= 0) return null;

      byte[] fullData = Connection.ReadFully(reader, packetLength);

      using var ms = new MemoryStream(fullData);
      using var packetReader = new BinaryReader(ms);

      if (compressionThreshold >= 0)
      {
        int dataLength = VarInt.ReadVarInt(packetReader);
        if (dataLength == 0)
          return packetReader.ReadBytes((int)(ms.Length - ms.Position));

        // Pula header ZLib
        packetReader.ReadByte();
        packetReader.ReadByte();

        using var deflate = new DeflateStream(ms, CompressionMode.Decompress);
        using var output = new MemoryStream();
        deflate.CopyTo(output);
        return output.ToArray();
      }

      return fullData;
    }
  }
}