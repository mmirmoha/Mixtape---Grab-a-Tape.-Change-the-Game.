using UnityEngine;

// Player-facing options (GDD 2.10), persisted in PlayerPrefs.
// Audio volumes, accessibility toggles, difficulty selection, and a remappable key.
public static class Settings
{
    public static float MasterVol = 1f;
    public static float MusicVol  = 0.6f;
    public static float SfxVol    = 1f;
    public static bool  ReducedFlashing;
    public static bool  Colorblind;
    public static bool  Hard;            // selected difficulty (gated by Economy unlock)
    public static KeyCode JumpKey = KeyCode.Space;

    static bool loaded;

    public static void Load()
    {
        if (loaded) return;
        loaded = true;
        MasterVol       = PlayerPrefs.GetFloat("mixtape.vol.master", 1f);
        MusicVol        = PlayerPrefs.GetFloat("mixtape.vol.music", 0.6f);
        SfxVol          = PlayerPrefs.GetFloat("mixtape.vol.sfx", 1f);
        ReducedFlashing = PlayerPrefs.GetInt("mixtape.reducedFlash", 0) == 1;
        Colorblind      = PlayerPrefs.GetInt("mixtape.colorblind", 0) == 1;
        Hard            = PlayerPrefs.GetInt("mixtape.hard", 0) == 1;
        JumpKey         = (KeyCode)PlayerPrefs.GetInt("mixtape.jumpKey", (int)KeyCode.Space);
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat("mixtape.vol.master", MasterVol);
        PlayerPrefs.SetFloat("mixtape.vol.music", MusicVol);
        PlayerPrefs.SetFloat("mixtape.vol.sfx", SfxVol);
        PlayerPrefs.SetInt("mixtape.reducedFlash", ReducedFlashing ? 1 : 0);
        PlayerPrefs.SetInt("mixtape.colorblind", Colorblind ? 1 : 0);
        PlayerPrefs.SetInt("mixtape.hard", Hard ? 1 : 0);
        PlayerPrefs.SetInt("mixtape.jumpKey", (int)JumpKey);
        PlayerPrefs.Save();
    }
}
