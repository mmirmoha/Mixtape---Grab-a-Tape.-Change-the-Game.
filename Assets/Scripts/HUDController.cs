using UnityEngine;
using UnityEngine.UI;

// Holds references built by UIBuilder and keeps the HUD in sync each frame.
// Also owns the Title overlay panels (Options / How-to-Play / Shop) and key rebinding.
public class HUDController : MonoBehaviour
{
    public GameObject TitlePanel, HudPanel, GameOverPanel;
    public GameObject OptionsPanel, HowToPanel, ShopPanel;
    public Text ScoreText, DistText, ModeText, BestTitleText, GoScoreText, GoBestText;
    public Text ComboText, SwapFlashText, TutorialPrompt;
    public Image ModeBadge, Flash, ComboFill;

    bool rebinding;
    Text rebindLabel;

    public void ApplyState(GameState s)
    {
        CloseOverlays();
        TitlePanel.SetActive(s == GameState.Title);
        HudPanel.SetActive(s == GameState.Run);
        GameOverPanel.SetActive(s == GameState.GameOver);
        if (SwapFlashText) SwapFlashText.text = "";
        if (TutorialPrompt) TutorialPrompt.text = "";

        var sc = GameManager.Instance.Score;
        if (s == GameState.Title)
            BestTitleText.text = sc.Best > 0 ? "BEST SCORE  " + sc.Best : "";
        if (s == GameState.GameOver)
        {
            GoScoreText.text = "SCORE  " + sc.Score;
            GoBestText.text = "BEST  " + sc.Best;
        }
    }

    public void SetMode(GameMode m)
    {
        var a = ModeInfo.Accent(m);
        ModeBadge.color = new Color(a.r, a.g, a.b, 0.2f);
        ModeText.text = "TRACK: " + ModeInfo.DisplayName(m);
        ModeText.color = a;
    }

    public void SetTutorialPrompt(string s)
    {
        if (TutorialPrompt) TutorialPrompt.text = s;
    }

    // ---------- Title overlays ----------

    public bool AnyOverlayOpen =>
        (OptionsPanel && OptionsPanel.activeSelf) ||
        (HowToPanel && HowToPanel.activeSelf) ||
        (ShopPanel && ShopPanel.activeSelf);

    public void OpenOverlay(GameObject panel)
    {
        CloseOverlays();
        if (panel) panel.SetActive(true);
    }

    public void CloseOverlays()
    {
        if (OptionsPanel) OptionsPanel.SetActive(false);
        if (HowToPanel) HowToPanel.SetActive(false);
        if (ShopPanel) ShopPanel.SetActive(false);
        rebinding = false;
    }

    public void BeginRebind(Text label)
    {
        rebinding = true;
        rebindLabel = label;
        if (label) label.text = "JUMP KEY:  PRESS A KEY…";
    }

    // ---------- per-frame ----------

    void Update()
    {
        if (rebinding) { CaptureRebind(); return; }

        var gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Run) return;
        ScoreText.text = "SCORE " + gm.Score.Score;
        DistText.text = Mathf.FloorToInt(gm.Score.Distance) + " m";
        UpdateCombo(gm);
    }

    void UpdateCombo(GameManager gm)
    {
        if (ComboFill == null) return;
        if (gm.Mode == GameMode.Rhythm)
        {
            float h = gm.Rhythm != null ? gm.Rhythm.Health : 1f;
            ComboFill.fillAmount = Mathf.Clamp01(h);
            ComboFill.color = ModeInfo.Accent(GameMode.Rhythm);
            ComboText.text = "BEAT";
        }
        else
        {
            float mult = gm.Score.Multiplier;
            ComboFill.fillAmount = Mathf.Clamp01((mult - 1f) / 4f);
            ComboFill.color = ModeInfo.Accent(gm.Mode);
            ComboText.text = mult > 1.01f ? "x" + mult.ToString("0.0") : "";
        }
    }

    void CaptureRebind()
    {
        if (!Input.anyKeyDown) return;
        foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (k == KeyCode.Mouse0 || k == KeyCode.Escape || k == KeyCode.None) continue;
            if (Input.GetKeyDown(k))
            {
                Settings.JumpKey = k;
                Settings.Save();
                rebinding = false;
                if (rebindLabel) rebindLabel.text = "JUMP KEY:  " + KeyName(k) + "   (CLICK TO REBIND)";
                return;
            }
        }
    }

    public static string KeyName(KeyCode k) => k == KeyCode.Space ? "SPACE" : k.ToString().ToUpper();
}
