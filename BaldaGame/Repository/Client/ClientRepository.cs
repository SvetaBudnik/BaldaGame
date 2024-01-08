using BaldaGame.Repository.Server;

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.Media.ClosedCaptioning;

namespace BaldaGame.Repository.Client
{
   public class ClientRepository
   {
      TcpClient client;

      StreamWriter? Writer { get; set; }
      StreamReader? Reader { get; set; }

      private bool _isClientRunning = false;
      private bool _isUdpScanRunning = false;
      private const int _udpSendPort = 8585;
      private const string _udpMessage = "Balda The game";

      public event ClientEventHandler? Notify;

      public ClientRepository()
      {
         client = new TcpClient();
      }

      public async Task<bool> TryConnectToServer()
      {
         Debug.WriteLine("Client was started");

         _isClientRunning = true;
         var endpoint = await GetIPEndPointAsync();
         if (endpoint is null) return false;

         Debug.WriteLine("Cliend found new server");

         try
         {
            client.Connect(endpoint);
            var stream = client.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream);

            Debug.WriteLine("Client was connected to server");
         }
         catch (Exception ex)
         {
            Debug.WriteLine(ex.Message);
            return false;
         }

         //_ = ListenMessages();
         new Thread(async () => await ListenMessages()).Start();

         var msg = new ServerMessage()
         {
            Id = ServerMessageIds.PlayerConnected
         };
         NotifyListeners(msg);

         return true;
      }

      public void SendMessage(ClientMessage message)
      {
         var encoded = message.Encode();
         Writer?.Write(encoded);
         Writer?.Flush();
      }

      public async Task ListenMessages()
      {
         while (client.Connected && _isClientRunning)
         {
            try
            {
               if (Reader is null) break;

               var encoded = await Reader.ReadLineAsync();
               if (encoded is null) continue;
               var message = ServerMessage.Decode(encoded);
               NotifyListeners(message);
            }
            catch
            {
               var message = new ServerMessage()
               {
                  Id = ServerMessageIds.PlayerDisconnected
               };
               NotifyListeners(message);
               break;
            }
         }
      }

      public void NotifyListeners(ServerMessage message)
      {
         Debug.WriteLine($"Клиент получил новое сообщение: {message.Id}");

         switch (message.Id)
         {
            case ServerMessageIds.PlayerConnected: OnPlayerConnected(message); break;
            case ServerMessageIds.PlayerInfo: OnPlayerInfo(message); break;
            case ServerMessageIds.MainWordWasChoosen: OnMainWordWasChoosen(message); break;
            case ServerMessageIds.PlayerDisconnected: OnPlayerDisconnected(message); break;
            case ServerMessageIds.PlayerCancelledGame: OnPlayerCancelledGame(message); break;
            case ServerMessageIds.PlayerMakeMove: OnPlayerMakeMove(message); break;
            case ServerMessageIds.PlayerSkipMove: OnPlayerSkipMove(message); break;
            case ServerMessageIds.PlayerAgreedWithWord: OnPlayerAgreedWithWord(message); break;
            case ServerMessageIds.PlayerDisagreedWithWord: OnPlayerDisagreedWithWord(message); break;
            default: throw new NotImplementedException();
         }
      }

      void OnPlayerConnected(ServerMessage message)
      {
         var playerEvent = new ClientPlayerConnected();
         Notify?.Invoke(playerEvent);
      }

      void OnPlayerInfo(ServerMessage message)
      {
         var playerInfo = Server.PlayerInfoMessage.TryFromMessage(message);
         if (playerInfo != null)
         {
            var playerEvent = new ClientGotPlayerInfo(playerInfo.PersonName, playerInfo.iconTag);
            Notify?.Invoke(playerEvent);
         }
         else Debug.WriteLine($"ClientRepository.OnPlayerInfo: Что-то пошло не так, сообщение не расшифровалось.\n Id:{message.Id}, Data: {message.Data}");
      }

      void OnPlayerMakeMove(ServerMessage message)
      {
         var playerMove = Server.PlayerMakeMoveMessage.TryFromMessage(message);
         if (playerMove != null)
         {
            var playerEvent = new ClientPlayerMakeMove(
               playerMove.ChoosenWord,
               playerMove.NewChar,
               playerMove.CharX,
               playerMove.CharY
            );
            Notify?.Invoke(playerEvent);
         }
         else Debug.WriteLine($"ClientRepository.OnPlayerMakeMove: Что-то пошло не так, сообщение не расшифровалось.\n Id:{message.Id}, Data: {message.Data}");
      }

      void OnMainWordWasChoosen(ServerMessage message)
      {
         var playerMove = Server.MainWordWasChoosenMessage.TryFromMessage(message);
         if (playerMove != null)
         {
            var playerEvent = new ClientServerChoosedMainWord(
               playerMove.ChoosenWord
            );
            Notify?.Invoke(playerEvent);
         }
         else Debug.WriteLine($"ClientRepository.OnMainWordWasChoosen: Что-то пошло не так, сообщение не расшифровалось.\n Id:{message.Id}, Data: {message.Data}");
      }

      void OnPlayerSkipMove(ServerMessage message)
      {
         var playerEvent = new ClientPlayerSkipMove();
         Notify?.Invoke(playerEvent);
      }

      void OnPlayerDisconnected(ServerMessage message)
      {
         var playerEvent = new ClientPlayerDisconnected();
         Notify?.Invoke(playerEvent);
      }

      void OnPlayerCancelledGame(ServerMessage message)
      {
         var clientEvent = new ClientPlayerCancelledGame();
         Notify?.Invoke(clientEvent);
      }

      void OnPlayerAgreedWithWord(ServerMessage message)
      {
         var clientEvent = new ClientPlayerAgreedWithWord();
         Notify?.Invoke(clientEvent);
      }

      void OnPlayerDisagreedWithWord(ServerMessage message)
      {
         var clientEvent = new ClientPlayerDisagreedWithWord();
         Notify?.Invoke(clientEvent);
      }

      async Task<IPEndPoint?> GetIPEndPointAsync()
      {
         using var udpClient = new UdpClient();
         udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, _udpSendPort));
         _isUdpScanRunning = true;

         while (_isClientRunning && _isUdpScanRunning)
         {
            var cancellation = new CancellationTokenSource();
            var asyncResult = udpClient.ReceiveAsync(cancellation.Token);
            await Task.Delay(1000);

            if (!asyncResult.IsCompleted)
            {
               cancellation.Cancel();
               cancellation.Dispose();
               continue;
            }

            cancellation.Dispose();

            var result = await asyncResult;
            var endpoint = result.RemoteEndPoint;
            var buffer = result.Buffer;

            var messageString = Encoding.UTF8.GetString(buffer);
            if (messageString != _udpMessage)
            {
               continue;
            }

            _isUdpScanRunning = false;
            return endpoint;
         }
         _isUdpScanRunning = false;

         return null;
      }

      public void Stop()
      {
         Debug.WriteLine("Client was stopped");

         _isClientRunning = false;
         _isUdpScanRunning = false;
         client.Close();
      }

      ~ClientRepository()
      {
         Stop();
      }
   }
}
