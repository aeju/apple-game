using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// 게임 상태, 타이머, 힌트/셔플 관리 등 전체 게임 흐름을 제어합니다.
/// </summary>
public enum GameState
{
    Menu,
    Countdown,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float GameDuration = 120f;
    public int TargetSum = 10;
    public int MaxHints = 3;
    public int MaxShuffles = 2;

    // 상태
    public GameState State { get; private set; } = GameState.Menu;
    public float TimeRemaining { get; private set; }
    public int HintsLeft { get; private set; }
    public int ShufflesLeft { get; private set; }

    [Header("Events")]
    public UnityEvent<GameState> OnStateChanged = new UnityEvent<GameState>();
    public UnityEvent<float> OnTimerTick = new UnityEvent<float>();
    public UnityEvent<int> OnCountdownTick = new UnityEvent<int>();
    public UnityEvent<int> OnHintsChanged = new UnityEvent<int>();
    public UnityEvent<int> OnShufflesChanged = new UnityEvent<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    /// <summary>
    /// 게임을 시작합니다.
    /// </summary>
    public void StartGame()
    {
        StopAllCoroutines();
        StartCoroutine(CoStartGame());
    }

    private IEnumerator CoStartGame()
    {
        // 초기화
        TimeRemaining = GameDuration;
        HintsLeft = MaxHints;
        ShufflesLeft = MaxShuffles;

        ScoreManager.Instance?.ResetScore();
        GridManager.Instance?.GenerateGrid();

        OnTimerTick?.Invoke(TimeRemaining);
        OnHintsChanged?.Invoke(HintsLeft);
        OnShufflesChanged?.Invoke(ShufflesLeft);

        // 카운트다운 상태로 전환 (그리드는 보이지만 터치 불가)
        SetState(GameState.Countdown);
        Debug.Log("[GameManager] Countdown started");
        
        // 4초 카운트다운 효과음 재생
        AudioManager.Instance?.PlaySFX("countdown");

        // 4초 카운트다운: 0초 시작
        yield return new WaitForSeconds(1f); // 0~1초
        Debug.Log("[GameManager] Countdown: 3");
        OnCountdownTick?.Invoke(3); // "3" 표시

        yield return new WaitForSeconds(1f); // 1~2초
        Debug.Log("[GameManager] Countdown: 2");
        OnCountdownTick?.Invoke(2); // "2" 표시

        yield return new WaitForSeconds(1f); // 2~3초
        Debug.Log("[GameManager] Countdown: 1");
        OnCountdownTick?.Invoke(1); // "1" 표시

        yield return new WaitForSeconds(1f); // 3~4초
        Debug.Log("[GameManager] Countdown: GO!");
        OnCountdownTick?.Invoke(0); // "GO!" 표시

        // 게임 시작 (터치 가능!)
        SetState(GameState.Playing);
        Debug.Log("[GameManager] Game started - Playing state");
        
        // BGM 재생
        AudioManager.Instance?.PlayBGM();
    }

    private void Update()
    {
        if (State != GameState.Playing) return;

        TimeRemaining -= Time.deltaTime;
        OnTimerTick?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            OnTimerTick?.Invoke(0f);
            EndGame();
        }
    }

    /// <summary>
    /// 게임을 종료합니다.
    /// </summary>
    public void EndGame()
    {
        if (State == GameState.GameOver) return;
        
        // BGM 정지
        AudioManager.Instance?.StopBGM();
        
        // 점수를 확정 시점에 즉시 캡처 (이후 변동 방지)
        if (ScoreManager.Instance != null && UIManager.Instance != null)
        {
            UIManager.Instance.CaptureSessionScore(ScoreManager.Instance.Score, ScoreManager.Instance.MaxCombo);
        }

        SetState(GameState.GameOver);
    }

    /// <summary>
    /// 메인 메뉴로 돌아갑니다.
    /// </summary>
    public void ReturnToMenu()
    {
        StopAllCoroutines();
        GridManager.Instance?.ClearGrid();
        
        // BGM 및 모든 효과음 정지
        AudioManager.Instance?.StopBGM();
        AudioManager.Instance?.StopAllSFX();
        
        SetState(GameState.Menu);
        Debug.Log("[GameManager] Returned to menu, BGM stopped");
    }

    /// <summary>
    /// 게임을 재시작합니다.
    /// </summary>
    public void RestartGame()
    {
        StopAllCoroutines();
        
        // 기존 BGM과 효과음 정지
        AudioManager.Instance?.StopBGM();
        AudioManager.Instance?.StopAllSFX();
        
        StartGame();
    }

    /// <summary>
    /// 힌트를 사용합니다.
    /// </summary>
    public bool UseHint()
    {
        Debug.Log($"[GameManager] UseHint called - HintsLeft: {HintsLeft}, State: {State}");
        
        if (HintsLeft <= 0 || State != GameState.Playing) return false;
        HintsLeft--;
        OnHintsChanged?.Invoke(HintsLeft);
        Debug.Log($"[GameManager] Showing hint for target sum: {TargetSum}");
        GridManager.Instance?.ShowHint(TargetSum);
        return true;
    }

    /// <summary>
    /// 셔플을 사용합니다.
    /// </summary>
    public bool UseShuffle()
    {
        if (ShufflesLeft <= 0 || State != GameState.Playing) return false;
        ShufflesLeft--;
        OnShufflesChanged?.Invoke(ShufflesLeft);
        GridManager.Instance?.ShuffleApples();
        return true;
    }

    private void SetState(GameState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }
}
