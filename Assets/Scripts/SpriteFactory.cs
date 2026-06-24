using System;
using System.Collections.Generic;
using UnityEngine;

// All placeholder pixel art is generated here at runtime — no image assets needed.
public static class SpriteFactory
{
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    // 1x1 white pixel at 1 PPU: scales to exactly (scale.x, scale.y) world units.
    public static Sprite Pixel => Get("pixel", 1, 1, 1f, px => px[0] = Color.white);

    public static Sprite Bit => BitSkinned(Palette.Cyan);
    public static Sprite Coin => Get("coin", 16, 16, 16f, DrawCoin);
    public static Sprite Cassette => Get("cassette", 16, 16, 16f, DrawCassette);
    public static Sprite Spike => Get("spike", 16, 16, 16f, DrawSpike);
    public static Sprite Block => Get("block", 16, 16, 16f, DrawBlock);
    public static Sprite GroundTile => Get("ground", 16, 16, 16f, DrawGroundTile);
    public static Sprite Reel => Get("reel", 16, 16, 16f, DrawReel);
    public static Sprite Note => Get("note", 16, 16, 16f, DrawNote);

    // Bit recolored for the cosmetic skins (cached per body color).
    public static Sprite BitSkinned(Color body) =>
        Get("bit_" + ColorUtility.ToHtmlStringRGB(body), 16, 16, 16f, px => DrawBit(px, body));

    static Sprite Get(string key, int w, int h, float ppu, Action<Color[]> draw)
    {
        if (cache.TryGetValue(key, out var s) && s != null) return s;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        draw(px);
        tex.SetPixels(px);
        tex.Apply();
        s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
        cache[key] = s;
        return s;
    }

    static void Set(Color[] px, int x, int y, Color c)
    {
        if (x >= 0 && x < 16 && y >= 0 && y < 16) px[y * 16 + x] = c;
    }

    static void Fill(Color[] px, int x0, int y0, int x1, int y1, Color c)
    {
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                Set(px, x, y, c);
    }

    static void DrawBit(Color[] px, Color body)
    {
        var shade = Color.Lerp(body, Palette.Navy, 0.45f);
        var face = Palette.Navy;
        // antenna note-head uses a contrasting accent so it reads on every skin
        var note = (body.r > 0.7f && body.b > 0.4f) ? Palette.Cyan : Palette.Magenta;

        Fill(px, 2, 1, 13, 12, body);
        // rounded corners
        Set(px, 2, 1, Color.clear); Set(px, 13, 1, Color.clear);
        Set(px, 2, 12, Color.clear); Set(px, 13, 12, Color.clear);
        // bottom + left shading
        Fill(px, 3, 1, 12, 1, shade);
        Fill(px, 2, 2, 2, 11, shade);
        // eyes (on the right half — Bit faces the direction of travel)
        Fill(px, 8, 6, 9, 8, Color.white);
        Fill(px, 11, 6, 12, 8, Color.white);
        Set(px, 9, 7, face);
        Set(px, 12, 7, face);
        // smile
        Fill(px, 9, 3, 11, 3, face);
        Set(px, 8, 4, face);
        Set(px, 12, 4, face);
        // antenna with a musical-note head
        Set(px, 7, 13, shade);
        Set(px, 7, 14, shade);
        Fill(px, 8, 14, 9, 15, note);
    }

    static void DrawNote(Color[] px)
    {
        // neon diamond; tinted to the rhythm accent at spawn time
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float d = Mathf.Abs(x - 7.5f) + Mathf.Abs(y - 7.5f);
                if (d <= 7f) Set(px, x, y, d >= 5.5f ? new Color(0.8f, 0.8f, 0.85f) : Color.white);
            }
    }

    static void DrawCoin(Color[] px)
    {
        var fill = Palette.Yellow;
        var rim = Color.Lerp(Palette.Yellow, Palette.Dark, 0.4f);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float dx = x - 7.5f, dy = y - 7.5f;
                float d2 = dx * dx + dy * dy;
                if (d2 <= 42f) Set(px, x, y, d2 >= 30f ? rim : fill);
            }
        Fill(px, 7, 4, 8, 11, rim);                 // coin slot
        Set(px, 4, 9, Color.white);                 // highlight
        Set(px, 5, 10, Color.white);
    }

    static void DrawCassette(Color[] px)
    {
        var body = new Color(0.16f, 0.18f, 0.36f);
        var edge = new Color(0.5f, 0.55f, 0.9f);
        var dark = new Color(0.07f, 0.08f, 0.18f);

        Fill(px, 1, 3, 14, 12, body);
        Fill(px, 1, 12, 14, 12, edge);
        Fill(px, 1, 3, 14, 3, dark);
        // label with neon stripes
        Fill(px, 3, 8, 12, 11, new Color(0.93f, 0.93f, 0.9f));
        Fill(px, 3, 10, 12, 10, Palette.Magenta);
        Fill(px, 3, 9, 12, 9, Palette.Cyan);
        // reel holes + tape window
        Fill(px, 4, 5, 5, 6, Color.white);
        Fill(px, 10, 5, 11, 6, Color.white);
        Fill(px, 7, 5, 8, 6, dark);
    }

    static void DrawSpike(Color[] px)
    {
        // grayscale triangle; tinted at spawn time
        for (int y = 0; y <= 12; y++)
        {
            int hw = Mathf.RoundToInt(6f * (12 - y) / 12f);
            for (int x = 7 - hw; x <= 8 + hw; x++)
            {
                bool edgePx = x == 7 - hw || x == 8 + hw;
                Set(px, x, y, edgePx ? new Color(0.65f, 0.65f, 0.65f) : new Color(0.95f, 0.95f, 0.95f));
            }
        }
    }

    static void DrawBlock(Color[] px)
    {
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float v = 0.8f;
                if (x == 0 || y == 15) v = 1f;
                else if (x == 15 || y == 0) v = 0.45f;
                else if ((x * 3 + y * 7) % 11 == 0) v = 0.65f;
                Set(px, x, y, new Color(v, v, v));
            }
    }

    static void DrawGroundTile(Color[] px)
    {
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float v;
                if (y >= 15) v = 1f;
                else if (y >= 13) v = 0.75f;
                else v = (x * 7 + y * 5) % 13 == 0 ? 0.32f : 0.5f;
                Set(px, x, y, new Color(v, v, v));
            }
    }

    static void DrawReel(Color[] px)
    {
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float dx = x - 7.5f, dy = y - 7.5f;
                float d2 = dx * dx + dy * dy;
                if (d2 <= 49f && d2 >= 30f) Set(px, x, y, new Color(0.85f, 0.85f, 0.9f));
                else if (d2 < 30f && d2 > 6f && (Mathf.Abs(dx) < 1.2f || Mathf.Abs(dy) < 1.2f))
                    Set(px, x, y, new Color(0.55f, 0.55f, 0.65f));
            }
    }
}
