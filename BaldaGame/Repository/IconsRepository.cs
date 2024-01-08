using BaldaGame.Models;

using System;
using System.Collections.Generic;

namespace BaldaGame.Repository
{
   public class IconsRepository
   {
      public string[] IconPaths { get; set; }
      public string[] IconTags { get; set; }

      public List<Icon> Icons { get; private set; } = [];

      public Icon DefaultIcon { get; private set; } = Icon.Empty;

      public bool IsRepositoryLoaded { get => Icons.Count > 0; }

      public IconsRepository()
      {
         IconPaths = [
            "/Assets/Icons/Players/art-woolf.jpg",
            "/Assets/Icons/Players/art-sloth.jpg",
            "/Assets/Icons/Players/art-woolf.jpg",
            "/Assets/Icons/Players/art-sloth.jpg",
            "/Assets/Icons/Players/art-woolf.jpg",
            "/Assets/Icons/Players/art-sloth.jpg"
         ];
         IconTags = [
            "art-woolf",
            "art-sloth",
            "art-woolf1",
            "art-sloth1",
            "art-woolf2",
            "art-sloth2"
         ];
      }

      public void LoadRepository()
      {
         var size = Math.Max(IconPaths.Length, IconTags.Length);
         for (int i = 0; i < size; i++)
         {
            var icon = new Icon(IconTags[i], IconPaths[i]);
            Icons.Add(icon);
         }

         DefaultIcon = Icons[0];
      }

      public Icon? TryFindIcon(string IconTag)
      {
         foreach (var icon in Icons)
         {
            if (icon.Tag == IconTag)
            {
               return icon;
            }
         }

         return null;
      }
   }
}
