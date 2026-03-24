using System.Net.Sockets;

namespace Network
{
  class Connection
  {
    private BinaryReader reader;
    private BinaryWriter writer;

    public (BinaryWriter, BinaryReader) Connect(string server, int port)
    {
      TcpClient client = new TcpClient();
      client.Connect(server, port);

      NetworkStream stream = client.GetStream();
      writer = new BinaryWriter(stream);
      reader = new BinaryReader(stream);

      Console.WriteLine("Conectado!");

      return (writer, reader);
    }

    public static byte[] ReadFully(BinaryReader reader, int length)
    {
      byte[] buffer = new byte[length];
      int offset = 0;

      while (offset < length)
      {
        int read = reader.BaseStream.Read(buffer, offset, length - offset);

        if (read == 0)
          throw new EndOfStreamException("Conexão fechada");

        offset += read;
      }

      return buffer;
    }
  }
}
