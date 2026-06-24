using UnityEngine;

// Pattern-based hazard/coin/cassette spawning. Hazards never spawn during grace
// windows and only off-screen (x > 10) so nothing pops into view.
public class SpawnDirector : MonoBehaviour
{
    public static SpawnDirector Instance { get; private set; }

    float nextCassetteTime;
    float distSinceTopPattern;

    void Awake() => Instance = this;

    public void ResetRun()
    {
        nextCassetteTime = Time.time + 9f;
        distSinceTopPattern = 0f;
    }

    public void OnSwapped()
    {
        nextCassetteTime = Time.time + CassetteInterval();
        distSinceTopPattern = 0f;
    }

    float CassetteInterval() => Mathf.Lerp(13f, 8f, GameManager.Instance.Ramp01);

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm.State != GameState.Run || gm.Frozen) return;
        if (gm.Tutorial) return; // the tutorial scripts its own pickups

        if (Time.time >= nextCassetteTime && !gm.GraceActive)
        {
            SpawnCassette(gm);
            nextCassetteTime = Time.time + CassetteInterval();
        }

        if (gm.Mode == GameMode.TopDown)
        {
            distSinceTopPattern += gm.Speed * Time.deltaTime;
            float spacing = Mathf.Lerp(10f, 6.5f, gm.Ramp01);
            if (distSinceTopPattern >= spacing && !gm.GraceActive)
            {
                distSinceTopPattern = 0f;
                SpawnTopDownPattern(gm);
            }
        }
    }

    void SpawnCassette(GameManager gm)
    {
        float y;
        switch (gm.Mode)
        {
            case GameMode.SideScroll: y = -2.6f; break;
            case GameMode.GravityFlip: y = Random.value < 0.5f ? -3.3f : 3.3f; break;
            case GameMode.Rhythm: y = 0f; break; // rides the center lane so you can swap out
            default: y = Random.Range(-3f, 3f); break;
        }
        var e = Entity.Spawn("Cassette", SpriteFactory.Cassette, new Vector3(13.5f, y, 0f),
            Vector2.one * 1.3f, Color.white, EntityKind.Cassette, new Vector2(0.6f, 0.5f), 8);
        e.swapTo = ModeInfo.RandomOther(gm.Mode);
        e.gameObject.AddComponent<Pulse>();
    }

    public void DecorateSideScroll(float x0, float x1)
    {
        var gm = GameManager.Instance;
        if (gm.Tutorial) return; // no random hazards/coins while teaching
        if (x0 < 10f || x1 - x0 < 4f) return;
        float r = Random.value;
        if (gm.GraceActive || r < 0.22f)
        {
            if (Random.value < 0.5f) CoinRow(Random.Range(x0 + 1.5f, x1 - 3f), -3.1f, 3);
            return;
        }
        if (r < 0.55f)
        {
            int n = 1 + Mathf.RoundToInt(gm.Ramp01 * 2f + Random.value);
            float cx = Random.Range(x0 + 1.5f, Mathf.Max(x0 + 1.6f, x1 - 1.5f - n * 0.75f));
            for (int i = 0; i < n; i++)
                Entity.Spawn("Spike", SpriteFactory.Spike, new Vector3(cx + i * 0.75f, -3.62f, 0f),
                    Vector2.one * 0.8f, Palette.Magenta, EntityKind.Hazard, new Vector2(0.26f, 0.3f), 6);
            CoinArc(cx + (n - 1) * 0.375f, -2.1f, 3);
        }
        else if (r < 0.75f)
        {
            float cx = Random.Range(x0 + 1.5f, x1 - 1.5f);
            Entity.Spawn("Block", SpriteFactory.Block, new Vector3(cx, -3.5f, 0f),
                Vector2.one, Palette.Purple, EntityKind.Hazard, new Vector2(0.45f, 0.45f), 6);
            CoinArc(cx, -1.9f, 3);
        }
        else
        {
            CoinRow(Random.Range(x0 + 1.5f, x1 - 3f), -3.1f, 4);
        }
    }

    public void DecorateGravity(float x0, float x1)
    {
        var gm = GameManager.Instance;
        if (x0 < 10f || x1 - x0 < 4f || gm.GraceActive) return;
        if (Random.value < 0.65f)
        {
            bool ceiling = Random.value < 0.5f;
            int n = 1 + Mathf.RoundToInt(gm.Ramp01 * 2f + Random.value);
            float cx = Random.Range(x0 + 1.5f, Mathf.Max(x0 + 1.6f, x1 - 1.5f - n * 0.75f));
            float y = ceiling ? 3.62f : -3.62f;
            for (int i = 0; i < n; i++)
                Entity.Spawn("Spike", SpriteFactory.Spike, new Vector3(cx + i * 0.75f, y, 0f),
                    new Vector2(0.8f, ceiling ? -0.8f : 0.8f), Palette.Magenta,
                    EntityKind.Hazard, new Vector2(0.26f, 0.3f), 6);
            // coins on the safe surface invite the flip
            CoinRow(cx, ceiling ? -3.3f : 3.3f, n + 1);
        }
        else
        {
            CoinRow(Random.Range(x0 + 1f, x1 - 3f), Random.value < 0.5f ? -3.3f : 3.3f, 3);
        }
    }

    void SpawnTopDownPattern(GameManager gm)
    {
        const float x = 13.5f;
        if (Random.value < 0.6f)
        {
            // wall spanning the lane with one opening
            float gapC = Random.Range(-2.2f, 2.2f);
            float gapHalf = Mathf.Lerp(1.6f, 1.1f, gm.Ramp01);
            for (float y = -3.5f; y <= 3.5f; y += 1f)
                if (Mathf.Abs(y - gapC) > gapHalf)
                    Entity.Spawn("Wall", SpriteFactory.Block, new Vector3(x, y, 0f),
                        Vector2.one, Palette.Magenta, EntityKind.Hazard, new Vector2(0.45f, 0.45f), 6);
            CoinAt(x, gapC);
        }
        else
        {
            // staggered slalom blocks
            float y1 = Random.Range(-2.5f, 2.5f);
            Entity.Spawn("Block", SpriteFactory.Block, new Vector3(x, y1, 0f),
                Vector2.one, Palette.Magenta, EntityKind.Hazard, new Vector2(0.45f, 0.45f), 6);
            Entity.Spawn("Block", SpriteFactory.Block, new Vector3(x + 3f, -y1, 0f),
                Vector2.one, Palette.Magenta, EntityKind.Hazard, new Vector2(0.45f, 0.45f), 6);
            CoinAt(x + 1.5f, 0f);
        }
    }

    void CoinAt(float x, float y)
    {
        Entity.Spawn("Coin", SpriteFactory.Coin, new Vector3(x, y, 0f),
            Vector2.one * 0.6f, Color.white, EntityKind.Coin, new Vector2(0.22f, 0.22f), 7);
    }

    void CoinRow(float x, float y, int n)
    {
        for (int i = 0; i < n; i++) CoinAt(x + i * 0.7f, y);
    }

    void CoinArc(float cx, float peakY, int n)
    {
        for (int i = 0; i < n; i++)
        {
            float t = n == 1 ? 0f : i / (float)(n - 1) * 2f - 1f;
            CoinAt(cx + t * 0.8f, peakY - t * t * 0.5f);
        }
    }
}
