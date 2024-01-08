namespace BaldaGame.Repository.Client
{
   public delegate void ClientEventHandler(IClientEvent clientEvent);

   public interface IClientEvent { }

   public class ClientPlayerConnected() : IClientEvent { }

   public class ClientGotPlayerInfo(string username, string iconTag) : IClientEvent
   {
      public string Username { get; set; } = username;
      public string IconTag { get; set; } = iconTag;
   }

   public class ClientPlayerMakeMove(string word, string newChar, int charX, int charY) : IClientEvent
   {
      public string NewChar { get; set; } = newChar;
      public int CharX { get; set; } = charX;
      public int CharY { get; set; } = charY;

      public string ChoosenWord { get; set; } = word;
   }

   public class ClientServerChoosedMainWord(string word) : IClientEvent
   {
      public string Word { get; set; } = word;
   }

   public class ClientPlayerSkipMove() : IClientEvent { }

   public class ClientPlayerDisconnected() : IClientEvent { }

   public class ClientPlayerCancelledGame() : IClientEvent { }

   public class ClientPlayerAgreedWithWord() : IClientEvent { }

   public class ClientPlayerDisagreedWithWord() : IClientEvent { }
}
