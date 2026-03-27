using System.Net.Sockets;
using Models;

namespace Network
{
  public class Connection(LogService logService)
  {
    private BinaryReader reader;
    private BinaryWriter writer;
    private readonly LogService _logService = logService;

    /// <summary>
    /// Conecta no servidor e retorna o BinaryWriter e BinaryReader
    /// </summary>
    public (BinaryWriter, BinaryReader) Connect(string server, int port)
    {
      var client = new TcpClient();
      client.Connect(server, port);

      var stream = client.GetStream();
      writer = new BinaryWriter(stream);
      reader = new BinaryReader(stream);

      _ = _logService.AddMessage(new Log {Message = ">>> [CONEXÃO] Conectado ao servidor: " + server + ":" + port});
      return (writer, reader);
    }

    /// <summary>
    /// Lê uma quantidade exata de bytes do stream
    /// </summary>
    public static byte[] ReadFully(BinaryReader reader, int length)
    {
      byte[] buffer = new byte[length];
      int offset = 0;

      while (offset < length)
      {
        int read = reader.BaseStream.Read(buffer, offset, length - offset);
        if (read == 0)
          throw new EndOfStreamException("Conexão fechada inesperadamente.");

        offset += read;
      }

      return buffer;
    }
  }
}