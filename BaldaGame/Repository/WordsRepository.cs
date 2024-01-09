using System;
using System.Collections.Generic;

namespace BaldaGame.Repository
{
   public class WordsRepository
   {
      public static WordsRepository Instance { get; } = new();

      readonly List<string> words;

      WordsRepository()
      {
         words = [
            "БАЛДА",
            "ПОКЕР",
            "ТРАНС",
            "ВЕДРО",
            "ОЛОВО",
            "МЕТРО",
            "ЧАШКА",
            "СЛОВО",
            "КОШКА",
            "ГРУША",
            "ВИЛКА",
            "КОМАР",
            "ДЯТЕЛ",
            "КОВЕР",
            "ВОБЛА",
            "РАМПА",
            "ИЗГОЙ"
         ];
      }

      public string GetRandomWord()
      {
         var rnd = new Random();
         var index = rnd.Next(words.Count);
         return words[index];
      }
   }
}
