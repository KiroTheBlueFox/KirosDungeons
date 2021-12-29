using KirosDungeons.Source.Game.Rooms;
using KirosDungeons.Source.Game.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace KirosDungeons.Source.Game.Entities
{
    public class SlopeRectangle : CollisionRectangle
    {
        public Direction Direction { get; private set; }
        public Vector2 Normal { get; private set; }

        public SlopeRectangle(Room room, RectangleF rectangle, Direction direction) : base(room, rectangle)
        {
            Direction = direction;

            RectangleF Bounds = (RectangleF)this.Bounds;
            Vector2 PointA = Bounds.BottomLeft, PointB = Bounds.TopRight;
            if (Direction == Direction.Left)
            {
                PointA = Bounds.TopLeft;
                PointB = Bounds.BottomRight;
            }
            Vector2 Vector = PointB - PointA;
            Normal = new Vector2(-Vector.Y, Vector.X).NormalizedCopy();
        }
    }
}
