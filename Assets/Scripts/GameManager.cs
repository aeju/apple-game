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

        OnHintsChanged?.Invoke(HintsLeft);
        OnShufflesChanged?.Invoke(ShufflesLeft);

        // 카운트다운
        SetState(GameState.Countdown);

        for (int i = 3; i >= 1; i--)
        {
            OnCountdownTick?.Invoke(i);
            yield return new WaitForSeconds(1f);
        }

        OnCountdownTick?.Invoke(0); // GO!
        yield return new WaitForSeconds(0.5f);

        // 게임 시작
        SetState(GameState.Playing);
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
        SetState(GameState.GameOver);
        ScoreManager.Instance?.SaveBestScore();
    }

    /// <summary>
    /// 메인 메뉴로 돌아갑니다.
    /// </summary>
    public void ReturnToMenu()
    {
        StopAllCoroutines();
        GridManager.Instance?.ClearGrid();
        SetState(GameState.Menu);
    }

    /// <summary>
    /// 게임을 재시작합니다.
    /// </summary>
    public void RestartGame()
    {
        StopAllCoroutines();
        StartGame();
    }

    /// <summary>
    /// 힌트를 사용합니다.
    /// </summary>
    public bool UseHint()
    {
        if (HintsLeft <= 0 || State != GameState.Playing) return false;
        HintsLeft--;
        OnHintsChanged?.Invoke(HintsLeft);
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
