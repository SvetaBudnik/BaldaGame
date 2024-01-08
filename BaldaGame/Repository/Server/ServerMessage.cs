using System.Text;
using System.Text.Json;

namespace BaldaGame.Repository.Server
{
   public enum ServerMessageIds
   {
      PlayerConnected,
      PlayerInfo,
      MainWordWasChoosen,
      PlayerMakeMove,
      PlayerSkipMove,
      PlayerCancelledGame,
      PlayerDisconnected,
      PlayerAgreedWithWord,
      PlayerDisagreedWithWord
   }

   public class ServerMessage
   {
      public ServerMessageIds Id { get; set; } = 0;
      public string Data { get; set; } = "";

      public string Encode()
      {
         return $"{(int)Id},{Data}";
      }

      public static ServerMessage Decode(string message)
      {
         StringBuilder idString = new();
         StringBuilder dataString = new();
         bool separatorReached = false;
         foreach (var ch in message)
         {
            if (!separatorReached)
            {
               if (ch == ',')
               {
                  separatorReached = true;
                  continue;
               }
               idString.Append(ch);
            }
            else
            {
               dataString.Append(ch);
            }
         }

         var mes = new ServerMessage
         {
            Id = (ServerMessageIds)int.Parse(idString.ToString()),
            Data = dataString.ToString()
         };
         return mes;
      }
   }

   public class PlayerInfoMessage(string personName, string iconTag)
   {
      public static ServerMessageIds Id { get => ServerMessageIds.PlayerInfo; }

      public string PersonName { get; set; } = personName;
      public string iconTag { get; set; } = iconTag;

      public static PlayerInfoMessage? TryFromMessage(ServerMessage message)
      {
         if (message.Id != Id) return null;
         return JsonSerializer.Deserialize<PlayerInfoMessage>(message.Data);
      }

      public ServerMessage GetServerMessage()
      {
         var message = new ServerMessage
         {
            Id = Id,
            Data = JsonSerializer.Serialize(this)
         };

         return message;
      }
   }

   public class MainWordWasChoosenMessage(string word)
   {
      public static ServerMessageIds Id { get => ServerMessageIds.MainWordWasChoosen; }
      
      public string ChoosenWord { get; set; } = word;

      public static MainWordWasChoosenMessage? TryFromMessage(ServerMessage message)
      {
         if (message.Id != Id) return null;
         return JsonSerializer.Deserialize<MainWordWasChoosenMessage>(message.Data);
      }

      public ServerMessage GetServerMessage()
      {
         var message = new ServerMessage
         {
            Id = Id,
            Data = JsonSerializer.Serialize(this)
         };

         return message;
      }
   }

   public class PlayerMakeMoveMessage(string word, string newChar, int charX, int charY)
   {
      public static ServerMessageIds Id { get => ServerMessageIds.PlayerMakeMove; }
      
      public string ChoosenWord { get; set; } = word;
      public string NewChar { get; set; } = newChar;
      public int CharX { get; set; } = charX;
      public int CharY { get; set; } = charY;

      public static PlayerMakeMoveMessage? TryFromMessage(ServerMessage message)
      {
         if (message.Id != Id) return null;
         return JsonSerializer.Deserialize<PlayerMakeMoveMessage>(message.Data);
      }

      public ServerMessage GetServerMessage()
      {
         var message = new ServerMessage
         {
            Id = Id,
            Data = JsonSerializer.Serialize(this)
         };

         return message;
      }
   }
}

