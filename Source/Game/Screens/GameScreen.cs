using KirosDungeons.Source.Game.Entities;
using KirosDungeons.Source.Game.Rooms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Tiled;
using System.Collections.Generic;

namespace KirosDungeons.Source.Game.Screens
{
    public class GameScreen : Screen
    {
        public Room Room { get; private set; }
        public Point CameraPosition { get; private set; }
        public RenderTarget2D GameTarget { get; private set; }
        public List<GameEntity> Entities { get; private set; }
        public GameEntity Player { get; private set; }
        protected CollisionComponent CollisionComponent { get; private set; }

        public GameScreen(KirosDungeons game) : this(game, null)
        {
        }

        public GameScreen(KirosDungeons game, Screen screen) : base(game, screen)
        {
            Entities = new List<GameEntity>();
        }

        public override void Initialize()
        {
            foreach (GameEntity entity in Entities)
                entity.Initialize();

            Room = new Room(Game, this, "Tilemaps/test");
            Player = new PlayerEntity(Game, this, Room, 0, 0);

            base.Initialize();
        }

        public override void Load()
        {
            Room.Load();

            RefreshCollisions();

            Player.Load();
            Player.Move(Room.WidthInPixels / 2, Room.HeightInPixels / 2);

            GameTarget = new RenderTarget2D(Game.GraphicsDevice, Room.WidthInPixels, Room.HeightInPixels);

            CameraPosition = new Point(Room.WidthInPixels / 2, Room.HeightInPixels / 2);

            foreach (GameEntity entity in Entities)
                entity.Load();

            base.Load();
        }

        public void RefreshCollisions()
        {
            CollisionComponent = new CollisionComponent(new RectangleF(0, 0, Room.WidthInPixels, Room.HeightInPixels));
            CollisionComponent.Initialize();

            foreach (CollisionRectangle collision in Room.Collisions)
            {
                CollisionComponent.Insert(collision);
            }

            CollisionComponent.Insert(Player);
            foreach (GameEntity entity in Entities)
                CollisionComponent.Insert(entity);
        }

        public override void Update(GameTime gameTime)
        {
            Room.Update(gameTime);

            Player.Update(gameTime);

            foreach (GameEntity entity in Entities)
                if (entity.Room == Room)
                    entity.Update(gameTime);

            CollisionComponent.Update(gameTime);

            MoveCamera(gameTime);

            base.Update(gameTime);
        }

        private void MoveCamera(GameTime gameTime)
        {
            CameraPosition = new Point(MathHelper.Clamp((int)Player.Position.X, KirosDungeons.WIDTH / 2, Room.WidthInPixels - KirosDungeons.WIDTH / 2),
                                        MathHelper.Clamp((int)Player.Position.Y, KirosDungeons.HEIGHT / 2, Room.HeightInPixels - KirosDungeons.HEIGHT / 2));
        }

        public override void Draw(GameTime gameTime)
        {
            Game.SetRenderTarget(GameTarget);
            Game.GraphicsDevice.Clear(Color.Transparent);

            SpriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);

            foreach (TiledMapLayer layer in Room.TiledMap.Layers)
                if (layer.IsVisible && layer.Properties.ContainsKey("Colorable"))
                    if (bool.Parse(layer.Properties["Colorable"]))
                        Room.TiledMapRenderer.Draw(layer);

            SpriteBatch.End();

            SpriteBatch.Begin(blendState: new BlendState()
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.Zero
            }, samplerState: SamplerState.PointClamp);

            SpriteBatch.FillRectangle(new Rectangle(0, 0, Room.WidthInPixels, Room.HeightInPixels), new Color(1, 0.75f, 0.75f, 1));

            SpriteBatch.End();

            SpriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);

            foreach (TiledMapLayer layer in Room.TiledMap.Layers)
                if (layer.IsVisible && ! (layer.Properties.ContainsKey("Colorable") && bool.Parse(layer.Properties["Colorable"])))
                    Room.TiledMapRenderer.Draw(layer);

            SpriteBatch.End();

            Game.SetRenderTarget(null);

            Vector2 camPosition = -CameraPosition.ToVector2() + Game.MainTarget.Bounds.Size.ToVector2() / 2;

            SpriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateTranslation(camPosition.X, camPosition.Y, 0));

            SpriteBatch.Draw(GameTarget, Vector2.Zero, Color.White);

            foreach (GameEntity entity in Entities)
                if (entity.Room == Room)
                    entity.Draw(gameTime);

            Player.Draw(gameTime);

            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
