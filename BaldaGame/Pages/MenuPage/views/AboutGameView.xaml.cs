using BaldaGame.Pages.MenuPage.Controller;

using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BaldaGame.Pages.MenuPage.views
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class AboutGameView : Page
   {
      public AboutGameView()
      {
         this.InitializeComponent();
      }

      private void ReturnButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
      {
         var controller = MenuPageNavigationController.Instance;
         controller.NavigateBack();
      }
   }
}
