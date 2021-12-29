using KirosDungeons.Source.Game.Entities.Bullets;
using KirosDungeons.Source.Game.Rooms;
using KirosDungeons.Source.Game.Screens;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Input;
using System;

namespace KirosDungeons.Source.Game.Entities
{
    public class PlayerEntity : GameEntity
    {
        public int JumpForce { get; protected set; } = -384;
        public int MaxJumpCount { get; protected set; } = 1;
        public int JumpCount { get; protected set; } = 0;
        public KeyboardStateExtended KeyboardState { get => Game.KeyboardState; }
        public MouseStateExtended MouseState { get => Game.MouseState; }
        private float ShootCooldown = 0;
        private float MaxShootCooldown = 0.1f;

        public PlayerEntity(KirosDungeons game, GameScreen screen, Room room, float x, float y) : base(game, screen, room, x, y, -8, 0, 16, 24)
        {
            Collide = false;
        }

        public override void Update(GameTime gameTime)
        {
            int direction = GetDirection(KeyboardState);

            if (Ground)
            {
                HorizontalVelocity = Speed * direction * SpeedMultiplier;
                if (JumpCount > 0)
                    JumpCount = 0;
            }
            else
                HorizontalVelocity = Math.Clamp(HorizontalVelocity + Room.AirFriction * direction, -Speed, Speed);

            GoesThrough = Settings.IsKeyDown(KeyboardState, Settings.FallKey);

            if (Settings.WasKeyJustUp(KeyboardState, Settings.JumpKey) && TimeInAir <= 0.1 && JumpCount < MaxJumpCount)
                Jump();

            if (MouseState.IsButtonDown(MouseButton.Left))
            {
                ShootCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (ShootCooldown <= 0)
                {
                    Shoot();
                    ShootCooldown = MaxShootCooldown;
                }
            }
            else
            {
                if (ShootCooldown > 0)
                    ShootCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                else
                    ShootCooldown = 0;
            }

            base.Update(gameTime);
        }

        private void Shoot()
        {
            Vector2 direction = MouseState.Position.ToVector2() / Game.Scale + (Screen.CameraPosition.ToVector2() - Game.MainTarget.Bounds.Size.ToVector2() / 2) - (Vector2)((RectangleF)Bounds).Center;
            direction.Normalize();
            Vector2 position = ((RectangleF)Bounds).Center + direction * 16;
            Screen.AddEntity(new BasicBullet(Game, Screen, Room, (int)position.X, (int)position.Y, 4, true, 1, direction, 384, 1, true, 0.99f, true, 0.9f, "Textures/Bullets/Wide", true));
        }

        private void Jump()
        {
            VerticalVelocity = JumpForce;
            JumpCount++;
            if (Ground)
                Ground = false;
        }

        private int GetDirection(KeyboardStateExtended state)
        {
            int direction = 0;
            if (Settings.IsKeyDown(state, Settings.MoveLeftKey))
                direction -= 1;
            if (Settings.IsKeyDown(state, Settings.MoveRightKey))
                direction += 1;
            return direction;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.FillRectangle(PixelPosition, Collision.Size, Color.Blue);

            base.Draw(gameTime);
        }

        public override void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is PlayerClipRectangle || collisionInfo.Other is BasicBullet)
                return;

            base.OnCollision(collisionInfo);
        }
    }
}
