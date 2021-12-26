using KirosDungeons.Source.Game.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KirosDungeons.Source.Game
{
    public abstract class GameElement : GameComponent
    {
        public new KirosDungeons Game { get; }
        protected SpriteBatch SpriteBatch { get => Game.SpriteBatch; }
        protected ContentManager Content { get => Game.Content; }
        public Screen Screen { get; private set; }

        public GameElement(KirosDungeons game, Screen screen) : base(game)
        {
            Game = game;
            Screen = screen;
        }

        public virtual void Load() { }

        public virtual void Draw(GameTime gameTime) { }
    }
}
