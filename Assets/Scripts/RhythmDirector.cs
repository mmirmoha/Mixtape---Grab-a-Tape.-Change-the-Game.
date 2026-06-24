using UnityEngine;

// Drives Rhythm-Tap mode (GDD 2.5): beat-synced notes scroll toward a fixed hit line at
// Bit's X. Tapping in the window scores big and refills the bar; a note that slips past
// unhit drains it. When the bar empties the run ends (record-scratch) — so rhythm is the
// one mode that is NOT one-hit, matching "missing the beat drains a combo bar."
public class RhythmDirector : MonoBehaviour
{
    const float HitX = -5f;        // matches PlayerController.PlayerX
    const float SpawnX = 13.5f;
    const float HitWindow = 0.95f; // distance from the hit line a tap will still count
    const float MissX = -6.0f;     // a note past this (unhit) is a miss

    public float Health { get; private set; } = 1f;

    float spawnTimer;
    GameObject hitMarker;

    public void ResetRhythm()
    {
        Health = 1f;
        spawnTimer = 0f;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        bool inRhythm = gm.Mode == GameMode.Rhythm && gm.State == GameState.Run;
        EnsureMarker(inRhythm);
        if (!inRhythm || gm.Frozen) return;

        // steady note cadence locked to the 120 BPM beat grid; stay quiet (and don't bank
        // up a burst) during the post-swap grace window
        if (gm.GraceActive)
        {
            spawnTimer = 0f;
        }
        else
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= AudioSynth.BeatLength)
            {
                spawnTimer -= AudioSynth.BeatLength;
                SpawnNote();
            }
        }

        // notes that scroll past the hit line unhit are misses
        var all = Entity.All;
        for (int i = all.Count - 1; i >= 0; i--)
        {
            var e = all[i];
            if (e.kind != EntityKind.Note || e.consumed) continue;
            if (e.transform.position.x < MissX)
            {
                e.consumed = true;
                Miss(gm);
            }
        }
    }

    void SpawnNote()
    {
        var a = ModeInfo.Accent(GameMode.Rhythm);
        var e = Entity.Spawn("Note", SpriteFactory.Note, new Vector3(SpawnX, 0f, 0f),
            Vector2.one * 0.7f, a, EntityKind.Note, new Vector2(0.3f, 0.3f), 9);
        e.gameObject.AddComponent<Pulse>();
    }

    // Called by the player's tap in rhythm mode.
    public void TryHit(GameManager gm)
    {
        Entity best = null;
        float bestDist = HitWindow;
        var all = Entity.All;
        for (int i = 0; i < all.Count; i++)
        {
            var e = all[i];
            if (e.kind != EntityKind.Note || e.consumed) continue;
            float d = Mathf.Abs(e.transform.position.x - HitX);
            if (d <= bestDist) { bestDist = d; best = e; }
        }
        if (best == null) return; // forgiving — empty taps cost nothing

        best.consumed = true;
        best.Kill();
        int pts = bestDist < 0.35f ? 20 : 12; // tighter taps score more
        gm.Score.AddPoints(pts, 1);
        gm.Audio.Play(AudioSynth.NoteHit, 0.6f);
        Health = Mathf.Min(1f, Health + 0.12f);
    }

    void Miss(GameManager gm)
    {
        gm.Score.ResetCombo();
        gm.Audio.Play(AudioSynth.NoteMiss, 0.6f);
        Health -= 0.2f;
        if (Health <= 0f)
        {
            Health = 0f;
            gm.KillPlayer();
        }
    }

    // The hit line is a fixed (non-scrolling) marker, present only while in rhythm mode.
    void EnsureMarker(bool inRhythm)
    {
        if (inRhythm && hitMarker == null)
        {
            hitMarker = new GameObject("HitLine");
            var sr = hitMarker.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Pixel;
            var a = ModeInfo.Accent(GameMode.Rhythm);
            sr.color = new Color(a.r, a.g, a.b, 0.9f);
            sr.sortingOrder = 8;
            hitMarker.transform.position = new Vector3(HitX, 0f, 0f);
            hitMarker.transform.localScale = new Vector3(0.12f, 3.4f, 1f);
        }
        else if (!inRhythm && hitMarker != null)
        {
            Destroy(hitMarker);
            hitMarker = null;
        }
    }
}
