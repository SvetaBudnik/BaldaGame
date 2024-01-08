using BaldaGame.Pages.MenuPage.Controller;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BaldaGame.Pages.MenuPage.views
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class SelectConnectionTypeView : Page
   {
      public SelectConnectionTypeView()
      {
         InitializeComponent();
      }

      private void BackButtonClick(object sender, RoutedEventArgs e)
      {
         var controller = MenuPageNavigationController.Instance;
         controller.NavigateBack();
      }

      private void CreateLobbyButtonClick(object sender, RoutedEventArgs e)
      {
         App.Instance.GameController.IsServer = true;

         var controller = MenuPageNavigationController.Instance;
         controller.NavigateTo(MenuPages.CreateLobby);
      }

      private void ConnectToLobbyButtonClick(object sender, RoutedEventArgs e)
      {
         App.Instance.GameController.IsServer = false;

         var controller = MenuPageNavigationController.Instance;
         controller.NavigateTo(MenuPages.CreateLobby);
      }
   }
}
