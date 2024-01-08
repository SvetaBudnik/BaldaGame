namespace BaldaGame.Models
{
   public class Icon(string tag, string assetPath)
   {
      public string Tag { get; init; } = tag;
      public string AssetPath { get; init; } = assetPath;

      public static Icon Empty { get; } = new Icon("", "");

      public Icon CopyWith(string? tag = null, string? assetPath = null)
      {
         return new Icon(
            tag: tag ?? Tag,
            assetPath: assetPath ?? AssetPath
         );
      }
   }
}
