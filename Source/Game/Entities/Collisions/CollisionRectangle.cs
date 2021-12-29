using KirosDungeons.Source.Game.Rooms;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace KirosDungeons.Source.Game.Entities
{
    public class CollisionRectangle : ICollisionActor
    {
        public Room Room { get; private set; }
        public IShapeF Bounds { get; private set; }

        public CollisionRectangle(Room room, RectangleF rectangle)
        {
            Room = room;
            Bounds = rectangle;
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
        }
    }
}
