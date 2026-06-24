using UnityEngine;

// Emits the scrolling environment per mode, driven by a "rightmost emitted x" watermark.
public class GroundBuilder : MonoBehaviour
{
    GameMode mode;
    float right;
    bool lastWasGap;

    public void Rebuild(GameMode m)
    {
        mode = m;
        World.Instance.Clear(EntityKind.Ground, EntityKind.Decor);
        right = -13f;
        lastWasGap = false;
        Fill();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm.State == GameState.Run && !gm.Frozen) Fill();
    }

    void Fill()
    {
        int guard = 0;
        while (right < 16f && guard++ < 300)
        {
            switch (mode)
            {
                case GameMode.SideScroll: EmitSide(); break;
                case GameMode.GravityFlip: EmitFlip(); break;
                case GameMode.Rhythm: EmitRhythm(); break;
                default: EmitTop(); break;
            }
        }
    }

    Color GroundColor => Color.Lerp(ModeInfo.Accent(mode), Palette.Dark, 0.35f);

    void EmitSide()
    {
        var gm = GameManager.Instance;
        // gaps only spawn off-screen (x > 10), never during grace, never in the tutorial
        if (!lastWasGap && !gm.GraceActive && !gm.Tutorial && right > 10f && Random.value < 0.3f)
        {
            right += Random.Range(1.8f, 2.6f) + gm.Ramp01;
            lastWasGap = true;
            return;
        }
        lastWasGap = false;
        float w = Random.Range(5f, 10f);
        Strip(right, w, -4f, 2.2f, true);
        SpawnDirector.Instance.DecorateSideScroll(right, right + w);
        right += w;
    }

    void EmitFlip()
    {
        float w = Random.Range(5f, 8f);
        Strip(right, w, -4f, 2.2f, true);   // floor
        Strip(right, w, 4f, 2.2f, false);   // ceiling
        SpawnDirector.Instance.DecorateGravity(right, right + w);
        right += w;
    }

    void EmitTop()
    {
        const float w = 2f;
        var a = ModeInfo.Accent(mode);
        Lane(right, w, 4f, a);
        Lane(right, w, -4f, a);
        // dashed centerline gives the overhead view its sense of motion
        Entity.Spawn("Dash", SpriteFactory.Pixel, new Vector3(right + 1f, 0f, 0f),
            new Vector2(0.8f, 0.09f), new Color(a.r, a.g, a.b, 0.3f),
            EntityKind.Decor, new Vector2(0.4f, 0.05f), 1);
        right += w;
    }

    void EmitRhythm()
    {
        const float w = 2f;
        var a = ModeInfo.Accent(GameMode.Rhythm);
        // soft glowing central band the notes ride along
        Entity.Spawn("Band", SpriteFactory.Pixel, new Vector3(right + w / 2f, 0f, 0f),
            new Vector2(w + 0.05f, 1.7f), new Color(a.r, a.g, a.b, 0.12f),
            EntityKind.Decor, new Vector2(w / 2f, 0.85f), 1);
        // bright rails top and bottom of the lane
        Lane(right, w, 0.9f, a);
        Lane(right, w, -0.9f, a);
        // faint beat tick scrolling through to sell the tempo
        Entity.Spawn("Tick", SpriteFactory.Pixel, new Vector3(right + 1f, 0f, 0f),
            new Vector2(0.08f, 1.5f), new Color(a.r, a.g, a.b, 0.2f),
            EntityKind.Decor, new Vector2(0.04f, 0.75f), 1);
        right += w;
    }

    void Lane(float x0, float w, float y, Color a)
    {
        Entity.Spawn("Lane", SpriteFactory.Pixel, new Vector3(x0 + w / 2f, y, 0f),
            new Vector2(w + 0.05f, 0.16f), a, EntityKind.Decor, new Vector2(w / 2f, 0.08f), 2);
    }

    void Strip(float x0, float w, float edgeY, float depth, bool edgeOnTop)
    {
        float cy = edgeOnTop ? edgeY - depth / 2f : edgeY + depth / 2f;
        var go = new GameObject("Ground");
        go.transform.position = new Vector3(x0 + w / 2f, cy, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.GroundTile;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(w, depth);
        sr.color = GroundColor;
        sr.sortingOrder = 3;
        if (!edgeOnTop) sr.flipY = true;
        var e = go.AddComponent<Entity>();
        e.kind = EntityKind.Ground;
        e.halfSize = new Vector2(w / 2f, depth / 2f);
        // bright accent line along the walkable edge
        var edge = new GameObject("Edge");
        edge.transform.SetParent(go.transform, false);
        edge.transform.localPosition = new Vector3(0f, edgeOnTop ? depth / 2f - 0.06f : -depth / 2f + 0.06f, 0f);
        edge.transform.localScale = new Vector3(w, 0.12f, 1f);
        var esr = edge.AddComponent<SpriteRenderer>();
        esr.sprite = SpriteFactory.Pixel;
        esr.color = ModeInfo.Accent(mode);
        esr.sortingOrder = 4;
    }
}
