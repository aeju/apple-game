using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 고양이 테마 UI를 프로그래밍 방식으로 생성하고 관리합니다.
/// 검정 고양이(노란 눈) 헌정 게임 - PAW 10
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
    private TextMeshProUGUI _countdownCatFace;

    // 결과
    private TextMeshProUGUI _resultScoreText;
    private TextMeshProUGUI _resultApplesText;
    private TextMeshProUGUI _resultBestText;
    private TextMeshProUGUI _resultComboText;
    private TextMeshProUGUI _resultNewRecordText;
    private TextMeshProUGUI _resultCatFace;

    // 메인메뉴
    private TextMeshProUGUI _menuBestScoreText;

    // ─── 고양이 테마 색상 팔레트 ───
    private static readonly Color CAT_BLACK = new Color(0.08f, 0.08f, 0.10f, 0.94f);
    private static readonly Color CAT_EYE_GOLD = new Color(0.95f, 0.78f, 0.10f);
    private static readonly Color CAT_EYE_BRIGHT = new Color(1.0f, 0.85f, 0.20f);
    private static readonly Color BTN_PRIMARY = new Color(0.90f, 0.72f, 0.12f);
    private static readonly Color BTN_SECONDARY = new Color(0.30f, 0.30f, 0.35f);
    private static readonly Color BTN_SUCCESS = new Color(0.40f, 0.65f, 0.45f);
    private static readonly Color HUD_BAR = new Color(0.06f, 0.06f, 0.08f, 0.88f);

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
        ShowPanel(GameState.Menu);
    }

    private void Start()
    {
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

    // ─── 메인 메뉴 (고양이 마스코트) ───

    private void CreateMainMenuPanel()
    {
        _mainMenuPanel = CreatePanel("MainMenuPanel", CAT_BLACK);
        var rt = _mainMenuPanel.GetComponent<RectTransform>();
        SetFullStretch(rt);

        // 고양이 실루엣 마스코트 (노란 눈의 검정 고양이)
        CreateCatSilhouette(_mainMenuPanel.transform, new Vector2(0, 300), new Vector2(200, 200));

        // 타이틀
        var title = CreateTextElement(_mainMenuPanel.transform, "PAW 10",
            72, CAT_EYE_GOLD, new Vector2(0, 150), new Vector2(800, 100));
        title.fontStyle = FontStyles.Bold;

        // 부제
        CreateTextElement(_mainMenuPanel.transform, "Drag paws that sum to 10!",
            22, new Color(1, 1, 1, 0.6f), new Vector2(0, 80), new Vector2(700, 50));

        // 고양이 장식 라인
        CreateTextElement(_mainMenuPanel.transform, "~  =^._.^=  ~",
            28, new Color(1, 1, 1, 0.3f), new Vector2(0, 30), new Vector2(400, 50));

        // Play 버튼
        CreateButton(_mainMenuPanel.transform, "PLAY", BTN_PRIMARY,
            new Vector2(0, -50), new Vector2(320, 70), 36, () =>
            {
                GameManager.Instance?.StartGame();
            });

        // 최고기록
        _menuBestScoreText = CreateTextElement(_mainMenuPanel.transform, "Best: 0",
            24, new Color(0.95f, 0.78f, 0.10f, 0.7f), new Vector2(0, -140), new Vector2(400, 50));
    }

    private void UpdateMenuBestScore()
    {
        if (_menuBestScoreText != null && ScoreManager.Instance != null)
            _menuBestScoreText.text = $"Best: {ScoreManager.Instance.BestScore}";
    }

    // ─── HUD (고양이 눈 색상 강조) ───

    private void CreateHUDPanel()
    {
        _hudPanel = CreatePanel("HUDPanel", Color.clear);
        var rt = _hudPanel.GetComponent<RectTransform>();
        SetFullStretch(rt);

        // 상단 바 배경
        var topBar = CreatePanel("TopBar", HUD_BAR, _hudPanel.transform);
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
            18, new Color(0.95f, 0.78f, 0.10f, 0.6f), new Vector2(-300, -42), new Vector2(200, 30));
        scoreLabel.alignment = TextAlignmentOptions.Left;

        // 타이머 (중앙)
        _timerText = CreateTextElement(topBar.transform, "2:00",
            42, Color.white, new Vector2(0, -10), new Vector2(200, 60));
        _timerText.fontStyle = FontStyles.Bold;
        _timerText.alignment = TextAlignmentOptions.Center;

        var timerLabel = CreateTextElement(topBar.transform, "TIME",
            18, new Color(0.95f, 0.78f, 0.10f, 0.6f), new Vector2(0, -42), new Vector2(200, 30));
        timerLabel.alignment = TextAlignmentOptions.Center;

        // 콤보 (우측)
        _comboText = CreateTextElement(topBar.transform, "",
            32, CAT_EYE_GOLD, new Vector2(300, -20), new Vector2(250, 60));
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

        // 30초 이하면 경고 (고양이 눈 색 깜빡임)
        if (time <= 30f)
            _timerText.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), CAT_EYE_GOLD, Mathf.PingPong(Time.time * 2f, 1f));
        else
            _timerText.color = Color.white;
    }

    private void UpdateCombo(int combo, int maxCombo)
    {
        if (_comboText == null) return;

        if (combo > 1)
        {
            if (combo >= 5)
                _comboText.text = $"x{combo} MEOW!!";
            else if (combo >= 3)
                _comboText.text = $"x{combo} NYA~!";
            else
                _comboText.text = $"x{combo} COMBO!";

            _comboText.color = Color.Lerp(CAT_EYE_GOLD, CAT_EYE_BRIGHT, Mathf.PingPong(Time.time * 3f, 0.5f));
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

    // ─── 카운트다운 (고양이 표정) ───

    private void CreateCountdownPanel()
    {
        _countdownPanel = CreatePanel("CountdownPanel", Color.clear);
        SetFullStretch(_countdownPanel.GetComponent<RectTransform>());

        // 고양이 표정 (숫자 위에 표시)
        _countdownCatFace = CreateTextElement(_countdownPanel.transform, "",
            52, CAT_EYE_GOLD, new Vector2(0, 80), new Vector2(500, 80));
        _countdownCatFace.alignment = TextAlignmentOptions.Center;

        // 카운트다운 숫자
        _countdownText = CreateTextElement(_countdownPanel.transform, "3",
            120, Color.white, new Vector2(0, -30), new Vector2(300, 200));
        _countdownText.fontStyle = FontStyles.Bold;
        _countdownText.alignment = TextAlignmentOptions.Center;

        _countdownPanel.SetActive(false);
    }

    private void UpdateCountdown(int value)
    {
        if (_countdownText == null) return;

        switch (value)
        {
            case 3:
                _countdownCatFace.text = "=u.u=";
                _countdownText.text = "3";
                _countdownText.color = new Color(1f, 1f, 1f, 0.6f);
                break;
            case 2:
                _countdownCatFace.text = "=-.o=";
                _countdownText.text = "2";
                _countdownText.color = new Color(1f, 1f, 1f, 0.8f);
                break;
            case 1:
                _countdownCatFace.text = "=^o^=";
                _countdownText.text = "1";
                _countdownText.color = CAT_EYE_GOLD;
                break;
            default:
                _countdownCatFace.text = "=^*_*^=";
                _countdownText.text = "GO!";
                _countdownText.color = CAT_EYE_GOLD;
                break;
        }

        StartCoroutine(CoCountdownPop());
    }

    private IEnumerator CoCountdownPop()
    {
        var rt = _countdownText.rectTransform;
        var catRt = _countdownCatFace.rectTransform;
        Vector3 bigScale = Vector3.one * 1.5f;
        Vector3 normalScale = Vector3.one;

        rt.localScale = bigScale;
        catRt.localScale = bigScale;

        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            float p = t / 0.3f;
            Vector3 s = Vector3.Lerp(bigScale, normalScale, p);
            rt.localScale = s;
            catRt.localScale = s;
            yield return null;
        }

        rt.localScale = normalScale;
        catRt.localScale = normalScale;
    }

    // ─── 결과 화면 (점수별 고양이 표정) ───

    private void CreateResultPanel()
    {
        _resultPanel = CreatePanel("ResultPanel", CAT_BLACK);
        SetFullStretch(_resultPanel.GetComponent<RectTransform>());

        // 고양이 표정 (점수 기반으로 변경됨)
        _resultCatFace = CreateTextElement(_resultPanel.transform, "",
            52, CAT_EYE_GOLD, new Vector2(0, 320), new Vector2(600, 80));
        _resultCatFace.fontStyle = FontStyles.Bold;

        // 타이틀
        var title = CreateTextElement(_resultPanel.transform, "GAME OVER!",
            48, CAT_EYE_GOLD, new Vector2(0, 250), new Vector2(600, 80));
        title.fontStyle = FontStyles.Bold;

        // 점수
        _resultScoreText = CreateTextElement(_resultPanel.transform, "Score: 0",
            40, Color.white, new Vector2(0, 165), new Vector2(500, 60));
        _resultScoreText.fontStyle = FontStyles.Bold;

        // 제거한 발바닥
        _resultApplesText = CreateTextElement(_resultPanel.transform, "Paws: 0",
            28, new Color(1, 1, 1, 0.8f), new Vector2(0, 105), new Vector2(500, 45));

        // 최대 콤보
        _resultComboText = CreateTextElement(_resultPanel.transform, "Max Combo: 0",
            28, new Color(1, 1, 1, 0.8f), new Vector2(0, 60), new Vector2(500, 45));

        // 최고기록
        _resultBestText = CreateTextElement(_resultPanel.transform, "Best: 0",
            28, CAT_EYE_GOLD, new Vector2(0, 15), new Vector2(500, 45));

        // 신기록!
        _resultNewRecordText = CreateTextElement(_resultPanel.transform, "PURRFECT RECORD!",
            36, CAT_EYE_BRIGHT, new Vector2(0, -40), new Vector2(500, 50));
        _resultNewRecordText.fontStyle = FontStyles.Bold;

        // 다시하기 버튼
        CreateButton(_resultPanel.transform, "RETRY", BTN_PRIMARY,
            new Vector2(-100, -130), new Vector2(240, 65), 26, () =>
            {
                GameManager.Instance?.RestartGame();
            });

        // 메뉴 버튼
        CreateButton(_resultPanel.transform, "MENU", BTN_SECONDARY,
            new Vector2(100, -130), new Vector2(240, 65), 26, () =>
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
        _resultApplesText.text = $"Paws: {sm.ApplesRemoved}";
        _resultComboText.text = $"Max Combo: {sm.MaxCombo}";
        _resultBestText.text = $"Best: {sm.BestScore}";

        // 점수별 고양이 표정
        if (sm.Score <= 30)
            _resultCatFace.text = "=_=   Meh...";
        else if (sm.Score <= 80)
            _resultCatFace.text = "=^.^=   Purr~";
        else if (sm.Score <= 150)
            _resultCatFace.text = "=^o^=   Nya~!";
        else
            _resultCatFace.text = "=^*o*^=   MEOW!!";

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

            float glow = 0.5f + 0.5f * Mathf.Sin(Time.time * 3f);
            _resultNewRecordText.color = Color.Lerp(CAT_EYE_GOLD, CAT_EYE_BRIGHT, glow);

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
        tmp.color = CAT_EYE_GOLD;
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
            tmp.color = new Color(CAT_EYE_GOLD.r, CAT_EYE_GOLD.g, CAT_EYE_GOLD.b, alpha);
            go.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        Destroy(go);
    }

    // ─── 고양이 실루엣 마스코트 생성 ───

    private void CreateCatSilhouette(Transform parent, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject("CatSilhouette");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.sprite = GenerateCatHeadSprite(256);
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    /// <summary>
    /// 검정 고양이 머리 실루엣을 프로그래밍 방식으로 생성합니다.
    /// 노란 눈, 삼각 귀, 분홍 코를 포함합니다.
    /// </summary>
    private static Sprite GenerateCatHeadSprite(int texSize)
    {
        var tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        float s = texSize;
        float cx = s * 0.5f;

        Color catBlack = new Color(0.12f, 0.12f, 0.14f);
        Color earInner = new Color(0.22f, 0.18f, 0.20f);
        Color eyeGold = new Color(0.95f, 0.78f, 0.10f);
        Color nosePink = new Color(0.72f, 0.48f, 0.48f);

        for (int y = 0; y < texSize; y++)
            for (int x = 0; x < texSize; x++)
                tex.SetPixel(x, y, Color.clear);

        for (int py = 0; py < texSize; py++)
        {
            for (int px = 0; px < texSize; px++)
            {
                float fx = px;
                float fy = py;

                bool inHead = false;
                bool inInnerEar = false;
                bool inEye = false;
                bool inNose = false;

                float headCy = s * 0.38f;
                float headR = s * 0.36f;
                float headDist = Vector2.Distance(new Vector2(fx, fy), new Vector2(cx, headCy));

                // 머리 원형
                if (headDist <= headR) inHead = true;

                // 왼쪽 귀 (외곽)
                if (IsInTriangle(fx, fy,
                    s * 0.14f, s * 0.60f, s * 0.06f, s * 0.96f, s * 0.42f, s * 0.70f))
                    inHead = true;
                // 왼쪽 귀 (내부 - 안쪽 살색)
                if (IsInTriangle(fx, fy,
                    s * 0.18f, s * 0.64f, s * 0.12f, s * 0.88f, s * 0.36f, s * 0.70f))
                    inInnerEar = true;

                // 오른쪽 귀 (외곽)
                if (IsInTriangle(fx, fy,
                    s * 0.86f, s * 0.60f, s * 0.94f, s * 0.96f, s * 0.58f, s * 0.70f))
                    inHead = true;
                // 오른쪽 귀 (내부 - 안쪽 살색)
                if (IsInTriangle(fx, fy,
                    s * 0.82f, s * 0.64f, s * 0.88f, s * 0.88f, s * 0.64f, s * 0.70f))
                    inInnerEar = true;

                // 왼쪽 눈 (타원형 - 황금색)
                {
                    float ecx = s * 0.35f, ecy = s * 0.42f;
                    float erx = s * 0.07f, ery = s * 0.05f;
                    float dx = (fx - ecx) / erx;
                    float dy = (fy - ecy) / ery;
                    if (dx * dx + dy * dy <= 1f) inEye = true;
                }

                // 오른쪽 눈 (타원형 - 황금색)
                {
                    float ecx = s * 0.65f, ecy = s * 0.42f;
                    float erx = s * 0.07f, ery = s * 0.05f;
                    float dx = (fx - ecx) / erx;
                    float dy = (fy - ecy) / ery;
                    if (dx * dx + dy * dy <= 1f) inEye = true;
                }

                // 코 (작은 역삼각형 - 분홍색)
                if (IsInTriangle(fx, fy,
                    cx, s * 0.33f, cx - s * 0.025f, s * 0.37f, cx + s * 0.025f, s * 0.37f))
                    inNose = true;

                // 렌더링 (우선순위: 눈 > 코 > 귀 내부 > 머리)
                if (inEye)
                    tex.SetPixel(px, py, eyeGold);
                else if (inNose && inHead)
                    tex.SetPixel(px, py, nosePink);
                else if (inInnerEar && inHead)
                    tex.SetPixel(px, py, earInner);
                else if (inHead)
                    tex.SetPixel(px, py, catBlack);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), Vector2.one * 0.5f);
    }

    /// <summary>
    /// 삼각형 내부 판정 (바리센트릭 좌표)
    /// </summary>
    private static bool IsInTriangle(float px, float py,
        float x1, float y1, float x2, float y2, float x3, float y3)
    {
        float d1 = (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);
        float d2 = (px - x3) * (y2 - y3) - (x2 - x3) * (py - y3);
        float d3 = (px - x1) * (y3 - y1) - (x3 - x1) * (py - y1);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
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

        var img = go.AddComponent<Image>();
        img.color = bgColor;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.2f);
        colors.pressedColor = Color.Lerp(bgColor, Color.black, 0.2f);
        colors.disabledColor = Color.Lerp(bgColor, Color.gray, 0.5f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

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
