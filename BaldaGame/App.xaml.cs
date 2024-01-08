using BaldaGame.Controllers;
using BaldaGame.Pages.GamePage;
using BaldaGame.Pages.MenuPage;
using BaldaGame.Repository;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BaldaGame
{
   /// <summary>
   /// Provides application-specific behavior to supplement the default Application class.
   /// </summary>
   public partial class App : Application
   {
      public static App Instance { get => _instance!; }

      static App? _instance;

      /// <summary>
      /// Initializes the singleton application object.  This is the first line of authored code
      /// executed, and as such is the logical equivalent of main() or WinMain().
      /// </summary>
      public App()
      {
#pragma warning disable S3010 // Static fields should not be updated in constructors
         _instance = this;
#pragma warning restore S3010 // Static fields should not be updated in constructors

         InitializeComponent();

         IconsRepository = new();
         IconsRepository.LoadRepository();
         MainPlayerController = new(IconsRepository);
      }

      /// <summary>
      /// Invoked when the application is launched.
      /// </summary>
      /// <param name="args">Details about the launch request and process.</param>
      protected override void OnLaunched(LaunchActivatedEventArgs args)
      {
         m_window = new Window();
         m_frame = new Frame();
         m_window.Content = m_frame;
         m_window.Activate();
         //m_frame.Navigate(typeof(MenuPage));
         m_frame.Navigate(typeof(GamePage));
      }

      public IconsRepository IconsRepository { get; private set; }
      public MainPlayerController MainPlayerController { get; private set; }

      public Frame? RootFrame { get => m_frame; }
      private Frame? m_frame;
      private Window? m_window;
   }
}
