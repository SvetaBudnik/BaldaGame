using BaldaGame.Pages.MenuPage.Controller;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using System;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BaldaGame.Pages.MenuPage
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class MenuPage : Page
   {
      public MenuPage()
      {
         InitializeComponent();
         var controller = MenuPageNavigationController.Instance;
         controller.Init(MenuPageFrame);

         controller.NavigateTo(MenuPages.MainMenu);
      }

      protected override void OnNavigatedTo(NavigationEventArgs e)
      {
         base.OnNavigatedTo(e);

         Debug.WriteLine("I'm here!");

         MenuPageFrame.BackStack.Clear();

         var controller = MenuPageNavigationController.Instance;
         controller.Init(MenuPageFrame);
         controller.NavigateTo(MenuPages.MainMenu);
      }

   }

}
