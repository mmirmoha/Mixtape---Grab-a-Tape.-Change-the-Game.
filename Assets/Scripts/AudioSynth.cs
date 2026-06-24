using System;
using UnityEngine;

// All SFX and the music loop are synthesized as PCM — no audio assets needed.
public static class AudioSynth
{
    const int SR = 44100;

    // The music loops are 8 s of a 120 BPM groove; one beat = 0.5 s. The rhythm mode
    // taps to this grid.
    public const float Bpm = 120f;
    public const float LoopLength = 8f;
    public static float BeatLength => 60f / Bpm;

    public static AudioClip Jump, Flip, Coin, Swap, Death, Click, NoteHit, NoteMiss, Music, Music2;
    static bool ready;

    public static void Init()
    {
        if (ready) return;
        ready = true;
        Jump = RenderSweep("sfx_jump", 0.18f, 240f, 520f, true, 0.5f);
        Flip = RenderSweep("sfx_flip", 0.16f, 520f, 240f, true, 0.5f);
        Coin = RenderSteps("sfx_coin", 0.16f, new[] { 988f, 1319f }, true, 0.4f);
        Swap = RenderSweep("sfx_swap", 0.5f, 160f, 1500f, true, 0.45f, 0.15f);
        Death = RenderSweep("sfx_death", 0.55f, 420f, 60f, true, 0.55f, 0.35f);
        Click = RenderSteps("sfx_click", 0.06f, new[] { 700f }, true, 0.4f);
        NoteHit = RenderSteps("sfx_notehit", 0.12f, new[] { 1319f, 1760f }, true, 0.45f);
        NoteMiss = RenderSweep("sfx_notemiss", 0.16f, 300f, 90f, true, 0.45f, 0.25f);
        Music = BuildMusic();
        Music2 = BuildMusicB();
    }

    static AudioClip Clip(string name, float[] data)
    {
        var clip = AudioClip.Create(name, data.Length, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }

    static float Osc(double phase, bool square)
    {
        float frac = (float)(phase - Math.Floor(phase));
        return square ? (frac < 0.5f ? 1f : -1f) : 4f * Mathf.Abs(frac - 0.5f) - 1f;
    }

    static AudioClip RenderSweep(string name, float dur, float f0, float f1, bool square, float amp, float noiseAmt = 0f)
    {
        int n = (int)(SR * dur);
        var data = new float[n];
        double phase = 0;
        var rng = new System.Random(name.GetHashCode());
        for (int i = 0; i < n; i++)
        {
            float t01 = (float)i / n;
            phase += Mathf.Lerp(f0, f1, t01) / SR;
            float s = Osc(phase, square);
            if (noiseAmt > 0f) s = Mathf.Lerp(s, (float)(rng.NextDouble() * 2 - 1), noiseAmt);
            float env = (1f - t01) * Mathf.Min(1f, i / (SR * 0.005f));
            data[i] = s * amp * env;
        }
        return Clip(name, data);
    }

    static AudioClip RenderSteps(string name, float dur, float[] freqs, bool square, float amp)
    {
        int n = (int)(SR * dur);
        var data = new float[n];
        double phase = 0;
        int per = Mathf.Max(1, n / freqs.Length);
        for (int i = 0; i < n; i++)
        {
            float f = freqs[Mathf.Min(i / per, freqs.Length - 1)];
            if (f <= 0f) continue;
            phase += f / SR;
            float tin = (i % per) / (float)per;
            float env = (1f - tin * 0.6f) * Mathf.Min(1f, (i % per) / (SR * 0.004f));
            data[i] = Osc(phase, square) * amp * env;
        }
        return Clip(name, data);
    }

    static void AddLine(float[] data, float[] notes, float noteDur, float amp, bool square)
    {
        double phase = 0;
        int per = (int)(SR * noteDur);
        for (int k = 0; k < notes.Length; k++)
        {
            float f = notes[k];
            for (int j = 0; j < per; j++)
            {
                int i = k * per + j;
                if (i >= data.Length) return;
                if (f <= 0f) continue;
                phase += f / SR;
                float tin = j / (float)per;
                float env = Mathf.Min(1f, j / (SR * 0.004f)) * (1f - 0.55f * tin);
                data[i] += Osc(phase, square) * amp * env;
            }
        }
    }

    // 8-second, 120 BPM chiptune loop: square lead (eighth notes) + triangle bass (quarters).
    static AudioClip BuildMusic()
    {
        var data = new float[SR * 8];
        float[] lead =
        {
            523f, 659f, 784f, 659f, 880f, 784f, 659f, 523f,
            587f, 659f, 587f, 523f, 659f, 784f, 659f, 587f,
            523f, 0f,   659f, 784f, 880f, 0f,   1047f, 880f,
            784f, 659f, 587f, 659f, 523f, 587f, 523f, 0f
        };
        float[] bass =
        {
            131f, 131f, 175f, 196f, 131f, 131f, 175f, 196f,
            110f, 110f, 147f, 147f, 131f, 131f, 196f, 196f
        };
        AddLine(data, lead, 0.25f, 0.10f, true);
        AddLine(data, bass, 0.5f, 0.13f, false);
        return Clip("music_loop", data);
    }

    // The unlockable "B-side": same 8 s / 120 BPM grid, a darker minor-key remix.
    static AudioClip BuildMusicB()
    {
        var data = new float[SR * 8];
        float[] lead =
        {
            440f, 523f, 659f, 523f, 698f, 659f, 523f, 440f,
            494f, 587f, 494f, 440f, 587f, 698f, 587f, 494f,
            440f, 0f,   523f, 659f, 784f, 0f,   880f, 784f,
            659f, 587f, 523f, 587f, 440f, 494f, 440f, 0f
        };
        float[] bass =
        {
            110f, 110f, 147f, 165f, 110f, 110f, 147f, 165f,
            98f,  98f,  131f, 131f, 110f, 110f, 165f, 165f
        };
        AddLine(data, lead, 0.25f, 0.10f, true);
        AddLine(data, bass, 0.5f, 0.14f, false);
        return Clip("music_loop_b", data);
    }
}
