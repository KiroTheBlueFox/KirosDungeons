using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using XnaGame = Microsoft.Xna.Framework.Game;

namespace KirosDungeons.Source.Game.Utils
{
    /// <summary>
    /// Made by KiroTheBlueFox.
    /// Free to use and modify.
    /// </summary>
    public class PixelatedFont
    {
        private string filePath;
        public Texture2D Texture { get; private set; }
        public FontParameters FontParameters { get; private set; }
        public Dictionary<char, FontCharParameters> Chars { get; private set; }
        public readonly XnaGame Game;
        public string Name { get => FontParameters.Name; }
        public int Height { get => FontParameters.Height; }

        public PixelatedFont(XnaGame game, string filePath)
        {
            Game = game;
            this.filePath = filePath;
        }

        public void Load()
        {
            Texture = Game.Content.Load<Texture2D>(filePath);

            var paramsPath = Path.Combine(Game.Content.RootDirectory, filePath + ".json");
            using var stream = TitleContainer.OpenStream(paramsPath);
            StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            FontParameters = JsonConvert.DeserializeObject<FontParameters>(json);

            if (FontParameters != null)
            {
                Chars = new Dictionary<char, FontCharParameters>();
                foreach (KeyValuePair<string, FontCharParameters> keyValuePair in FontParameters.Characters)
                    Chars.Add(keyValuePair.Key[0], keyValuePair.Value);
            }
        }

        public void Draw(SpriteBatch spriteBatch, string text, Vector2 position, FontOptions fontOptions, FontStyle fontStyle)
        {
            int currentHeight = 0;

            if (FontParameters.UppersAreLowers) text = text.ToLower();
            if (!FontParameters.Accents) text = StringHelper.RemoveDiacritics(text);

            string[] textLines = text.Split("\n");

            switch (fontOptions.HorizontalAlignment)
            {
                case HorizontalAlignment.Centered:
                    position.X -= TextSize(text, fontOptions).X / 2f;
                    break;
                case HorizontalAlignment.Right:
                    position.X -= TextSize(text, fontOptions).X;
                    break;
            }

            switch (fontOptions.VerticalAlignment)
            {
                case VerticalAlignment.Centered:
                    position.Y -= TextSize(text, fontOptions).Y / 2f;
                    break;
                case VerticalAlignment.Bottom:
                    position.Y -= TextSize(text, fontOptions).Y;
                    break;
            }

            Random random = new Random();
            
            if (fontStyle.HorizontalVibration != 0)
            {
                position.X += random.Next(-Math.Abs(fontStyle.HorizontalVibration), Math.Abs(fontStyle.HorizontalVibration) + 1);
            }

            if (fontStyle.VerticalVibration != 0)
            {
                position.Y += random.Next(-Math.Abs(fontStyle.VerticalVibration), Math.Abs(fontStyle.VerticalVibration) + 1);
            }

            for (int i = 0; i < textLines.Length; i++)
            {
                int currentWidth;
                switch (fontOptions.HorizontalAlignment)
                {
                    case HorizontalAlignment.Centered:
                        currentWidth = (int)Math.Round((TextSize(text, fontOptions).X - TextSize(textLines[i], fontOptions).X) / (2 * fontOptions.Size));
                        break;
                    default:
                    case HorizontalAlignment.Left:
                        currentWidth = 0;
                        break;
                    case HorizontalAlignment.Right:
                        currentWidth = (int)Math.Round((TextSize(text, fontOptions).X - TextSize(textLines[i], fontOptions).X) / fontOptions.Size);
                        break;
                }
                for (int j = 0; j < textLines[i].Length; j++)
                {
                    char character = textLines[i][j];
                    int offset = 0;
                    if (Chars.ContainsKey(character))
                    {
                        if (Chars[character].Offset != 0)
                        {
                            try
                            {
                                if (Chars[textLines[i][j + 1]].ShouldOffset)
                                    offset = Chars[character].Offset;
                            }
                            catch (Exception) { }
                        }
                        foreach (KeyValuePair<Vector2, Color> keyValuePair in fontStyle.Shadows)
                        {
                            spriteBatch.Draw(Texture, position + ((new Vector2(currentWidth, currentHeight) + keyValuePair.Key) * fontOptions.Size), Chars[character].Rectangle, keyValuePair.Value, 0, Vector2.Zero, fontOptions.Size, SpriteEffects.None, 0);
                        }
                        spriteBatch.Draw(Texture, position + (new Vector2(currentWidth, currentHeight) * fontOptions.Size), Chars[character].Rectangle, fontStyle.Color, 0, Vector2.Zero, fontOptions.Size, SpriteEffects.None, 0);
                        currentWidth += Chars[character].Rectangle.Width + fontOptions.CharSpacing + offset;
                    }
                    else
                    {
                        if (character == ' ')
                        {
                            currentWidth += FontParameters.SpaceLength + fontOptions.CharSpacing;
                        }
                        else if (!char.IsWhiteSpace(character))
                        {
                            char missingChar = FontParameters.MissingCharacter[0];
                            if (Chars[missingChar].Offset != 0)
                            {
                                try
                                {
                                    if (Chars[textLines[i][j + 1]].ShouldOffset)
                                        offset = Chars[missingChar].Offset;
                                }
                                catch (Exception) { }
                            }
                            foreach (KeyValuePair<Vector2, Color> keyValuePair in fontStyle.Shadows)
                            {
                                spriteBatch.Draw(Texture, position + ((new Vector2(currentWidth, currentHeight) + keyValuePair.Key) * fontOptions.Size), Chars[character].Rectangle, keyValuePair.Value, 0, Vector2.Zero, fontOptions.Size, SpriteEffects.None, 0);
                            }
                            spriteBatch.Draw(Texture, position + (new Vector2(currentWidth, currentHeight) * fontOptions.Size), Chars[missingChar].Rectangle, fontStyle.Color, 0, Vector2.Zero, fontOptions.Size, SpriteEffects.None, 0);
                            currentWidth += Chars[missingChar].Rectangle.Width + fontOptions.CharSpacing + offset;
                        }
                    }
                }
                currentHeight += FontParameters.Height + fontOptions.LineSpacing - FontParameters.BaseLine;
            }
        }

        public Vector2 TextSize(string text, FontOptions fontOptions)
        {
            List<int> widths = new List<int>();
            int height = FontParameters.Height + fontOptions.LineSpacing, line = 0;
            widths.Add(0);
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                int offset = 0;
                if (Chars.ContainsKey(character))
                {
                    if (Chars[character].Offset != 0)
                    {
                        try
                        {
                            if (Chars[text[i + 1]].ShouldOffset)
                                offset = Chars[character].Offset;
                        }
                        catch (Exception) { }
                    }
                    widths[line] += Chars[character].Rectangle.Width + fontOptions.CharSpacing + offset;
                }
                else
                {
                    if (character == ' ')
                    {
                        widths[line] += FontParameters.SpaceLength + fontOptions.CharSpacing + offset;
                    }
                    else if (character == '\n')
                    {
                        height += FontParameters.Height + fontOptions.LineSpacing - FontParameters.BaseLine;
                        line++;
                        widths.Add(0);
                    }
                    else if (!char.IsWhiteSpace(character))
                    {
                        char missingChar = FontParameters.MissingCharacter[0];
                        if (Chars[missingChar].Offset != 0)
                        {
                            try
                            {
                                if (Chars[text[i + 1]].ShouldOffset)
                                    offset = Chars[missingChar].Offset;
                            }
                            catch (Exception) { }
                        }
                        widths[line] += Chars[missingChar].Rectangle.Width + fontOptions.CharSpacing + offset;
                    }
                }
            }
            int maxWidth = 0;
            foreach (int width in widths)
            {
                if (width > maxWidth)
                    maxWidth = width;
            }
            return new Vector2(maxWidth, height - fontOptions.LineSpacing) * fontOptions.Size;
        }
        
        public string GetCharacters()
        {
            string chars = "";
            foreach (char character in Chars.Keys)
            {
                chars += character;
            }
            return chars;
        }
    }

    public class FontParameters
    {
        public string Name { get; set; }
        public string MissingCharacter { get; set; } = "?";
        public int Height { get; set; }
        public int SpaceLength { get; set; }
        public int BaseLine { get; set; } = 0;
        public Dictionary<string, FontCharParameters> Characters;
        public bool UppersAreLowers { get; set; } = false;
        public bool Accents { get; set; } = false;
    }

    public class FontCharParameters
    {
        public int[] Bounds { get; set; }
        public Rectangle Rectangle { get => new Rectangle(Bounds[0], Bounds[1], Bounds[2], Bounds[3]); }
        public bool ShouldOffset { get; set; } = false;
        public int Offset { get; set; } = 0;
    }

    public class FontOptions
    {
        /// <summary>
        /// Size multiplicator of the font. 1 by default.
        /// </summary>
        public float Size = 1;

        /// <summary>
        /// Separation between characters (in pixels). 0 by default.
        /// </summary>
        public int CharSpacing = 0;

        /// <summary>
        /// Separation between lines (in pixels). 0 by default.
        /// </summary>
        public int LineSpacing = 0;

        /// <summary>
        /// Horizontal Alignment of the text. LEFT by default.
        /// </summary>
        public HorizontalAlignment HorizontalAlignment = HorizontalAlignment.Left;

        /// <summary>
        /// Vertical Alignment of the text. TOP by default.
        /// </summary>
        public VerticalAlignment VerticalAlignment = VerticalAlignment.Top;

        public static readonly FontOptions Default = new FontOptions();
    }

    public class FontStyle
    {
        /// <summary>
        /// Color of the font. White by default
        /// </summary>
        public Color Color = Color.White;

        /// <summary>
        /// Shadows of the font. Empty by default
        /// </summary>
        public Dictionary<Vector2, Color> Shadows { get; } = new Dictionary<Vector2, Color>();

        /// <summary>
        /// Horizontal vibration of the font. 0 by default (off)
        /// </summary>
        public int HorizontalVibration = 0;

        /// <summary>
        /// Vertical vibration of the font. 0 by default (off)
        /// </summary>
        public int VerticalVibration = 0;

        public static readonly FontStyle Default = new FontStyle();

        /// <summary>
        /// Add a shadow to the list of shadows.
        /// </summary>
        /// <param name="position">Position of the shadow relative to the text</param>
        /// <param name="color">Color of the shadow</param>
        public void AddShadow(Vector2 position, Color color)
        {
            if (!position.Equals(Vector2.Zero))
            {
                Shadows[position] = color;
            }
        }

        public void AddOutline(Color color)
        {
            AddShadow(new Vector2(-1, -1), color);
            AddShadow(new Vector2(1, 1), color);
            AddShadow(new Vector2(1, -1), color);
            AddShadow(new Vector2(-1, 1), color);
        }
    }
}
