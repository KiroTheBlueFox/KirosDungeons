using KirosDungeons.Source.Game.Rooms;
using MonoGame.Extended;

namespace KirosDungeons.Source.Game.Entities
{
    public class JumpThroughRectangle : CollisionRectangle
    {
        public JumpThroughRectangle(Room room, RectangleF rectangle) : base(room, rectangle) { }
    }
}
