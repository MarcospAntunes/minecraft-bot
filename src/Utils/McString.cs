using System.Text;

namespace Utils
{
  public static class McString
  {
    public static void WriteString(BinaryWriter writer, string text)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(text);
      VarInt.WriteVarInt(writer, bytes.Length);
      writer.Write(bytes);
    }

   public static string ReadString(BinaryReader reader)
    {
      int length = VarInt.ReadVarInt(reader);
      byte[] bytes = reader.ReadBytes(length);
      return Encoding.UTF8.GetString(bytes);
    }
  }
}