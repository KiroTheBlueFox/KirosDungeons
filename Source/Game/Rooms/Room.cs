using KirosDungeons.Source.Game.Entities;
using KirosDungeons.Source.Game.Screens;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using System;
using System.Collections.Generic;

namespace KirosDungeons.Source.Game.Rooms
{
    public class Room : GameElement
    {
        public TiledMap TiledMap { get; private set; }
        public TiledMapRenderer TiledMapRenderer { get; private set; }

        private readonly string MapFile;
        public int Width => TiledMap.Width;
        public int Height => TiledMap.Height;
        public int WidthInPixels => TiledMap.WidthInPixels;
        public int HeightInPixels => TiledMap.HeightInPixels;
        public List<CollisionRectangle> Collisions = new List<CollisionRectangle>();
        public float Gravity { get; private set; } = 256;
        public float GravityStrength { get; private set; } = 16;

        public Room(KirosDungeons game, Screen screen, string mapFile) : base(game, screen)
        {
            MapFile = mapFile;
        }

        public override void Load()
        {
            TiledMap = Content.Load<TiledMap>(MapFile);
            TiledMapRenderer = new TiledMapRenderer(Game.GraphicsDevice, TiledMap);

            SetupCollisions();

            base.Load();
        }

        private void SetupCollisions()
        {
            foreach (TiledMapLayer layer in TiledMap.Layers)
            {
                if (layer is TiledMapObjectLayer objectLayer && layer.Name.ToLower().Contains("collisions"))
                {
                    foreach (TiledMapObject objectInLayer in objectLayer.Objects)
                    {
                        Collisions.Add(new CollisionRectangle(this, new RectangleF(objectInLayer.Position, objectInLayer.Size)));
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
