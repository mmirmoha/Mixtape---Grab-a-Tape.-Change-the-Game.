using System.Collections.Generic;
using UnityEngine;

// Light, cosmetic-only economy (GDD 2.9). Coins earned during runs are banked across
// sessions and spent on a small catalog of skins, an alternate palette, a second music
// track, and the Hard-mode unlock. No pay-to-win, all local (PlayerPrefs).
public static class Economy
{
    public enum ItemKind { Skin, Palette, Track, Hard }

    public class Item
    {
        public string id;
        public string name;
        public int price;
        public ItemKind kind;
        public int value;     // skin index / palette index
        public Color swatch;
    }

    // Bit body colors, indexed by skin value.
    public static readonly Color[] SkinColor = { Palette.Cyan, Palette.Magenta, Palette.Yellow, Palette.Green };

    public static readonly List<Item> Catalog = new List<Item>
    {
        new Item { id = "skin0", name = "CYAN BIT",       price = 0,   kind = ItemKind.Skin,    value = 0, swatch = Palette.Cyan },
        new Item { id = "skin1", name = "MAGENTA BIT",    price = 50,  kind = ItemKind.Skin,    value = 1, swatch = Palette.Magenta },
        new Item { id = "skin2", name = "YELLOW BIT",     price = 100, kind = ItemKind.Skin,    value = 2, swatch = Palette.Yellow },
        new Item { id = "skin3", name = "GREEN BIT",      price = 150, kind = ItemKind.Skin,    value = 3, swatch = Palette.Green },
        new Item { id = "pal1",  name = "SUNSET PALETTE", price = 120, kind = ItemKind.Palette, value = 1, swatch = Palette.Purple },
        new Item { id = "trk1",  name = "B-SIDE TRACK",   price = 200, kind = ItemKind.Track,   value = 1, swatch = Palette.Cyan },
        new Item { id = "hard",  name = "HARD MODE",      price = 300, kind = ItemKind.Hard,    value = 0, swatch = Palette.Magenta },
    };

    public static int  TotalCoins      { get; private set; }
    public static int  SelectedSkin    { get; private set; }
    public static int  SelectedPalette { get; private set; }   // 0 = default, 1 = sunset
    public static bool Track2Selected  { get; private set; }

    static bool loaded;

    public static void Load()
    {
        if (loaded) return;
        loaded = true;
        TotalCoins      = PlayerPrefs.GetInt("mixtape.coins", 0);
        SelectedSkin    = PlayerPrefs.GetInt("mixtape.skin", 0);
        SelectedPalette = PlayerPrefs.GetInt("mixtape.palette", 0);
        Track2Selected  = PlayerPrefs.GetInt("mixtape.track2", 0) == 1;
    }

    public static bool IsUnlocked(Item it) =>
        it.price == 0 || PlayerPrefs.GetInt("mixtape.unlock." + it.id, 0) == 1;

    public static bool HardUnlocked => PlayerPrefs.GetInt("mixtape.unlock.hard", 0) == 1;

    // Whether this item is the currently-selected one in its group.
    public static bool IsSelected(Item it)
    {
        switch (it.kind)
        {
            case ItemKind.Skin:    return SelectedSkin == it.value;
            case ItemKind.Palette: return SelectedPalette == it.value;
            case ItemKind.Track:   return Track2Selected == (it.value == 1);
            default:               return false;
        }
    }

    public static void AddCoins(int n)
    {
        if (n <= 0) return;
        TotalCoins += n;
        PlayerPrefs.SetInt("mixtape.coins", TotalCoins);
        PlayerPrefs.Save();
    }

    public static bool Buy(Item it)
    {
        if (IsUnlocked(it) || TotalCoins < it.price) return false;
        TotalCoins -= it.price;
        PlayerPrefs.SetInt("mixtape.coins", TotalCoins);
        PlayerPrefs.SetInt("mixtape.unlock." + it.id, 1);
        PlayerPrefs.Save();
        return true;
    }

    public static void Select(Item it)
    {
        if (!IsUnlocked(it)) return;
        switch (it.kind)
        {
            case ItemKind.Skin:
                SelectedSkin = it.value;
                PlayerPrefs.SetInt("mixtape.skin", it.value);
                break;
            case ItemKind.Palette:
                SelectedPalette = it.value;
                PlayerPrefs.SetInt("mixtape.palette", it.value);
                break;
            case ItemKind.Track:
                Track2Selected = !Track2Selected;
                PlayerPrefs.SetInt("mixtape.track2", Track2Selected ? 1 : 0);
                break;
            case ItemKind.Hard:
                break; // Hard is toggled in Options once unlocked.
        }
        PlayerPrefs.Save();
    }

    public static Color CurrentSkinColor =>
        SkinColor[Mathf.Clamp(SelectedSkin, 0, SkinColor.Length - 1)];
}
