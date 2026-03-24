
using Utils;
using Protocol.Packets;
using Network;
using Protocol;

namespace Core
{
  public class Bot
  {
    // Conexão
    private Connection connection;
    private BinaryReader reader;
    private BinaryWriter writer;

    // Estado
    private State state = State.Handshake;
    public byte CurrentWindowId;

    public int CompressionThreshold = -1;
    public bool Logged = false;
    public bool LoginSent = false;
    public bool CanMove = false;

    // Posição
    public double PosX, PosY, PosZ;

    // Controle de tempo
    private DateTime lastMove = DateTime.Now;
    private DateTime lastLoginAttempt = DateTime.MinValue;

    private Random rnd = new Random();

    // Dados
    private string username;
    public string playerName;
    private string server;
    private int port;
    public bool CompassUsed = false;
    public short ActionNumber = 0;
    public int JoinCount = 0;

    private PacketHandler handler;

    public Bot(string username, string server, int port, string playerName)
    {
      this.username = username;
      this.server = server;
      this.port = port;
      this.playerName = playerName;

      connection = new Connection();
      handler = new PacketHandler();
    }

    public void Start()
    {
      (writer, reader) = connection.Connect(server, port);

      HandshakePacket.SendHandshake(writer, server, port, CompressionThreshold);
      LoginPacket.SendLoginStart(writer, username, CompressionThreshold, this);

      Loop();
    }

    private void Loop()
    {
      while (true)
      {
        try
        {
          var packetData = PacketReader.ReadPacket(reader, CompressionThreshold);
          if (packetData == null) continue;

          using var ms = new MemoryStream(packetData);
          using var packetReader = new BinaryReader(ms);
          int packetId = VarInt.ReadVarInt(packetReader);

          handler.Handle(packetId, packetReader, this, state, CompressionThreshold, writer);

          // Limpa bytes restantes
          if (packetReader.BaseStream.Position < packetReader.BaseStream.Length)
            packetReader.ReadBytes((int)(packetReader.BaseStream.Length - packetReader.BaseStream.Position));
        }
        catch (Exception ex)
        {
          Console.WriteLine("Erro no Loop: " + ex.Message);
          break;
        }
      }
    }

    public void SetState(State newState) => state = newState;
    public BinaryWriter GetWriter() => writer;
    public BinaryReader GetReader() => reader;
  }
}