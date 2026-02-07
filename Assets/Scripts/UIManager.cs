using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 모든 UI를 프로그래밍 방식으로 생성하고 관리합니다.
/// 메인메뉴, HUD, 카운트다운, 결과 패널을 포함합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // 패널
    private GameObject _mainMenuPanel;
    private GameObject _hudPanel;
    private GameObject _countdownPanel;
    private GameObject _resultPanel;

    // HUD 요소
    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _timerText;
    private TextMeshProUGUI _comboText;
    private TextMeshProUGUI _hintButtonText;
    private TextMeshProUGUI _shuffleButtonText;
    private Button _hintButton;
    private Button _shuffleButton;

    // 카운트다운
    private TextMeshProUGUI _countdownText;

    // 결과
    private TextMeshProUGUI _resultScoreText;
    private TextMeshProUGUI _resultApplesText;
    private TextMeshProUGUI _resultBestText;
    private TextMeshProUGUI _resultComboText;
    private TextMeshProUGUI _resultNewRecordText;

    // 메인메뉴
    private TextMeshProUGUI _menuBestScoreText;

    // 색상
    private static readonly Color BG_DARK = new Color(0.12f, 0.14f, 0.18f, 0.92f);
    private static readonly Color BTN_PRIMARY = new Color(0.93f, 0.28f, 0.22f);
    private static readonly Color BTN_SECONDARY = new Color(0.20f, 0.60f, 0.86f);
    private static readonly Color BTN_SUCCESS = new Color(0.18f, 0.75f, 0.42f);
    private static readonly Color ACCENT_YELLOW = new Color(0.96f, 0.80f, 0.25f);

    private Canvas _canvas;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    /// <summary>
    /// UI를 초기화합니다. AppleGameSetup에서 호출됩니다.
    /// </summary>
    public void Initialize(Canvas canvas)
    {
        _canvas = canvas;

        CreateMainMenuPanel();
        CreateHUDPanel();
        CreateCountdownPanel();
        CreateResultPanel();
        // 초기 상태: 메인메뉴만 표시
        ShowPanel(GameState.Menu);
    }

    private void Start()
    {
        // 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged.AddListener(ShowPanel);
            GameManager.Instance.OnTimerTick.AddListener(UpdateTimer);
            GameManager.Instance.OnCountdownTick.AddListener(UpdateCountdown);
            GameManager.Instance.OnHintsChanged.AddListener(UpdateHintButton);
            GameManager.Instance.OnShufflesChanged.AddListener(UpdateShuffleButton);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.AddListener(UpdateScore);
            ScoreManager.Instance.OnComboChanged.AddListener(UpdateCombo);
            ScoreManager.Instance.OnScorePopup.AddListener(SpawnFloatingScore);
        }
    }

    // ─── 패널 전환 ───

    private void ShowPanel(GameState state)
    {
        _mainMenuPanel.SetActive(state == GameState.Menu);
        _hudPanel.SetActive(state == GameState.Playing || state == GameState.Countdown);
        _countdownPanel.SetActive(state == GameState.Countdown);
        _resultPanel.SetActive(state == GameState.GameOver);

        if (state == GameState.Menu)
            UpdateMenuBestScore();

        if (state == GameState.GameOver)
            PopulateResult();
    }

    // ─── 메인 메뉴 ───

    private void CreateMainMenuPanel()
    {
        _mainMenuPanel = CreatePanel("MainMenuPanel", BG_DARK);
        var rt = _mainMenuPanel.GetComponent<RectTransform>();
        SetFullStretch(rt);

        // 타이틀
        var title = CreateTextElement(_mainMenuPanel.transform, "Apple Game",
            64, ACCENT_YELLOW, new Vector2(0, 200), new Vector2(800, 100));
        title.fontStyle = FontStyles.Bold;

        // 부제
        CreateTextElement(_mainMenuPanel.transform, "Drag to select apples that sum to 10!",
            24, Color.white, new Vector2(0, 120), new Vector2(700, 50));

        // Play 버튼
        CreateButton(_mainMenuPanel.transform, "PLAY", BTN_PRIMARY,
            new Vector2(0, -20), new Vector2(320, 70), 36, () =>
            {
                GameManager.Instance?.StartGame();
            });

        // 최고기록
        _menuBestScoreText = CreateTextElement(_mainMenuPanel.transform, "Best: 0",
            26, new Color(1, 1, 1, 0.7f), new Vector2(0, -120), new Vector2(400, 50));
    }

    private void UpdateMenuBestScore()
    {
        if (_menuBestScoreText != null && ScoreManager.Instance != null)
            _menuBestScoreText.text = $"Best: {ScoreManager.Instance.BestScore}";
    }

    // ─── HUD ───

    private void CreateHUDPanel()
    {
        _hudPanel = CreatePanel("HUDPanel", Color.clear);
        var rt = _hudPanel.GetComponent<RectTransform>();
        SetFullStretch(rt);

        // 상단 바 배경
        var topBar = CreatePanel("TopBar", new Color(0.1f, 0.12f, 0.16f, 0.85f), _hudPanel.transform);
        var topRt = topBar.GetComponent<RectTransform>();
        topRt.anchorMin = new Vector2(0, 1);
        topRt.anchorMax = new Vector2(1, 1);
        topRt.pivot = new Vector2(0.5f, 1);
        topRt.sizeDelta = new Vector2(0, 90);
        topRt.anchoredPosition = Vector2.zero;

        // 점수 (좌측)
        _scoreText = CreateTextElement(topBar.transform, "0",
            36, Color.white, new Vector2(-300, -10), new Vector2(200, 60));
        _scoreText.alignment = TextAlignmentOptions.Left;
        _scoreText.fontStyle = FontStyles.Bold;

        var scoreLabel = CreateTextElement(topBar.transform, "SCORE",
            18, new Color(1, 1, 1, 0.6f), new Vector2(-300, -42), new Vector2(200, 30));
        scoreLabel.alignment = TextAlignmentOptions.Left;

        // 타이머 (중앙)
        _timerText = CreateTextElement(topBar.transform, "2:00",
            42, Color.white, new Vector2(0, -10), new Vector2(200, 60));
        _timerText.fontStyle = FontStyles.Bold;
        _timerText.alignment = TextAlignmentOptions.Center;

        var timerLabel = CreateTextElement(topBar.transform, "TIME",
            18, new Color(1, 1, 1, 0.6f), new Vector2(0, -42), new Vector2(200, 30));
        timerLabel.alignment = TextAlignmentOptions.Center;

        // 콤보 (우측)
        _comboText = CreateTextElement(topBar.transform, "",
            32, ACCENT_YELLOW, new Vector2(300, -20), new Vector2(200, 60));
        _comboText.alignment = TextAlignmentOptions.Right;

        // 하단 버튼 바
        var bottomBar = CreatePanel("BottomBar", Color.clear, _hudPanel.transform);
        var botRt = bottomBar.GetComponent<RectTransform>();
        botRt.anchorMin = new Vector2(0, 0);
        botRt.anchorMax = new Vector2(1, 0);
        botRt.pivot = new Vector2(0.5f, 0);
        botRt.sizeDelta = new Vector2(0, 80);
        botRt.anchoredPosition = new Vector2(0, 10);

        // 힌트 버튼 (좌측)
        var hintBtnGo = CreateButton(bottomBar.transform, "HINT (3)", BTN_SECONDARY,
            new Vector2(-120, 0), new Vector2(180, 55), 22, () =>
            {
                GameManager.Instance?.UseHint();
            });
        _hintButton = hintBtnGo.GetComponent<Button>();
        _hintButtonText = hintBtnGo.GetComponentInChildren<TextMeshProUGUI>();

        // 셔플 버튼 (우측)
        var shuffleBtnGo = CreateButton(bottomBar.transform, "SHUFFLE (2)", BTN_SUCCESS,
            new Vector2(120, 0), new Vector2(180, 55), 22, () =>
            {
                GameManager.Instance?.UseShuffle();
            });
        _shuffleButton = shuffleBtnGo.GetComponent<Button>();
        _shuffleButtonText = shuffleBtnGo.GetComponentInChildren<TextMeshProUGUI>();

        _hudPanel.SetActive(false);
    }

    private void UpdateScore(int score)
    {
        if (_scoreText != null)
            _scoreText.text = score.ToString();
    }

    private void UpdateTimer(float time)
    {
        if (_timerText == null) return;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        _timerText.text = $"{minutes}:{seconds:D2}";

        // 30초 이하면 빨간색 경고
        if (time <= 30f)
            _timerText.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), Color.white, Mathf.PingPong(Time.time * 2f, 1f));
        else
            _timerText.color = Color.white;
    }

    private void UpdateCombo(int combo, int maxCombo)
    {
        if (_comboText == null) return;

        if (combo > 1)
        {
            _comboText.text = $"{combo} COMBO!";
            _comboText.color = Color.Lerp(ACCENT_YELLOW, Color.white, Mathf.PingPong(Time.time * 3f, 0.3f));
        }
        else
        {
            _comboText.text = "";
        }
    }

    private void UpdateHintButton(int remaining)
    {
        if (_hintButtonText != null)
            _hintButtonText.text = $"HINT ({remaining})";
        if (_hintButton != null)
            _hintButton.interactable = remaining > 0;
    }

    private void UpdateShuffleButton(int remaining)
    {
        if (_shuffleButtonText != null)
            _shuffleButtonText.text = $"SHUFFLE ({remaining})";
        if (_shuffleButton != null)
            _shuffleButton.interactable = remaining > 0;
    }

    // ─── 카운트다운 ───

    private void CreateCountdownPanel()
    {
        _countdownPanel = CreatePanel("CountdownPanel", Color.clear);
        SetFullStretch(_countdownPanel.GetComponent<RectTransform>());

        _countdownText = CreateTextElement(_countdownPanel.transform, "3",
            120, Color.white, Vector2.zero, new Vector2(300, 200));
        _countdownText.fontStyle = FontStyles.Bold;
        _countdownText.alignment = TextAlignmentOptions.Center;

        _countdownPanel.SetActive(false);
    }

    private void UpdateCountdown(int value)
    {
        if (_countdownText == null) return;

        if (value > 0)
        {
            _countdownText.text = value.ToString();
            _countdownText.color = Color.white;
        }
        else
        {
            _countdownText.text = "GO!";
            _countdownText.color = BTN_SUCCESS;
        }

        // 팝 애니메이션
        StartCoroutine(CoCountdownPop());
    }

    private IEnumerator CoCountdownPop()
    {
        var rt = _countdownText.rectTransform;
        Vector3 bigScale = Vector3.one * 1.5f;
        Vector3 normalScale = Vector3.one;

        rt.localScale = bigScale;

        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            float p = t / 0.3f;
            rt.localScale = Vector3.Lerp(bigScale, normalScale, p);
            yield return null;
        }

        rt.localScale = normalScale;
    }

    // ─── 결과 화면 ───

    private void CreateResultPanel()
    {
        _resultPanel = CreatePanel("ResultPanel", BG_DARK);
        SetFullStretch(_resultPanel.GetComponent<RectTransform>());

        // 타이틀
        var title = CreateTextElement(_resultPanel.transform, "GAME OVER!",
            56, ACCENT_YELLOW, new Vector2(0, 250), new Vector2(600, 80));
        title.fontStyle = FontStyles.Bold;

        // 점수
        _resultScoreText = CreateTextElement(_resultPanel.transform, "Score: 0",
            40, Color.white, new Vector2(0, 150), new Vector2(500, 60));
        _resultScoreText.fontStyle = FontStyles.Bold;

        // 제거한 사과
        _resultApplesText = CreateTextElement(_resultPanel.transform, "Apples: 0",
            28, new Color(1, 1, 1, 0.8f), new Vector2(0, 85), new Vector2(500, 45));

        // 최대 콤보
        _resultComboText = CreateTextElement(_resultPanel.transform, "Max Combo: 0",
            28, new Color(1, 1, 1, 0.8f), new Vector2(0, 40), new Vector2(500, 45));

        // 최고기록
        _resultBestText = CreateTextElement(_resultPanel.transform, "Best: 0",
            28, ACCENT_YELLOW, new Vector2(0, -10), new Vector2(500, 45));

        // 신기록!
        _resultNewRecordText = CreateTextElement(_resultPanel.transform, "NEW RECORD!",
            36, new Color(1f, 0.85f, 0f), new Vector2(0, -70), new Vector2(500, 50));
        _resultNewRecordText.fontStyle = FontStyles.Bold;

        // 다시하기 버튼
        CreateButton(_resultPanel.transform, "RETRY", BTN_PRIMARY,
            new Vector2(-100, -160), new Vector2(240, 65), 26, () =>
            {
                GameManager.Instance?.RestartGame();
            });

        // 메뉴 버튼
        CreateButton(_resultPanel.transform, "MENU", BTN_SECONDARY,
            new Vector2(100, -160), new Vector2(240, 65), 26, () =>
            {
                GameManager.Instance?.ReturnToMenu();
            });

        _resultPanel.SetActive(false);
    }

    private void PopulateResult()
    {
        var sm = ScoreManager.Instance;
        if (sm == null) return;

        _resultScoreText.text = $"Score: {sm.Score}";
        _resultApplesText.text = $"Apples: {sm.ApplesRemoved}";
        _resultComboText.text = $"Max Combo: {sm.MaxCombo}";
        _resultBestText.text = $"Best: {sm.BestScore}";

        bool isNewRecord = sm.Score >= sm.BestScore && sm.Score > 0;
        _resultNewRecordText.gameObject.SetActive(isNewRecord);

        if (isNewRecord)
            StartCoroutine(CoPulseNewRecord());
    }

    private IEnumerator CoPulseNewRecord()
    {
        while (_resultPanel.activeSelf && _resultNewRecordText.gameObject.activeSelf)
        {
            float scale = 1f + 0.1f * Mathf.Sin(Time.time * 4f);
            _resultNewRecordText.rectTransform.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    // ─── 플로팅 점수 텍스트 ───

    private void SpawnFloatingScore(int points, Vector3 worldPos)
    {
        StartCoroutine(CoFloatingScore(points, worldPos));
    }

    private IEnumerator CoFloatingScore(int points, Vector3 worldPos)
    {
        var go = new GameObject("FloatingScore");
        go.transform.position = worldPos;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = $"+{points}";
        tmp.fontSize = 6;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = ACCENT_YELLOW;
        tmp.sortingOrder = 15;
        tmp.rectTransform.sizeDelta = new Vector2(3, 1);

        float dur = 1f;
        Vector3 startPos = worldPos;
        Vector3 endPos = worldPos + Vector3.up * 1.2f;

        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float p = t / dur;
            go.transform.position = Vector3.Lerp(startPos, endPos, p);

            float alpha = p < 0.5f ? 1f : 1f - (p - 0.5f) * 2f;
            float scale = p < 0.2f ? (0.8f + p * 1f) : 1f;
            tmp.color = new Color(ACCENT_YELLOW.r, ACCENT_YELLOW.g, ACCENT_YELLOW.b, alpha);
            go.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        Destroy(go);
    }

    // ─── UI 헬퍼 메서드 ───

    private GameObject CreatePanel(string name, Color bgColor, Transform parent = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent != null ? parent : _canvas.transform, false);

        var rt = go.AddComponent<RectTransform>();

        if (bgColor.a > 0.01f)
        {
            var img = go.AddComponent<Image>();
            img.color = bgColor;
        }

        return go;
    }

    private TextMeshProUGUI CreateTextElement(Transform parent, string text,
        float fontSize, Color color, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        return tmp;
    }

    private GameObject CreateButton(Transform parent, string label, Color bgColor,
        Vector2 anchoredPos, Vector2 sizeDelta, float fontSize, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Button");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        // 배경
        var img = go.AddComponent<Image>();
        img.color = bgColor;

        // 둥근 느낌을 위해 (기본 스프라이트 사용)
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1;

        // 버튼
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.2f);
        colors.pressedColor = Color.Lerp(bgColor, Color.black, 0.2f);
        colors.disabledColor = Color.Lerp(bgColor, Color.gray, 0.5f);
        btn.colors = colors;

        btn.onClick.AddListener(onClick);

        // 텍스트
        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);

        var textRt = textGo.AddComponent<RectTransform>();
        SetFullStretch(textRt);

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return go;
    }

    private void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
}
