using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;

namespace KirosDungeons.Source.Game
{
    public class Settings
    {
        public static readonly string
            MoveLeftKey = "MoveLeft",
            MoveRightKey = "MoveRight",
            JumpKey = "Jump",
            PauseKey = "Pause",
            InteractKey = "Interact",
            FallKey = "Fall";

        public string Language { get; set; } = "fr_fr";
        public bool Fullscreen { get; set; } = false;
        public bool Borderless { get; set; } = false;
        public int Width { get; set; } = 640;
        public int Height { get; set; } = 360;
        public Dictionary<string, string> Controls { get; set; } = new Dictionary<string, string>()
        {
            { MoveLeftKey, "Q" },
            { MoveRightKey, "D" },
            { FallKey, "S" },
            { JumpKey, "Space" },
            { InteractKey, "E" },
            { PauseKey, "Escape" }
        };

        public Keys? GetKey(string name)
        {
            if (Controls.ContainsKey(name))
                return (Keys)Enum.Parse(typeof(Keys), Controls[name]);
            else
                return null;
        }

        public bool IsKeyDown(KeyboardStateExtended state, string controlName)
        {
            Keys? key = GetKey(controlName);
            if (key == null)
                return false;
            else
                return state.IsKeyDown((Keys)key);
        }

        public bool IsKeyUp(KeyboardStateExtended state, string controlName)
        {
            Keys? key = GetKey(controlName);
            if (key == null)
                return false;
            else
                return state.IsKeyUp((Keys)key);
        }

        public bool WasKeyJustDown(KeyboardStateExtended state, string controlName)
        {
            Keys? key = GetKey(controlName);
            if (key == null)
                return false;
            else
                return state.WasKeyJustDown((Keys)key);
        }

        public bool WasKeyJustUp(KeyboardStateExtended state, string controlName)
        {
            Keys? key = GetKey(controlName);
            if (key == null)
                return false;
            else
                return state.WasKeyJustUp((Keys)key);
        }
    }
}
