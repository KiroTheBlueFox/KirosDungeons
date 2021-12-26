using KirosDungeons.Source.Game.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System.IO;

namespace KirosDungeons.Source.Game.Utils
{
    public class AnimatedTexture2D : GameElement
    {
        private readonly string spritePropertiesPath, texturePath;
        private string animationName;
        public string AnimationName { get => animationName; set => Play(value); }
        private JObject spriteProperties;
        private double animationTime;
        public Texture2D Texture { get; private set; }
        public bool Animated = true;
        public int Frame { get => currentFrame; set => currentFrame = Frame % numberOfFrames; }
        private int numberOfFrames, columns, lines, currentFrame;
        public int FrameWidth { get => spriteProperties["textureAtlas"]["regionWidth"].ToObject<int>(); }
        public int FrameHeight { get => spriteProperties["textureAtlas"]["regionHeight"].ToObject<int>(); }

        public AnimatedTexture2D(KirosDungeons game, Screen screen, string spritePropertiesPath, string texturePath) : base(game, screen)
        {
            this.spritePropertiesPath = spritePropertiesPath;
            this.texturePath = texturePath;
            animationTime = 0;
        }

        public override void Load()
        {
            var jsonPath = Path.Combine(Game.Content.RootDirectory, spritePropertiesPath);
            using (var stream = TitleContainer.OpenStream(jsonPath))
            {
                StreamReader reader = new StreamReader(stream);
                string json = reader.ReadToEnd();
                spriteProperties = JObject.Parse(json);
            }
            Texture = Game.Content.Load<Texture2D>(texturePath);
            columns = Texture.Width / spriteProperties["textureAtlas"]["regionWidth"].ToObject<int>();
            lines = Texture.Height / spriteProperties["textureAtlas"]["regionHeight"].ToObject<int>();
        }

        public void Play(string animationName)
        {
            if (animationName != this.animationName)
            {
                numberOfFrames = spriteProperties["cycles"][animationName]["frames"].ToObject<int[]>().Length;
                currentFrame = 0;
                animationTime = 0;
                this.animationName = animationName;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Animated)
            {
                animationTime += gameTime.ElapsedGameTime.TotalSeconds;
                if (animationTime >= spriteProperties["cycles"][animationName]["frameDuration"].ToObject<double>())
                {
                    animationTime = 0;
                    currentFrame += 1;
                    if (currentFrame >= numberOfFrames)
                    {
                        if (spriteProperties["cycles"][animationName]["isLooping"].ToObject<bool>())
                            currentFrame = 0;
                        else
                            currentFrame = numberOfFrames - 1;
                    }
                }
            }

            base.Update(gameTime);
        }

        public void Draw(GameTime gameTime, Vector2 position)
        {
            int frame = spriteProperties["cycles"][animationName]["frames"].ToObject<int[]>()[currentFrame];
            int x = frame % columns;
            int y = frame / columns;

            SpriteBatch.Draw(Texture, position,
                new Rectangle(x * spriteProperties["textureAtlas"]["regionWidth"].ToObject<int>(),
                    y * spriteProperties["textureAtlas"]["regionHeight"].ToObject<int>(),
                    spriteProperties["textureAtlas"]["regionWidth"].ToObject<int>(),
                    spriteProperties["textureAtlas"]["regionHeight"].ToObject<int>()), Color.White);

            base.Draw(gameTime);
        }
    }
}
