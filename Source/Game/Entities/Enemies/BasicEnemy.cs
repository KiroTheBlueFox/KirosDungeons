using KirosDungeons.Source.Game.Rooms;
using KirosDungeons.Source.Game.Screens;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;

namespace KirosDungeons.Source.Game.Entities.Enemies
{
    public class BasicEnemy : GameEntity
    {
        public BasicEnemy(KirosDungeons game, GameScreen screen, Room room, float x, float y, int boundsOffsetX, int boundsOffsetY, int width, int height) : base(game, screen, room, x, y, boundsOffsetX, boundsOffsetY, width, height)
        {
            Collide = false;
            Speed = 32;
            Health = 10;
        }

        public override void Update(GameTime gameTime)
        {
            if (Health <= 0)
                Kill();

            Vector2 target = Screen.Player.Position;

            int direction = Math.Sign(target.X - Position.X);

            if (Ground)
                HorizontalVelocity = Speed * direction * SpeedMultiplier;
            else
                HorizontalVelocity = Math.Clamp(HorizontalVelocity + Room.AirFriction * direction, -Speed, Speed);

            GoesThrough = target.Y > Position.Y;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.FillRectangle(PixelPosition, Collision.Size, Color.Red);

            base.Draw(gameTime);
        }
    }
}
