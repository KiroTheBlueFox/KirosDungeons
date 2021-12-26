using KirosDungeons.Source.Game.Rooms;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace KirosDungeons.Source.Game.Entities
{
    public class CollisionRectangle : ICollisionActor
    {
        public IShapeF Bounds { get; }
        public Room Room;

        public CollisionRectangle(Room room, RectangleF collisionRect)
        {
            Room = room;
            Bounds = collisionRect;
        }

        public virtual void OnCollision(CollisionEventArgs collisionInfo) { }
    }
}
