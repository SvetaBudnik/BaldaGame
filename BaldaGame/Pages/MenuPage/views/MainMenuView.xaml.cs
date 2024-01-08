using BaldaGame.Pages.MenuPage.Controller;
using BaldaGame.Repository;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BaldaGame.Pages.MenuPage.views
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class MainMenuView : Page
   {

      public MainMenuView()
      {
         InitializeComponent();
      }


      private async void CloseButtonClick(object sender, RoutedEventArgs e)
      {
         var dialog = new ContentDialog()
         {
            XamlRoot = XamlRoot,
            Title = "Выход из игры",
            Content = "Вы действительно хотите выйти из игры?",
            PrimaryButtonText = "Да",
            SecondaryButtonText = "Нет"
         };
         var result = await dialog.ShowAsync();
         if (result == ContentDialogResult.Primary)
         {
            App.Instance.Exit();
         }
      }

      private void StartGameClick(object sender, RoutedEventArgs e)
      {
         var controller = MenuPageNavigationController.Instance;
         controller.NavigateTo(MenuPages.RegisterPerson);
      }
   }
}
