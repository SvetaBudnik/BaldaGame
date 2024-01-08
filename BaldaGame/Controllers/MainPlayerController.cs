using BaldaGame.Models;
using BaldaGame.Repository;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BaldaGame.Controllers
{
   public class MainPlayerController : INotifyPropertyChanged
   {
      public Person Person { get; private set; } = Person.Empty;


      bool _isSettedUp = false;
      public bool IsSettedUp
      {
         get => _isSettedUp;
         set
         {
            if (_isSettedUp == value) { return; }
            _isSettedUp = value;
            OnPropertyChanged(nameof(IsSettedUp));
         }
      }

      public string PersonName
      {
         get => Person.Name;

         set
         {
            if (Person.Name == value) { return; }
            Person = Person.CopyWith(name: value);
            OnPropertyChanged(nameof(PersonName));
            SetPerson();
         }
      }

      public Icon PersonIcon
      {
         get => Person.Icon;

         set
         {
            if (Person.Icon == value) { return; }
            Person = Person.CopyWith(icon: value);
            OnPropertyChanged(nameof(PersonIcon));
            SetPerson();
         }
      }

      public MainPlayerController(IconsRepository repository)
      {
         if (repository.IsRepositoryLoaded)
         {
            PersonIcon = repository.DefaultIcon;
         }
      }

      public event PropertyChangedEventHandler? PropertyChanged;

      public void OnPropertyChanged([CallerMemberName] string prop = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
      }

      public void SetPerson()
      {
         if (PersonIcon != Icon.Empty && PersonName != "")
         {
            IsSettedUp = true;
         }
         else
         {
            IsSettedUp = false;
         }
      }
   }
}
