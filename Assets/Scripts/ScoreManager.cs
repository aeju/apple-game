using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 점수, 콤보, 최고기록을 관리합니다.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }
    public int ApplesRemoved { get; private set; }
    public int Combo { get; private set; }
    public int MaxCombo { get; private set; }
    public int BestScore { get; private set; }

    [Header("Combo Settings")]
    public float ComboTimeout = 3f;

    [Header("Events")]
    public UnityEvent<int> OnScoreChanged = new UnityEvent<int>();
    public UnityEvent<int, int> OnComboChanged = new UnityEvent<int, int>();
    public UnityEvent<int, Vector3> OnScorePopup = new UnityEvent<int, Vector3>();

    private float _comboTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        BestScore = PlayerPrefs.GetInt("AppleGame_BestScore", 0);
    }

    private void Update()
    {
        if (Combo > 0)
        {
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0f)
            {
                Combo = 0;
                OnComboChanged?.Invoke(0, MaxCombo);
            }
        }
    }

    /// <summary>
    /// 사과 제거 시 점수를 추가합니다.
    /// </summary>
    public void AddScore(int appleCount, Vector3 worldPos = default)
    {
        // 콤보 증가
        Combo++;
        if (Combo > MaxCombo) MaxCombo = Combo;
        _comboTimer = ComboTimeout;

        // 점수 계산 (콤보 보너스 적용)
        int basePoints = appleCount;
        float comboMultiplier = 1f + (Combo - 1) * 0.15f;
        int totalPoints = Mathf.RoundToInt(basePoints * comboMultiplier);

        Score += totalPoints;
        ApplesRemoved += appleCount;

        Debug.Log($"[ScoreManager] Score updated: {Score}, Combo: {Combo}, Points added: {totalPoints}");
        OnScoreChanged?.Invoke(Score);
        OnComboChanged?.Invoke(Combo, MaxCombo);
        OnScorePopup?.Invoke(totalPoints, worldPos);
    }

    /// <summary>
    /// 점수를 리셋합니다.
    /// </summary>
    public void ResetScore()
    {
        Score = 0;
        ApplesRemoved = 0;
        Combo = 0;
        MaxCombo = 0;
        _comboTimer = 0;
        OnScoreChanged?.Invoke(0);
        OnComboChanged?.Invoke(0, 0);
    }

    /// <summary>
    /// 최고기록을 저장합니다.
    /// </summary>
    public bool SaveBestScore()
    {
        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt("AppleGame_BestScore", BestScore);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }
}
