using KirosDungeons.Source.Game.Rooms;
using KirosDungeons.Source.Game.Screens;
using KirosDungeons.Source.Game.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System;

namespace KirosDungeons.Source.Game.Entities
{
    public class GameEntity : GameElement, ICollisionActor
    {
        public readonly long ID = LastID++;
        public static long LastID { get; private set; } = long.MinValue;
        public new GameScreen Screen { get; protected set; }
        public float Speed { get; protected set; } = 192;
        public int Health = 3;
        public Room Room;
        public Vector2 Position { get => Bounds.Position; set => Bounds.Position = value; }
        public Vector2 PixelPosition { get => new Vector2((int)Position.X, (int)Position.Y); }
        public RectangleF Collision { get; protected set; }
        public bool Fly = false, HasCollided = false, Ground = false, GoesThrough = false;
        public int Wall = 0;
        public float ClimbMargin { get; protected set; } = 0.25f;
        public Vector2 Velocity = Vector2.Zero;
        public float VerticalVelocity { get => Velocity.Y; set => Velocity.Y = value; }
        public float HorizontalVelocity { get => Velocity.X; set => Velocity.X = value; }
        public float TimeInAir = 0, SpeedMultiplier = 1;
        public bool LockVerticalVelocity { get; protected set; } = true;

        public IShapeF Bounds { get; set; }

        public bool Collide = true;

        public GameEntity(KirosDungeons game, GameScreen screen, Room room, float x, float y, int boundsOffsetX, int boundsOffsetY, int width, int height) : base(game, screen)
        {
            Screen = screen;
            Room = room;
            Collision = new RectangleF(boundsOffsetX, boundsOffsetY, width, height);
            Bounds = new RectangleF(x + boundsOffsetX, y + boundsOffsetY - height, width, height);
        }

        public override void Load()
        {
            base.Load();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Ground && !Fly) {
                TimeInAir += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (VerticalVelocity < Room.Gravity || !LockVerticalVelocity)
                    VerticalVelocity += Room.GravityStrength;
            }

            if (!HasCollided && !Fly)
                Ground = false;

            Vector2 movement = Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!Fly)
                Move(Wall == Math.Sign(movement.X) && HasCollided ? 0 : movement.X, Ground && movement.Y > 0 ? 0 : movement.Y);
            else
                Move(movement);

            base.Update(gameTime);

            HasCollided = false;
        }

        public virtual void Move(float x = 0, float y = 0) => Position += new Vector2(x, y);

        public void Move(Point point) => Move(point.X, point.Y);

        public void Move(Vector2 vector) => Move(vector.X, vector.Y);

        public bool IsInRoomBounds() => Position.X >= 0 || Position.X < Room.WidthInPixels || Position.Y >= 0 || Position.Y < Room.HeightInPixels;

        public void Kill()
        {
            Screen.RemoveEntity(ID);
        }

        public Direction? GetSideOfCollision(Vector2 penetrationVector)
        {
            if (penetrationVector.X > 0)
                return Direction.Right;
            if (penetrationVector.X < 0)
                return Direction.Left;
            if (penetrationVector.Y > 0)
                return Direction.Up;
            if (penetrationVector.Y < 0)
                return Direction.Down;
            return null;
        }

        public virtual void OnCollision(CollisionEventArgs collisionInfo)
        {
            Direction? sideOfOther = GetSideOfCollision(collisionInfo.PenetrationVector);

            if (collisionInfo.Other is SlopeRectangle slope)
            {
                RectangleF slopeRect = (RectangleF)slope.Bounds;
                RectangleF thisRect = (RectangleF)Bounds;
                float playerPosInSlope = 0;
                switch (slope.Direction)
                {
                    case Direction.Right:
                        playerPosInSlope = 1 - (thisRect.Right - slopeRect.Left) / slopeRect.Width;
                        break;
                    case Direction.Left:
                        playerPosInSlope = (thisRect.Left - slopeRect.Left) / slopeRect.Width;
                        break;
                }
                if (playerPosInSlope >= 0 && playerPosInSlope <= 1)
                {
                    float newY = slopeRect.Top - thisRect.Height + playerPosInSlope * slopeRect.Height;
                    if (Position.Y >= newY)
                    {
                        HasCollided = true;
                        Position = new Vector2(Position.X, newY);
                        Ground = true;
                        VerticalVelocity = 0;
                        TimeInAir = 0;
                        SpeedMultiplier = 1 - MathHelper.ToDegrees((float)Math.Atan(slopeRect.Width / slopeRect.Height)) / 90;
                    }
                    else
                    {
                        Ground = false;
                    }
                }
            }
            else if ((collisionInfo.Other is GameEntity entity && entity.Collide && Collide) || !(collisionInfo.Other is GameEntity))
            {
                if (collisionInfo.Other is JumpThroughRectangle && (Velocity.Y < 0 || GoesThrough || ((RectangleF)Bounds).Bottom - collisionInfo.PenetrationVector.Y > ((RectangleF)collisionInfo.Other.Bounds).Top))
                    return;

                HasCollided = true;

                RectangleF rect = (RectangleF)collisionInfo.Other.Bounds;
                RectangleF thisRect = (RectangleF)Bounds;

                if (sideOfOther == Direction.Left)
                    Wall = 1;
                else if (sideOfOther == Direction.Right)
                    Wall = -1;
                else
                    Wall = 0;

                if (sideOfOther == Direction.Left || sideOfOther == Direction.Right)
                {
                    float differenceInPixels = Math.Abs(rect.Top - thisRect.Bottom);
                    float difference = differenceInPixels / thisRect.Height;
                    if (difference <= ClimbMargin)
                    {
                        Position -= Vector2.UnitY * differenceInPixels;
                        HorizontalVelocity *= difference / 1;
                    }
                    else
                        HorizontalVelocity = 0;
                }

                Position -= collisionInfo.PenetrationVector;

                if (sideOfOther == Direction.Up)
                    Ground = true;
                else
                    Ground = false;

                if (sideOfOther == Direction.Up || sideOfOther == Direction.Down)
                {
                    SpeedMultiplier = 1;
                    VerticalVelocity = 0;
                    TimeInAir = 0;
                }
            }
        }
    }
}
