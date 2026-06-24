using System.Collections;
using UnityEngine;

// First-run tutorial (GDD 4.2). A slow, non-lethal guided sequence: teach the one control
// (jump for a coin) and one calm swap (grab a telegraphed cassette), then hand back to the
// real run. Kept non-lethal so a first-timer can never fail out of the lesson.
public class TutorialController : MonoBehaviour
{
    bool running;
    Coroutine routine;

    public void Begin()
    {
        Stop();
        running = true;
        routine = StartCoroutine(Run());
    }

    public void Stop()
    {
        running = false;
        if (routine != null) StopCoroutine(routine);
        routine = null;
        var gm = GameManager.Instance;
        if (gm != null && gm.HUD != null) gm.HUD.SetTutorialPrompt("");
    }

    IEnumerator Run()
    {
        var gm = GameManager.Instance;
        var hud = gm.HUD;

        hud.SetTutorialPrompt("WELCOME TO MIXTAPE!");
        yield return Wait(gm, 1.6f);

        // 1) teach the jump with a coin arc at jump height
        hud.SetTutorialPrompt("TAP  " + PrettyKey() + "  TO JUMP!");
        SpawnCoinArc(11f, -1.6f);
        yield return Wait(gm, 3.4f);

        // 2) teach the swap with a slow, telegraphed cassette (respawn if it's missed)
        hud.SetTutorialPrompt("GRAB THE TAPE TO SWITCH TRACKS!");
        SpawnTutorialCassette(12f);
        float since = 0f;
        while (running && gm.Mode == GameMode.SideScroll)
        {
            since += Time.deltaTime;
            if (since > 5f && !AnyCassette())
            {
                SpawnTutorialCassette(12f);
                since = 0f;
            }
            yield return null;
        }
        if (!running) yield break;

        // the swap fired — hand back to the normal generator (grace covers re-orientation)
        gm.EndTutorial();
        hud.SetTutorialPrompt("NICE! KEEP GOING →");
        yield return new WaitForSeconds(1.6f);
        hud.SetTutorialPrompt("");
        running = false;
    }

    // Advances only while unfrozen so prompts don't tick during the swap transition.
    IEnumerator Wait(GameManager gm, float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (!gm.Frozen) t += Time.deltaTime;
            yield return null;
        }
    }

    void SpawnCoinArc(float cx, float peakY)
    {
        for (int i = 0; i < 3; i++)
        {
            float t = i - 1; // -1, 0, 1
            Entity.Spawn("Coin", SpriteFactory.Coin,
                new Vector3(cx + t * 0.8f, peakY - t * t * 0.4f, 0f),
                Vector2.one * 0.6f, Color.white, EntityKind.Coin, new Vector2(0.22f, 0.22f), 7);
        }
    }

    void SpawnTutorialCassette(float x)
    {
        var e = Entity.Spawn("Cassette", SpriteFactory.Cassette, new Vector3(x, -2.6f, 0f),
            Vector2.one * 1.3f, Color.white, EntityKind.Cassette, new Vector2(0.6f, 0.5f), 8);
        e.swapTo = GameMode.GravityFlip; // a calm, readable first swap
        e.gameObject.AddComponent<Pulse>();
    }

    static bool AnyCassette()
    {
        var all = Entity.All;
        for (int i = 0; i < all.Count; i++)
            if (all[i].kind == EntityKind.Cassette) return true;
        return false;
    }

    static string PrettyKey()
    {
        var k = Settings.JumpKey;
        return k == KeyCode.Space ? "SPACE" : k.ToString().ToUpper();
    }
}
