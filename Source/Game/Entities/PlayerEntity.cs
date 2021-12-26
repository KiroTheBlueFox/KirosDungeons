using KirosDungeons.Source.Game.Rooms;
using KirosDungeons.Source.Game.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;

namespace KirosDungeons.Source.Game.Entities
{
    public class PlayerEntity : GameEntity
    {
        public PlayerEntity(KirosDungeons game, Screen screen, Room room, float x, float y) : base(game, screen, room, x, y, -8, 0, 16, 24)
        {
        }

        public override void Update(GameTime gameTime)
        {
            int movement = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                movement -= 1;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                movement += 1;

            HorizontalVelocity = Speed * movement;

            if (KeyboardExtended.GetState().WasKeyJustDown(Keys.Space))
                VerticalVelocity = 256;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.FillRectangle((Rectangle) Bounds, Color.Blue);

            base.Draw(gameTime);
        }
    }
}
