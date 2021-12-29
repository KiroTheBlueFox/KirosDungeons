using KirosDungeons.Source.Game.Rooms;
using KirosDungeons.Source.Game.Screens;
using KirosDungeons.Source.Game.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System;
using System.Diagnostics;

namespace KirosDungeons.Source.Game.Entities.Bullets
{
    public class BasicBullet : GameEntity
    {
        public const string DEFAULT_TEXTURE = "Textures/Bullets/Normal";
        public new IShapeF Bounds;
        public bool FromPlayer;
        public Texture2D Texture { get; protected set; }
        public int Radius { get; protected set; }
        public int Damages { get; protected set; }
        public bool DirectionalTexture { get; protected set; }
        public float SpeedFactor { get; protected set; }
        public bool Bounces { get; protected set; } 
        public float BounceFactor { get; protected set; }
        public float DisappearingTime { get; protected set; }
        private float TimeBeforeDisappearing = 0;

        public BasicBullet(KirosDungeons game, GameScreen screen, Room room, float x, float y, int radius, bool player, int damages, Vector2 direction, float speed, float disappearingTime, bool gravity = false, float speedFactor = 1, bool bounces = false, float bounceFactor = 1, string texture = DEFAULT_TEXTURE, bool directionalTexture = false) : base(game, screen, room, x, y, 0, 0, 0, 0)
        {
            Bounds = new CircleF(new Point2(x, y), radius);
            Radius = radius;
            Speed = speed;
            Velocity = direction * speed;
            Bounces = bounces;
            BounceFactor = bounceFactor;
            Damages = damages;
            DisappearingTime = disappearingTime;
            Texture = Content.Load<Texture2D>(texture);
            Fly = !gravity;
            LockVerticalVelocity = false;
            SpeedFactor = speedFactor;
            FromPlayer = player;
            DirectionalTexture = directionalTexture;
        }

        public override void Update(GameTime gameTime)
        {
            Debug.WriteLine(Velocity);

            if (!Fly)
                HorizontalVelocity *= SpeedFactor;

            TimeBeforeDisappearing += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (TimeBeforeDisappearing >= DisappearingTime || !IsInRoomBounds())
            {
                Kill();
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 pos = PixelPosition - Vector2.One * Radius;
            if (DirectionalTexture)
            {
                float scale = Math.Max(Radius * 2f / Texture.Width, Radius * 2f / Texture.Height);
                SpriteBatch.Draw(Texture, PixelPosition, Texture.Bounds, Color.Yellow, (float)Math.Atan2(Velocity.Y, Velocity.X), Texture.Bounds.Size.ToVector2() / 2, new Vector2(scale, scale), SpriteEffects.None, 0);
            }
            else
            {
                SpriteBatch.Draw(Texture, new Rectangle((int)pos.X, (int)pos.Y, Radius * 2, Radius * 2), Color.Yellow);
            }

            base.Draw(gameTime);
        }

        public override void OnCollision(CollisionEventArgs collisionInfo)
        {
            base.OnCollision(collisionInfo);

            if (collisionInfo.Other is GameEntity entity)
            {
                if (FromPlayer && !(entity is PlayerEntity))
                {
                    entity.Health -= Damages;
                    Kill();
                }
                else if (!FromPlayer && entity is PlayerEntity)
                {
                    entity.Health -= 1;
                    Kill();
                }
            }

            if (HasCollided && !(collisionInfo.Other is PlayerClipRectangle))
            {
                if (Bounces)
                {
                    if (collisionInfo.Other is SlopeRectangle slope)
                    {
                        Velocity = -(2 * slope.Normal.Dot(Velocity) * slope.Normal - Velocity) * BounceFactor;
                    }
                    else if (!(collisionInfo.Other is JumpThroughRectangle))
                    {
                        Direction? sideOfOther = GetSideOfCollision(collisionInfo.PenetrationVector);

                        if (sideOfOther == Direction.Left || sideOfOther == Direction.Right)
                            HorizontalVelocity = -(HorizontalVelocity * BounceFactor);
                        else if (sideOfOther == Direction.Up || sideOfOther == Direction.Down)
                            VerticalVelocity = -(VerticalVelocity * BounceFactor);
                    }
                }
                else
                    Kill();
            }
        }
    }
}
