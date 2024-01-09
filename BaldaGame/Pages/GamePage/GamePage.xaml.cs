using BaldaGame.Controllers;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BaldaGame.Pages.GamePage
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class GamePage : Page
   {
      GameController controller;
      GameDataController gameDataController;
      MainPlayerController mainPlayerController;
      SecondPlayerController secondPlayerController;

      public GamePage()
      {
         InitializeComponent();
         var app = App.Instance;
         controller = app.GameController;
         gameDataController = controller.DataController;

         mainPlayerController = app.MainPlayerController;
         secondPlayerController = app.SecondPlayerController;

         controller.Init(MainGrid, this);
      }

      private async void CancelGameButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
      {
         await controller.FinalizeGame();
      }

      private void ReverseStepButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
      {
         controller.CancelMove();
      }

      private void SkipStepButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
      {
         controller.SkipMove();
      }
   }
}
