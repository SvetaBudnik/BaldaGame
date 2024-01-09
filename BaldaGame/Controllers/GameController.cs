using BaldaGame.Models;
using BaldaGame.Pages.GamePage;
using BaldaGame.Pages.MenuPage;
using BaldaGame.Pages.MenuPage.views;
using BaldaGame.Repository;
using BaldaGame.Repository.Client;
using BaldaGame.Repository.Server;

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BaldaGame.Controllers
{
   public delegate void OnTimerRunedOutEventHandler();

   public class GameDataController : INotifyPropertyChanged
   {
      Person currentPerson = Person.Empty;
      string currentPersonName = "";
      bool isCurrentStepByPlayer = false;
      ObservableCollection<string> mainPlayerWords = [];
      ObservableCollection<string> secondPlayerWords = [];

      const int defaultTimerValue = 100;
      int timerValue = 0;
      bool isTimerEnabled = false;
      public event OnTimerRunedOutEventHandler? TimerRunedOutEvent;


      public int TimerValue
      {
         get => timerValue;
         set
         {
            if (value == timerValue) return;
            timerValue = value;
            OnPropertyChanged(nameof(TimerValue));
         }
      }

      public bool IsCurrentStepByPlayer
      {
         get => isCurrentStepByPlayer;
         set
         {
            if (value == isCurrentStepByPlayer) return;
            isCurrentStepByPlayer = value;
            OnPropertyChanged(nameof(IsCurrentStepByPlayer));
         }
      }

      public Person CurrentPerson
      {
         get => currentPerson;

         set
         {
            if (value == currentPerson) return;
            currentPerson = value;
            OnPropertyChanged(nameof(CurrentPerson));
            CurrentPersonName = currentPerson.Name;
         }
      }

      public string CurrentPersonName
      {
         get => currentPersonName;

         set
         {
            if (value == currentPersonName) return;
            currentPersonName = value;
            OnPropertyChanged(nameof(CurrentPersonName));
         }
      }

      public ObservableCollection<string> MainPlayerWords => mainPlayerWords;
      public ObservableCollection<string> SecondPlayerWords => secondPlayerWords;

      public void Clear()
      {
         DisableTimer();
         currentPerson = Person.Empty;
         currentPersonName = "";
         isCurrentStepByPlayer = false;
         mainPlayerWords = [];
         secondPlayerWords = [];
         timerValue = 0;
         isTimerEnabled = false;
      }

      public void AddMainPlayerWord(string word)
      {
         mainPlayerWords.Add(word);
         OnPropertyChanged(nameof(MainPlayerWords));
      }

      public void AddSecondPlayerWord(string word)
      {
         secondPlayerWords.Add(word);
         OnPropertyChanged(nameof(SecondPlayerWords));
      }

      public void ResetTimer()
      {
         TimerValue = defaultTimerValue;
      }

      public void EnableTimer()
      {
         if (isTimerEnabled) return;
         isTimerEnabled = true;
         TimerTickerRepository.Instance.TimerTickerEvent += OnTimerTicked;
      }

      public void DisableTimer()
      {
         if (!isTimerEnabled) return;
         isTimerEnabled = false;
         TimerTickerRepository.Instance.TimerTickerEvent -= OnTimerTicked;
      }

      void OnTimerTicked()
      {
         var newTimerValue = TimerValue - 1;
         if (newTimerValue == 0)
         {
            TimerValue = newTimerValue;
            DisableTimer();
            if (IsCurrentStepByPlayer)
            {
               TimerRunedOutEvent?.Invoke();
            }
         }
         else
         {
            TimerValue = newTimerValue;
         }
      }

      public event PropertyChangedEventHandler? PropertyChanged;

      public void OnPropertyChanged([CallerMemberName] string prop = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
      }
   }

   public class GameController
   {
      public GameDataController DataController { get; set; } = new();

      public ServerRepository? serverRepository;
      public ClientRepository? clientRepository;

      public CreateLobbyView? CreateLobbyView { get; set; }
      public GamePage? GamePage { get; set; }
      public Grid? GameFieldGrid { get; set; }

      public bool IsServer { get; set; } = false;

      public double GridSize { get => GameFieldGrid!.ActualWidth; }
      public double CellSize { get => GridSize / 5.0; }

      public List<Cell> Elems { get; private set; } = [];
      public List<Cell> Selected { get; private set; } = [];
      public bool IsPointerPressed { get; private set; } = false;
      public Cell? ChangedCell { get; private set; }

      private string mainWord = "";
      private string selectedWord = "";
      private bool isGameStopped = false;

      public void StopClientServer()
      {
         serverRepository?.Stop();
         clientRepository?.Stop();
         serverRepository = null;
         clientRepository = null;
      }

      public void StartClientServer()
      {
         if (CreateLobbyView is null)
         {
            throw new NullReferenceException($"Перед стартом сервера необходимо обязательно выполнить инициализацию переменной {CreateLobbyView}");
         }
         StopClientServer();
         if (IsServer)
         {
            serverRepository = new ServerRepository();
            serverRepository.Notify += OnServerEvent;
            DataController.CurrentPerson = App.Instance.MainPlayerController.Person;
            DataController.IsCurrentStepByPlayer = true;
            mainWord = WordsRepository.Instance.GetRandomWord();

            if (GamePage?.IsLoaded == true)
            {
               GamePage!.SkipStepButton.IsEnabled = true;
            }

            serverRepository.Run();
         }
         else
         {
            clientRepository = new ClientRepository();
            clientRepository.Notify += OnClientEvent;
            clientRepository.TryConnectToServer();
         }
         if (GameFieldGrid?.IsLoaded == true)
         {
            GridInit();
         }
      }

      public void CancelMove()
      {
         GamePage!.TextBlockAwaitNewWord.Visibility = Visibility.Collapsed;
         GamePage!.TextBlockAwaitNewCell.Visibility = Visibility.Visible;
         GamePage!.ReverseStepButton.IsEnabled = false;

         if (ChangedCell is not null)
         {
            ChangedCell.Content = "";
            ChangedCell.UnselectBorder();
            ChangedCell = null;
         }

         selectedWord = "";
      }

      public void SkipMove()
      {
         CancelMove();
         GamePage!.TextBlockAwaitNewCell.Visibility = Visibility.Collapsed;
         GamePage!.TextBlockAwaitNewWord.Visibility = Visibility.Collapsed;
         GamePage!.SkipStepButton.IsEnabled = false;
         GamePage!.ReverseStepButton.IsEnabled = false;

         IsPointerPressed = false;
         foreach (Cell elem in Selected)
         {
            elem.TryUnselect();
         }
         Selected = [];

         if (IsServer)
         {
            var msg = new ServerMessage()
            {
               Id = ServerMessageIds.PlayerSkipMove
            };
            serverRepository?.SendMessage(msg);
         }
         else
         {
            var msg = new ClientMessage()
            {
               Id = ClientMessageIds.PlayerSkipMove
            };
            clientRepository?.SendMessage(msg);
         }

         DataController.IsCurrentStepByPlayer = false;
         DataController.CurrentPerson = App.Instance.SecondPlayerController.Person;

         DataController.ResetTimer();
         DataController.EnableTimer();
      }

      public async Task FinalizeGame(bool fromUser = true)
      {
         if (isGameStopped) return;
         isGameStopped = true;

         DataController.DisableTimer();

         if (fromUser)
         {
            if (IsServer)
            {
               var msg = new ServerMessage()
               {
                  Id = ServerMessageIds.PlayerCancelledGame
               };
               serverRepository?.SendMessage(msg);
            }
            else
            {
               var msg = new ClientMessage()
               {
                  Id = ClientMessageIds.PlayerCancelledGame
               };
               clientRepository?.SendMessage(msg);
            }
         }

         StopClientServer();
         var userScore = 0;
         foreach (var word in DataController.MainPlayerWords)
         {
            userScore += word.Length;
         }
         var enemyScore = 0;
         foreach (var word in DataController.SecondPlayerWords)
         {
            enemyScore += word.Length;
         }

         var isUserWin = userScore > enemyScore;
         var isDraw = userScore == enemyScore;

         if (GamePage is not null)
         {
            var dialog = UserInputContentDialog.GetGameIsOverPrompt(GamePage.XamlRoot, isUserWin, isDraw, userScore, enemyScore);
            await dialog.ShowAsync();
         }

         Selected = [];
         selectedWord = "";
         ChangedCell = null;
         IsPointerPressed = false;

         foreach (var cell in Elems)
         {
            cell.Content = "";
            cell.TryUnselect();
            cell.UnselectBorder();
         }

         DataController.TimerRunedOutEvent -= SkipMove;
         DataController.Clear();

         App.Instance.RootFrame!.Navigate(typeof(MenuPage));
         isGameStopped = false;
      }

      bool IsFieldIsFullfilled()
      {
         foreach (var el in Elems)
         {
            if (el.Content == "") return false;
         }

         return true;
      }


      public void Init(Grid mainGrid, GamePage page)
      {
         GamePage = page;

         GamePage.Loaded += OnGamePageLoaded;

         GameFieldGrid = mainGrid;
         GameFieldGrid.Loaded += OnGameGridLoaded;

         GameFieldGrid.PointerExited += OnPointerExited;
         GameFieldGrid.PointerPressed += OnPointerPressed;
         GameFieldGrid.PointerReleased += OnPointerReleased;
         GameFieldGrid.PointerMoved += OnPointerMoved;

         DataController.TimerRunedOutEvent += SkipMove;
      }

      void OnGamePageLoaded(object sender, RoutedEventArgs e)
      {
         if (DataController.IsCurrentStepByPlayer)
         {
            GamePage!.SkipStepButton.IsEnabled = true;
         }
      }

      void OnGameGridLoaded(object sender, RoutedEventArgs e)
      {
         GridInit();
      }

      void GridInit()
      {
         GameFieldGrid?.Children.Clear();
         Elems.Clear();

         for (int i = 0; i < 5; i++)
         {
            for (int j = 0; j < 5; j++)
            {
               var index = new CellIndex(i, j);
               var el = new Cell(
                  index: index
               );

               Elems.Add(el);

               if (mainWord != "" && i == 2)
               {
                  el.Content = mainWord[j].ToString();
               }

               GameFieldGrid?.Children.Add(el.border);
               Grid.SetColumn(el.border, j);
               Grid.SetRow(el.border, i);
            }
         }

         DataController.ResetTimer();
         DataController.EnableTimer();
      }


      void OnPointerExited(object sender, PointerRoutedEventArgs e)
      {
         IsPointerPressed = false;
         foreach (Cell elem in Selected)
         {
            elem.TryUnselect();
         }
         Selected = [];
      }

      void OnPointerPressed(object sender, PointerRoutedEventArgs e)
      {
         if (!DataController.IsCurrentStepByPlayer || selectedWord != "") return;

         if (ChangedCell is not null)
         {
            var cell = GetCell(e);
            if (!cell.HasContent) return;

            IsPointerPressed = true;
            cell.TrySelect();
            Selected.Add(cell);
         }
      }

      async void OnPointerReleased(object sender, PointerRoutedEventArgs e)
      {
         if (!DataController.IsCurrentStepByPlayer || selectedWord != "") return;

         if (ChangedCell is null)
         {
            var cell = GetCell(e);
            if (cell.HasContent) return;

            var hasNeightboursWithContent = false;
            foreach (Cell elem in Elems)
            {
               if (cell.IsNeighbourTo(elem) && elem.HasContent)
               {
                  hasNeightboursWithContent = true;
                  break;
               }
            }

            if (!hasNeightboursWithContent) return;

            var result = await GetUserCharPrompt();
            if (result == "") return;
            cell.Content = result;
            ChangedCell = cell;
            cell.SelectBorder();

            GamePage!.TextBlockAwaitNewCell.Visibility = Visibility.Collapsed;
            GamePage!.TextBlockAwaitNewWord.Visibility = Visibility.Visible;
            GamePage!.ReverseStepButton.IsEnabled = true;
         }
         else
         {
            var isNewWordSelected = false;
            var wordBuilder = new StringBuilder();
            foreach (var cell in Selected)
            {
               if (cell == ChangedCell) { isNewWordSelected = true; }
               wordBuilder.Append(cell.Content);
            }
            if (!isNewWordSelected)
            {
               OnPointerExited(sender, e);
               return;
            }

            var word = wordBuilder.ToString();
            selectedWord = word;
            var dialog = UserInputContentDialog.GetNewWordPrompt(GamePage!.XamlRoot, word);
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
               DataController.DisableTimer();

               if (IsServer)
               {
                  var msg = new Repository.Server.PlayerMakeMoveMessage(
                     word,
                     ChangedCell.Content,
                     ChangedCell.cellIndex.i,
                     ChangedCell.cellIndex.j
                  );
                  serverRepository?.SendMessage(msg.GetServerMessage());
               }
               else
               {
                  var msg = new Repository.Client.PlayerMakeMoveMessage(
                     word,
                     ChangedCell.Content,
                     ChangedCell.cellIndex.i,
                     ChangedCell.cellIndex.j
                  );
                  clientRepository?.SendMessage(msg.GetClientMessage());
               }

               GamePage!.TextBlockAwaitNewWord.Visibility = Visibility.Collapsed;
               GamePage!.TextBlockAwaitingForConfirm.Visibility = Visibility.Visible;
               GamePage.SkipStepButton.IsEnabled = false;
            }
            else
            {
               selectedWord = "";
            }

            OnPointerExited(sender, e);
         }
      }

      void OnPointerMoved(object sender, PointerRoutedEventArgs e)
      {
         if (!IsPointerPressed) return;
         if (Selected.Count == 0) return;

         var cell = GetCell(e);
         if (!cell.HasContent) return;

         var lastSelectedCell = Selected[^1];

         // Если выбираем впервые
         if (!cell.IsSelected)
         {
            if (cell.IsNeighbourTo(lastSelectedCell))
            {
               cell.TrySelect();
               Selected.Add(cell);
            }
         }
         // Если идём назад
         else
         {
            if (Selected.Count == 1) { return; }

            var lastLastSelectedCell = Selected[^2];
            if (cell == lastLastSelectedCell)
            {
               lastSelectedCell.TryUnselect();
               Selected.RemoveAt(Selected.Count - 1);
            }
         }
      }


      private CellIndex GetCellIndex(double cursX, double cursY)
      {
         if (cursX < 0 || cursX > GridSize) throw new ArgumentOutOfRangeException(nameof(cursX), $"Parameter must be in range 0 <= cursX <= {GridSize}");
         if (cursY < 0 || cursY > GridSize) throw new ArgumentOutOfRangeException(nameof(cursX), $"Parameter must be in range 0 <= cursY <= {GridSize}");

         var iFrac = cursY / CellSize;
         var jFrac = cursX / CellSize;
         var i = (int)Math.Floor(iFrac);
         var j = (int)Math.Floor(jFrac);

         // На случай, если положение курсора будет ровно на границе поля
         if (i == 5) { i = 4; }
         if (j == 5) { j = 4; }

         return new CellIndex(i, j);
      }

      private Cell GetCell(PointerRoutedEventArgs e)
      {
         var pointerInfo = e.GetCurrentPoint(GameFieldGrid);
         var pos = pointerInfo.Position;
         var cellIndex = GetCellIndex(pos.X, pos.Y);
         var cell = Elems[cellIndex.i * 5 + cellIndex.j];

         return cell;
      }

      async Task<string> GetUserCharPrompt()
      {
         var dialog = UserInputContentDialog.GetCharPrompt(GamePage!.XamlRoot);

         ContentDialogResult result = await dialog.ShowAsync();
         if (result == ContentDialogResult.Primary)
         {
            var output = dialog.Content as TextBox;
            return output!.Text;
         }

         return "";
      }


      private void OnClientEvent(IClientEvent clientEvent)
      {
         Debug.WriteLine($"Client in game: Получено событие {clientEvent}");
         switch (clientEvent)
         {
            case ClientPlayerConnected se: OnClientPlayerConnected(se); break;
            case ClientGotPlayerInfo se: OnClientGotPlayerInfo(se); break;
            case ClientPlayerMakeMove se: OnClientPlayerMakeMove(se); break;
            case ClientPlayerAgreedWithWord se: OnClientPlayerAgreedWithWord(se); break;
            case ClientPlayerDisagreedWithWord se: OnClientPlayerDisagreedWithWord(se); break;
            case ClientPlayerSkipMove se: OnClientPlayerSkipMove(se); break;
            case ClientPlayerDisconnected se: OnClientPlayerDisconnected(se); break;
            case ClientPlayerCancelledGame se: OnClientPlayerCancelledGame(se); break;
            case ClientServerChoosedMainWord se: OnClientServerChoosedMainWord(se); break;

            default: throw new NotImplementedException();
         }
      }

      void OnClientPlayerConnected(ClientPlayerConnected info)
      {
         var player = App.Instance.MainPlayerController;

         var msg = new Repository.Client.PlayerInfoMessage(player.PersonName, player.PersonIcon.Tag);
         clientRepository?.SendMessage(msg.GetClientMessage());
      }

      void OnClientGotPlayerInfo(ClientGotPlayerInfo info)
      {
         var iconsRepository = App.Instance.IconsRepository;
         var icon = iconsRepository.TryFindIcon(info.IconTag);
         if (icon is null)
         {
            Debug.WriteLine($"OnServerGotPlayerInfo: Не получилось найти иконку {info.IconTag}");
            icon = iconsRepository.DefaultIcon;
         }

         var secondPlayerController = App.Instance.SecondPlayerController;
         secondPlayerController.PersonIcon = icon;
         secondPlayerController.PersonName = info.Username;

         DataController.CurrentPerson = secondPlayerController.Person;

         CreateLobbyView?.AnimateTransitWhenConnect();
         DataController.ResetTimer();
         DataController.EnableTimer();
      }

      async void OnClientPlayerMakeMove(ClientPlayerMakeMove info)
      {
         DataController.DisableTimer();

         var dialog = UserInputContentDialog.GetAnotherWordPrompt(GamePage!.XamlRoot, info.ChoosenWord);
         var result = await dialog.ShowAsync();
         if (result == ContentDialogResult.Primary)
         {
            var cell = Elems[info.CharX * 5 + info.CharY];
            cell.Content = info.NewChar;
            DataController.AddSecondPlayerWord(info.ChoosenWord);
            var msg = new ClientMessage()
            {
               Id = ClientMessageIds.PlayerAgreedWithWord
            };
            clientRepository?.SendMessage(msg);

            if (IsFieldIsFullfilled())
            {
               var gameEndMsg = new ClientMessage()
               {
                  Id = ClientMessageIds.PlayerCancelledGame
               };
               clientRepository?.SendMessage(gameEndMsg);
            }

            DataController.CurrentPerson = App.Instance.MainPlayerController.Person;
            DataController.IsCurrentStepByPlayer = true;
            GamePage.TextBlockAwaitNewCell.Visibility = Visibility.Visible;
            GamePage.SkipStepButton.IsEnabled = true;

            DataController.ResetTimer();
         }
         else
         {
            var msg = new ClientMessage()
            {
               Id = ClientMessageIds.PlayerDisagreedWithWord
            };
            clientRepository?.SendMessage(msg);
         }

         DataController.EnableTimer();
      }

      void OnClientPlayerSkipMove(ClientPlayerSkipMove info)
      {
         DataController.ResetTimer();
         DataController.IsCurrentStepByPlayer = true;
         DataController.CurrentPerson = App.Instance.MainPlayerController.Person;
         CancelMove();
         GamePage!.SkipStepButton.IsEnabled = true;
         DataController.ResetTimer();
         DataController.EnableTimer();
      }

      void OnClientPlayerDisconnected(ClientPlayerDisconnected info)
      {
         FinalizeGame(fromUser: false);
      }

      void OnClientPlayerCancelledGame(ClientPlayerCancelledGame info)
      {
         FinalizeGame(fromUser: false);
      }

      void OnClientPlayerAgreedWithWord(ClientPlayerAgreedWithWord info)
      {
         DataController.IsCurrentStepByPlayer = false;
         DataController.CurrentPerson = App.Instance.SecondPlayerController.Person;
         DataController.AddMainPlayerWord(selectedWord);
         GamePage!.TextBlockAwaitingForConfirm.Visibility = Visibility.Collapsed;

         if (ChangedCell is not null)
         {
            ChangedCell.UnselectBorder();
            ChangedCell = null;
         }
         selectedWord = "";

         GamePage!.ReverseStepButton.IsEnabled = false;
         GamePage!.SkipStepButton.IsEnabled = false;
         DataController.ResetTimer();
         DataController.EnableTimer();
      }

      async void OnClientPlayerDisagreedWithWord(ClientPlayerDisagreedWithWord info)
      {
         GamePage!.TextBlockAwaitingForConfirm.Visibility = Visibility.Collapsed;

         CancelMove();
         GamePage.SkipStepButton.IsEnabled = true;
         DataController.EnableTimer();

         var dialog = UserInputContentDialog.GetDeniedWordPrompt(GamePage.XamlRoot);

         await dialog.ShowAsync();
      }

      void OnClientServerChoosedMainWord(ClientServerChoosedMainWord info)
      {
         mainWord = info.Word;
      }



      private void OnServerEvent(IServerEvent serverEvent)
      {
         Debug.WriteLine($"Server in game: Получено событие {serverEvent}");
         switch (serverEvent)
         {
            case ServerPlayerConnected se: OnServerPlayerConnected(se); break;
            case ServerGotPlayerInfo se: OnServerGotPlayerInfo(se); break;
            case ServerPlayerMakeMove se: OnServerPlayerMakeMove(se); break;
            case ServerPlayerAgreedWithWord se: OnServerPlayerAgreedWithWord(se); break;
            case ServerPlayerDisagreedWithWord se: OnServerPlayerDisagreedWithWord(se); break;
            case ServerPlayerSkipMove se: OnServerPlayerSkipMove(se); break;
            case ServerPlayerDisconnected se: OnServerPlayerDisconnected(se); break;
            case ServerPlayerCancelledGame se: OnServerPlayerCancelledGame(se); break;

            default: throw new NotImplementedException();
         }
      }

      void OnServerPlayerConnected(ServerPlayerConnected info)
      {
         var player = App.Instance.MainPlayerController;

         var msg = new Repository.Server.PlayerInfoMessage(player.PersonName, player.PersonIcon.Tag);
         serverRepository?.SendMessage(msg.GetServerMessage());
      }

      void OnServerGotPlayerInfo(ServerGotPlayerInfo info)
      {
         var iconsRepository = App.Instance.IconsRepository;
         var icon = iconsRepository.TryFindIcon(info.IconTag);
         if (icon is null)
         {
            Debug.WriteLine($"OnServerGotPlayerInfo: Не получилось найти иконку {info.IconTag}");
            icon = iconsRepository.DefaultIcon;
         }

         var secondPlayerController = App.Instance.SecondPlayerController;
         secondPlayerController.PersonIcon = icon;
         secondPlayerController.PersonName = info.Username;

         var sendMainWord = new MainWordWasChoosenMessage(mainWord);
         serverRepository?.SendMessage(sendMainWord.GetServerMessage());

         CreateLobbyView?.AnimateTransitWhenConnect();
         DataController.ResetTimer();
         DataController.EnableTimer();
      }

      async void OnServerPlayerMakeMove(ServerPlayerMakeMove info)
      {
         DataController.DisableTimer();

         var dialog = UserInputContentDialog.GetAnotherWordPrompt(GamePage!.XamlRoot, info.ChoosenWord);
         var result = await dialog.ShowAsync();
         if (result == ContentDialogResult.Primary)
         {
            var cell = Elems[info.CharX * 5 + info.CharY];
            cell.Content = info.NewChar;
            DataController.AddSecondPlayerWord(info.ChoosenWord);
            var msg = new ServerMessage()
            {
               Id = ServerMessageIds.PlayerAgreedWithWord
            };
            serverRepository?.SendMessage(msg);

            if (IsFieldIsFullfilled())
            {
               var gameEndMsg = new ServerMessage()
               {
                  Id = ServerMessageIds.PlayerCancelledGame
               };
               serverRepository?.SendMessage(gameEndMsg);
            }

            DataController.CurrentPerson = App.Instance.MainPlayerController.Person;
            DataController.IsCurrentStepByPlayer = true;

            GamePage.TextBlockAwaitNewCell.Visibility = Visibility.Visible;
            GamePage.SkipStepButton.IsEnabled = true;
            DataController.ResetTimer();
         }
         else
         {
            var msg = new ServerMessage()
            {
               Id = ServerMessageIds.PlayerDisagreedWithWord
            };
            serverRepository?.SendMessage(msg);
         }

         DataController.EnableTimer();
      }

      void OnServerPlayerAgreedWithWord(ServerPlayerAgreedWithWord info)
      {
         DataController.IsCurrentStepByPlayer = false;
         DataController.CurrentPerson = App.Instance.SecondPlayerController.Person;
         DataController.AddMainPlayerWord(selectedWord);
         GamePage!.TextBlockAwaitingForConfirm.Visibility = Visibility.Collapsed;

         if (ChangedCell is not null)
         {
            ChangedCell.UnselectBorder();
            ChangedCell = null;
         }
         selectedWord = "";

         GamePage!.ReverseStepButton.IsEnabled = false;
         GamePage!.SkipStepButton.IsEnabled = false;
         DataController.ResetTimer();
         DataController.EnableTimer();
      }

      async void OnServerPlayerDisagreedWithWord(ServerPlayerDisagreedWithWord info)
      {
         GamePage!.TextBlockAwaitingForConfirm.Visibility = Visibility.Collapsed;

         CancelMove();
         GamePage.SkipStepButton.IsEnabled = true;
         DataController.EnableTimer();

         var dialog = UserInputContentDialog.GetDeniedWordPrompt(GamePage.XamlRoot);

         await dialog.ShowAsync();
      }

      void OnServerPlayerSkipMove(ServerPlayerSkipMove info)
      {
         DataController.ResetTimer();
         DataController.IsCurrentStepByPlayer = true;
         DataController.CurrentPerson = App.Instance.MainPlayerController.Person;
         CancelMove();
         GamePage!.SkipStepButton.IsEnabled = true;
         DataController.ResetTimer();
         DataController.EnableTimer();
      }

      void OnServerPlayerDisconnected(ServerPlayerDisconnected info)
      {
         FinalizeGame(fromUser: false);
      }

      void OnServerPlayerCancelledGame(ServerPlayerCancelledGame info)
      {
         FinalizeGame(fromUser: false);
      }
   }

   public struct CellIndex(int i, int j)
   {
      public int i = i;
      public int j = j;
   }

   public class Cell
   {
      public TextBlock el;
      public Border border;
      public CellIndex cellIndex;

      private string _content;

      public string Content
      {
         get => _content;
         set
         {
            _content = value;
            el.Text = value;
         }
      }
      public bool IsSelected { get; private set; } = false;
      public bool HasContent { get => Content != ""; }

      public static readonly SolidColorBrush goodSelectedColor = new(Colors.Green);
      public static readonly SolidColorBrush defaultColor = new(Colors.LightSkyBlue);
      public static readonly SolidColorBrush selectedBorderColor = new(Colors.Orange);
      public static readonly SolidColorBrush defaultBorderColor = new(Colors.Black);

      public Cell(CellIndex index, string content = "")
      {
         el = new TextBlock
         {
            Text = content,
            FontSize = 34.0,
            IsTextSelectionEnabled = false,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
         };
         border = new Border
         {
            BorderBrush = defaultBorderColor,
            BorderThickness = new Thickness(2),
            Background = defaultColor,
            Child = el,
         };
         cellIndex = index;
         _content = content;
      }

      public bool TrySelect()
      {
         if (IsSelected)
         {
            return false;
         }

         border.Background = goodSelectedColor;
         IsSelected = true;

         return true;
      }

      public bool TryUnselect()
      {
         if (!IsSelected)
         {
            return false;
         }

         border.Background = defaultColor;
         IsSelected = false;

         return true;
      }

      public void SelectBorder()
      {
         border.BorderBrush = selectedBorderColor;
      }

      public void UnselectBorder()
      {
         border.BorderBrush = defaultBorderColor;
      }

      public bool IsNeighbourTo(Cell other)
      {
         var deltaX = Math.Abs(this.cellIndex.i - other.cellIndex.i);
         var deltaY = Math.Abs(this.cellIndex.j - other.cellIndex.j);

         return (deltaX, deltaY) switch
         {
            (1, 0) => true,
            (0, 1) => true,
            _ => false
         };

      }
   }

   public static class UserInputContentDialog
   {
      static void OnTextChanged(object sender, TextChangedEventArgs e)
      {
         if (sender is TextBox textBox)
         {
            var isEmpty = textBox.Text.Length == 0;
            if (isEmpty)
            {
               textBox.PlaceholderText = "Введите русскую букву...";
            }
            else
            {
               var lastLetter = textBox.Text[^1];
               var newText = $"{lastLetter}".ToUpper();

               if (newText[0] < 'А' || newText[0] > 'Я')
               {
                  textBox.Text = "";
               }
               else
               {
                  textBox.Text = newText;
                  textBox.Select(1, 0);
               }
            }
         }
      }

      static public ContentDialog GetCharPrompt(XamlRoot root)
      {
         var input = new TextBox()
         {
            Height = (double)App.Current.Resources["TextControlThemeMinHeight"],
            PlaceholderText = "Введите русскую букву..."
         };
         input.TextChanged += OnTextChanged;

         var dialog = new ContentDialog()
         {
            XamlRoot = root,
            Title = "Выбор буквы",
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Отмена",
            Content = input
         };

         return dialog;
      }

      static public ContentDialog GetNewWordPrompt(XamlRoot root, string word)
      {
         return new ContentDialog()
         {
            XamlRoot = root,
            Title = "Подтвердите выбор слова",
            Content = $"Вы уверены, что хотите выбрать слово \"{word}\"?",
            PrimaryButtonText = "Да",
            SecondaryButtonText = "Нет"
         };
      }

      static public ContentDialog GetAnotherWordPrompt(XamlRoot root, string word)
      {
         var secondPlayer = App.Instance.SecondPlayerController.PersonName;
         return new ContentDialog()
         {
            XamlRoot = root,
            Title = "Предложенное слово",
            Content = $"Игрок {secondPlayer} предложил слово \"{word}\". Оно верное?",
            PrimaryButtonText = "Да",
            SecondaryButtonText = "Нет"
         };
      }

      static public ContentDialog GetDeniedWordPrompt(XamlRoot root)
      {
         var secondPlayer = App.Instance.SecondPlayerController.PersonName;
         return new ContentDialog()
         {
            XamlRoot = root,
            Title = "Плохое слово",
            Content = $"Игрок {secondPlayer} отверг ваше слово 😢",
            PrimaryButtonText = "Ладно",
            SecondaryButtonText = "Жаль..."
         };
      }

      static public ContentDialog GetGameIsOverPrompt(XamlRoot root, bool isUserWin, bool isDraw, int userScore, int enemyScore)
      {
         var content = new StackPanel()
         {
            HorizontalAlignment = HorizontalAlignment.Center,
         };
         var textBlockOfStack = new TextBlock()
         {
            Margin = new Thickness(5)
         };
         content.Children.Add(textBlockOfStack);

         var scoreStack = new StackPanel
         {
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Horizontal
         };

         var userScoreStack = new StackPanel()
         {
            HorizontalAlignment = HorizontalAlignment.Left
         };
         userScoreStack.Children.Add(new TextBlock()
         {
            Text = "Вы",
            TextAlignment = TextAlignment.Left,
            Margin = new Thickness(0, 5, 20, 5)
         });
         userScoreStack.Children.Add(new TextBlock()
         {
            Text = userScore.ToString(),
            TextAlignment = TextAlignment.Left,
            Margin = new Thickness(0, 5, 20, 5)
         });

         var enemyScoreStack = new StackPanel()
         {
            HorizontalAlignment = HorizontalAlignment.Right
         };
         enemyScoreStack.Children.Add(new TextBlock()
         {
            Text = App.Instance.SecondPlayerController.PersonName,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(20, 5, 0, 5)
         });
         enemyScoreStack.Children.Add(new TextBlock()
         {
            Text = enemyScore.ToString(),
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(20, 5, 0, 5)
         });

         scoreStack.Children.Add(userScoreStack);
         scoreStack.Children.Add(enemyScoreStack);

         content.Children.Add(scoreStack);

         if (isUserWin)
         {
            textBlockOfStack.Text = "Вы победили!";
            return new ContentDialog()
            {
               XamlRoot = root,
               Title = "Победа!",
               Content = content,
               PrimaryButtonText = "Вау!"
            };
         }
         else if (!isDraw)
         {
            textBlockOfStack.Text = "Вы проиграли...";
            return new ContentDialog()
            {
               XamlRoot = root,
               Title = "Неудача...",
               Content = content,
               PrimaryButtonText = "Печаль"
            };
         }
         else
         {
            textBlockOfStack.Text = "Ничья!";
            return new ContentDialog()
            {
               XamlRoot = root,
               Title = "Кажется, вы равны в своих силах!",
               Content = content,
               PrimaryButtonText = "Ого"
            };
         }
      }
   }
}
