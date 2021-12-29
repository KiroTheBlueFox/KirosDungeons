using KirosDungeons.Source.Game.Entities;
using KirosDungeons.Source.Game.Entities.Enemies;
using KirosDungeons.Source.Game.Rooms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KirosDungeons.Source.Game.Screens
{
    public class GameScreen : Screen
    {
        private Room _room;
        public Room Room
        {
            get => _room;
            set
            {
                _room = value;

                CollisionComponent = new CollisionComponent(new RectangleF(0, 0, Room.WidthInPixels, Room.HeightInPixels));
                CollisionComponent.Initialize();

                RefreshCollisions();
            }
        }
        public Point CameraPosition { get; private set; }
        public RenderTarget2D GameTarget { get; private set; }
        public List<GameEntity> Entities { get; private set; }
        public List<long> EntitiesToKill { get; private set; }
        public GameEntity Player { get; private set; }
        protected CollisionComponent CollisionComponent { get; private set; }

        public GameScreen(KirosDungeons game) : this(game, null)
        {
        }

        public GameScreen(KirosDungeons game, Screen screen) : base(game, screen)
        {
            Entities = new List<GameEntity>();
            EntitiesToKill = new List<long>();
        }

        public override void Initialize()
        {
            foreach (GameEntity entity in Entities)
                entity.Initialize();

            _room = new Room(Game, this, "Tilemaps/test");
            Player = new PlayerEntity(Game, this, Room, 0, 0);

            base.Initialize();
        }

        public void RemoveEntity(long id)
        {
            EntitiesToKill.Add(id);
        }

        public void AddEntity(GameEntity entity)
        {
            Entities.Add(entity);
            CollisionComponent.Insert(entity);
        }

        public override void Load()
        {
            Room.Load();

            CollisionComponent = new CollisionComponent(new RectangleF(0, 0, Room.WidthInPixels, Room.HeightInPixels));
            CollisionComponent.Initialize();

            Entities.Add(new BasicEnemy(Game, this, Room, Room.WidthInPixels / 2, Room.HeightInPixels / 2, -8, -24, 16, 24));

            RefreshCollisions();

            Player.Load();
            Player.Move(Room.WidthInPixels / 2f, Room.HeightInPixels / 2f);

            GameTarget = new RenderTarget2D(Game.GraphicsDevice, Room.WidthInPixels, Room.HeightInPixels);

            CameraPosition = new Point(Room.WidthInPixels / 2, Room.HeightInPixels / 2);

            foreach (GameEntity entity in Entities)
                entity.Load();

            base.Load();
        }

        public void RefreshCollisions()
        {
            foreach (CollisionRectangle collision in Room.Collisions)
                CollisionComponent.Insert(collision);

            foreach (SlopeRectangle collision in Room.SlopeCollisions)
                CollisionComponent.Insert(collision);

            foreach (JumpThroughRectangle collision in Room.JumpThroughCollisions)
                CollisionComponent.Insert(collision);

            foreach (CollisionRectangle collision in Room.PlayerClipCollisions)
                CollisionComponent.Insert(collision);

            CollisionComponent.Insert(Player);
            foreach (GameEntity entity in Entities)
                if (entity.Room == Room)
                {
                    CollisionComponent.Insert(entity);
                    Debug.WriteLine("LOL");
                }
        }

        public override void Update(GameTime gameTime)
        {
            Room.Update(gameTime);

            Player.Update(gameTime);

            foreach (long entityID in EntitiesToKill)
            {
                GameEntity entity = Entities.Find(entity => entity.ID == entityID);
                if (entity != null)
                {
                    Entities.Remove(entity);
                    CollisionComponent.Remove(entity);
                }
            }

            EntitiesToKill.Clear();

            foreach (GameEntity entity in Entities)
                if (entity.Room == Room)
                    entity.Update(gameTime);

            CollisionComponent.Update(gameTime);

            MoveCamera(gameTime);

            base.Update(gameTime);
        }

        private void MoveCamera(GameTime gameTime)
        {
            CameraPosition = new Point(MathHelper.Clamp((int)((RectangleF)Player.Bounds).Center.X, KirosDungeons.WIDTH / 2, Room.WidthInPixels - KirosDungeons.WIDTH / 2),
                                        MathHelper.Clamp((int)((RectangleF)Player.Bounds).Center.Y, KirosDungeons.HEIGHT / 2, Room.HeightInPixels - KirosDungeons.HEIGHT / 2));
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

            SpriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);

            PixelFont.Draw(SpriteBatch, ((int) Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds)).ToString(), Vector2.Zero, Utils.FontOptions.Default, new Utils.FontStyle() { Color = Color.White });

            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
