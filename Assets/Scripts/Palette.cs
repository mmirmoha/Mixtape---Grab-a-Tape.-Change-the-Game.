using UnityEngine;

// Single source of truth for the neon-on-navy palette from the GDD.
// Mode accents are resolved through ModeAccent() so the colorblind toggle and the
// unlockable "Sunset" palette can re-tint all four modes at once.
public static class Palette
{
    public static readonly Color Navy      = Hex("0B0E2A");
    public static readonly Color Dark      = Hex("060818");
    public static readonly Color Magenta   = Hex("FF2E88");
    public static readonly Color Cyan      = Hex("2EE6FF");
    public static readonly Color Yellow    = Hex("FFD93D");
    public static readonly Color Green      = Hex("3DFF8C");
    public static readonly Color Purple    = Hex("8C5BFF");
    public static readonly Color DeckBody  = Hex("1A2046");
    public static readonly Color DeckLabel = Hex("262E63");
    public static readonly Color TextDim   = new Color(1f, 1f, 1f, 0.45f);

    // Accent tables indexed by (int)GameMode: SideScroll, GravityFlip, TopDown, Rhythm.
    static readonly Color[] DefaultAccents = { Cyan, Yellow, Green, Magenta };

    // Okabe-Ito-derived set chosen to stay distinct under common color-vision deficiencies.
    static readonly Color[] ColorblindAccents =
    {
        Hex("56B4E9"), // sky blue
        Hex("E69F00"), // orange
        Hex("009E73"), // bluish green
        Hex("CC79A7"), // reddish purple
    };

    // Cosmetic "Sunset" palette unlocked in the shop.
    static readonly Color[] SunsetAccents =
    {
        Hex("FF8C42"), // warm orange
        Hex("FFD23F"), // gold
        Hex("FF5C8A"), // coral pink
        Hex("A06CD5"), // violet
    };

    public static Color ModeAccent(int modeIndex)
    {
        modeIndex = Mathf.Clamp(modeIndex, 0, 3);
        if (Settings.Colorblind) return ColorblindAccents[modeIndex];
        if (Economy.SelectedPalette == 1) return SunsetAccents[modeIndex];
        return DefaultAccents[modeIndex];
    }

    static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString("#" + h, out var c);
        return c;
    }
}
