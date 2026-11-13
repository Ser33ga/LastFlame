using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Single source of truth for the Last Flame color palette and UI/world tint helpers.
    /// </summary>
    public static class Palette
    {
        public static readonly Color VoidNight = Hex(0x0A, 0x0D, 0x16);
        public static readonly Color RuinsStone = Hex(0x1C, 0x22, 0x33);
        public static readonly Color ObsidianSilhouette = Hex(0x12, 0x16, 0x22);

        public static readonly Color CandleWax = Hex(0xED, 0xE3, 0xCE);
        public static readonly Color CandleWarm = Hex(0xD9, 0x9B, 0x52);

        public static readonly Color GaleShadow = Hex(0x2C, 0x3A, 0x57);
        public static readonly Color GaleWisp = Hex(0x6E, 0x8C, 0xA8);

        public static readonly Color FlameAccent = Hex(0xFF, 0x8C, 0x2B);
        public static readonly Color FlameCore = Hex(0xFF, 0xD2, 0x69);

        public static readonly Color DangerCrimson = Hex(0xB8, 0x3B, 0x3B);
        public static readonly Color PanelBackdrop = new Color(0.039f, 0.051f, 0.086f, 0.86f);
        public static readonly Color TextMuted = new Color(0.929f, 0.890f, 0.808f, 0.68f);

        public static Color GetFlameColor(float normalizedFlame)
        {
            float clamped = Mathf.Clamp01(normalizedFlame);
            return clamped < 0.35f
                ? Color.Lerp(DangerCrimson, FlameAccent, clamped / 0.35f)
                : Color.Lerp(FlameAccent, FlameCore, (clamped - 0.35f) / 0.65f);
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
        }

        private static Color Hex(byte r, byte g, byte b, byte a = 255)
        {
            return new Color32(r, g, b, a);
        }
    }
}
