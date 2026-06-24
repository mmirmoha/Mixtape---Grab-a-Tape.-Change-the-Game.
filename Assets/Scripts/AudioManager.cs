using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource music, sfx;
    bool ducking;

    public void Init()
    {
        AudioSynth.Init();
        music = gameObject.AddComponent<AudioSource>();
        music.clip = Economy.Track2Selected ? AudioSynth.Music2 : AudioSynth.Music;
        music.loop = true;
        music.Play();
        sfx = gameObject.AddComponent<AudioSource>();
        ApplyVolumes();
    }

    // Re-reads Settings (volumes) and applies them. Called on boot and whenever the
    // options menu changes a slider.
    public void ApplyVolumes()
    {
        AudioListener.volume = Settings.MasterVol;
        if (!ducking && music != null) music.volume = Settings.MusicVol;
    }

    // Switch the music track to match the current cosmetic selection.
    public void RefreshTrack()
    {
        if (music == null) return;
        var want = Economy.Track2Selected ? AudioSynth.Music2 : AudioSynth.Music;
        if (music.clip == want) return;
        music.clip = want;
        music.Play();
        ApplyVolumes();
    }

    public void Play(AudioClip clip, float vol = 1f) => sfx.PlayOneShot(clip, vol * Settings.SfxVol);

    // Where we are inside the looping track, in seconds — used to align rhythm taps.
    public float MusicTime => music != null ? music.time : 0f;

    public void Duck(float seconds)
    {
        StopAllCoroutines();
        StartCoroutine(DuckRoutine(seconds));
    }

    IEnumerator DuckRoutine(float seconds)
    {
        ducking = true;
        music.volume = Settings.MusicVol * 0.25f;
        yield return new WaitForSeconds(seconds);
        while (music.volume < Settings.MusicVol)
        {
            music.volume = Mathf.MoveTowards(music.volume, Settings.MusicVol, Time.deltaTime);
            yield return null;
        }
        ducking = false;
    }
}
