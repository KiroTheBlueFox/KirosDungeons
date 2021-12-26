using FontStashSharp;
using System.Collections.Generic;
using System.IO;

namespace KirosDungeons.Source.Game.Utils
{
    public class Fonts
    {
        public Dictionary<string, FontSystem> FontList { get; private set; }
        public FontSystem Normal { get => FontList["normal"]; }
        public FontSystem Bold { get => FontList["bold"]; }
        public PixelatedFont PixelFont { get; private set; }
        public KirosDungeons Game { get; }

        public Fonts(KirosDungeons game)
        {
            Game = game;
            FontList = new Dictionary<string, FontSystem>();

            FontSystem Normal = new FontSystem();
            Normal.AddFont(File.ReadAllBytes(Game.Content.RootDirectory + @"/Fonts/Roboto-Regular.ttf"));
            FontList["normal"] = Normal;

            FontSystem Bold = new FontSystem();
            Bold.AddFont(File.ReadAllBytes(Game.Content.RootDirectory + @"/Fonts/Roboto-Bold.ttf"));
            FontList["bold"] = Bold;

            PixelFont = new PixelatedFont(game, @"Fonts/PixelFont");
        }

        public void Load()
        {
            PixelFont.Load();
        }

        public FontSystem Get(string name)
        {
            return FontList[name];
        }
    }
}
