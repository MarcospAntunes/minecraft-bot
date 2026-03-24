namespace Utils
{
  public static class VarInt
  {
    public static int ReadVarInt(BinaryReader reader)
    {
      int numRead = 0;
      int result = 0;
      byte read;

      do
      {
        read = reader.ReadByte();
        int value = read & 0b01111111;
        result |= value << (7 * numRead);

        numRead++;
        if (numRead > 5)
          throw new Exception("VarInt muito grande");

      } while ((read & 0b10000000) != 0);

      return result;
    }

    public static void WriteVarInt(BinaryWriter writer, long value)
    {
      while ((value & -128) != 0)
      {
        writer.Write((byte)(value & 127 | 128));
        value >>= 7;
      }

      writer.Write((byte)value);
    }

    public static int GetVarIntSize(int value)
    {
      int size = 0;
      do
      {
        value >>= 7;
        size++;
      } while (value != 0);

      return size;
    }
  }
}