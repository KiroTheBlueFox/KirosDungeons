using KirosDungeons.Source.Game.Rooms;
using KirosDungeons.Source.Game.Screens;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System;

namespace KirosDungeons.Source.Game.Entities
{
    public class GameEntity : GameElement, ICollisionActor
    {
        public int Speed = 256;
        public Room Room;
        public Vector2 Position { get; protected set; }
        public RectangleF Collision { get; protected set; }
        public RectangleF Bounds { get => new RectangleF(Position.X + Collision.X, Position.Y + Collision.Y - Collision.Height, Collision.Width, Collision.Height); }
        public bool Fly = false, Ground = false;
        public int Wall = 0;
        public Vector2 Velocity = Vector2.Zero;
        public float VerticalVelocity { get => Velocity.Y; set => Velocity.Y = value; }
        public float HorizontalVelocity { get => Velocity.X; set => Velocity.X = value; }

        IShapeF ICollisionActor.Bounds => Bounds;

        public bool Collide = true;

        public GameEntity(KirosDungeons game, Screen screen, Room room, float x, float y, int boundsOffsetX, int boundsOffsetY, int width, int height) : base(game, screen)
        {
            Room = room;
            Position = new Vector2(x, y);
            Collision = new RectangleF(boundsOffsetX, boundsOffsetY, width, height);
        }

        public override void Load()
        {
            base.Load();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Fly && !Ground)
            {
                if (VerticalVelocity < Room.Gravity)
                    VerticalVelocity += Room.GravityStrength;
            }

            Vector2 movement = Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Move(movement);

            base.Update(gameTime);
        }

        public virtual void Move(float x = 0, float y = 0) => Position += new Vector2(x, y);

        public void Move(Point point) => Move(point.X, point.Y);

        public void Move(Vector2 vector) => Move(vector.X, vector.Y);

        public virtual void OnCollision(CollisionEventArgs collisionInfo)
        {
            if ((collisionInfo.Other is GameEntity entity && entity.Collide && Collide) || !(collisionInfo.Other is GameEntity))
                Position -= collisionInfo.PenetrationVector;

            Ground = collisionInfo.PenetrationVector.Y <= 0;

            Wall = Math.Sign(collisionInfo.PenetrationVector.X);
        }
    }
}
