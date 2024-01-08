using System.Threading.Tasks;

namespace BaldaGame.Repository
{
   public delegate void TimerTickerEventHandler();
   
   public class TimerTickerRepository
   {

      public static TimerTickerRepository Instance { get; } = new();

      public event TimerTickerEventHandler? TimerTickerEvent;

      public void DisableTicker()
      {
         isTimerEnabled = false;
      }

      public void EnableTicker()
      {
         if (isTimerEnabled) return;

         isTimerEnabled = true;
         RunTicker();
      }

      TimerTickerRepository() {
         RunTicker();
      }

      bool isTimerEnabled = true;

      async Task RunTicker()
      {
         while (isTimerEnabled)
         {
            TimerTickerEvent?.Invoke();
            await Task.Delay(1000);
         }
      }
   }
}
