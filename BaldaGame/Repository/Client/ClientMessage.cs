using System.Text;
using System.Text.Json;

namespace BaldaGame.Repository.Client
{
   public enum ClientMessageIds
   {
      PlayerConnected,
      PlayerInfo,
      PlayerMakeMove,
      PlayerSkipMove,
      PlayerCancelledGame,
      PlayerDisconnected,
      PlayerAgreedWithWord,
      PlayerDisagreedWithWord
   }

   public class ClientMessage
   {
      public ClientMessageIds Id { get; set; } = 0;
      public string Data { get; set; } = "";

      public string Encode()
      {
         return $"{(int)Id},{Data}";
      }

      public static ClientMessage Decode(string message)
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

         var mes = new ClientMessage
         {
            Id = (ClientMessageIds)int.Parse(idString.ToString()),
            Data = dataString.ToString()
         };
         return mes;
      }
   }

   public class PlayerInfoMessage(string personName, string iconTag)
   {
      public static ClientMessageIds Id { get => ClientMessageIds.PlayerInfo; }

      public string PersonName { get; set; } = personName;
      public string IconTag { get; set; } = iconTag;

      public static PlayerInfoMessage? TryFromMessage(ClientMessage message)
      {
         if (message.Id != Id) return null;
         return JsonSerializer.Deserialize<PlayerInfoMessage>(message.Data);
      }

      public ClientMessage GetClientMessage()
      {
         var message = new ClientMessage
         {
            Id = Id,
            Data = JsonSerializer.Serialize(this)
         };

         return message;
      }
   }

   public class PlayerMakeMoveMessage(string choosenWord, string newChar, int charX, int charY)
   {
      public static ClientMessageIds Id { get => ClientMessageIds.PlayerMakeMove; }

      public string NewChar { get; set; } = newChar;
      public int CharX { get; set; } = charX;
      public int CharY { get; set; } = charY;

      public string ChoosenWord { get; set; } = choosenWord;

      public static PlayerMakeMoveMessage? TryFromMessage(ClientMessage message)
      {
         if (message.Id != Id) return null;
         return JsonSerializer.Deserialize<PlayerMakeMoveMessage>(message.Data);
      }

      public ClientMessage GetClientMessage()
      {
         var message = new ClientMessage
         {
            Id = Id,
            Data = JsonSerializer.Serialize(this)
         };

         return message;
      }
   }
}


