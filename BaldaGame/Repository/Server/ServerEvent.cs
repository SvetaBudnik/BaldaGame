namespace BaldaGame.Repository.Server
{
   public delegate void ServerEventHandler(IServerEvent serverEvent);

   public interface IServerEvent { }

   public class ServerPlayerConnected() : IServerEvent { }

   public class ServerGotPlayerInfo(string username, string iconTag) : IServerEvent
   {
      public string Username { get; set; } = username;
      public string IconTag { get; set; } = iconTag;
   }

   public class ServerPlayerMakeMove(string word, string newChar, int charX, int charY) : IServerEvent
   {
      public string NewChar { get; set; } = newChar;
      public int CharX { get; set; } = charX;
      public int CharY { get; set; } = charY;

      public string ChoosenWord { get; set; } = word;
   }

   public class ServerPlayerSkipMove() : IServerEvent { }

   public class ServerPlayerDisconnected() : IServerEvent { }

   public class ServerPlayerCancelledGame() : IServerEvent { }

   public class ServerPlayerAgreedWithWord() :IServerEvent { }

   public class ServerPlayerDisagreedWithWord() : IServerEvent { }
}
