using System.Collections;
using UnityEngine;

public enum GameState { Title, Run, GameOver }

// Root state machine. Created by Bootstrap; constructs the whole game from code —
// the scene itself is empty.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Title;
    public GameMode Mode { get; private set; } = GameMode.SideScroll;
    public bool Frozen { get; set; }
    public bool Tutorial { get; private set; }
    public float GraceUntil { get; set; }

    public ScoreManager Score { get; private set; }
    public AudioManager Audio { get; private set; }
    public PlayerController Player { get; private set; }
    public HUDController HUD { get; private set; }
    public GroundBuilder Ground { get; private set; }
    public SpawnDirector Spawner { get; private set; }
    public SwapTransition Swapper { get; private set; }
    public RhythmDirector Rhythm { get; private set; }
    public TutorialController Tutor { get; private set; }

    float runTime;
    Camera cam;

    public float Speed
    {
        get
        {
            if (Tutorial) return 3.5f; // slow, calm pace while teaching
            bool hard = Settings.Hard && Economy.HardUnlocked;
            float baseSpeed = hard ? 8.5f : 6f;
            float cap = hard ? 16f : 14f;
            return Mathf.Min(baseSpeed + 0.15f * runTime, cap);
        }
    }
    public float Ramp01 => Mathf.InverseLerp(6f, 14f, Speed);
    public bool WorldScrolling => State == GameState.Run && !Frozen;
    public bool GraceActive => Time.time < GraceUntil;

    void Awake()
    {
        Instance = this;
        Settings.Load();
        Economy.Load();

        var camGo = new GameObject("MainCamera") { tag = "MainCamera" };
        cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Palette.Navy;
        camGo.AddComponent<AudioListener>();

        Audio = gameObject.AddComponent<AudioManager>();
        Audio.Init();
        gameObject.AddComponent<World>();
        Ground = gameObject.AddComponent<GroundBuilder>();
        Spawner = gameObject.AddComponent<SpawnDirector>();
        Score = gameObject.AddComponent<ScoreManager>();
        Swapper = gameObject.AddComponent<SwapTransition>();
        Rhythm = gameObject.AddComponent<RhythmDirector>();
        Tutor = gameObject.AddComponent<TutorialController>();

        var bit = new GameObject("Bit");
        Player = bit.AddComponent<PlayerController>();
        bit.SetActive(false);

        HUD = UIBuilder.Build();
        HUD.ApplyState(State);
    }

    void Update()
    {
        if (State == GameState.Run && !Frozen && !Tutorial) runTime += Time.deltaTime;

        if (State == GameState.Title)
        {
            if (HUD.AnyOverlayOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) HUD.CloseOverlays();
            }
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                StartRun();
            }
        }
        else if (State == GameState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R)) StartRun();
            else if (Input.GetKeyDown(KeyCode.Escape)) GoTitle();
        }
    }

    public void StartRun(bool forceTutorial = false)
    {
        HUD.CloseOverlays();
        World.Instance.ClearAll();
        Mode = GameMode.SideScroll;
        runTime = 0f;
        Frozen = false;
        Tutorial = forceTutorial || PlayerPrefs.GetInt("mixtape.tutorialDone", 0) == 0;
        GraceUntil = Time.time + 1.5f;
        Score.ResetRun();
        Ground.Rebuild(Mode);
        Spawner.ResetRun();
        Player.gameObject.SetActive(true);
        Player.ResetFor(Mode);
        ApplyModeTint();
        HUD.SetMode(Mode);
        SetState(GameState.Run);
        Audio.Play(AudioSynth.Click);

        if (Tutorial)
        {
            PlayerPrefs.SetInt("mixtape.tutorialDone", 1); // shows once
            PlayerPrefs.Save();
            Tutor.Begin();
        }
        else
        {
            Tutor.Stop();
        }
    }

    // Called by the tutorial once the first swap fires — resume the normal generator.
    public void EndTutorial()
    {
        Tutorial = false;
        runTime = 0f;
        GraceUntil = Mathf.Max(GraceUntil, Time.time + 1f);
        Spawner.OnSwapped();
    }

    public void GoTitle()
    {
        Tutor.Stop();
        World.Instance.ClearAll();
        Player.gameObject.SetActive(false);
        Frozen = false;
        Tutorial = false;
        SetState(GameState.Title);
    }

    public void Swap(GameMode next) => Swapper.Begin(next);

    public void SetMode(GameMode m)
    {
        Mode = m;
        if (m == GameMode.Rhythm && Rhythm != null) Rhythm.ResetRhythm();
        ApplyModeTint();
        HUD.SetMode(m);
    }

    public void KillPlayer()
    {
        if (State != GameState.Run || Frozen) return;
        Tutor.Stop();
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        Frozen = true;
        Tutorial = false;
        Audio.Play(AudioSynth.Death);
        Audio.Duck(0.8f);
        Player.FlashWhite();
        yield return new WaitForSeconds(0.8f);
        Score.CommitBest();
        SetState(GameState.GameOver);
    }

    void ApplyModeTint() =>
        cam.backgroundColor = Color.Lerp(Palette.Navy, ModeInfo.Accent(Mode), 0.10f);

    void SetState(GameState s)
    {
        State = s;
        HUD.ApplyState(s);
    }
}
