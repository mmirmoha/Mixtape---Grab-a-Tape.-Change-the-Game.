using UnityEngine;

// The four "tracks" of the mixtape. Side-Scroll, Gravity-Flip, Top-Down, and Rhythm.
public enum GameMode { SideScroll = 0, GravityFlip = 1, TopDown = 2, Rhythm = 3 }

public static class ModeInfo
{
    public const int Count = 4;

    // Accent is resolved through Palette so the colorblind option and unlockable
    // palettes re-tint every mode consistently.
    public static Color Accent(GameMode m) => Palette.ModeAccent((int)m);

    public static string DisplayName(GameMode m)
    {
        switch (m)
        {
            case GameMode.SideScroll: return "SIDE-SCROLL";
            case GameMode.GravityFlip: return "GRAVITY-FLIP";
            case GameMode.TopDown: return "TOP-DOWN";
            default: return "RHYTHM";
        }
    }

    public static GameMode RandomOther(GameMode current)
    {
        GameMode next;
        do { next = (GameMode)Random.Range(0, Count); } while (next == current);
        return next;
    }
}
