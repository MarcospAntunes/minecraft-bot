using Utils;
using Network;
using Protocol;
using Protocol.Packets;
using Features;

namespace Core
{
  public class Bot
  {
    // 🔌 Conexão
    private Connection connection;
    private BinaryReader reader;
    private BinaryWriter writer;

    // 🔄 Estado do jogo
    private State state = State.Handshake;
    public byte CurrentWindowId;

    // 📦 Controle de compressão
    public int CompressionThreshold = -1;

    // 🤖 Estado do bot
    public bool Logged = false;
    public bool LoginSent = false;
    public bool CanMove = false;
    public bool PwAfkSent = false;
    public int PwAfkCount = 0;

    // 📍 Posição
    public double PosX = 0;
    public double PosY = 0;
    public double PosZ = 0;

    // ⏱ Controle de tempo
    private DateTime lastMove = DateTime.Now;
    private DateTime lastLoginAttempt = DateTime.MinValue;

    // 🎲 Random
    private Random rnd = new Random();

    // 👤 Dadoss
    private string username;
    public string playerName;
    private string server;
    private int port;
    public bool CompassUsed = false;
    public bool PickaxeClicked = false;
    public int[] Hotbar = new int[9];
    public short ActionNumber = 0;
    public int JoinCount = 0;

    // 🧩 Handler de pacotes
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

    //////////////////////////////////////////////////////////
    // 🚀 INÍCIO DO BOT
    //////////////////////////////////////////////////////////
    public void Start()
    {
      (writer, reader) = connection.Connect(server, port);
      Console.WriteLine("Conectado!");

      // 📡 Handshake + Login
      HandshakePacket.SendHandshake(writer, server, port, CompressionThreshold);
      LoginPacket.SendLoginStart(writer, username, CompressionThreshold, this);

      Loop();
    }

    //////////////////////////////////////////////////////////
    // 🔁 LOOP PRINCIPAL
    //////////////////////////////////////////////////////////
    // No Bot.cs - Loop principal
    private void Loop()
    {
      while (true)
      {
        try
        {
          var packetData = PacketReader.ReadPacket(reader, CompressionThreshold);
          if (packetData == null) continue;

          using (var ms = new MemoryStream(packetData))
          using (var packetReader = new BinaryReader(ms))
          {
            int packetId = VarInt.ReadVarInt(packetReader);

            // CHAMA O HANDLER
            handler.Handle(packetId, packetReader, this, state, CompressionThreshold, writer);

            // 🔥 LIMPEZA TOTAL: Descarta bytes não lidos do pacote atual
            // Isso impede que o lixo de um pacote estrague o ID do próximo
            if (packetReader.BaseStream.Position < packetReader.BaseStream.Length)
            {
              int remaining = (int)(packetReader.BaseStream.Length - packetReader.BaseStream.Position);
              packetReader.ReadBytes(remaining);
            }
          }
          
        }
        catch (Exception ex)
        {
          Console.WriteLine("Erro no Loop: " + ex.Message);
          break;
        }
      }
    }


    //////////////////////////////////////////////////////////
    // 🏃 MOVIMENTO (Feature)
    //////////////////////////////////////////////////////////
    private void HandleMovement()
    {
      if (Logged && CanMove && (DateTime.Now - lastMove).TotalSeconds > 10)
      {
        // ID 0x12 para Player Position na 1.16.5
        using MemoryStream ms = new MemoryStream();
        using BinaryWriter packet = new BinaryWriter(ms);

        VarInt.WriteVarInt(packet, 0x12);
        packet.Write(PosX + (rnd.NextDouble() * 0.1));
        packet.Write(PosY);
        packet.Write(PosZ);
        packet.Write(true); // OnGround

        PacketWriter.SendPacket(writer, ms.ToArray(), CompressionThreshold);

        lastMove = DateTime.Now;
        Console.WriteLine(">>> [MOVIMENTO] Pequeno ajuste de posição enviado.");
      }
    }

    //////////////////////////////////////////////////////////
    // 🔐 AUTO LOGIN (Feature)
    //////////////////////////////////////////////////////////
    public void HandleAutoLogin()
    {
      if (state != State.Play || Logged) return; // Se já logou, para.

      if ((DateTime.Now - lastLoginAttempt).TotalSeconds >= 10) // Tente a cada 10s no início
      {
        PlayPacket.SendChatMessage(writer, "/login Marquinhos_1212", CompressionThreshold);
        lastLoginAttempt = DateTime.Now;
        Console.WriteLine(">>> [BOT] Tentativa de login enviada.");
      }
    }

    //////////////////////////////////////////////////////////
    // 📥 USADO PELO PACKET HANDLER
    //////////////////////////////////////////////////////////

    public void SetState(State newState) { state = newState; }
    public BinaryWriter getWriter() { return writer; }
    public BinaryReader GetReader() { return reader; }
  }
}