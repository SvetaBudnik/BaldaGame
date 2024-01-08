using BaldaGame.Controllers;
using BaldaGame.Pages.MenuPage.Controller;
using BaldaGame.Repository;

using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BaldaGame.Pages.MenuPage.views
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class RegisterPersonView : Page
   {
      readonly IconsRepository iconsRepository = App.Instance.IconsRepository;
      readonly MainPlayerController mainPlayerController = App.Instance.MainPlayerController;

      public RegisterPersonView()
      {
         InitializeComponent();
      }

      private void BackButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
      {
         var controller = MenuPageNavigationController.Instance;
         controller.NavigateBack();
      }

      private void NextButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
      {
         var controller = MenuPageNavigationController.Instance;
         controller.NavigateTo(MenuPages.SelectConnectionType);
      }
   }
}
