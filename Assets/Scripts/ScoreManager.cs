using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    const string BestKey = "mixtape.best";
    const string BestComboKey = "mixtape.bestcombo";

    public float Distance { get; private set; }
    public int Coins { get; private set; }
    public int Tapes { get; private set; }
    public int Best { get; private set; }

    public int Combo { get; private set; }
    public int BestCombo { get; private set; }
    public int ComboBonus { get; private set; } // multiplier bonus points banked this run

    // Clean play builds a multiplier (GDD 2.3) — biggest in rhythm sections.
    public float Multiplier => Mathf.Clamp(1f + Combo / 8f, 1f, 5f);

    public int Score => Mathf.FloorToInt(Distance) + Coins * 10 + Tapes * 50 + ComboBonus;

    void Awake()
    {
        Best = PlayerPrefs.GetInt(BestKey, 0);
        BestCombo = PlayerPrefs.GetInt(BestComboKey, 0);
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm != null && gm.WorldScrolling) Distance += gm.Speed * Time.deltaTime;
    }

    public void ResetRun()
    {
        Distance = 0f;
        Coins = 0;
        Tapes = 0;
        Combo = 0;
        ComboBonus = 0;
    }

    public void AddCoin()
    {
        Coins++;
        AddCombo(1);
        ComboBonus += Mathf.RoundToInt(10 * (Multiplier - 1f));
    }

    public void AddTape()
    {
        Tapes++;
        AddCombo(2);
        ComboBonus += Mathf.RoundToInt(50 * (Multiplier - 1f));
    }

    // Generic combo gain (near-misses, flip-chains).
    public void AddCombo(int n)
    {
        Combo += n;
        if (Combo > BestCombo) BestCombo = Combo;
    }

    // Points scaled by the live multiplier — used by rhythm note hits and near-misses.
    public void AddPoints(int basePoints, int comboGain)
    {
        AddCombo(comboGain);
        ComboBonus += basePoints + Mathf.RoundToInt(basePoints * (Multiplier - 1f));
    }

    public void ResetCombo() => Combo = 0;

    public void CommitBest()
    {
        Economy.AddCoins(Coins); // bank this run's coins toward cosmetic unlocks
        if (BestCombo > PlayerPrefs.GetInt(BestComboKey, 0))
            PlayerPrefs.SetInt(BestComboKey, BestCombo);
        if (Score > Best)
        {
            Best = Score;
            PlayerPrefs.SetInt(BestKey, Best);
        }
        PlayerPrefs.Save();
    }
}
