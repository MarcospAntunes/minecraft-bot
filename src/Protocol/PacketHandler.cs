using Utils;
using Core;
using Network;
using Features;

namespace Protocol
{
  public class PacketHandler
  {
    private ChatHandler chatHandler = new ChatHandler();
    public void Handle(int packetId, BinaryReader packetReader, Bot bot, State state, int CompressionThreshold, BinaryWriter writer)
    {
      if (state == State.Login)
      {
        if (packetId == 0x03)
        { // Set Compression
          bot.CompressionThreshold = VarInt.ReadVarInt(packetReader);
        }
        else if (packetId == 0x02)
        { // Login Success 1.8
          string uuid = McString.ReadString(packetReader); // UUID na 1.8 é String
          string name = McString.ReadString(packetReader); // Username
          bot.SetState(State.Play);
          bot.Logged = true;
          Console.WriteLine($">>> [LOGIN 1.8] Logado como: {name}");
        }
      }
      else if (state == State.Play)
      {
        switch (packetId)
        {
          case 0x01: // Join Game
            bot.JoinCount++;
            Console.WriteLine($">>> [MUNDO] Servidor #{bot.JoinCount} conectado.");
            // Resetamos o CompassUsed se mudarmos de servidor para garantir o clique no lobby real
            if (bot.JoinCount == 2) bot.CompassUsed = false;
            break;

          case 0x2D: // Open Window (Menu da Bússola)
            byte windowId = packetReader.ReadByte();
            Console.WriteLine($">>> [MENU] Janela {windowId} aberta. Clicando no RankUP...");

            Task.Run(async () =>
              {
                await Task.Delay(3000);
                // RankUp = 10
                short slotRankUp = 10;

                InventoryActions.SendClickWindow(bot.getWriter(), windowId, slotRankUp, CompressionThreshold, bot);
                Console.WriteLine($">>> [SISTEMA] Clique enviado no Slot {slotRankUp}!");
              });
            break;
          case 0x44: // World Border (1.8)
          case 0x3E: // World Border (Alternativo 1.8)
                     // Lê os dados da borda para limpar o buffer
            int action = VarInt.ReadVarInt(packetReader);
            // Dependendo da action, o tamanho muda, mas o bot só precisa pular
            // Para simplificar, vamos deixar o try-catch do Loop cuidar do resto
            break;
          case 0x00: // Keep Alive (Server -> Client na 1.8)
            int kaId = VarInt.ReadVarInt(packetReader); // VarInt na 1.8!
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
              VarInt.WriteVarInt(bw, 0x00);
              VarInt.WriteVarInt(bw, kaId);
              PacketWriter.SendPacket(bot.getWriter(), ms.ToArray(), CompressionThreshold);
            }
            break;

          case 0x08: // Position and Look
            double x = packetReader.ReadDouble();
            double y = packetReader.ReadDouble();
            double z = packetReader.ReadDouble();
            float yaw = packetReader.ReadSingle();
            float pitch = packetReader.ReadSingle();
            byte flags = packetReader.ReadByte();

            using (MemoryStream msPos = new MemoryStream())
            using (BinaryWriter bwPos = new BinaryWriter(msPos))
            {
              VarInt.WriteVarInt(bwPos, 0x06);
              bwPos.Write(x); bwPos.Write(y); bwPos.Write(z);
              bwPos.Write(yaw); bwPos.Write(pitch);
              bwPos.Write(true);
              PacketWriter.SendPacket(bot.getWriter(), msPos.ToArray(), CompressionThreshold);
            }
            break; ;

          case 0x02: // Chat Message (1.8)
            string rawJson = McString.ReadString(packetReader);
            chatHandler.ListenChat(packetId, packetReader, bot, CompressionThreshold, writer, rawJson);
            break;
          case 0x30: // Window Items
            byte winId = packetReader.ReadByte();
            short count = packetReader.ReadInt16();

            // Age apenas quando o inventário do Lobby Real chegar
            if (winId == 0 && !bot.CompassUsed && bot.CanMove)
            {
              bot.CompassUsed = true;
              Task.Run(async () =>
              {
                Console.WriteLine(">>> [SISTEMA] Inventário recebido. Aguardando estabilização...");
                await Task.Delay(4000); // MUITO IMPORTANTE (anti-cheat)
                // Andar para frente
                for (int i = 0; i < 10; i++)
                {
                  bot.PosX += 0.2;

                  Movement.SendSmallMovement(
                    bot.getWriter(),
                    bot.PosX,
                    bot.PosY,
                    bot.PosZ,
                    CompressionThreshold
                  );

                  Console.WriteLine($">>> [BOT] Andando: {bot.PosX}");

                  await Task.Delay(120); // mais humano
                }
                // Clica com o direito
                //InventoryActions.SendRightClick(bot.getWriter(), CompressionThreshold);
                await Task.Delay(500);
                // vamos clicar direto no que já estiver na mão (provavelmente slot 0)
                Console.WriteLine(">>> [BOT] Clicando com o Direito (Slot padrão)...");
                //InventoryActions.SendRightClick(bot.getWriter(), CompressionThreshold);
              });
            }

            if (packetReader.BaseStream.Position < packetReader.BaseStream.Length)
              packetReader.ReadBytes((int)(packetReader.BaseStream.Length - packetReader.BaseStream.Position));
            break;

          case 0x32: // Confirm Transaction (1.8)
            byte tid_winId = packetReader.ReadByte();
            short actionId = packetReader.ReadInt16();
            bool accepted = packetReader.ReadBoolean();

            // Sincroniza o número da transação para o próximo clique
            bot.ActionNumber = actionId;

            using (MemoryStream msT = new MemoryStream())
            using (BinaryWriter bwT = new BinaryWriter(msT))
            {
              VarInt.WriteVarInt(bwT, 0x0F); // Resposta 0x0F
              bwT.Write(tid_winId);
              bwT.Write(actionId);
              bwT.Write(accepted);
              PacketWriter.SendPacket(bot.getWriter(), msT.ToArray(), CompressionThreshold);
            }
            break;
        }
      }
    }
  }
}
