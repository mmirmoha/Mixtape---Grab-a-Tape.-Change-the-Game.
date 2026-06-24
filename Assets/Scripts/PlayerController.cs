using System.Collections;
using UnityEngine;

// Bit. Transform-based movement; the active control scheme depends on the current mode.
public class PlayerController : MonoBehaviour
{
    const float Gravity = 45f;
    const float JumpV = 14f;
    const float FloorTop = -4f;
    const float CeilBottom = 4f;
    const float Strafe = 7f;
    const float PlayerX = -5f;

    static readonly Vector2 half = new Vector2(0.38f, 0.42f);

    SpriteRenderer sr;
    float velY;
    int gravitySign = 1;
    bool grounded;

    // The primary action is the remappable key (GDD 5.2) plus mouse/touch tap.
    static bool ActionDown => Input.GetKeyDown(Settings.JumpKey) || Input.GetMouseButtonDown(0);
    static bool ActionUp => Input.GetKeyUp(Settings.JumpKey) || Input.GetMouseButtonUp(0);

    void Awake()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.BitSkinned(Economy.CurrentSkinColor);
        sr.sortingOrder = 10;
        transform.localScale = new Vector3(0.9f, 0.9f, 1f);
    }

    public void ResetFor(GameMode m)
    {
        StopAllCoroutines();
        sr.enabled = true;
        sr.flipY = false;
        sr.color = Color.white;
        sr.sprite = SpriteFactory.BitSkinned(Economy.CurrentSkinColor); // apply chosen skin
        velY = 0f;
        gravitySign = 1;
        grounded = true;
        bool centered = m == GameMode.TopDown || m == GameMode.Rhythm;
        transform.position = centered
            ? new Vector3(PlayerX, 0f, 0f)
            : new Vector3(PlayerX, FloorTop + half.y, 0f);
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm.State != GameState.Run || gm.Frozen) return;
        switch (gm.Mode)
        {
            case GameMode.SideScroll: TickSide(gm); break;
            case GameMode.GravityFlip: TickFlip(gm); break;
            case GameMode.TopDown: TickTop(gm); break;
            default: TickRhythm(gm); break;
        }
        if (gm.State == GameState.Run && !gm.Frozen) Collide(gm);
    }

    void TickSide(GameManager gm)
    {
        float dt = Time.deltaTime;
        if (grounded && ActionDown)
        {
            velY = JumpV;
            grounded = false;
            gm.Audio.Play(AudioSynth.Jump, 0.7f);
        }
        // releasing early cuts the jump short → hold-to-jump-higher
        if (!grounded && ActionUp && velY > 0f) velY *= 0.45f;

        float prevFoot = transform.position.y - half.y;
        velY -= Gravity * dt;
        var pos = transform.position;
        pos.y += velY * dt;
        grounded = false;
        if (velY <= 0f)
        {
            float? top = World.Instance.GroundTopUnder(pos.x - half.x * 0.8f, pos.x + half.x * 0.8f, prevFoot + 0.12f);
            if (top.HasValue && pos.y - half.y <= top.Value)
            {
                pos.y = top.Value + half.y;
                velY = 0f;
                grounded = true;
            }
        }
        transform.position = pos;
        if (pos.y < -6.5f) gm.KillPlayer(); // fell into a gap
    }

    void TickFlip(GameManager gm)
    {
        float dt = Time.deltaTime;
        if (grounded && ActionDown)
        {
            gravitySign = -gravitySign;
            grounded = false;
            gm.Audio.Play(AudioSynth.Flip, 0.7f);
            gm.Score.AddCombo(1); // flip-chains build the multiplier (GDD 2.5)
        }
        velY -= Gravity * gravitySign * dt;
        var pos = transform.position;
        pos.y += velY * dt;
        float surface = gravitySign > 0 ? FloorTop + half.y : CeilBottom - half.y;
        if ((gravitySign > 0 && pos.y <= surface) || (gravitySign < 0 && pos.y >= surface))
        {
            pos.y = surface;
            velY = 0f;
            grounded = true;
        }
        transform.position = pos;
        sr.flipY = gravitySign < 0;
    }

    void TickTop(GameManager gm)
    {
        float v = Input.GetAxisRaw("Vertical");
        var pos = transform.position;
        pos.y = Mathf.Clamp(pos.y + v * Strafe * Time.deltaTime, -3.4f, 3.4f);
        transform.position = pos;
        NearMissScan(gm);
    }

    void TickRhythm(GameManager gm)
    {
        // Bit rides the beat: gentle bob on the lane, no free movement, no spatial hazards.
        float bob = Mathf.Sin(Time.time * 9f) * 0.16f;
        var pos = transform.position;
        pos.x = PlayerX;
        pos.y = Mathf.Lerp(pos.y, bob, 0.25f);
        transform.position = pos;
        if (ActionDown) gm.Rhythm.TryHit(gm);
    }

    // Squeaking past a hazard in Top-Down rewards a near-miss combo (GDD 2.5).
    void NearMissScan(GameManager gm)
    {
        if (gm.GraceActive) return;
        float by = transform.position.y;
        var all = Entity.All;
        for (int i = 0; i < all.Count; i++)
        {
            var e = all[i];
            if (e.kind != EntityKind.Hazard || e.consumed) continue;
            if (Mathf.Abs(e.transform.position.x - PlayerX) > 0.5f) continue; // only as it passes Bit
            float dy = Mathf.Abs(e.transform.position.y - by);
            float clear = half.y + e.halfSize.y;
            if (dy > clear && dy < clear + 0.6f)
            {
                e.consumed = true; // award once
                gm.Score.AddPoints(15, 1);
                gm.Audio.Play(AudioSynth.Coin, 0.4f);
            }
        }
    }

    void Collide(GameManager gm)
    {
        var w = World.Instance;
        var pr = new Rect((Vector2)transform.position - half, half * 2f);
        Entity e;
        while ((e = w.Overlap(pr, EntityKind.Coin)) != null)
        {
            gm.Score.AddCoin();
            gm.Audio.Play(AudioSynth.Coin, 0.6f);
            e.Kill();
        }
        if ((e = w.Overlap(pr, EntityKind.Cassette)) != null)
        {
            var target = e.swapTo;
            gm.Score.AddTape();
            e.Kill();
            gm.Swap(target);
            return;
        }
        if (!gm.GraceActive && w.Overlap(pr, EntityKind.Hazard) != null)
            gm.KillPlayer();
    }

    public void FlashWhite() =>
        StartCoroutine(Settings.ReducedFlashing ? SoftBlink() : Blink());

    IEnumerator Blink()
    {
        for (int i = 0; i < 8; i++)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.07f);
        }
        sr.enabled = true;
    }

    // Gentler death feedback when "reduced flashing" is enabled (GDD 2.10).
    IEnumerator SoftBlink()
    {
        for (int i = 0; i < 3; i++)
        {
            sr.color = new Color(1f, 1f, 1f, 0.35f);
            yield return new WaitForSeconds(0.12f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.12f);
        }
    }
}
