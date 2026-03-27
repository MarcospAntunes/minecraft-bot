using Utils;
using Protocol.Packets;
using Network;
using Protocol;
using Features;
using Models;

namespace Core
{
  public class Bot(Guid Id, string username, string server, int port, string playerName, string password, LogService logService, ChatService chatService)
  {
    // Conexão
    private Connection connection = new Connection(logService);
    private BinaryReader reader;
    private BinaryWriter writer;

    // Estado
    private State state = State.Handshake;
    public bool MenuHandled = false;
    public bool LoginHandled = false;

    public int CompressionThreshold = -1;
    public bool Logged = false;
    public bool CanMove = false;
    public bool stopTryClickButton = false;

    // Posição
    public double PosX, PosY, PosZ, Yaw, Pitch;

    // Dados
    private string username = username;
    public string playerName = playerName;
    private string password = password;
    private string server = server;
    private int port = port;
    public bool CompassUsed = false;
    public short ActionNumber = 0;
    public int JoinCount = 0;
    public Guid Id = Id;
    private PacketHandler handler = new PacketHandler(logService, chatService);
    private readonly ChatHandler chatHandler = new ChatHandler(chatService, logService);
    private readonly LogService _logService = logService;

    public void Start()
    {
      (writer, reader) = connection.Connect(server, port);

      HandshakePacket.SendHandshake(writer, server, port, CompressionThreshold);
      LoginPacket.SendLoginStart(writer, username, CompressionThreshold, this);

      Task.Run(() => chatHandler.StartChatPollingAsync(this, writer));
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
          _ = _logService.AddMessage(new Log { Message = $"Erro no Loop: {ex.Message}" });
          break;
        }
      }
    }

    // Apenas métodos necessários
    public void SetState(State newState) => state = newState;
    public BinaryWriter GetWriter() => writer;
    public BinaryReader GetReader() => reader;
    public string GetPassword() => password;
    public State GetState() => state;
  }
}