using KirosDungeons.Source.Game.Rooms;
using MonoGame.Extended;

namespace KirosDungeons.Source.Game.Entities
{
    public class PlayerClipRectangle : CollisionRectangle
    {
        public PlayerClipRectangle(Room room, RectangleF rectangle) : base(room, rectangle) { }
    }
}
