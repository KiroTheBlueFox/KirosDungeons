using KirosDungeons.Source.Game;
using KirosDungeons.Source.Game.Screens;
using KirosDungeons.Source.Game.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace KirosDungeons
{
    public class KirosDungeons : Game
    {
        public static readonly string SETTINGS_FILE = "Settings.json";
        public static readonly string GAME_SAVE_FILE = "GameSave.sav";
        public static readonly int AUTOSAVE_TIMER = 5, WIDTH = 640, HEIGHT = 360;


        public GraphicsDeviceManager Graphics { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public Screen Screen { get; private set; }
        public Fonts Fonts { get; private set; }
        public Settings Settings { get; private set; }
        public GameSave GameSave { get; private set; }
        public GameStats GameStats { get; private set; }
        public float hScale { get; private set; }
        public float vScale { get; private set; }
        public float Scale { get => Math.Min(hScale, vScale); }
        public RenderTarget2D MainTarget { get; private set; }
        private Dictionary<string, Language> Languages;
        public Language Language { get { foreach (KeyValuePair<string, Language> Language in Languages) if (Language.Key == Settings.Language || Language.Value.Name == Settings.Language) return Language.Value; return Languages["en_us"]; } }
        private bool SaveResize = true;
        
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
        private double TimeSinceLastSave = 0;
        public KeyboardStateExtended KeyboardState { get; private set; }
        public MouseStateExtended MouseState { get; private set; }

        public KirosDungeons()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Screen = new GameScreen(this);

            Settings = EnsureJson<Settings>(SETTINGS_FILE);

            Languages = new Dictionary<string, Language>();
            Languages["fr_fr"] = new Language(this, "Français", "Langs/fr_fr");
            Languages["en_us"] = new Language(this, "English", "Langs/en_us");
        }

        public void SetScreen(Screen screen)
        {
            Screen = screen;
        }

        public void NewStats()
        {
            GameStats = new GameStats();
        }

        protected override void Initialize()
        {
            Graphics.IsFullScreen = Settings.Fullscreen;
            if (Settings.Fullscreen)
            {
                Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                Graphics.PreferredBackBufferWidth = Settings.Width;
                Graphics.PreferredBackBufferHeight = Settings.Height;
            }
            Graphics.ApplyChanges();
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(OnResize);

            MainTarget = new RenderTarget2D(GraphicsDevice, WIDTH, HEIGHT);

            Fonts = new Fonts(this);

            if (Screen != null)
                Screen.Initialize();

            base.Initialize();
        }
        public void OnResize(object sender, EventArgs e)
        {
            if (SaveResize)
            {
                Settings.Width = GraphicsDevice.Viewport.Width;
                Settings.Height = GraphicsDevice.Viewport.Height;
                SaveJson(SETTINGS_FILE, Settings);
            }
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Fonts.Load();

            foreach (KeyValuePair<string,Language> Language in Languages)
                Language.Value.Load();

            if (Screen != null)
                Screen.Load();
        }

        protected override void UnloadContent()
        {
            SaveJson(SETTINGS_FILE, Settings);

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState = KeyboardExtended.GetState();
            MouseState = MouseExtended.GetState();

            if (KeyboardState.WasKeyJustDown(Keys.F11))
            {
                if (Settings.Fullscreen)
                    SwitchMode(WindowMode.Borderless);
                else if (Settings.Borderless)
                    SwitchMode(WindowMode.Windowed);
                else
                    SwitchMode(WindowMode.Fullscreen);
            }

            TimeSinceLastSave += gameTime.TotalGameTime.TotalMinutes;
            if (TimeSinceLastSave >= AUTOSAVE_TIMER)
            {
                SaveJson(GAME_SAVE_FILE, GameSave);
            }

            if (Screen != null)
                Screen.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(MainTarget);
            GraphicsDevice.Clear(Color.Transparent);

            if (Screen != null)
                Screen.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            SpriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);

            hScale = GraphicsDevice.Viewport.Width / (float)MainTarget.Width;
            vScale = GraphicsDevice.Viewport.Height / (float)MainTarget.Height;
            Rectangle destRect;

            if (hScale > vScale)
            {
                Vector2 newSize = MainTarget.Bounds.Size.ToVector2() * vScale;
                destRect = new Rectangle((int)Math.Round((GraphicsDevice.Viewport.Width - (int)Math.Round(newSize.X)) / 2f),
                    (int)Math.Round((GraphicsDevice.Viewport.Height - (int)Math.Round(newSize.Y)) / 2f),
                    (int)Math.Round(newSize.X),
                    (int)Math.Round(newSize.Y));
            }
            else if (hScale < vScale)
            {
                Vector2 newSize = MainTarget.Bounds.Size.ToVector2() * hScale;
                destRect = new Rectangle((int)Math.Round((GraphicsDevice.Viewport.Width - (int)Math.Round(newSize.X)) / 2f),
                    (int)Math.Round((GraphicsDevice.Viewport.Height - (int)Math.Round(newSize.Y)) / 2f),
                    (int)Math.Round(newSize.X),
                    (int)Math.Round(newSize.Y));
            }
            else
            {
                destRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height);
            }

            SpriteBatch.Draw(MainTarget, destRect, MainTarget.Bounds, Color.White);

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        public void SwitchMode(WindowMode mode)
        {
            SaveResize = false;
            switch (mode)
            {
                case WindowMode.Fullscreen:
                    Settings.Fullscreen = true;
                    Settings.Borderless = false;
                    Graphics.IsFullScreen = true;
                    Window.IsBorderless = false;
                    Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    break;
                case WindowMode.Windowed:
                    Settings.Fullscreen = false;
                    Settings.Borderless = false;
                    Graphics.IsFullScreen = false;
                    Window.IsBorderless = false;
                    Graphics.PreferredBackBufferWidth = Settings.Width;
                    Graphics.PreferredBackBufferHeight = Settings.Height;
                    break;
                case WindowMode.Borderless:
                    Settings.Fullscreen = false;
                    Settings.Borderless = true;
                    Graphics.IsFullScreen = false;
                    Window.IsBorderless = true;
                    Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    break;
            }
            Graphics.ApplyChanges();
            SaveJson(SETTINGS_FILE, Settings);
            SaveResize = true;
        }

        public void ChangeLanguage(string language)
        {
            Settings.Language = language;
            SaveJson(SETTINGS_FILE, Settings);
        }

        public static string GetPath(string name) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
        public static T LoadJson<T>(string name) where T : new()
        {
            T json;
            string jsonPath = GetPath(name);

            if (File.Exists(jsonPath))
            {
                json = JsonSerializer.Deserialize<T>(File.ReadAllText(jsonPath), _options);
            }
            else
            {
                json = new T();
            }

            return json;
        }
        public static void SaveJson<T>(string name, T json)
        {
            string jsonPath = GetPath(name);
            string jsonString = JsonSerializer.Serialize(json, _options);
            File.WriteAllText(jsonPath, jsonString);
        }
        public static T EnsureJson<T>(string name) where T : new()
        {
            T json;
            string jsonPath = GetPath(name);

            if (File.Exists(jsonPath))
            {
                json = JsonSerializer.Deserialize<T>(File.ReadAllText(jsonPath), _options);
            }
            else
            {
                json = new T();
                string jsonString = JsonSerializer.Serialize(json, _options);
                File.WriteAllText(jsonPath, jsonString);
            }

            return json;
        }

        public void SetRenderTarget(RenderTarget2D RenderTarget)
        {
            GraphicsDevice.SetRenderTarget(RenderTarget ?? MainTarget);
        }
    }

    public enum WindowMode
    {
        Fullscreen,
        Windowed,
        Borderless
    }
}
