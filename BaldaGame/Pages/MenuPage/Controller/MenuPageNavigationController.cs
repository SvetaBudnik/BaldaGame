using BaldaGame.Pages.MenuPage.views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

using System;

namespace BaldaGame.Pages.MenuPage.Controller
{
   public class NotInitializedControllerException(string err) : Exception(err)
   {
   }

   public class MenuPageNavigationController
   {
      Frame? _navFrame;
      public Frame? Frame { get => _navFrame; }

      public static MenuPageNavigationController Instance { get; } = new();

      MenuPageNavigationController() { }

      public void Init(Frame navFrame)
      {
            _navFrame = navFrame;
      }

      public bool NavigateBack()
      {
         if (_navFrame is null)
            throw new NotInitializedControllerException($"{nameof(MenuPageNavigationController)} must be initialized first. Try call ${nameof(Init)} method");

         if (_navFrame.CanGoBack)
         {
            _navFrame.GoBack();
            return true;
         }

         return false;
      }

      public void NavigateTo(MenuPages page)
      {

         switch (page)
         {
            case MenuPages.MainMenu:
               NavigateToType(typeof(MainMenuView));
               break;

            case MenuPages.RegisterPerson:
               NavigateToType(typeof(RegisterPersonView));
               break;

            case MenuPages.SelectConnectionType:
               NavigateToType(typeof(SelectConnectionTypeView));
               break;

            case MenuPages.CreateLobby:
               NavigateToType(typeof(CreateLobbyView));
               break;

            case MenuPages.AboutPage:
               NavigateToType(typeof(AboutGameView));
               break;


            default:
               throw new NotImplementedException();
         }
      }

      private void NavigateToType(Type pageType)
      {
         if (_navFrame is null)
            throw new NotInitializedControllerException($"{nameof(MenuPageNavigationController)} must be initialized first. Try call ${nameof(Init)} method");
         _navFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
      }
   }

   public enum MenuPages
   {
      MainMenu,
      AboutPage,
      RegisterPerson,
      SelectConnectionType,
      CreateLobby,
      ConnectToLobby
   }
}
