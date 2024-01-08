namespace BaldaGame.Models
{
   public class Person(string name, Icon icon)
   {
      public string Name { get; init; } = name;
      public Icon Icon { get; init; } = icon;

      public static Person Empty { get; } = new Person("", Icon.Empty);

      public Person CopyWith(string? name = null, Icon? icon = null)
      {
         return new Person(
            name: name ?? Name,
            icon: icon ?? Icon
         );
      }
   }
}
