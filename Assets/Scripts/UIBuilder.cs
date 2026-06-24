using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Builds the entire uGUI tree (title / HUD / game over / overlays / flash) in code.
public static class UIBuilder
{
    static Font font;

    public static HUDController Build()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var hud = canvasGo.AddComponent<HUDController>();
        hud.HudPanel = BuildHud(canvasGo.transform, hud);
        hud.TitlePanel = BuildTitle(canvasGo.transform, hud);
        hud.GameOverPanel = BuildGameOver(canvasGo.transform, hud);
        hud.OptionsPanel = BuildOptions(canvasGo.transform, hud);
        hud.HowToPanel = BuildHowTo(canvasGo.transform, hud);
        hud.ShopPanel = BuildShop(canvasGo.transform, hud);
        hud.Flash = BuildFlash(canvasGo.transform);

        hud.OptionsPanel.SetActive(false);
        hud.HowToPanel.SetActive(false);
        hud.ShopPanel.SetActive(false);
        return hud;
    }

    // ---------- panels ----------

    static GameObject BuildTitle(Transform parent, HUDController hud)
    {
        var p = Panel(parent, "TitlePanel");
        p.AddComponent<Image>().color = Palette.Navy;
        var t = p.transform;
        var center = new Vector2(0.5f, 0.5f);

        // cassette deck framing
        Img(t, "DeckBody", center, new Vector2(0f, 60f), new Vector2(780f, 400f), Palette.DeckBody);
        Img(t, "Label", center, new Vector2(0f, 140f), new Vector2(700f, 160f), Palette.DeckLabel);
        Img(t, "LabelLine", center, new Vector2(0f, 222f), new Vector2(700f, 6f), Palette.Magenta);

        Txt(t, "Title", "MIXTAPE", 84, Palette.Magenta, center, new Vector2(0f, 168f));
        Txt(t, "Subtitle", "GRAB A TAPE. CHANGE THE GAME.", 22, Palette.Cyan, center, new Vector2(0f, 96f));

        // reels + tape window
        var reelL = Img(t, "ReelL", center, new Vector2(-200f, -10f), new Vector2(100f, 100f), new Color(0.55f, 0.6f, 0.95f));
        reelL.sprite = SpriteFactory.Reel;
        var reelR = Img(t, "ReelR", center, new Vector2(200f, -10f), new Vector2(100f, 100f), new Color(0.55f, 0.6f, 0.95f));
        reelR.sprite = SpriteFactory.Reel;
        Img(t, "TapeWindow", center, new Vector2(0f, -10f), new Vector2(140f, 56f), Palette.Dark);

        Btn(t, "PLAY", new Vector2(0f, -232f), new Vector2(260f, 64f), Palette.Magenta,
            () => GameManager.Instance.StartRun());

        Btn(t, "OPTIONS", new Vector2(-196f, -300f), new Vector2(176f, 46f),
            Color.Lerp(Palette.Cyan, Palette.Dark, 0.5f),
            () => hud.OpenOverlay(hud.OptionsPanel));
        Btn(t, "HOW TO PLAY", new Vector2(0f, -300f), new Vector2(192f, 46f),
            Color.Lerp(Palette.Cyan, Palette.Dark, 0.5f),
            () => hud.OpenOverlay(hud.HowToPanel));
        Btn(t, "SHOP", new Vector2(196f, -300f), new Vector2(176f, 46f),
            Color.Lerp(Palette.Yellow, Palette.Dark, 0.45f),
            () => hud.OpenOverlay(hud.ShopPanel));

        hud.BestTitleText = Txt(t, "Best", "", 24, Palette.Yellow, center, new Vector2(0f, -344f));

        Txt(t, "Hint", "GRAB CASSETTES TO SWITCH BETWEEN FOUR TRACKS — SURVIVE & SCORE",
            16, Palette.TextDim, new Vector2(0.5f, 0f), new Vector2(0f, 24f));
        return p;
    }

    static GameObject BuildHud(Transform parent, HUDController hud)
    {
        var p = Panel(parent, "HudPanel");
        var t = p.transform;

        hud.ScoreText = Txt(t, "Score", "SCORE 0", 30, Color.white,
            new Vector2(0f, 1f), new Vector2(28f, -26f), TextAnchor.UpperLeft);
        hud.DistText = Txt(t, "Dist", "0 m", 30, Color.white,
            new Vector2(1f, 1f), new Vector2(-28f, -26f), TextAnchor.UpperRight);

        hud.ModeBadge = Img(t, "ModeBadge", new Vector2(0.5f, 1f), new Vector2(0f, -22f),
            new Vector2(290f, 46f), new Color(1f, 1f, 1f, 0.2f));
        hud.ModeText = Txt(hud.ModeBadge.transform, "ModeText", "TRACK: SIDE-SCROLL", 24,
            Palette.Cyan, new Vector2(0.5f, 0.5f), Vector2.zero);

        // combo bar (top-left, under the score)
        Img(t, "ComboBG", new Vector2(0f, 1f), new Vector2(28f, -64f), new Vector2(220f, 16f),
            new Color(1f, 1f, 1f, 0.12f));
        hud.ComboFill = Img(t, "ComboFill", new Vector2(0f, 1f), new Vector2(28f, -64f),
            new Vector2(220f, 16f), Palette.Cyan);
        hud.ComboFill.sprite = SpriteFactory.Pixel;
        hud.ComboFill.type = Image.Type.Filled;
        hud.ComboFill.fillMethod = Image.FillMethod.Horizontal;
        hud.ComboFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        hud.ComboFill.fillAmount = 0f;
        hud.ComboText = Txt(t, "ComboText", "", 20, Color.white,
            new Vector2(0f, 1f), new Vector2(256f, -64f), TextAnchor.MiddleLeft, 120);

        // big mode-name flash on swap
        hud.SwapFlashText = Txt(t, "SwapFlash", "", 70, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, 70f));
        hud.SwapFlashText.text = "";

        // tutorial prompt
        hud.TutorialPrompt = Txt(t, "TutPrompt", "", 30, Palette.Yellow, new Vector2(0.5f, 0.5f), new Vector2(0f, -150f));
        return p;
    }

    static GameObject BuildGameOver(Transform parent, HUDController hud)
    {
        var p = Panel(parent, "GameOverPanel");
        p.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.66f);
        var t = p.transform;
        var center = new Vector2(0.5f, 0.5f);

        Txt(t, "Header", "GAME OVER", 72, Palette.Magenta, center, new Vector2(0f, 150f));
        hud.GoScoreText = Txt(t, "Score", "SCORE  0", 38, Color.white, center, new Vector2(0f, 45f));
        hud.GoBestText = Txt(t, "Best", "BEST  0", 26, Palette.Yellow, center, new Vector2(0f, -5f));

        Btn(t, "RETRY", new Vector2(0f, -90f), new Vector2(250f, 64f),
            Color.Lerp(Palette.Cyan, Palette.Dark, 0.45f),
            () => GameManager.Instance.StartRun());
        Btn(t, "TITLE", new Vector2(0f, -172f), new Vector2(250f, 56f),
            new Color(0.28f, 0.3f, 0.45f),
            () => GameManager.Instance.GoTitle());

        Txt(t, "Hint", "SPACE / R = RETRY      ESC = TITLE", 18, Palette.TextDim, center, new Vector2(0f, -245f));
        return p;
    }

    // ---------- overlays ----------

    static GameObject BuildOptions(Transform parent, HUDController hud)
    {
        var p = Panel(parent, "OptionsPanel");
        p.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.12f, 0.97f);
        var t = p.transform;
        var c = new Vector2(0.5f, 0.5f);

        Txt(t, "H", "OPTIONS", 52, Palette.Cyan, c, new Vector2(0f, 250f));

        Sldr(t, "MASTER", new Vector2(0f, 170f), Settings.MasterVol, v =>
        {
            Settings.MasterVol = v; Settings.Save();
            if (GameManager.Instance != null) GameManager.Instance.Audio.ApplyVolumes();
        });
        Sldr(t, "MUSIC", new Vector2(0f, 122f), Settings.MusicVol, v =>
        {
            Settings.MusicVol = v; Settings.Save();
            if (GameManager.Instance != null) GameManager.Instance.Audio.ApplyVolumes();
        });
        Sldr(t, "SFX", new Vector2(0f, 74f), Settings.SfxVol, v =>
        {
            Settings.SfxVol = v; Settings.Save();
        });

        Tgl(t, "REDUCED FLASHING", new Vector2(0f, 20f), Settings.ReducedFlashing, v =>
        {
            Settings.ReducedFlashing = v; Settings.Save();
        });
        Tgl(t, "COLORBLIND PALETTE", new Vector2(0f, -22f), Settings.Colorblind, v =>
        {
            Settings.Colorblind = v; Settings.Save();
        });

        // difficulty (gated by the Hard-mode unlock)
        Text diffLbl = null;
        Action refreshDiff = () =>
        {
            if (!Economy.HardUnlocked) diffLbl.text = "DIFFICULTY:  NORMAL   (HARD LOCKED — BUY IN SHOP)";
            else diffLbl.text = "DIFFICULTY:  " + (Settings.Hard ? "HARD" : "NORMAL") + "   (CLICK TO TOGGLE)";
        };
        var diffBtn = Btn(t, "", new Vector2(0f, -78f), new Vector2(560f, 40f),
            Color.Lerp(Palette.Purple, Palette.Dark, 0.5f), () =>
            {
                if (!Economy.HardUnlocked) return;
                Settings.Hard = !Settings.Hard; Settings.Save(); refreshDiff();
            });
        diffLbl = diffBtn.GetComponentInChildren<Text>();
        diffLbl.fontSize = 18;
        refreshDiff();

        // key rebinding
        var rebindBtn = Btn(t, "", new Vector2(0f, -126f), new Vector2(560f, 40f),
            Color.Lerp(Palette.Cyan, Palette.Dark, 0.55f), null);
        var rebindLbl = rebindBtn.GetComponentInChildren<Text>();
        rebindLbl.fontSize = 18;
        rebindLbl.text = "JUMP KEY:  " + HUDController.KeyName(Settings.JumpKey) + "   (CLICK TO REBIND)";
        rebindBtn.onClick.AddListener(() => hud.BeginRebind(rebindLbl));

        Btn(t, "BACK", new Vector2(0f, -210f), new Vector2(220f, 54f), Palette.Magenta,
            () => hud.CloseOverlays());
        return p;
    }

    static GameObject BuildHowTo(Transform parent, HUDController hud)
    {
        var p = Panel(parent, "HowToPanel");
        p.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.12f, 0.97f);
        var t = p.transform;
        var c = new Vector2(0.5f, 0.5f);

        Txt(t, "H", "HOW TO PLAY", 52, Palette.Cyan, c, new Vector2(0f, 250f));
        Txt(t, "Sub", "BIT AUTO-RUNS. GRAB CASSETTES TO SWAP TRACKS. ONE HIT ENDS THE RUN.",
            18, Palette.TextDim, c, new Vector2(0f, 200f));

        string jk = HUDController.KeyName(Settings.JumpKey);
        ModeRow(t, GameMode.SideScroll, "TAP " + jk + " / CLICK = JUMP  (HOLD = HIGHER)", 140f);
        ModeRow(t, GameMode.GravityFlip, "TAP " + jk + " / CLICK = FLIP GRAVITY", 84f);
        ModeRow(t, GameMode.TopDown, "W / S / ARROWS = STRAFE  —  GRAZE FOR COMBO", 28f);
        ModeRow(t, GameMode.Rhythm, "TAP " + jk + " ON THE BEAT AT THE HIT LINE", -28f);

        Txt(t, "Combo", "CLEAN PLAY BUILDS A SCORE MULTIPLIER — RHYTHM HITS ARE WORTH THE MOST.",
            17, Palette.Yellow, c, new Vector2(0f, -92f));

        Btn(t, "TRY THE TUTORIAL", new Vector2(0f, -150f), new Vector2(300f, 50f),
            Color.Lerp(Palette.Green, Palette.Dark, 0.4f),
            () => { hud.CloseOverlays(); GameManager.Instance.StartRun(true); });
        Btn(t, "BACK", new Vector2(0f, -212f), new Vector2(220f, 52f), Palette.Magenta,
            () => hud.CloseOverlays());
        return p;
    }

    static void ModeRow(Transform parent, GameMode m, string controls, float y)
    {
        var a = ModeInfo.Accent(m);
        var chip = Img(parent, "Chip" + (int)m, new Vector2(0.5f, 0.5f), new Vector2(-300f, y),
            new Vector2(220f, 44f), new Color(a.r, a.g, a.b, 0.22f));
        Txt(chip.transform, "Name", ModeInfo.DisplayName(m), 22, a, new Vector2(0.5f, 0.5f), Vector2.zero);
        Txt(parent, "Ctrl" + (int)m, controls, 18, Color.white, new Vector2(0.5f, 0.5f),
            new Vector2(110f, y), TextAnchor.MiddleLeft, 560);
    }

    static GameObject BuildShop(Transform parent, HUDController hud)
    {
        var p = Panel(parent, "ShopPanel");
        p.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.12f, 0.97f);
        var t = p.transform;
        var c = new Vector2(0.5f, 0.5f);

        Txt(t, "H", "SHOP", 52, Palette.Yellow, c, new Vector2(0f, 258f));
        var coinText = Txt(t, "Coins", "", 26, Palette.Yellow, c, new Vector2(0f, 212f));

        var refreshers = new List<Action>();
        Action refreshCoins = () => coinText.text = "COINS:  " + Economy.TotalCoins;

        float y = 150f;
        foreach (var item in Economy.Catalog)
        {
            var captured = item;
            Img(t, "Sw" + captured.id, c, new Vector2(-280f, y), new Vector2(30f, 30f), captured.swatch);
            Txt(t, "Nm" + captured.id, captured.name, 22, Color.white, c,
                new Vector2(-90f, y), TextAnchor.MiddleLeft, 360);

            var btn = Btn(t, "", new Vector2(220f, y), new Vector2(200f, 40f),
                Color.Lerp(Palette.Cyan, Palette.Dark, 0.5f), null);
            var lbl = btn.GetComponentInChildren<Text>();
            lbl.fontSize = 18;

            Action refresh = () =>
            {
                if (!Economy.IsUnlocked(captured))
                {
                    lbl.text = "BUY  " + captured.price;
                    ((Image)btn.targetGraphic).color = Color.Lerp(Palette.Yellow, Palette.Dark, 0.45f);
                }
                else if (captured.kind == Economy.ItemKind.Hard)
                {
                    lbl.text = "UNLOCKED";
                    ((Image)btn.targetGraphic).color = new Color(0.2f, 0.4f, 0.25f);
                }
                else if (Economy.IsSelected(captured))
                {
                    lbl.text = "SELECTED";
                    ((Image)btn.targetGraphic).color = Color.Lerp(captured.swatch, Palette.Dark, 0.3f);
                }
                else
                {
                    lbl.text = "SELECT";
                    ((Image)btn.targetGraphic).color = Color.Lerp(Palette.Cyan, Palette.Dark, 0.5f);
                }
            };
            refreshers.Add(refresh);

            btn.onClick.AddListener(() =>
            {
                if (!Economy.IsUnlocked(captured)) Economy.Buy(captured);
                else Economy.Select(captured);
                if (captured.kind == Economy.ItemKind.Track && GameManager.Instance != null)
                    GameManager.Instance.Audio.RefreshTrack();
                refreshCoins();
                foreach (var r in refreshers) r();
            });

            y -= 52f;
        }

        // refresh whenever the shop is shown
        var shower = p.AddComponent<OnEnableRunner>();
        shower.OnShown = () => { refreshCoins(); foreach (var r in refreshers) r(); };

        Btn(t, "BACK", new Vector2(0f, -250f), new Vector2(220f, 54f), Palette.Magenta,
            () => hud.CloseOverlays());
        return p;
    }

    static Image BuildFlash(Transform parent)
    {
        var p = Panel(parent, "Flash");
        var img = p.AddComponent<Image>();
        img.color = Color.clear;
        img.raycastTarget = false;
        return img;
    }

    // ---------- helpers ----------

    static GameObject Panel(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    static Image Img(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color c)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        return img;
    }

    static Text Txt(Transform parent, string name, string s, int size, Color c,
        Vector2 anchor, Vector2 pos, TextAnchor align = TextAnchor.MiddleCenter, int width = 900)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(width, size * 1.6f);
        var tx = go.AddComponent<Text>();
        tx.font = font;
        tx.fontSize = size;
        tx.fontStyle = FontStyle.Bold;
        tx.color = c;
        tx.text = s;
        tx.alignment = align;
        tx.horizontalOverflow = HorizontalWrapMode.Overflow;
        tx.verticalOverflow = VerticalWrapMode.Overflow;
        tx.raycastTarget = false;
        return tx;
    }

    static Button Btn(Transform parent, string label, Vector2 pos, Vector2 size, Color bg, UnityAction onClick)
    {
        var img = Img(parent, label + "Btn", new Vector2(0.5f, 0.5f), pos, size, bg);
        img.raycastTarget = true;
        var b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        if (onClick != null) b.onClick.AddListener(onClick);
        Txt(img.transform, "Label", label, 28, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, (int)size.x);
        return b;
    }

    static Slider Sldr(Transform parent, string label, Vector2 pos, float value, UnityAction<float> onChange)
    {
        Txt(parent, label + "Lbl", label, 20, Palette.Cyan, new Vector2(0.5f, 0.5f),
            pos + new Vector2(-220f, 0f), TextAnchor.MiddleLeft, 200);

        var root = Img(parent, label + "Sld", new Vector2(0.5f, 0.5f), pos + new Vector2(70f, 0f),
            new Vector2(260f, 18f), new Color(1f, 1f, 1f, 0.12f));
        root.raycastTarget = true;
        var slider = root.gameObject.AddComponent<Slider>();

        var fill = Img(root.transform, "Fill", new Vector2(0f, 0f), Vector2.zero, Vector2.zero, Palette.Cyan);
        var fillRt = (RectTransform)fill.transform;
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        var handle = Img(root.transform, "Handle", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(16f, 26f), Color.white);
        var handleRt = (RectTransform)handle.transform;

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;
        slider.onValueChanged.AddListener(onChange);
        return slider;
    }

    static Toggle Tgl(Transform parent, string label, Vector2 pos, bool value, UnityAction<bool> onChange)
    {
        var root = new GameObject(label + "Tgl", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        var rt = (RectTransform)root.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(420f, 30f);
        var toggle = root.AddComponent<Toggle>();

        var box = Img(root.transform, "Box", new Vector2(0.5f, 0.5f), new Vector2(-170f, 0f),
            new Vector2(26f, 26f), new Color(1f, 1f, 1f, 0.18f));
        box.raycastTarget = true;
        var check = Img(box.transform, "Check", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(16f, 16f), Palette.Green);
        Txt(root.transform, "Lbl", label, 20, Color.white, new Vector2(0.5f, 0.5f),
            new Vector2(20f, 0f), TextAnchor.MiddleLeft, 320);

        toggle.targetGraphic = box;
        toggle.graphic = check;
        toggle.isOn = value;
        toggle.onValueChanged.AddListener(onChange);
        return toggle;
    }
}
