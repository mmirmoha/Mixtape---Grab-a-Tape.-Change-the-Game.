using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// The signature "fast-forward" tape swap: flash, world rebuild, retint, big mode-name
// flash, and the post-swap grace window. Softens itself when "reduced flashing" is on.
public class SwapTransition : MonoBehaviour
{
    public void Begin(GameMode next) => StartCoroutine(Run(next));

    IEnumerator Run(GameMode next)
    {
        var gm = GameManager.Instance;
        gm.Frozen = true;
        gm.Audio.Play(AudioSynth.Swap);
        gm.Audio.Duck(0.6f);

        Image flash = gm.HUD.Flash;
        float peak = Settings.ReducedFlashing ? 0.35f : 1f;

        for (float tm = 0f; tm < 0.15f; tm += Time.deltaTime)
        {
            flash.color = new Color(1f, 1f, 1f, (tm / 0.15f) * peak);
            yield return null;
        }
        flash.color = new Color(1f, 1f, 1f, peak);

        // grace covers the fade-out plus ~2 s of play so the swap is never unfair
        gm.GraceUntil = Time.time + 2.6f;
        World.Instance.Clear(EntityKind.Hazard, EntityKind.Coin, EntityKind.Cassette, EntityKind.Note);
        gm.SetMode(next);
        gm.Ground.Rebuild(next);
        gm.Player.ResetFor(next);
        SpawnDirector.Instance.OnSwapped();
        yield return null;

        var accent = ModeInfo.Accent(next);
        StartCoroutine(FlashName(gm, next, accent)); // big mode-name pop, overlaps the fade

        for (float tm = 0f; tm < 0.4f; tm += Time.deltaTime)
        {
            float a = 1f - tm / 0.4f;
            var c = Color.Lerp(accent, Color.white, a * 0.7f);
            flash.color = new Color(c.r, c.g, c.b, a * peak);
            yield return null;
        }
        flash.color = Color.clear;
        gm.Frozen = false;
    }

    // Large centered mode name that pops and fades on every swap (GDD 5.4).
    IEnumerator FlashName(GameManager gm, GameMode next, Color accent)
    {
        var txt = gm.HUD.SwapFlashText;
        if (txt == null) yield break;
        txt.text = ModeInfo.DisplayName(next);
        const float dur = 0.9f;
        for (float tm = 0f; tm < dur; tm += Time.deltaTime)
        {
            float k = tm / dur;
            var c = accent;
            c.a = 1f - k;
            txt.color = c;
            float scale = 1f + 0.25f * (1f - k);
            txt.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        txt.text = "";
        txt.transform.localScale = Vector3.one;
    }
}
