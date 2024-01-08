using BaldaGame.Pages.GamePage;
using BaldaGame.Pages.MenuPage.Controller;

using Microsoft.UI.Xaml.Controls;

using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BaldaGame.Pages.MenuPage.views
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class CreateLobbyView : Page
   {
      public CreateLobbyView()
      {
         InitializeComponent();
      }

      private void BackButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
      {
         var controller = MenuPageNavigationController.Instance;
         controller.NavigateBack();
      }

      public async Task AnimateTransitWhenConnect()
      {
         var delayTime = 500;
         WaitingLoading.OpacityTransition.Duration = new System.TimeSpan(0, 0, 0, 0, delayTime);
         await Task.Delay(2000);
         WaitingLoading.Opacity = 0.0;
         await Task.Delay(delayTime);
         WaitingLoading.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

         NewConnection.OpacityTransition.Duration = new System.TimeSpan(0, 0, 0, 0, delayTime);
         NewConnection.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
         NewConnection.Opacity = 1.0;

         await Task.Delay(2000);
         App.Instance.RootFrame?.Navigate(typeof(GamePage.GamePage));
      }
   }
}
