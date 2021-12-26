using KirosDungeons.Source.Game.Utils;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace KirosDungeons.Source.Game.Screens
{
    public abstract class Screen : GameElement
    {
        public List<Screen> Overlays { get; private set; }
        public List<GameElement> Elements { get; private set; }

        protected PixelatedFont PixelFont { get => Game.Fonts.PixelFont; }

        public Screen(KirosDungeons game) : this(game, null) { }

        public Screen(KirosDungeons game, Screen screen) : base(game, screen) { }

        public override void Initialize()
        {
            Overlays = new List<Screen>();
            Elements = new List<GameElement>();

            base.Initialize();
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (GameElement element in Elements)
                element.Draw(gameTime);

            foreach (Screen screen in Overlays)
                screen.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
