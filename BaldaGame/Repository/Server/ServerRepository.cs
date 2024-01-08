using BaldaGame.Repository.Client;

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaldaGame.Repository.Server
{
   public class ServerRepository
   {
      TcpListener? listener = null;
      ClientObject? client = null;

      private bool _isBroadcastEnabled = false;
      private bool _isServerEnabled = false;
      public bool IsServerEnabled => _isServerEnabled;

      private const int _udpSendPort = 8585;
      private const int _tcpPort = 8485;
      private const string _udpMessage = "Balda The game";

      public event ServerEventHandler? Notify;

      public ServerRepository() { }

      public void Run()
      {
         Debug.WriteLine("Server was started");

         listener = new TcpListener(IPAddress.Any, _tcpPort);
         listener.Start();
         _isServerEnabled = true;
         RunBroadcast();
         RunClientFinder();
      }

      public void Stop()
      {
         Debug.WriteLine("Server was stopped");

         _isServerEnabled = false;
         _isBroadcastEnabled = false;

         client?.Close();
         client = null;
         listener?.Stop();
         listener = null;
      }

      public void NotifyListeners(ClientMessage message)
      {
         Debug.WriteLine($"Сервер получил новое сообщение: {message.Id}");

         switch (message.Id)
         {
            case ClientMessageIds.PlayerConnected: OnPlayerConnected(message); break;
            case ClientMessageIds.PlayerInfo: OnPlayerInfo(message); break;
            case ClientMessageIds.PlayerDisconnected: OnPlayerDisconnected(message); break;
            case ClientMessageIds.PlayerCancelledGame: OnPlayerCancelledGame(message); break;
            case ClientMessageIds.PlayerMakeMove: OnPlayerMakeMove(message); break;
            case ClientMessageIds.PlayerSkipMove: OnPlayerSkipMove(message); break;
            case ClientMessageIds.PlayerAgreedWithWord: OnPlayerAgreedWithWord(message); break;
            case ClientMessageIds.PlayerDisagreedWithWord: OnPlayerDisagreedWithWord(message); break;
            default: throw new NotImplementedException();
         }
      }

      public void SendMessage(ServerMessage message)
      {
         client?.SendMessage(message);
      }

      void OnPlayerConnected(ClientMessage message)
      {
         var clientEvent = new ServerPlayerConnected();
         Notify?.Invoke(clientEvent);
      }

      void OnPlayerInfo(ClientMessage message)
      {
         var playerInfo = Client.PlayerInfoMessage.TryFromMessage(message);
         if (playerInfo != null)
         {
            var playerEvent = new ServerGotPlayerInfo(playerInfo.PersonName, playerInfo.IconTag);
            Notify?.Invoke(playerEvent);
         }
         else Debug.WriteLine($"ServerRepository.OnPlayerInfo: Что-то пошло не так, сообщение не расшифровалось.\n Id:{message.Id}, Data: {message.Data}");
      }

      void OnPlayerMakeMove(ClientMessage message)
      {
         var playerMove = Client.PlayerMakeMoveMessage.TryFromMessage(message);
         if (playerMove != null)
         {
            var playerEvent = new ServerPlayerMakeMove(
               playerMove.ChoosenWord,
               playerMove.NewChar,
               playerMove.CharX,
               playerMove.CharY
            );
            Notify?.Invoke(playerEvent);
         }
         else Debug.WriteLine($"ServerRepository.OnPlayerMakeMove: Что-то пошло не так, сообщение не расшифровалось.\n Id:{message.Id}, Data: {message.Data}");
      }

      void OnPlayerSkipMove(ClientMessage message)
      {
         var playerEvent = new ServerPlayerSkipMove();
         Notify?.Invoke(playerEvent);
      }

      void OnPlayerDisconnected(ClientMessage message)
      {
         var clientEvent = new ServerPlayerDisconnected();
         Notify?.Invoke(clientEvent);
      }

      void OnPlayerCancelledGame(ClientMessage message)
      {
         var clientEvent = new ServerPlayerCancelledGame();
         Notify?.Invoke(clientEvent);
      }

      void OnPlayerAgreedWithWord(ClientMessage message)
      {
         var clientEvent = new ServerPlayerAgreedWithWord();
         Notify?.Invoke(clientEvent);
      }

      void OnPlayerDisagreedWithWord(ClientMessage message)
      {
         var clientEvent = new ServerPlayerDisagreedWithWord();
         Notify?.Invoke(clientEvent);
      }

      private async Task RunBroadcast()
      {
         var bytes = Encoding.UTF8.GetBytes(_udpMessage);

         var udpClient = new UdpClient();
         var endpoint = new IPEndPoint(IPAddress.Any, _tcpPort);
         udpClient.Client.Bind(endpoint);

         _isBroadcastEnabled = true;
         while (_isServerEnabled && _isBroadcastEnabled)
         {
            udpClient.Send(bytes, bytes.Length, "255.255.255.255", _udpSendPort);
            await Task.Delay(1000);
         }

         udpClient.Close();
         _isBroadcastEnabled = true;
      }

      private async Task RunClientFinder()
      {
         if (listener == null)
            return;

         using var cancellation = new CancellationTokenSource();

         var acceptingTask = listener.AcceptTcpClientAsync(cancellation.Token);
         while (_isServerEnabled && !acceptingTask.IsCompleted)
         {
            await Task.Delay(200);
         }
         if (_isServerEnabled)
         {
            client = new ClientObject(await acceptingTask, this);
            _isBroadcastEnabled = false;
            listener.Stop();

            var msg = new ClientMessage()
            {
               Id = ClientMessageIds.PlayerConnected
            };
            NotifyListeners(msg);
         }
         else
         {
            cancellation.Cancel();
         }
      }
   }

   class ClientObject
   {
      readonly TcpClient client;
      readonly ServerRepository server;

      StreamWriter Writer { get; init; }
      StreamReader Reader { get; init; }

      public ClientObject(TcpClient tcpClient, ServerRepository serverRepository)
      {
         client = tcpClient;
         server = serverRepository;

         var stream = client.GetStream();

         Writer = new StreamWriter(stream);
         Reader = new StreamReader(stream);

         //_ = ListenMessages();
         new Thread(async () => await ListenMessages()).Start();
      }

      public void SendMessage(ServerMessage message)
      {
         var encoded = message.Encode();
         Writer.Write(encoded);
         Writer.Flush();
      }

      public async Task ListenMessages()
      {
         while (client.Connected && server.IsServerEnabled)
         {
            try
            {
               var encoded = await Reader.ReadLineAsync();
               if (encoded == null) continue;
               var message = ClientMessage.Decode(encoded);
               server.NotifyListeners(message);
            }
            catch
            {
               var message = new ClientMessage()
               {
                  Id = ClientMessageIds.PlayerDisconnected
               };
               server.NotifyListeners(message);
               break;
            }
         }
      }

      public void Close()
      {
         client.Close();
      }

      ~ClientObject()
      {
         Close();
      }
   }
}
