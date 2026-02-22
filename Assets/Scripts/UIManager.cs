using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 고양이 테마 UI - 밝은/다크 모드 지원
/// 검정 고양이(노란 눈) 헌정 게임 - PAW 10
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Floating Score Settings")]
    [Tooltip("점수 팝업 텍스트에 사용할 TMP 폰트 (null이면 기본 폰트 사용)")]
    [SerializeField] private TMP_FontAsset floatingScoreFont = null;
    
    [Tooltip("점수 팝업 텍스트 크기 (기본값: 6)")]
    [SerializeField] private float floatingScoreFontSize = 6f;
    
    [Header("Menu Button Settings")]
    [Tooltip("메인 메뉴 버튼 텍스트에 사용할 TMP 폰트 (null이면 기본 폰트 사용)")]
    [SerializeField] private TMP_FontAsset menuButtonFont = null;
    
    [Tooltip("메인 메뉴 버튼 텍스트 크기 (기본값: 36)")]
    [SerializeField] private float menuButtonFontSize = 36f;
    
    [Header("Button Sound Settings")]
    [Tooltip("버튼 클릭 시 재생할 AudioClip (null이면 사운드 없음)")]
    [SerializeField] private AudioClip buttonClickSound = null;

    [Header("Main Menu - Title")]
    [SerializeField] private Vector2 titlePosition = new Vector2(0, 150);
    [SerializeField] private float titleFontSize = 72f;
    [SerializeField] private TMP_FontAsset titleFont = null;
    [SerializeField] private Color titleColor = new Color(0.95f, 0.78f, 0.10f);
    [SerializeField] private Vector2 subtitlePosition = new Vector2(0, 80);
    [SerializeField] private float subtitleFontSize = 22f;
    [SerializeField] private Vector2 decorationPosition = new Vector2(0, 30);

    [Header("Main Menu - Cat Logo")]
    [SerializeField] private Vector2 catLogoPosition = new Vector2(0, 300);
    [SerializeField] private Vector2 catLogoSize = new Vector2(200, 200);
    [Tooltip("고양이 로고 펄스 애니메이션 속도 (클수록 빠름)")]
    [SerializeField] private float catAnimationSpeed = 1.2f;
    [Tooltip("고양이 로고 펄스 애니메이션 크기 범위 (0.1 = 0.9배~1.1배)")]
    [SerializeField] private float catAnimationAmplitude = 0.1f;

    [Header("Main Menu - PLAY / RANKING")]
    [Tooltip("씬에 미리 배치한 PLAY 버튼 (null이면 자동 생성)")]
    [SerializeField] private Button serializedPlayButton = null;
    [SerializeField] private Vector2 playButtonPosition = new Vector2(0, -200);
    [SerializeField] private Vector2 playButtonSize = new Vector2(350, 80);
    [SerializeField] private Color playButtonColor = new Color(1.0f, 0.2f, 0.5f);
    [Tooltip("씬에 미리 배치한 RANKING 버튼 (null이면 자동 생성)")]
    [SerializeField] private Button serializedRankingButton = null;
    [SerializeField] private Vector2 rankingButtonPosition = new Vector2(0, -310);
    [SerializeField] private Vector2 rankingButtonSize = new Vector2(350, 80);
    [SerializeField] private Color rankingButtonColor = new Color(0.2f, 0.5f, 0.9f);

    [Header("Main Menu - Best Score")]
    [SerializeField] private Vector2 bestScorePosition = new Vector2(0, -190);
    [SerializeField] private float bestScoreFontSize = 24f;

    [Header("HUD - Texts")]
    [Tooltip("씬에 미리 배치한 SCORE 숫자 텍스트 (null이면 자동 생성)")]
    [SerializeField] private TextMeshProUGUI serializedScoreText = null;
    [Tooltip("씬에 미리 배치한 SCORE 라벨 텍스트 (null이면 자동 생성)")]
    [SerializeField] private TextMeshProUGUI serializedScoreLabelText = null;
    [Tooltip("씬에 미리 배치한 타이머 텍스트 (null이면 자동 생성)")]
    [SerializeField] private TextMeshProUGUI serializedTimerText = null;
    [Tooltip("씬에 미리 배치한 타이머 슬라이더 (null이면 자동 생성)")]
    [SerializeField] private Slider serializedTimerSlider = null;
    [Tooltip("씬에 미리 배치한 콤보 텍스트 (null이면 자동 생성)")]
    [SerializeField] private TextMeshProUGUI serializedComboText = null;

    [Header("HUD - BGM Control")]
    [Tooltip("씬에 미리 배치한 BGM 토글 (null이면 자동 생성)")]
    [SerializeField] private Toggle serializedBgmToggle = null;
    [Tooltip("씬에 미리 배치한 BGM 볼륨 슬라이더 (null이면 자동 생성)")]
    [SerializeField] private Slider serializedBgmSlider = null;
    [Tooltip("씬에 미리 배치한 BGM 볼륨 텍스트 (null이면 자동 생성)")]
    [SerializeField] private TextMeshProUGUI serializedBgmVolumeText = null;

    [Header("HUD - Buttons")]
    [Tooltip("씬에 미리 배치한 HINT 버튼 (null이면 자동 생성)")]
    [SerializeField] private Button serializedHintButton = null;
    [Tooltip("씬에 미리 배치한 SHUFFLE 버튼 (null이면 자동 생성)")]
    [SerializeField] private Button serializedShuffleButton = null;
    [Tooltip("씬에 미리 배치한 RESTART 버튼 (null이면 자동 생성)")]
    [SerializeField] private Button serializedRestartButton = null;
    [Tooltip("씬에 미리 배치한 HOME 버튼 (null이면 자동 생성)")]
    [SerializeField] private Button serializedHomeButton = null;

    [Header("Countdown")]
    [Tooltip("씬에 미리 배치한 카운트다운 숫자 텍스트 (null이면 자동 생성)")]
    [SerializeField] private TextMeshProUGUI serializedCountdownText = null;
    [Tooltip("씬에 미리 배치한 카운트다운 고양이 얼굴 텍스트 (null이면 자동 생성)")]
    [SerializeField] private TextMeshProUGUI serializedCountdownCatFace = null;
    [SerializeField] private TMP_FontAsset countdownNumberFont = null;
    [SerializeField] private float countdownNumberFontSize = 120f;
    [SerializeField] private TMP_FontAsset countdownCatFaceFont = null;
    [SerializeField] private float countdownCatFaceFontSize = 52f;

    [Header("Result - Buttons")]
    [Tooltip("씬에 미리 배치한 RETRY 버튼 (null이면 자동 생성)")]
    [SerializeField] private Button serializedRetryButton = null;
    [Tooltip("씬에 미리 배치한 MENU 버튼 (null이면 자동 생성)")]
    [SerializeField] private Button serializedMenuButton = null;
    [Tooltip("씬에 미리 배치한 결과화면 RANKING 버튼 (null이면 자동 생성)")]
    [SerializeField] private Button serializedResultRankingButton = null;

    [Header("Leaderboard - Background")]
    [Tooltip("리더보드 패널 배경색 (투명도 조절 가능)")]
    [SerializeField] private Color leaderboardPanelBgColor = Color.white;

    [Header("Leaderboard - Title")]
    [SerializeField] private string leaderboardTitleText = "🏆 LEADERBOARD 🏆";
    [SerializeField] private float leaderboardTitleFontSize = 48f;
    [SerializeField] private Color leaderboardTitleColor = new Color(0.85f, 0.65f, 0.13f);
    [SerializeField] private Vector2 leaderboardTitlePosition = new Vector2(0, 350);
    [SerializeField] private TMP_FontAsset leaderboardTitleFont = null;

    [Header("Leaderboard - Scroll Area")]
    [SerializeField] private Vector2 leaderboardScrollAreaSize = new Vector2(600, 550);
    [SerializeField] private Color leaderboardScrollBgColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Leaderboard - Entry Style")]
    [SerializeField] private float leaderboardEntryHeight = 68f;
    [SerializeField] private float leaderboardEntrySpacing = 75f;
    [SerializeField] private float leaderboardRankFontSize = 36f;
    [SerializeField] private float leaderboardNicknameFontSize = 33f;
    [SerializeField] private float leaderboardScoreFontSize = 36f;
    [SerializeField] private TMP_FontAsset leaderboardEntryFont = null;
    [Tooltip("1등 배경색")]
    [SerializeField] private Color leaderboardGoldColor = new Color(1f, 0.84f, 0f, 0.3f);
    [Tooltip("2등 배경색")]
    [SerializeField] private Color leaderboardSilverColor = new Color(0.75f, 0.75f, 0.75f, 0.3f);
    [Tooltip("3등 배경색")]
    [SerializeField] private Color leaderboardBronzeColor = new Color(0.8f, 0.5f, 0.2f, 0.3f);
    [Tooltip("4등 이하 배경색")]
    [SerializeField] private Color leaderboardDefaultColor = new Color(1f, 1f, 1f, 0.1f);
    [Tooltip("상위 3명 순위/점수 색상")]
    [SerializeField] private Color leaderboardTopRankColor = new Color(0.85f, 0.65f, 0.13f);
    [Tooltip("상위 3명 닉네임 색상")]
    [SerializeField] private Color leaderboardTopNicknameColor = Color.black;
    [Tooltip("4등 이하 텍스트 색상")]
    [SerializeField] private Color leaderboardNormalTextColor = Color.gray;

    [Header("Leaderboard - Empty Message")]
    [SerializeField] private string leaderboardEmptyMessage = "No records yet!\nPlay to set your first score!";
    [SerializeField] private float leaderboardEmptyFontSize = 28f;

    [Header("Leaderboard - Close Button")]
    [Tooltip("씬에 미리 배치한 CLOSE 버튼 (null이면 자동 생성)")]
    [SerializeField] private Button serializedCloseButton = null;
    [SerializeField] private Vector2 leaderboardCloseButtonPosition = new Vector2(0, -360);
    [SerializeField] private Vector2 leaderboardCloseButtonSize = new Vector2(280, 65);
    [SerializeField] private float leaderboardCloseButtonFontSize = 28f;

    // 패널
    private GameObject _mainMenuPanel;
    private GameObject _hudPanel;
    private GameObject _countdownPanel;
    private GameObject _resultPanel;

    // 테마별로 색상 적용할 요소들
    private List<Image> _panelBackgrounds = new List<Image>();
    private List<Image> _barBackgrounds = new List<Image>();
    private List<TextMeshProUGUI> _primaryTexts = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> _secondaryTexts = new List<TextMeshProUGUI>();
    private List<Button> _primaryButtons = new List<Button>();
    private List<Button> _secondaryButtons = new List<Button>();
    private List<Button> _successButtons = new List<Button>();

    // 메인 메뉴 요소
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _subtitleText;
    private TextMeshProUGUI _decorationText;
    
    [Header("고양이 로고 애니메이션")]
    [SerializeField] private GameObject catSilhouetteObject; // Inspector에서 할당 가능 (씬에 있는 CatSilhouette 오브젝트)
    private GameObject _catSilhouette; // 실제 참조 (자동 찾기 또는 할당)
    private bool _catSilhouetteAnimating = false; // 애니메이션 실행 중 플래그
    
    // HUD 요소
    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _scoreLabelText; // "SCORE" 라벨
    private TextMeshProUGUI _timerText;
    private Slider _timerSlider;
    private Image _timerSliderFill;
    private TextMeshProUGUI _comboText;
    private TextMeshProUGUI _hintButtonText;
    private TextMeshProUGUI _shuffleButtonText;
    private Button _hintButton;
    private Button _shuffleButton;
    
    // BGM 컨트롤
    private Toggle _bgmToggle;
    private Slider _bgmVolumeSlider;
    private TextMeshProUGUI _bgmVolumeText;

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
    private TMP_InputField _nicknameInputField;
    private Button _nicknameChangeButton;
    private string _currentNickname;

    // 메인메뉴
    private TextMeshProUGUI _menuBestScoreText;

    // 랭킹
    private GameObject _leaderboardPanel;
    private Transform _leaderboardEntriesContainer;
    private GameObject _previousActivePanel; // 리더보드 열기 전 활성화된 패널
    
    // 현재 게임 세션 점수 (결과 화면 떠날 때 제출)
    private int _currentSessionScore = 0;
    private int _currentSessionCombo = 0;
    private bool _sessionSubmitted = false;

    // 공통 색상
    private static readonly Color CAT_EYE_GOLD = new Color(0.95f, 0.78f, 0.10f);
    private static readonly Color CAT_EYE_BRIGHT = new Color(1.0f, 0.85f, 0.20f);
    private static readonly Color CAT_NOSE_PINK = new Color(1.0f, 0.7f, 0.8f);  // 핑크색 (재시작)
    private static readonly Color CAT_PAW_CREAM = new Color(0.95f, 0.85f, 0.7f); // 크림색 (홈)
    private static readonly Color LIGHT_PINK = new Color(1.0f, 0.85f, 0.9f);    // 연한 핑크 (SHUFFLE)
    private static readonly Color LIGHT_SKY = new Color(0.8f, 0.9f, 1.0f);      // 연한 하늘색 (HINT)
    private static readonly Color DEEP_PINK = new Color(1.0f, 0.2f, 0.5f);      // 진분홍 (PLAY)
    private static readonly Color DEEP_BLUE = new Color(0.2f, 0.5f, 0.9f);      // 진파랑 (RANKING)

    private Canvas _canvas;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        
        // 닉네임 초기화
        LoadNickname();
    }
    
    /// <summary>
    /// 저장된 닉네임을 불러오거나 Guest_XXXX 생성
    /// </summary>
    private void LoadNickname()
    {
        if (PlayerPrefs.HasKey("PlayerNickname"))
        {
            _currentNickname = PlayerPrefs.GetString("PlayerNickname");
            Debug.Log($"[UIManager] ✅ Nickname loaded from PlayerPrefs: '{_currentNickname}'");
        }
        else
        {
            // 랜덤 4자리 숫자로 Guest_XXXX 생성
            int randomNum = Random.Range(1000, 9999);
            _currentNickname = $"Guest_{randomNum}";
            PlayerPrefs.SetString("PlayerNickname", _currentNickname);
            PlayerPrefs.Save();
            Debug.Log($"[UIManager] ✅ New nickname generated and saved: '{_currentNickname}'");
        }
        
        // 로드 후 검증
        string verified = PlayerPrefs.GetString("PlayerNickname", "ERROR");
        Debug.Log($"[UIManager] 🔍 PlayerPrefs verification after load: '{verified}'");
    }
    
    /// <summary>
    /// 닉네임 저장
    /// </summary>
    private void SaveNickname(string nickname)
    {
        string oldNickname = _currentNickname;
        
        if (string.IsNullOrWhiteSpace(nickname))
        {
            nickname = $"Guest_{Random.Range(1000, 9999)}";
            Debug.LogWarning($"[UIManager] Empty nickname, generated new: {nickname}");
        }
        
        _currentNickname = nickname;
        PlayerPrefs.SetString("PlayerNickname", nickname);
        PlayerPrefs.Save();
        
        Debug.Log($"[UIManager] ✅ Nickname saved! '{oldNickname}' → '{_currentNickname}'");
        Debug.Log($"[UIManager] PlayerPrefs verification: {PlayerPrefs.GetString("PlayerNickname")}");
    }
    
    /// <summary>
    /// 현재 닉네임 반환
    /// </summary>
    public string GetNickname()
    {
        return _currentNickname;
    }

    public void Initialize(Canvas canvas)
    {
        _canvas = canvas;

        // 이미 패널이 생성되어 있는지 확인 (Edit 모드에서 생성된 경우)
        Transform existingMenu = canvas.transform.Find("MainMenuPanel");
        
        if (existingMenu != null && Application.isPlaying)
        {
            Debug.Log("[UIManager] Panels already exist. Re-referencing...");
            ReReferenceExistingPanels();
        }
        else
        {
            Debug.Log("[UIManager] Creating new UI panels...");
            CreateMainMenuPanel();
            CreateHUDPanel();
            CreateCountdownPanel();
            CreateResultPanel();
            CreateLeaderboardPanel();
        }

        Debug.Log("[UIManager] About to show Menu panel...");
        ShowPanel(GameState.Menu);

        Debug.Log("[UIManager] Initialize complete!");
    }

    /// <summary>
    /// Editor에서 개별 패널을 생성합니다. (Edit 모드 전용)
    /// </summary>
    public void EditorGeneratePanel(string panelName)
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas == null)
            {
                Debug.LogError("[UIManager] Canvas not found!");
                return;
            }
        }

        switch (panelName)
        {
            case "MainMenuPanel":
                var existing = _canvas.transform.Find("MainMenuPanel");
                if (existing != null) DestroyImmediate(existing.gameObject);
                CreateMainMenuPanel();
                _mainMenuPanel?.SetActive(true);
                break;
            case "HUDPanel":
                var existingHud = _canvas.transform.Find("HUDPanel");
                if (existingHud != null) DestroyImmediate(existingHud.gameObject);
                CreateHUDPanel();
                _hudPanel?.SetActive(true);
                if (_scoreText != null) _scoreText.text = "1,250";
                if (_timerText != null) _timerText.text = "1:30";
                break;
            case "CountdownPanel":
                var existingCd = _canvas.transform.Find("CountdownPanel");
                if (existingCd != null) DestroyImmediate(existingCd.gameObject);
                CreateCountdownPanel();
                _countdownPanel?.SetActive(true);
                break;
            case "ResultPanel":
                var existingRes = _canvas.transform.Find("ResultPanel");
                if (existingRes != null) DestroyImmediate(existingRes.gameObject);
                CreateResultPanel();
                _resultPanel?.SetActive(true);
                if (_resultScoreText != null) _resultScoreText.text = "1,250";
                if (_resultComboText != null) _resultComboText.text = "Best Combo: 8";
                if (_resultBestText != null) _resultBestText.text = "BEST: 2,500";
                if (_resultCatFace != null) _resultCatFace.text = "=^.^=";
                break;
            case "LeaderboardPanel":
                var existingLb = _canvas.transform.Find("LeaderboardPanel");
                if (existingLb != null) DestroyImmediate(existingLb.gameObject);
                CreateLeaderboardPanel();
                _leaderboardPanel?.SetActive(true);
                ShowPlaceholderLeaderboard();
                break;
        }

        Debug.Log($"[UIManager] Editor: {panelName} generated!");
    }

    /// <summary>
    /// Editor에서 특정 패널을 삭제합니다.
    /// </summary>
    public void EditorDeletePanel(string panelName)
    {
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas == null) return;
        }

        var panel = _canvas.transform.Find(panelName);
        if (panel != null)
        {
            DestroyImmediate(panel.gameObject);
            Debug.Log($"[UIManager] Editor: {panelName} deleted!");
        }
    }

    /// <summary>
    /// 이미 생성된 패널들을 다시 참조합니다 (Edit 모드에서 생성된 경우)
    /// </summary>
    private void ReReferenceExistingPanels()
    {
        // 패널 재참조
        _mainMenuPanel = _canvas.transform.Find("MainMenuPanel")?.gameObject;
        _hudPanel = _canvas.transform.Find("HUDPanel")?.gameObject;
        _countdownPanel = _canvas.transform.Find("CountdownPanel")?.gameObject;
        _resultPanel = _canvas.transform.Find("ResultPanel")?.gameObject;
        _leaderboardPanel = _canvas.transform.Find("LeaderboardPanel")?.gameObject;

        if (_mainMenuPanel == null || _hudPanel == null || _countdownPanel == null || _resultPanel == null)
        {
            Debug.LogError("[UIManager] Some panels not found! Re-creating all panels...");
            CreateMainMenuPanel();
            CreateHUDPanel();
            CreateCountdownPanel();
            CreateResultPanel();
            CreateLeaderboardPanel();
            return;
        }
        
        // 랭킹보드 패널이 없으면 생성
        if (_leaderboardPanel == null)
        {
            Debug.LogWarning("[UIManager] Leaderboard panel not found, creating...");
            CreateLeaderboardPanel();
        }
        else
        {
            Debug.Log("[UIManager] Leaderboard panel found, re-referencing container...");
            // 랭킹보드 엔트리 컨테이너 재참조
            var scrollArea = _leaderboardPanel.transform.Find("ScrollArea");
            if (scrollArea != null)
            {
                _leaderboardEntriesContainer = scrollArea.Find("Entries");
                Debug.Log($"[UIManager] Entries container found: {_leaderboardEntriesContainer != null}");
            }
            else
            {
                Debug.LogError("[UIManager] ScrollArea not found in leaderboard panel!");
            }
            
            // 컨테이너가 없으면 재생성
            if (_leaderboardEntriesContainer == null)
            {
                Debug.LogWarning("[UIManager] Entries container missing, recreating leaderboard panel...");
                Destroy(_leaderboardPanel);
                CreateLeaderboardPanel();
            }
            else
            {
                // CLOSE 버튼 재연결
                if (serializedCloseButton != null)
                {
                    serializedCloseButton.onClick.RemoveAllListeners();
                    serializedCloseButton.onClick.AddListener(() => { PlayButtonSound(); HideLeaderboard(); });
                    _primaryButtons.Add(serializedCloseButton);
                }
                else
                {
                    var leaderboardButtons = _leaderboardPanel.GetComponentsInChildren<Button>(true);
                    foreach (var btn in leaderboardButtons)
                    {
                        var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                        if (btnText != null && btnText.text == "CLOSE")
                        {
                            btn.onClick.RemoveAllListeners();
                            btn.onClick.AddListener(() =>
                            {
                                PlayButtonSound();
                                HideLeaderboard();
                            });
                            break;
                        }
                    }
                }
            }
        }

        // HUD 요소 재참조: 직렬화 우선
        _scoreText = serializedScoreText;
        _scoreLabelText = serializedScoreLabelText;
        _timerText = serializedTimerText;
        _comboText = serializedComboText;

        if (serializedTimerSlider != null)
        {
            _timerSlider = serializedTimerSlider;
            _timerSliderFill = _timerSlider.fillRect?.GetComponent<Image>();
        }

        // 직렬화 안 된 것들은 TopBar에서 찾기
        var topBar = _hudPanel.transform.Find("TopBar");
        if (topBar != null)
        {
            if (_scoreText == null || _timerText == null || _scoreLabelText == null || _comboText == null)
            {
                var texts = topBar.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var txt in texts)
                {
                    if (_scoreText == null && txt.name == "Text" && (txt.text == "0" || (int.TryParse(txt.text, out _) && !txt.text.Contains(":"))))
                    {
                        var rt = txt.GetComponent<RectTransform>();
                        if (rt != null && rt.anchoredPosition.x < -100)
                            _scoreText = txt;
                    }
                    else if (_timerText == null && txt.name == "Text" && txt.text.Contains(":"))
                    {
                        _timerText = txt;
                    }
                    else if (_scoreLabelText == null && txt.text.Contains("SCORE"))
                    {
                        _scoreLabelText = txt;
                    }
                }

                if (_comboText == null)
                {
                    var texts2 = topBar.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var txt in texts2)
                    {
                        var rt = txt.GetComponent<RectTransform>();
                        if (rt != null && rt.anchoredPosition.x > 200 && txt.text == "")
                        {
                            _comboText = txt;
                            break;
                        }
                    }
                }
            }

            if (_timerSlider == null)
            {
                var slider = topBar.GetComponentInChildren<Slider>(true);
                if (slider != null)
                {
                    _timerSlider = slider;
                    _timerSliderFill = slider.fillRect?.GetComponent<Image>();
                }
            }
        }

        Debug.Log($"[UIManager] HUD refs - Score:{_scoreText != null} Timer:{_timerText != null} Slider:{_timerSlider != null} Combo:{_comboText != null}");

        // 버튼 재참조 및 이벤트 재연결
        var buttons = _hudPanel.GetComponentsInChildren<Button>(true);
        bool hasRestartButton = false;
        bool hasHomeButton = false;
        bool hasHintButton = false;
        bool hasShuffleButton = false;
        Button hintBtnComponent = null;
        Button shuffleBtnComponent = null;
        
        foreach (var btn in buttons)
        {
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                if (label.text.Contains("HINT"))
                {
                    hasHintButton = true;
                    _hintButton = btn;
                    _hintButtonText = label;
                    hintBtnComponent = btn;
                    // 힌트 버튼 이벤트 재연결
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => 
                    {
                        PlayButtonSound();
                        Debug.Log("[UIManager] HINT button clicked!");
                        GameManager.Instance?.UseHint(); 
                    });
                    Debug.Log("[UIManager] HINT button event reconnected");
                    
                    // 버튼 색상 초기화 (연한 하늘색)
                    ResetButtonColor(btn, LIGHT_SKY);
                }
                else if (label.text.Contains("SHUFFLE"))
                {
                    hasShuffleButton = true;
                    _shuffleButton = btn;
                    _shuffleButtonText = label;
                    shuffleBtnComponent = btn;
                    // 셔플 버튼 이벤트 재연결
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.UseShuffle(); });
                    
                    // 버튼 색상 초기화 (연한 핑크)
                    ResetButtonColor(btn, LIGHT_PINK);
                }
                else if (label.text.Contains("RESTART"))
                {
                    hasRestartButton = true;
                    // 재시작 버튼 이벤트 재연결
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.RestartGame(); });
                    
                    // 버튼 색상 초기화 (핑크색)
                    ResetButtonColor(btn, CAT_NOSE_PINK);
                }
                else if (label.text.Contains("HOME"))
                {
                    hasHomeButton = true;
                    // 홈 버튼 이벤트 재연결
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.ReturnToMenu(); });
                    
                    // 버튼 색상 초기화 (크림색)
                    ResetButtonColor(btn, CAT_PAW_CREAM);
                }
            }
        }
        
        // BottomBar 찾기 또는 생성
        var bottomBar = _hudPanel.transform.Find("BottomBar");
        if (bottomBar == null)
        {
            Debug.Log("[UIManager] BottomBar not found, creating...");
            var bottomBarGo = new GameObject("BottomBar");
            bottomBarGo.transform.SetParent(_hudPanel.transform, false);
            var rt = bottomBarGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 20);
            rt.sizeDelta = new Vector2(600, 70);
            bottomBar = bottomBarGo.transform;
        }
        
        // HINT, SHUFFLE 버튼이 없으면 생성
        if (!hasHintButton || !hasShuffleButton)
        {
            Debug.Log("[UIManager] HINT or SHUFFLE button missing, creating...");
            
            if (!hasHintButton)
            {
                var hintBtnGo = CreateButton(bottomBar.transform, "HINT (3)", LIGHT_SKY,
                    new Vector2(-240, 0), new Vector2(140, 55), 20, () =>
                    {
                        Debug.Log("[UIManager] HINT button clicked!");
                        GameManager.Instance?.UseHint();
                    });
                _hintButton = hintBtnGo.GetComponent<Button>();
                _hintButtonText = hintBtnGo.GetComponentInChildren<TextMeshProUGUI>();
                _secondaryButtons.Add(_hintButton);
                Debug.Log("[UIManager] HINT button created");
            }
            
            if (!hasShuffleButton)
            {
                var shuffleBtnGo = CreateButton(bottomBar.transform, "SHUFFLE (3)", LIGHT_PINK,
                    new Vector2(-80, 0), new Vector2(140, 55), 20, () =>
                    {
                        GameManager.Instance?.UseShuffle();
                    });
                _shuffleButton = shuffleBtnGo.GetComponent<Button>();
                _shuffleButtonText = shuffleBtnGo.GetComponentInChildren<TextMeshProUGUI>();
                _secondaryButtons.Add(_shuffleButton);
                Debug.Log("[UIManager] SHUFFLE button created");
            }
        }
        
        // 재시작, 홈 버튼이 없으면 생성
        if (!hasRestartButton || !hasHomeButton)
        {
            Debug.Log("[UIManager] Restart or Home button missing, creating new buttons...");
            
            if (!hasRestartButton)
            {
                var restartBtnGo = CreateButton(bottomBar.transform, "RESTART", CAT_NOSE_PINK,
                    new Vector2(80, 0), new Vector2(140, 55), 18, () =>
                    {
                        GameManager.Instance?.RestartGame();
                    });
                _secondaryButtons.Add(restartBtnGo.GetComponent<Button>());
                Debug.Log("[UIManager] Restart button created");
            }
            
            if (!hasHomeButton)
            {
                var homeBtnGo = CreateButton(bottomBar.transform, "HOME", CAT_PAW_CREAM,
                    new Vector2(240, 0), new Vector2(140, 55), 18, () =>
                    {
                        GameManager.Instance?.ReturnToMenu();
                    });
                _secondaryButtons.Add(homeBtnGo.GetComponent<Button>());
                Debug.Log("[UIManager] Home button created");
            }
        }
        
        // BGM 컨트롤: 직렬화 우선 → 씬 내 BGMControl → 코드 생성
        if (serializedBgmToggle != null && serializedBgmSlider != null)
        {
            _bgmToggle = serializedBgmToggle;
            _bgmVolumeSlider = serializedBgmSlider;
            _bgmVolumeText = serializedBgmVolumeText;
            SetupBGMEvents();

            // 코드 생성된 BGMControl이 있으면 제거
            var oldBgm = _hudPanel.transform.Find("BGMControl");
            if (oldBgm != null) Destroy(oldBgm.gameObject);

            Debug.Log("[UIManager] BGM Control: using serialized components (re-ref)");
        }
        else
        {
            var bgmControl = _hudPanel.transform.Find("BGMControl");
            if (bgmControl != null)
            {
                Debug.Log("[UIManager] BGMControl found, re-referencing...");
                _bgmToggle = bgmControl.GetComponentInChildren<Toggle>(true);
                _bgmVolumeSlider = bgmControl.GetComponentInChildren<Slider>(true);

                var bgmTexts = bgmControl.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var txt in bgmTexts)
                {
                    if (txt.text.Contains("%"))
                        _bgmVolumeText = txt;
                }

                if (_bgmToggle != null && _bgmVolumeSlider != null)
                    SetupBGMEvents();
            }
            else
            {
                Debug.LogWarning("[UIManager] BGMControl not found! Creating...");
                CreateBGMControl();
            }
        }

        // 카운트다운 요소: 직렬화 우선 → 이름 검색 → 폴백 → 생성
        if (serializedCountdownText != null)
        {
            _countdownText = serializedCountdownText;
            Debug.Log("[UIManager] Countdown text assigned from serialized field.");
        }
        else
        {
            var countdownNumber = _countdownPanel.transform.Find("CountdownNumber");
            if (countdownNumber != null)
                _countdownText = countdownNumber.GetComponent<TextMeshProUGUI>();
        }

        if (serializedCountdownCatFace != null)
        {
            _countdownCatFace = serializedCountdownCatFace;
            Debug.Log("[UIManager] Countdown cat face assigned from serialized field.");
        }
        else
        {
            var countdownCat = _countdownPanel.transform.Find("CatFace");
            if (countdownCat != null)
                _countdownCatFace = countdownCat.GetComponent<TextMeshProUGUI>();
        }

        // 이름으로 못 찾았으면 텍스트 내용으로 폴백
        if (_countdownText == null || _countdownCatFace == null)
        {
            var cdTexts = _countdownPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in cdTexts)
            {
                if (_countdownText == null && (txt.text == "3" || txt.text.Length <= 2 && !txt.text.Contains("=")))
                    _countdownText = txt;
                else if (_countdownCatFace == null && txt.text.Contains("="))
                    _countdownCatFace = txt;
            }
        }

        // 폰트/크기 적용 (직렬화가 아닌 경우만)
        if (_countdownText != null && serializedCountdownText == null)
        {
            _countdownText.fontSize = countdownNumberFontSize;
            if (countdownNumberFont != null) _countdownText.font = countdownNumberFont;
        }
        if (_countdownCatFace != null && serializedCountdownCatFace == null)
        {
            _countdownCatFace.fontSize = countdownCatFaceFontSize;
            if (countdownCatFaceFont != null) _countdownCatFace.font = countdownCatFaceFont;
        }

        // 텍스트가 없으면 기존 패널에 추가 (패널 자체는 유지)
        if (_countdownCatFace == null)
        {
            _countdownCatFace = CreateTextElement(_countdownPanel.transform, "",
                countdownCatFaceFontSize, CAT_EYE_GOLD, new Vector2(0, 80), new Vector2(500, 80), isPrimary: false);
            _countdownCatFace.alignment = TextAlignmentOptions.Center;
            _countdownCatFace.gameObject.name = "CatFace";
            if (countdownCatFaceFont != null) _countdownCatFace.font = countdownCatFaceFont;
        }
        if (_countdownText == null)
        {
            _countdownText = CreateTextElement(_countdownPanel.transform, "3",
                countdownNumberFontSize, Color.white, new Vector2(0, -30), new Vector2(300, 200), isPrimary: true);
            _countdownText.fontStyle = FontStyles.Bold;
            _countdownText.alignment = TextAlignmentOptions.Center;
            _countdownText.gameObject.name = "CountdownNumber";
            if (countdownNumberFont != null) _countdownText.font = countdownNumberFont;
        }

        // 결과 화면 요소
        var resultTexts = _resultPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in resultTexts)
        {
            if (txt.text.Contains("Score:"))
                _resultScoreText = txt;
            else if (txt.text.Contains("Paws:"))
                _resultApplesText = txt;
            else if (txt.text.Contains("Best:"))
                _resultBestText = txt;
            else if (txt.text.Contains("Combo:"))
                _resultComboText = txt;
            else if (txt.text.Contains("RECORD"))
                _resultNewRecordText = txt;
            else if (txt.text.Contains("=^") || txt.text.Contains("Meh"))
                _resultCatFace = txt;
        }
        
        // 필수 요소가 없으면 결과 패널 재생성 (단, 직렬화된 버튼이 있으면 보존)
        if (_resultScoreText == null || _resultCatFace == null || _resultNewRecordText == null)
        {
            bool hasSerializedButtons = serializedRetryButton != null || serializedMenuButton != null || serializedResultRankingButton != null;
            if (hasSerializedButtons)
            {
                Debug.LogWarning("[UIManager] Result panel texts missing but serialized buttons exist - adding missing texts only");
                if (_resultCatFace == null)
                    _resultCatFace = CreateTextElement(_resultPanel.transform, "=^.^=", 52, CAT_EYE_GOLD, new Vector2(0, 320), new Vector2(600, 80), isPrimary: false);
                if (_resultScoreText == null)
                {
                    _resultScoreText = CreateTextElement(_resultPanel.transform, "Score: 0", 40, Color.black, new Vector2(0, 165), new Vector2(500, 60), isPrimary: true);
                    _resultScoreText.fontStyle = FontStyles.Bold;
                }
                if (_resultApplesText == null)
                {
                    _resultApplesText = CreateTextElement(_resultPanel.transform, "Paws: 0", 28, Color.gray, new Vector2(0, 105), new Vector2(500, 45), isPrimary: false);
                    _secondaryTexts.Add(_resultApplesText);
                }
                if (_resultComboText == null)
                {
                    _resultComboText = CreateTextElement(_resultPanel.transform, "Max Combo: 0", 28, Color.gray, new Vector2(0, 60), new Vector2(500, 45), isPrimary: false);
                    _secondaryTexts.Add(_resultComboText);
                }
                if (_resultBestText == null)
                    _resultBestText = CreateTextElement(_resultPanel.transform, "Best: 0", 28, CAT_EYE_GOLD, new Vector2(0, 15), new Vector2(500, 45), isPrimary: false);
                if (_resultNewRecordText == null)
                {
                    _resultNewRecordText = CreateTextElement(_resultPanel.transform, "PURRFECT RECORD!", 36, CAT_EYE_BRIGHT, new Vector2(0, -80), new Vector2(500, 50), isPrimary: false);
                    _resultNewRecordText.fontStyle = FontStyles.Bold;
                }
            }
            else
            {
                Debug.LogWarning("[UIManager] Result panel elements missing, recreating result panel...");
                Destroy(_resultPanel);
                CreateResultPanel();
                goto skipResultNickname;
            }
        }

        // 닉네임 입력 필드 재참조 (패널 보존된 경우)
        _nicknameInputField = _resultPanel.GetComponentInChildren<TMP_InputField>(true);
        if (_nicknameInputField != null)
        {
            _nicknameInputField.text = "";
            
            var nicknameButtons = _resultPanel.GetComponentsInChildren<Button>(true);
            foreach (var btn in nicknameButtons)
            {
                var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null && btnText.text == "SAVE")
                {
                    _nicknameChangeButton = btn;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OnNicknameChangeClicked);
                    break;
                }
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] Nickname input not found, creating...");
            var nicknameContainer = _resultPanel.transform.Find("NicknameContainer");
            if (nicknameContainer != null)
                Destroy(nicknameContainer.gameObject);
            CreateNicknameInput(_resultPanel.transform);
        }
        skipResultNickname:;

        // 메인메뉴 요소
        var menuTexts = _mainMenuPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in menuTexts)
        {
            if (txt.text.Contains("Best:"))
            {
                _menuBestScoreText = txt;
                txt.fontSize = bestScoreFontSize;
            }
            else if (txt.text.Contains("PAW 10"))
            {
                _titleText = txt;
                txt.fontSize = titleFontSize;
                txt.color = titleColor;
                if (titleFont != null) txt.font = titleFont;
            }
            else if (txt.text.Contains("Drag paws"))
            {
                _subtitleText = txt;
                txt.fontSize = subtitleFontSize;
            }
            else if (txt.text.Contains("=^._.^="))
                _decorationText = txt;
        }
        
        // 타이틀 애니메이션 시작
        if (_titleText != null && _subtitleText != null && _decorationText != null)
        {
            StartCoroutine(CoAnimateMenuTitle());
        }
        
        // 고양이 로고는 Start()의 InitializeCatSilhouette()에서 처리됨

        // 직렬화된 버튼 우선 처리
        bool hasPlayButton = false;
        bool hasRankingButton = false;

        if (serializedPlayButton != null)
        {
            hasPlayButton = true;
            SetupSerializedButton(serializedPlayButton, playButtonColor, () =>
            {
                PlayButtonSound();
                GameManager.Instance?.StartGame();
            });
            _primaryButtons.Add(serializedPlayButton);
            Debug.Log("[UIManager] PLAY button (serialized) reconnected!");
        }

        if (serializedRankingButton != null)
        {
            hasRankingButton = true;
            SetupSerializedButton(serializedRankingButton, rankingButtonColor, () =>
            {
                PlayButtonSound();
                ShowLeaderboard();
            });
            _secondaryButtons.Add(serializedRankingButton);
            Debug.Log("[UIManager] RANKING button (serialized) reconnected!");
        }

        // 메인메뉴 버튼 재연결 (직렬화 안 된 것들)
        var menuButtons = _mainMenuPanel.GetComponentsInChildren<Button>(true);
        Debug.Log($"[UIManager] Found {menuButtons.Length} buttons in MainMenu");
        
        foreach (var btn in menuButtons)
        {
            if (btn == serializedPlayButton || btn == serializedRankingButton)
                continue;

            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                Debug.Log($"[UIManager] Button found with label: {label.text}");
                
                if (!hasPlayButton && label.text == "PLAY")
                {
                    hasPlayButton = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        PlayButtonSound();
                        Debug.Log("[UIManager] PLAY button clicked!");
                        GameManager.Instance?.StartGame();
                    });
                    Debug.Log("[UIManager] PLAY button event reconnected!");
                    
                    ResetButtonColor(btn, playButtonColor);
                    
                    if (menuButtonFont != null)
                        label.font = menuButtonFont;
                    
                    label.fontSize = menuButtonFontSize;
                }
                else if (!hasRankingButton && label.text == "RANKING")
                {
                    hasRankingButton = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        PlayButtonSound();
                        Debug.Log("[UIManager] RANKING button clicked!");
                        ShowLeaderboard();
                    });
                    Debug.Log("[UIManager] RANKING button event reconnected!");
                    
                    ResetButtonColor(btn, rankingButtonColor);
                    
                    if (menuButtonFont != null)
                        label.font = menuButtonFont;
                    
                    label.fontSize = menuButtonFontSize;
                }
            }
        }
        
        
        // PLAY 버튼이 없으면 생성
        Debug.Log($"[UIManager] hasPlayButton = {hasPlayButton}, _mainMenuPanel = {_mainMenuPanel != null}");
        
        if (!hasPlayButton)
        {
            if (_mainMenuPanel == null)
            {
                Debug.LogError("[UIManager] Cannot create PLAY button: _mainMenuPanel is null!");
            }
            else
            {
                Debug.Log("[UIManager] PLAY button not found, creating...");
                
                var playBtnGo = CreateButton(_mainMenuPanel.transform, "PLAY", playButtonColor,
                    playButtonPosition, playButtonSize, menuButtonFontSize, () =>
                    {
                        Debug.Log("[UIManager] PLAY button clicked!");
                        GameManager.Instance?.StartGame();
                    });
                ResetButtonColor(playBtnGo.GetComponent<Button>(), playButtonColor);
                
                // 커스텀 폰트 적용
                if (menuButtonFont != null)
                {
                    var tmpText = playBtnGo.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmpText != null)
                        tmpText.font = menuButtonFont;
                }
                
                _primaryButtons.Add(playBtnGo.GetComponent<Button>());
                Debug.Log("[UIManager] PLAY button created successfully!");
            }
        }
        else
        {
            Debug.Log("[UIManager] PLAY button already exists, skipping creation.");
        }
        
        // RANKING 버튼이 없으면 생성
        if (!hasRankingButton)
        {
            Debug.LogWarning("[UIManager] RANKING button not found, creating...");
            
            var rankingBtn = CreateButton(_mainMenuPanel.transform, "RANKING", rankingButtonColor,
                rankingButtonPosition, rankingButtonSize, menuButtonFontSize, () =>
                {
                    ShowLeaderboard();
                });
            ResetButtonColor(rankingBtn.GetComponent<Button>(), rankingButtonColor);
            
            // 커스텀 폰트 적용
            if (menuButtonFont != null)
            {
                var tmpText = rankingBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                    tmpText.font = menuButtonFont;
            }
            
            _secondaryButtons.Add(rankingBtn.GetComponent<Button>());
            Debug.Log("[UIManager] RANKING button created!");
        }

        // HUD 직렬화 버튼 우선 처리
        if (serializedHintButton != null)
        {
            _hintButton = serializedHintButton;
            _hintButtonText = serializedHintButton.GetComponentInChildren<TextMeshProUGUI>();
            SetupSerializedButton(serializedHintButton, LIGHT_SKY, () => { PlayButtonSound(); GameManager.Instance?.UseHint(); });
            _secondaryButtons.Add(serializedHintButton);
        }
        if (serializedShuffleButton != null)
        {
            _shuffleButton = serializedShuffleButton;
            _shuffleButtonText = serializedShuffleButton.GetComponentInChildren<TextMeshProUGUI>();
            SetupSerializedButton(serializedShuffleButton, LIGHT_PINK, () => { PlayButtonSound(); GameManager.Instance?.UseShuffle(); });
            _successButtons.Add(serializedShuffleButton);
        }
        if (serializedRestartButton != null)
        {
            SetupSerializedButton(serializedRestartButton, CAT_NOSE_PINK, () => { PlayButtonSound(); GameManager.Instance?.RestartGame(); });
            _secondaryButtons.Add(serializedRestartButton);
        }
        if (serializedHomeButton != null)
        {
            SetupSerializedButton(serializedHomeButton, CAT_PAW_CREAM, () => { PlayButtonSound(); GameManager.Instance?.ReturnToMenu(); });
            _secondaryButtons.Add(serializedHomeButton);
        }
        // HUD 비직렬화 버튼: 직렬화 버튼이 있으면 코드 생성 버튼 제거, 없으면 재연결
        var hudButtons2 = _hudPanel.GetComponentsInChildren<Button>(true);
        foreach (var btn in hudButtons2)
        {
            if (btn == serializedHintButton || btn == serializedShuffleButton ||
                btn == serializedRestartButton || btn == serializedHomeButton)
                continue;

            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                // 직렬화 버튼이 할당되어 있으면 코드 생성 버튼 제거
                if (serializedHintButton != null && label.text.Contains("HINT"))
                {
                    Destroy(btn.gameObject);
                    continue;
                }
                if (serializedShuffleButton != null && label.text.Contains("SHUFFLE"))
                {
                    Destroy(btn.gameObject);
                    continue;
                }
                if (serializedRestartButton != null && label.text == "RESTART")
                {
                    Destroy(btn.gameObject);
                    continue;
                }
                if (serializedHomeButton != null && label.text == "HOME")
                {
                    Destroy(btn.gameObject);
                    continue;
                }
                // 직렬화 버튼이 없으면 코드 생성 버튼 재연결
                if (serializedHintButton == null && label.text.Contains("HINT"))
                {
                    _hintButton = btn;
                    _hintButtonText = label;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => 
                    { 
                        Debug.Log("[UIManager] HINT button clicked (hudButtons2)!");
                        GameManager.Instance?.UseHint(); 
                    });
                }
                else if (serializedShuffleButton == null && label.text.Contains("SHUFFLE"))
                {
                    _shuffleButton = btn;
                    _shuffleButtonText = label;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { GameManager.Instance?.UseShuffle(); });
                }
            }
        }

        // 결과 화면 직렬화 버튼 우선 처리
        if (serializedRetryButton != null)
        {
            serializedRetryButton.onClick.RemoveAllListeners();
            serializedRetryButton.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.RestartGame(); });
        }
        if (serializedMenuButton != null)
        {
            serializedMenuButton.onClick.RemoveAllListeners();
            serializedMenuButton.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.ReturnToMenu(); });
        }
        if (serializedResultRankingButton != null)
        {
            serializedResultRankingButton.onClick.RemoveAllListeners();
            serializedResultRankingButton.onClick.AddListener(() => { PlayButtonSound(); ShowLeaderboard(); });
        }

        // 결과 화면 비직렬화 버튼 재연결
        var resultButtons = _resultPanel.GetComponentsInChildren<Button>(true);
        foreach (var btn in resultButtons)
        {
            if (btn == serializedRetryButton || btn == serializedMenuButton || btn == serializedResultRankingButton)
                continue;

            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                if (serializedRetryButton == null && label.text == "RETRY")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.RestartGame(); });
                    ResetButtonColor(btn, CAT_EYE_GOLD);
                }
                else if (serializedMenuButton == null && label.text == "MENU")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.ReturnToMenu(); });
                    ResetButtonColor(btn, CAT_EYE_GOLD);
                }
                else if (serializedResultRankingButton == null && label.text == "RANKING")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { PlayButtonSound(); ShowLeaderboard(); });
                    ResetButtonColor(btn, CAT_EYE_GOLD);
                }
            }
        }

        Debug.Log("[UIManager] Re-referenced all UI elements and reconnected button events!");
    }

    private void Start()
    {
        // Play 모드에서만 이벤트 구독
        if (!Application.isPlaying) return;

        Debug.Log("[UIManager] Start() - Registering event listeners...");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged.AddListener(ShowPanel);
            GameManager.Instance.OnTimerTick.AddListener(UpdateTimer);
            GameManager.Instance.OnCountdownTick.AddListener(UpdateCountdown);
            GameManager.Instance.OnHintsChanged.AddListener(UpdateHintButton);
            GameManager.Instance.OnShufflesChanged.AddListener(UpdateShuffleButton);
            Debug.Log("[UIManager] GameManager events registered");
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.AddListener(UpdateScore);
            ScoreManager.Instance.OnComboChanged.AddListener(UpdateCombo);
            ScoreManager.Instance.OnScorePopup.AddListener(SpawnFloatingScore);
            Debug.Log("[UIManager] ScoreManager events registered");
        }
        else
        {
            Debug.LogWarning("[UIManager] ScoreManager.Instance is null in Start()!");
        }


        Debug.Log($"[UIManager] _scoreText is null: {_scoreText == null}");
        
        // ⭐ 고양이 로고 찾기 및 애니메이션 시작
        InitializeCatSilhouette();
    }
    
    /// <summary>
    /// 고양이 로고 초기화 및 애니메이션 시작
    /// </summary>
    private void InitializeCatSilhouette()
    {
        // 0. 이미 설정되어 있으면 바로 애니메이션 시작
        if (_catSilhouette != null)
        {
            Debug.Log("[UIManager] ✅ Cat silhouette already set (from CreateMainMenuPanel)!");
        }
        // 1. Inspector에서 할당된 것이 있으면 사용
        else if (catSilhouetteObject != null)
        {
            _catSilhouette = catSilhouetteObject;
            Debug.Log("[UIManager] ✅ Cat silhouette assigned from Inspector!");
        }
        // 2. 할당되지 않았으면 자동으로 찾기
        else if (_mainMenuPanel != null)
        {
            var catSilhouetteTransform = _mainMenuPanel.transform.Find("CatSilhouette");
            if (catSilhouetteTransform != null)
            {
                _catSilhouette = catSilhouetteTransform.gameObject;
                Debug.Log("[UIManager] ✅ Cat silhouette found automatically in MainMenuPanel!");
            }
            else
            {
                Debug.LogWarning("[UIManager] ⚠️ Cat silhouette not found! Please assign 'catSilhouetteObject' in Inspector or ensure 'CatSilhouette' object exists in MainMenuPanel.");
            }
        }
        
        // 3. 찾았으면 애니메이션 시작 (이미 실행 중이 아닐 때만)
        if (_catSilhouette != null && !_catSilhouetteAnimating)
        {
            StartCoroutine(CoAnimateCatSilhouette());
            _catSilhouetteAnimating = true;
            Debug.Log("[UIManager] 🐱 Cat silhouette animation started!");
        }
        else if (_catSilhouetteAnimating)
        {
            Debug.Log("[UIManager] ℹ️ Cat silhouette animation already running.");
        }
        else
        {
            Debug.LogError("[UIManager] ❌ Cat silhouette could not be initialized! Animation will not start.");
        }
    }


    // ─── 패널 전환 ───

    private void ShowPanel(GameState state)
    {
        Debug.Log($"[UIManager] ShowPanel called with state: {state}");
        Debug.Log($"[UIManager] Panel null checks - Menu:{_mainMenuPanel == null}, HUD:{_hudPanel == null}, Countdown:{_countdownPanel == null}, Result:{_resultPanel == null}");

        if (_mainMenuPanel != null) _mainMenuPanel.SetActive(state == GameState.Menu);
        if (_hudPanel != null) _hudPanel.SetActive(state == GameState.Playing || state == GameState.Countdown);
        if (_countdownPanel != null) _countdownPanel.SetActive(state == GameState.Countdown);
        if (_resultPanel != null) _resultPanel.SetActive(state == GameState.GameOver);

        Debug.Log($"[UIManager] MainMenu active: {_mainMenuPanel?.activeSelf}");

        if (state == GameState.Menu)
        {
            SubmitSessionIfNeeded();
            UpdateMenuBestScore();
            _currentSessionScore = 0;
            _currentSessionCombo = 0;
            _sessionSubmitted = false;
        }
        
        if (state == GameState.Countdown)
        {
            SubmitSessionIfNeeded();
            _currentSessionScore = 0;
            _currentSessionCombo = 0;
            _sessionSubmitted = false;
        }

        if (state == GameState.GameOver)
            PopulateResult();
    }

    // ─── 메인 메뉴 (테마 토글 포함) ───

    private void CreateMainMenuPanel()
    {
        _mainMenuPanel = CreatePanel("MainMenuPanel", Color.white, true);
        var rt = _mainMenuPanel.GetComponent<RectTransform>();
        SetFullStretch(rt);

        // 고양이 실루엣 마스코트
        CreateCatSilhouette(_mainMenuPanel.transform, catLogoPosition, catLogoSize);

        // 타이틀
        _titleText = CreateTextElement(_mainMenuPanel.transform, "PAW 10",
            titleFontSize, titleColor, titlePosition, new Vector2(800, 100), isPrimary: false);
        _titleText.fontStyle = FontStyles.Bold;
        if (titleFont != null) _titleText.font = titleFont;

        // 부제
        _subtitleText = CreateTextElement(_mainMenuPanel.transform, "Drag paws that sum to 10!",
            subtitleFontSize, Color.gray, subtitlePosition, new Vector2(700, 50), isPrimary: false);
        _secondaryTexts.Add(_subtitleText);

        // 고양이 장식
        _decorationText = CreateTextElement(_mainMenuPanel.transform, "~  =^._.^=  ~",
            28, Color.gray, decorationPosition, new Vector2(400, 50), isPrimary: false);
        _secondaryTexts.Add(_decorationText);
        
        // 타이틀 애니메이션 시작
        StartCoroutine(CoAnimateMenuTitle());
        
        // 고양이 로고 애니메이션 시작
        StartCoroutine(CoAnimateCatSilhouette());

        // Play 버튼
        if (serializedPlayButton != null)
        {
            SetupSerializedButton(serializedPlayButton, playButtonColor, () =>
            {
                PlayButtonSound();
                GameManager.Instance?.StartGame();
            });
            _primaryButtons.Add(serializedPlayButton);
        }
        else
        {
            var playBtn = CreateButton(_mainMenuPanel.transform, "PLAY", playButtonColor,
                playButtonPosition, playButtonSize, menuButtonFontSize, () =>
                {
                    GameManager.Instance?.StartGame();
                });
            _primaryButtons.Add(playBtn.GetComponent<Button>());
            ResetButtonColor(playBtn.GetComponent<Button>(), playButtonColor);
            if (menuButtonFont != null)
            {
                var playTmp = playBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (playTmp != null) playTmp.font = menuButtonFont;
            }
        }

        // Ranking 버튼
        if (serializedRankingButton != null)
        {
            SetupSerializedButton(serializedRankingButton, rankingButtonColor, () =>
            {
                PlayButtonSound();
                ShowLeaderboard();
            });
            _secondaryButtons.Add(serializedRankingButton);
        }
        else
        {
            var rankingBtn = CreateButton(_mainMenuPanel.transform, "RANKING", rankingButtonColor,
                rankingButtonPosition, rankingButtonSize, menuButtonFontSize, () =>
                {
                    ShowLeaderboard();
                });
            _secondaryButtons.Add(rankingBtn.GetComponent<Button>());
            ResetButtonColor(rankingBtn.GetComponent<Button>(), rankingButtonColor);
            if (menuButtonFont != null)
            {
                var rankTmp = rankingBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (rankTmp != null) rankTmp.font = menuButtonFont;
            }
        }

        // 최고기록
        _menuBestScoreText = CreateTextElement(_mainMenuPanel.transform, "Best: 0",
            bestScoreFontSize, CAT_EYE_GOLD, bestScorePosition, new Vector2(400, 50), isPrimary: false);

        // 다크모드 토글 제거됨
    }

    private void UpdateMenuBestScore()
    {
        if (_menuBestScoreText != null && ScoreManager.Instance != null)
            _menuBestScoreText.text = $"Best: {ScoreManager.Instance.BestScore}";
    }

    // ─── HUD ───

    private void CreateHUDPanel()
    {
        _hudPanel = CreatePanel("HUDPanel", Color.clear, false);
        var rt = _hudPanel.GetComponent<RectTransform>();
        SetFullStretch(rt);

        // 상단 바
        var topBar = CreatePanel("TopBar", Color.white, true, _hudPanel.transform);
        var topRt = topBar.GetComponent<RectTransform>();
        topRt.anchorMin = new Vector2(0, 1);
        topRt.anchorMax = new Vector2(1, 1);
        topRt.pivot = new Vector2(0.5f, 1);
        topRt.sizeDelta = new Vector2(0, 100); // 슬라이더 공간 확보
        topRt.anchoredPosition = Vector2.zero;
        _barBackgrounds.Add(topBar.GetComponent<Image>());

        // 점수 (좌측) - 직렬화 우선
        if (serializedScoreText != null)
        {
            _scoreText = serializedScoreText;
        }
        else
        {
            _scoreText = CreateTextElement(topBar.transform, "0",
                36, new Color(0.17f, 0.17f, 0.18f), new Vector2(-300, -10), new Vector2(200, 60), isPrimary: true);
            _scoreText.alignment = TextAlignmentOptions.Left;
            _scoreText.fontStyle = FontStyles.Bold;
        }

        // SCORE 라벨 - 직렬화 우선
        if (serializedScoreLabelText != null)
        {
            _scoreLabelText = serializedScoreLabelText;
        }
        else
        {
            _scoreLabelText = CreateTextElement(topBar.transform, "SCORE",
                18, new Color(0.17f, 0.17f, 0.18f), new Vector2(-300, -42), new Vector2(200, 30), isPrimary: false);
            _scoreLabelText.alignment = TextAlignmentOptions.Left;
        }

        // 타이머 (중앙) - 직렬화 우선
        if (serializedTimerText != null)
        {
            _timerText = serializedTimerText;
        }
        else
        {
            _timerText = CreateTextElement(topBar.transform, "2:00",
                38, new Color(0.17f, 0.17f, 0.18f), new Vector2(0, -8), new Vector2(200, 50), isPrimary: true);
            _timerText.fontStyle = FontStyles.Bold;
            _timerText.alignment = TextAlignmentOptions.Center;
        }

        // 타이머 슬라이더 - 직렬화 우선
        if (serializedTimerSlider != null)
        {
            _timerSlider = serializedTimerSlider;
            _timerSliderFill = _timerSlider.fillRect?.GetComponent<Image>();
        }
        else
        {
            var sliderGo = new GameObject("TimerSlider");
            sliderGo.transform.SetParent(topBar.transform, false);
            var sliderRt = sliderGo.AddComponent<RectTransform>();
            sliderRt.anchoredPosition = new Vector2(0, -55);
            sliderRt.sizeDelta = new Vector2(180, 12);

            _timerSlider = sliderGo.AddComponent<Slider>();
            _timerSlider.minValue = 0f;
            _timerSlider.maxValue = 1f;
            _timerSlider.value = 1f;
            _timerSlider.interactable = false;

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(sliderGo.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            SetFullStretch(bgRt);
            bgGo.AddComponent<Image>();

            var fillAreaGo = new GameObject("Fill Area");
            fillAreaGo.transform.SetParent(sliderGo.transform, false);
            var fillAreaRt = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.sizeDelta = new Vector2(-4, -4);
            fillAreaRt.anchoredPosition = Vector2.zero;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            fillRt.anchoredPosition = Vector2.zero;
            _timerSliderFill = fillGo.AddComponent<Image>();

            _timerSlider.fillRect = fillRt;
            _timerSlider.targetGraphic = _timerSliderFill;

            var timerLabel = CreateTextElement(topBar.transform, "TIME",
                16, CAT_EYE_GOLD, new Vector2(0, -72), new Vector2(200, 25), isPrimary: false);
            timerLabel.alignment = TextAlignmentOptions.Center;
        }

        // 콤보 (우측) - 직렬화 우선
        if (serializedComboText != null)
        {
            _comboText = serializedComboText;
        }
        else
        {
            _comboText = CreateTextElement(topBar.transform, "",
                32, CAT_EYE_GOLD, new Vector2(300, -20), new Vector2(250, 60), isPrimary: false);
            _comboText.alignment = TextAlignmentOptions.Right;
        }

        // 하단 버튼 바
        var bottomBar = CreatePanel("BottomBar", Color.clear, false, _hudPanel.transform);
        var botRt = bottomBar.GetComponent<RectTransform>();
        botRt.anchorMin = new Vector2(0, 0);
        botRt.anchorMax = new Vector2(1, 0);
        botRt.pivot = new Vector2(0.5f, 0);
        botRt.sizeDelta = new Vector2(0, 80);
        botRt.anchoredPosition = new Vector2(0, 10);

        // 힌트 버튼 (연한 하늘색)
        if (serializedHintButton != null)
        {
            _hintButton = serializedHintButton;
            _hintButtonText = serializedHintButton.GetComponentInChildren<TextMeshProUGUI>();
            SetupSerializedButton(serializedHintButton, LIGHT_SKY, () =>
            {
                PlayButtonSound();
                GameManager.Instance?.UseHint();
            });
            _secondaryButtons.Add(_hintButton);
        }
        else
        {
            var hintBtnGo = CreateButton(bottomBar.transform, "HINT (3)", LIGHT_SKY,
                new Vector2(-240, 0), new Vector2(140, 55), 20, () =>
                {
                    Debug.Log("[UIManager] HINT button clicked (CreateButton)!");
                    GameManager.Instance?.UseHint();
                });
            _hintButton = hintBtnGo.GetComponent<Button>();
            _hintButtonText = hintBtnGo.GetComponentInChildren<TextMeshProUGUI>();
            _secondaryButtons.Add(_hintButton);
        }

        // 셔플 버튼 (연한 핑크)
        if (serializedShuffleButton != null)
        {
            _shuffleButton = serializedShuffleButton;
            _shuffleButtonText = serializedShuffleButton.GetComponentInChildren<TextMeshProUGUI>();
            SetupSerializedButton(serializedShuffleButton, LIGHT_PINK, () =>
            {
                PlayButtonSound();
                GameManager.Instance?.UseShuffle();
            });
            _successButtons.Add(_shuffleButton);
        }
        else
        {
            var shuffleBtnGo = CreateButton(bottomBar.transform, "SHUFFLE (2)", LIGHT_PINK,
                new Vector2(-80, 0), new Vector2(140, 55), 20, () =>
                {
                    GameManager.Instance?.UseShuffle();
                });
            _shuffleButton = shuffleBtnGo.GetComponent<Button>();
            _shuffleButtonText = shuffleBtnGo.GetComponentInChildren<TextMeshProUGUI>();
            _successButtons.Add(_shuffleButton);
        }

        // 재시작 버튼 (핑크색)
        if (serializedRestartButton != null)
        {
            SetupSerializedButton(serializedRestartButton, CAT_NOSE_PINK, () =>
            {
                PlayButtonSound();
                GameManager.Instance?.RestartGame();
            });
            _secondaryButtons.Add(serializedRestartButton);
        }
        else
        {
            var restartBtnGo = CreateButton(bottomBar.transform, "RESTART", CAT_NOSE_PINK,
                new Vector2(80, 0), new Vector2(140, 55), 18, () =>
                {
                    GameManager.Instance?.RestartGame();
                });
            _secondaryButtons.Add(restartBtnGo.GetComponent<Button>());
        }

        // 홈 버튼
        if (serializedHomeButton != null)
        {
            SetupSerializedButton(serializedHomeButton, CAT_PAW_CREAM, () =>
            {
                PlayButtonSound();
                GameManager.Instance?.ReturnToMenu();
            });
            _secondaryButtons.Add(serializedHomeButton);
        }
        else
        {
            var homeBtnGo = CreateButton(bottomBar.transform, "HOME", CAT_PAW_CREAM,
                new Vector2(240, 0), new Vector2(140, 55), 18, () =>
                {
                    GameManager.Instance?.ReturnToMenu();
                });
            _secondaryButtons.Add(homeBtnGo.GetComponent<Button>());
        }

        // 다크모드 HUD 토글 제거됨

        // BGM 컨트롤: 직렬화 할당되어 있으면 이벤트만 연결, 없으면 코드 생성
        if (serializedBgmToggle != null && serializedBgmSlider != null)
        {
            _bgmToggle = serializedBgmToggle;
            _bgmVolumeSlider = serializedBgmSlider;
            _bgmVolumeText = serializedBgmVolumeText;
            SetupBGMEvents();
        }
        else
        {
            CreateBGMControl();
        }

        _hudPanel.SetActive(false);
    }

    private void CreateBGMControl()
    {
        if (_hudPanel == null)
        {
            Debug.LogError("[UIManager] Cannot create BGM control - HUDPanel is null!");
            return;
        }

        // 직렬화 컴포넌트가 모두 있으면 이벤트만 연결
        if (serializedBgmToggle != null && serializedBgmSlider != null)
        {
            _bgmToggle = serializedBgmToggle;
            _bgmVolumeSlider = serializedBgmSlider;
            _bgmVolumeText = serializedBgmVolumeText;

            SetupBGMEvents();
            Debug.Log("[UIManager] BGM Control: using serialized components");
            return;
        }

        // BGM 컨트롤 패널 (좌측 하단)
        var bgmPanel = CreatePanel("BGMControl", Color.clear, false, _hudPanel.transform);
        var bgmPanelRt = bgmPanel.GetComponent<RectTransform>();
        bgmPanelRt.anchorMin = new Vector2(0, 0);
        bgmPanelRt.anchorMax = new Vector2(0, 0);
        bgmPanelRt.pivot = new Vector2(0, 0);
        bgmPanelRt.anchoredPosition = new Vector2(20, 100);
        bgmPanelRt.sizeDelta = new Vector2(200, 80);

        // BGM 토글
        var toggleGo = new GameObject("BGMToggle");
        toggleGo.transform.SetParent(bgmPanel.transform, false);
        var toggleRt = toggleGo.AddComponent<RectTransform>();
        toggleRt.anchoredPosition = new Vector2(0, 45);
        toggleRt.sizeDelta = new Vector2(180, 25);

        _bgmToggle = toggleGo.AddComponent<Toggle>();
        _bgmToggle.isOn = true;

        var toggleBgGo = new GameObject("Background");
        toggleBgGo.transform.SetParent(toggleGo.transform, false);
        var toggleBgRt = toggleBgGo.AddComponent<RectTransform>();
        SetFullStretch(toggleBgRt);
        var toggleBgImg = toggleBgGo.AddComponent<Image>();
        toggleBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        var checkmarkGo = new GameObject("Checkmark");
        checkmarkGo.transform.SetParent(toggleBgGo.transform, false);
        var checkmarkRt = checkmarkGo.AddComponent<RectTransform>();
        checkmarkRt.anchorMin = new Vector2(0, 0.5f);
        checkmarkRt.anchorMax = new Vector2(0, 0.5f);
        checkmarkRt.pivot = new Vector2(0, 0.5f);
        checkmarkRt.anchoredPosition = new Vector2(5, 0);
        checkmarkRt.sizeDelta = new Vector2(18, 18);
        var checkmarkImg = checkmarkGo.AddComponent<Image>();
        checkmarkImg.color = CAT_EYE_GOLD;

        _bgmToggle.graphic = checkmarkImg;
        _bgmToggle.targetGraphic = toggleBgImg;

        var toggleLabelGo = new GameObject("Label");
        toggleLabelGo.transform.SetParent(toggleGo.transform, false);
        var toggleLabelRt = toggleLabelGo.AddComponent<RectTransform>();
        toggleLabelRt.anchorMin = new Vector2(0, 0.5f);
        toggleLabelRt.anchorMax = new Vector2(1, 0.5f);
        toggleLabelRt.pivot = new Vector2(0, 0.5f);
        toggleLabelRt.anchoredPosition = new Vector2(30, 0);
        toggleLabelRt.sizeDelta = new Vector2(-30, 25);
        var toggleLabel = toggleLabelGo.AddComponent<TextMeshProUGUI>();
        toggleLabel.text = "BGM";
        toggleLabel.fontSize = 18;
        toggleLabel.fontStyle = FontStyles.Bold;
        toggleLabel.alignment = TextAlignmentOptions.Left;
        _primaryTexts.Add(toggleLabel);

        // BGM 볼륨 슬라이더
        var volumeSliderGo = new GameObject("BGMVolumeSlider");
        volumeSliderGo.transform.SetParent(bgmPanel.transform, false);
        var volumeSliderRt = volumeSliderGo.AddComponent<RectTransform>();
        volumeSliderRt.anchoredPosition = new Vector2(0, 10);
        volumeSliderRt.sizeDelta = new Vector2(130, 20);

        _bgmVolumeSlider = volumeSliderGo.AddComponent<Slider>();
        _bgmVolumeSlider.minValue = 0f;
        _bgmVolumeSlider.maxValue = 1f;
        _bgmVolumeSlider.value = 0.5f;
        _bgmVolumeSlider.interactable = true;

        var volBgGo = new GameObject("Background");
        volBgGo.transform.SetParent(volumeSliderGo.transform, false);
        var volBgRt = volBgGo.AddComponent<RectTransform>();
        SetFullStretch(volBgRt);
        var volBgImg = volBgGo.AddComponent<Image>();
        volBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        var volFillAreaGo = new GameObject("Fill Area");
        volFillAreaGo.transform.SetParent(volumeSliderGo.transform, false);
        var volFillAreaRt = volFillAreaGo.AddComponent<RectTransform>();
        volFillAreaRt.anchorMin = Vector2.zero;
        volFillAreaRt.anchorMax = Vector2.one;
        volFillAreaRt.sizeDelta = new Vector2(-4, -4);
        volFillAreaRt.anchoredPosition = Vector2.zero;

        var volFillGo = new GameObject("Fill");
        volFillGo.transform.SetParent(volFillAreaGo.transform, false);
        var volFillRt = volFillGo.AddComponent<RectTransform>();
        volFillRt.anchorMin = Vector2.zero;
        volFillRt.anchorMax = Vector2.one;
        volFillRt.sizeDelta = Vector2.zero;
        volFillRt.anchoredPosition = Vector2.zero;
        var volFillImg = volFillGo.AddComponent<Image>();
        volFillImg.color = CAT_EYE_GOLD;

        _bgmVolumeSlider.fillRect = volFillRt;
        _bgmVolumeSlider.targetGraphic = volFillImg;

        var handleAreaGo = new GameObject("Handle Slide Area");
        handleAreaGo.transform.SetParent(volumeSliderGo.transform, false);
        var handleAreaRt = handleAreaGo.AddComponent<RectTransform>();
        SetFullStretch(handleAreaRt);

        var handleGo = new GameObject("Handle");
        handleGo.transform.SetParent(handleAreaGo.transform, false);
        var handleRt = handleGo.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(16, 16);
        var handleImg = handleGo.AddComponent<Image>();
        handleImg.color = Color.white;

        _bgmVolumeSlider.handleRect = handleRt;

        var volumeTextGo = new GameObject("VolumeText");
        volumeTextGo.transform.SetParent(bgmPanel.transform, false);
        var volumeTextRt = volumeTextGo.AddComponent<RectTransform>();
        volumeTextRt.anchoredPosition = new Vector2(150, 10);
        volumeTextRt.sizeDelta = new Vector2(40, 20);
        _bgmVolumeText = volumeTextGo.AddComponent<TextMeshProUGUI>();
        _bgmVolumeText.fontSize = 16;
        _bgmVolumeText.alignment = TextAlignmentOptions.Left;
        _primaryTexts.Add(_bgmVolumeText);

        SetupBGMEvents();
        Debug.Log("[UIManager] BGM Control created (code-generated)");
    }

    private void SetupBGMEvents()
    {
        _bgmToggle.onValueChanged.RemoveAllListeners();
        _bgmToggle.onValueChanged.AddListener((isOn) =>
        {
            if (AudioManager.Instance != null)
            {
                if (isOn)
                    AudioManager.Instance.PlayBGM();
                else
                    AudioManager.Instance.StopBGM();
            }
        });

        _bgmVolumeSlider.onValueChanged.RemoveAllListeners();
        _bgmVolumeSlider.onValueChanged.AddListener((value) =>
        {
            if (_bgmVolumeText != null)
                _bgmVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
            
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetBGMVolume(value);
        });

        if (AudioManager.Instance != null)
        {
            float currentVolume = AudioManager.Instance.GetComponent<AudioSource>()?.volume ?? 0.5f;
            _bgmVolumeSlider.value = currentVolume;
            if (_bgmVolumeText != null)
                _bgmVolumeText.text = $"{Mathf.RoundToInt(currentVolume * 100)}%";
        }
        else
        {
            _bgmVolumeSlider.value = 0.5f;
            if (_bgmVolumeText != null)
                _bgmVolumeText.text = "50%";
        }
    }

    private void UpdateScore(int score)
    {
        if (_scoreText != null)
        {
            _scoreText.text = score.ToString();
        }
        else
        {
            Debug.LogError("[UIManager] _scoreText is null! Cannot update score.");
        }
    }

    private void UpdateTimer(float time)
    {
        if (_timerText == null) return;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        _timerText.text = $"{minutes}:{seconds:D2}";

        // 슬라이더 업데이트 (원작 스타일)
        if (_timerSlider != null && GameManager.Instance != null)
        {
            float maxTime = GameManager.Instance.GameDuration;
            float ratio = time / maxTime;
            _timerSlider.value = ratio;
        }

        // 직렬화 안 된 경우만 색상 변경
        if (serializedTimerText == null)
        {
            if (time <= 30f)
                _timerText.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), CAT_EYE_GOLD, Mathf.PingPong(Time.time * 2f, 1f));
            else
                _timerText.color = new Color(0.17f, 0.17f, 0.18f);
        }
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

    // ─── 카운트다운 ───

    private void CreateCountdownPanel()
    {
        _countdownPanel = CreatePanel("CountdownPanel", Color.clear, false);
        SetFullStretch(_countdownPanel.GetComponent<RectTransform>());

        if (serializedCountdownCatFace != null)
        {
            _countdownCatFace = serializedCountdownCatFace;
        }
        else
        {
            _countdownCatFace = CreateTextElement(_countdownPanel.transform, "",
                countdownCatFaceFontSize, CAT_EYE_GOLD, new Vector2(0, 80), new Vector2(500, 80), isPrimary: false);
            _countdownCatFace.alignment = TextAlignmentOptions.Center;
            _countdownCatFace.gameObject.name = "CatFace";
            if (countdownCatFaceFont != null) _countdownCatFace.font = countdownCatFaceFont;
        }

        if (serializedCountdownText != null)
        {
            _countdownText = serializedCountdownText;
        }
        else
        {
            _countdownText = CreateTextElement(_countdownPanel.transform, "3",
                countdownNumberFontSize, Color.white, new Vector2(0, -30), new Vector2(300, 200), isPrimary: true);
            _countdownText.fontStyle = FontStyles.Bold;
            _countdownText.alignment = TextAlignmentOptions.Center;
            _countdownText.gameObject.name = "CountdownNumber";
            if (countdownNumberFont != null) _countdownText.font = countdownNumberFont;
        }

        _countdownPanel.SetActive(false);
    }

    private void UpdateCountdown(int value)
    {
        Debug.Log($"[UIManager] UpdateCountdown called with value: {value}");
        
        if (_countdownText == null)
        {
            Debug.LogError("[UIManager] UpdateCountdown failed: _countdownText is null!");
            return;
        }
        
        if (_countdownCatFace == null)
        {
            Debug.LogError("[UIManager] UpdateCountdown failed: _countdownCatFace is null!");
            return;
        }

        switch (value)
        {
            case 3:
                _countdownCatFace.text = "=u.u=";
                _countdownText.text = "3";
                _countdownText.color = new Color(0.4f, 0.4f, 0.4f);
                break;
            case 2:
                _countdownCatFace.text = "=-.o=";
                _countdownText.text = "2";
                _countdownText.color = new Color(0.17f, 0.17f, 0.18f);
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

    // ─── 결과 화면 ───

    private void CreateResultPanel()
    {
        _resultPanel = CreatePanel("ResultPanel", Color.white, true);
        SetFullStretch(_resultPanel.GetComponent<RectTransform>());

        _resultCatFace = CreateTextElement(_resultPanel.transform, "",
            52, CAT_EYE_GOLD, new Vector2(0, 320), new Vector2(600, 80), isPrimary: false);
        _resultCatFace.fontStyle = FontStyles.Bold;

        var title = CreateTextElement(_resultPanel.transform, "GAME OVER!",
            48, CAT_EYE_GOLD, new Vector2(0, 250), new Vector2(600, 80), isPrimary: false);
        title.fontStyle = FontStyles.Bold;

        _resultScoreText = CreateTextElement(_resultPanel.transform, "Score: 0",
            40, Color.black, new Vector2(0, 165), new Vector2(500, 60), isPrimary: true);
        _resultScoreText.fontStyle = FontStyles.Bold;

        _resultApplesText = CreateTextElement(_resultPanel.transform, "Paws: 0",
            28, Color.gray, new Vector2(0, 105), new Vector2(500, 45), isPrimary: false);
        _secondaryTexts.Add(_resultApplesText);

        _resultComboText = CreateTextElement(_resultPanel.transform, "Max Combo: 0",
            28, Color.gray, new Vector2(0, 60), new Vector2(500, 45), isPrimary: false);
        _secondaryTexts.Add(_resultComboText);

        _resultBestText = CreateTextElement(_resultPanel.transform, "Best: 0",
            28, CAT_EYE_GOLD, new Vector2(0, 15), new Vector2(500, 45), isPrimary: false);

        // 닉네임 입력 영역
        CreateNicknameInput(_resultPanel.transform);

        _resultNewRecordText = CreateTextElement(_resultPanel.transform, "PURRFECT RECORD!",
            36, CAT_EYE_BRIGHT, new Vector2(0, -80), new Vector2(500, 50), isPrimary: false);
        _resultNewRecordText.fontStyle = FontStyles.Bold;

        // 랭킹 버튼
        if (serializedResultRankingButton != null)
        {
            serializedResultRankingButton.onClick.RemoveAllListeners();
            serializedResultRankingButton.onClick.AddListener(() => { PlayButtonSound(); ShowLeaderboard(); });
        }
        else
        {
            var rankingBtn = CreateButton(_resultPanel.transform, "RANKING", CAT_EYE_GOLD,
                new Vector2(0, -250), new Vector2(280, 60), 24, () =>
                {
                    ShowLeaderboard();
                });
            _secondaryButtons.Add(rankingBtn.GetComponent<Button>());
        }
        
        // RETRY 버튼
        if (serializedRetryButton != null)
        {
            serializedRetryButton.onClick.RemoveAllListeners();
            serializedRetryButton.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.RestartGame(); });
        }
        else
        {
            var retryBtn = CreateButton(_resultPanel.transform, "RETRY", Color.white,
                new Vector2(-180, -170), new Vector2(240, 65), 26, () =>
                {
                    GameManager.Instance?.RestartGame();
                });
            _primaryButtons.Add(retryBtn.GetComponent<Button>());
        }

        // MENU 버튼
        if (serializedMenuButton != null)
        {
            serializedMenuButton.onClick.RemoveAllListeners();
            serializedMenuButton.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.ReturnToMenu(); });
        }
        else
        {
            var menuBtn = CreateButton(_resultPanel.transform, "MENU", Color.white,
                new Vector2(180, -170), new Vector2(240, 65), 26, () =>
                {
                    GameManager.Instance?.ReturnToMenu();
                });
            _secondaryButtons.Add(menuBtn.GetComponent<Button>());
        }

        _resultPanel.SetActive(false);
    }

    public void CaptureSessionScore(int score, int combo)
    {
        _currentSessionScore = score;
        _currentSessionCombo = combo;
        _sessionSubmitted = false;
        Debug.Log($"[UIManager] Session captured at EndGame: {score} (combo: {combo})");
    }

    private void SubmitSessionIfNeeded()
    {
        if (_sessionSubmitted || _currentSessionScore <= 0) return;

        string nickname = _currentNickname;
        if (string.IsNullOrEmpty(nickname))
        {
            Debug.LogWarning("[UIManager] Nickname is empty, skipping submission");
            return;
        }

        _sessionSubmitted = true;
        Debug.Log($"[UIManager] Submitting score: {nickname} - {_currentSessionScore} (combo: {_currentSessionCombo})");

        if (FirebaseManager.Instance != null)
            FirebaseManager.Instance.SubmitScore(nickname, _currentSessionScore, _currentSessionCombo);
    }

    private void PopulateResult()
    {
        var sm = ScoreManager.Instance;
        if (sm == null) 
        {
            Debug.LogError("[UIManager] ScoreManager is null!");
            return;
        }

        // Null 체크
        if (_resultScoreText == null || _resultApplesText == null || 
            _resultComboText == null || _resultBestText == null || 
            _resultCatFace == null || _resultNewRecordText == null)
        {
            Debug.LogError("[UIManager] Result UI elements are null! Re-referencing...");
            return;
        }

        _resultScoreText.text = $"Score: {sm.Score}";
        _resultApplesText.text = $"Paws: {sm.ApplesRemoved}";
        _resultComboText.text = $"Max Combo: {sm.MaxCombo}";
        _resultBestText.text = $"Best: {sm.BestScore}";
        
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
        
        // 닉네임 입력 필드를 비움 (매 게임마다 새로 입력)
        _currentNickname = "";
        if (_nicknameInputField != null)
        {
            _nicknameInputField.text = "";
            Debug.Log("[UIManager] Nickname field cleared for new input");
        }
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

    // ─── 랭킹보드 ───

    private void CreateLeaderboardPanel()
    {
        _leaderboardPanel = CreatePanel("LeaderboardPanel", leaderboardPanelBgColor, true);
        SetFullStretch(_leaderboardPanel.GetComponent<RectTransform>());

        var title = CreateTextElement(_leaderboardPanel.transform, leaderboardTitleText,
            leaderboardTitleFontSize, leaderboardTitleColor, leaderboardTitlePosition, new Vector2(700, 80), isPrimary: false);
        title.fontStyle = FontStyles.Bold;
        if (leaderboardTitleFont != null) title.font = leaderboardTitleFont;

        var scrollArea = new GameObject("ScrollArea");
        scrollArea.transform.SetParent(_leaderboardPanel.transform, false);
        var scrollAreaRt = scrollArea.AddComponent<RectTransform>();
        scrollAreaRt.anchoredPosition = new Vector2(0, 0);
        scrollAreaRt.sizeDelta = leaderboardScrollAreaSize;

        var scrollBg = scrollArea.AddComponent<Image>();
        scrollBg.color = leaderboardScrollBgColor;
        scrollBg.sprite = CreateRoundedRectSprite(100, 100, 15);
        scrollBg.type = Image.Type.Sliced;
        _panelBackgrounds.Add(scrollBg);

        var entriesContainer = new GameObject("Entries");
        entriesContainer.transform.SetParent(scrollArea.transform, false);
        var entriesRt = entriesContainer.AddComponent<RectTransform>();
        entriesRt.anchorMin = new Vector2(0.5f, 1f);
        entriesRt.anchorMax = new Vector2(0.5f, 1f);
        entriesRt.pivot = new Vector2(0.5f, 1f);
        entriesRt.anchoredPosition = new Vector2(0, -10);
        entriesRt.sizeDelta = new Vector2(580, 0);

        _leaderboardEntriesContainer = entriesContainer.transform;

        if (serializedCloseButton != null)
        {
            serializedCloseButton.onClick.RemoveAllListeners();
            serializedCloseButton.onClick.AddListener(() => { PlayButtonSound(); HideLeaderboard(); });
            _primaryButtons.Add(serializedCloseButton);
        }
        else
        {
            var closeBtn = CreateButton(_leaderboardPanel.transform, "CLOSE", Color.white,
                leaderboardCloseButtonPosition, leaderboardCloseButtonSize, leaderboardCloseButtonFontSize, () =>
                {
                    HideLeaderboard();
                });
            _primaryButtons.Add(closeBtn.GetComponent<Button>());
        }

        _leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        if (_leaderboardPanel != null)
        {
            // ⭐ 현재 활성화된 패널 저장 (돌아갈 곳)
            if (_mainMenuPanel != null && _mainMenuPanel.activeSelf)
            {
                _previousActivePanel = _mainMenuPanel;
                Debug.Log("[UIManager] 📍 Leaderboard opened from Main Menu");
            }
            else if (_resultPanel != null && _resultPanel.activeSelf)
            {
                _previousActivePanel = _resultPanel;
                Debug.Log("[UIManager] 📍 Leaderboard opened from Result Panel");
            }
            else
            {
                _previousActivePanel = _mainMenuPanel; // 기본값
                Debug.Log("[UIManager] 📍 Leaderboard opened (default to Main Menu)");
            }
            
            _leaderboardPanel.SetActive(true);
            _mainMenuPanel?.SetActive(false);
            _resultPanel?.SetActive(false);
            
            Debug.Log("[UIManager] ShowLeaderboard called");
            
            _leaderboardRetryCount = 0;

#if UNITY_EDITOR
            ShowPlaceholderLeaderboard();
#else
            ShowLoadingMessage();
            
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.LoadLeaderboard(OnLeaderboardLoaded);
            }
            else
            {
                Debug.LogWarning("[UIManager] FirebaseManager is null");
                DisplayLeaderboard(new List<LeaderboardEntry>());
            }
#endif
        }
        else
        {
            Debug.LogError("[UIManager] ❌ Leaderboard panel is NULL!");
        }
    }
    
    private void ShowLoadingMessage()
    {
        if (_leaderboardEntriesContainer == null) return;
        foreach (Transform child in _leaderboardEntriesContainer)
            Destroy(child.gameObject);
        
        var loadingMsg = CreateTextElement(_leaderboardEntriesContainer, 
            "Loading...",
            32, CAT_EYE_GOLD, new Vector2(0, -100), new Vector2(550, 100), isPrimary: false);
        loadingMsg.alignment = TextAlignmentOptions.Center;
        loadingMsg.fontStyle = FontStyles.Bold;
    }


    public void HideLeaderboard()
    {
        if (_leaderboardPanel != null)
        {
            _leaderboardPanel.SetActive(false);
            
            // ⭐ 이전에 활성화되었던 패널로 돌아가기
            if (_previousActivePanel != null)
            {
                _previousActivePanel.SetActive(true);
                Debug.Log($"[UIManager] 🔙 Returning to {_previousActivePanel.name}");
            }
            else
            {
                // 이전 패널 정보가 없으면 메인 메뉴로 (안전장치)
                _mainMenuPanel?.SetActive(true);
                Debug.Log("[UIManager] 🔙 Returning to Main Menu (fallback)");
            }
        }
    }

    private int _leaderboardRetryCount = 0;
    private const int MAX_LEADERBOARD_RETRIES = 3;

    private void OnLeaderboardLoaded(System.Collections.Generic.List<LeaderboardEntry> entries)
    {
        if (entries == null && _leaderboardRetryCount < MAX_LEADERBOARD_RETRIES)
        {
            _leaderboardRetryCount++;
            Debug.Log($"[UIManager] Firebase not ready, retrying... ({_leaderboardRetryCount}/{MAX_LEADERBOARD_RETRIES})");
            StartCoroutine(RetryLoadLeaderboard());
            return;
        }

        _leaderboardRetryCount = 0;
        DisplayLeaderboard(entries ?? new System.Collections.Generic.List<LeaderboardEntry>());
    }

    private System.Collections.IEnumerator RetryLoadLeaderboard()
    {
        yield return new WaitForSeconds(1.5f);

        if (_leaderboardPanel != null && _leaderboardPanel.activeSelf && FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.LoadLeaderboard(OnLeaderboardLoaded);
        }
    }

    private void DisplayLeaderboard(System.Collections.Generic.List<LeaderboardEntry> entries)
    {
        Debug.Log($"[UIManager] DisplayLeaderboard called with {(entries == null ? "null" : entries.Count.ToString())} entries");
        
        if (_leaderboardEntriesContainer == null)
        {
            Debug.LogError("[UIManager] ❌ _leaderboardEntriesContainer is NULL! Cannot display leaderboard.");
            return;
        }
        
        // 기존 엔트리 삭제
        foreach (Transform child in _leaderboardEntriesContainer)
        {
            Destroy(child.gameObject);
        }

        // 엔트리가 없으면 메시지 표시
        if (entries == null || entries.Count == 0)
        {
            Debug.LogWarning("[UIManager] No leaderboard entries to display");
            var emptyMsg = CreateTextElement(_leaderboardEntriesContainer, 
                leaderboardEmptyMessage,
                leaderboardEmptyFontSize, leaderboardNormalTextColor, new Vector2(0, -100), new Vector2(550, 200), isPrimary: false);
            emptyMsg.alignment = TextAlignmentOptions.Center;
            
            var containerRt = _leaderboardEntriesContainer.GetComponent<RectTransform>();
            containerRt.sizeDelta = new Vector2(containerRt.sizeDelta.x, 500);
            return;
        }

        Debug.Log($"[UIManager] Creating {entries.Count} leaderboard entries");
        float yOffset = 0f;
        int rank = 1;

        foreach (var entry in entries)
        {
            CreateLeaderboardEntry(rank, entry.nickname, entry.score, yOffset);
            yOffset -= leaderboardEntrySpacing;
            rank++;
        }

        // 컨테이너 높이 조정
        var containerRt2 = _leaderboardEntriesContainer.GetComponent<RectTransform>();
        containerRt2.sizeDelta = new Vector2(containerRt2.sizeDelta.x, Mathf.Max(500, -yOffset + 50));
        
        Debug.Log("[UIManager] Leaderboard display complete");
    }

    private void CreateLeaderboardEntry(int rank, string nickname, int score, float yOffset)
    {
        var entry = new GameObject($"Entry_{rank}");
        entry.transform.SetParent(_leaderboardEntriesContainer, false);
        
        var entryRt = entry.AddComponent<RectTransform>();
        entryRt.anchorMin = new Vector2(0, 1);
        entryRt.anchorMax = new Vector2(1, 1);
        entryRt.pivot = new Vector2(0.5f, 1);
        entryRt.anchoredPosition = new Vector2(0, yOffset);
        entryRt.sizeDelta = new Vector2(0, leaderboardEntryHeight);

        var bg = entry.AddComponent<Image>();
        if (rank == 1)
            bg.color = leaderboardGoldColor;
        else if (rank == 2)
            bg.color = leaderboardSilverColor;
        else if (rank == 3)
            bg.color = leaderboardBronzeColor;
        else
            bg.color = leaderboardDefaultColor;
        
        bg.sprite = CreateRoundedRectSprite(100, 100, 10);
        bg.type = Image.Type.Sliced;

        Color rankScoreColor = rank <= 3 ? leaderboardTopRankColor : leaderboardNormalTextColor;
        Color nickColor = rank <= 3 ? leaderboardTopNicknameColor : leaderboardNormalTextColor;

        var rankText = CreateTextElement(entry.transform, $"{rank}.",
            leaderboardRankFontSize, rankScoreColor, 
            new Vector2(-250, 0), new Vector2(60, leaderboardEntryHeight), isPrimary: false);
        rankText.alignment = TextAlignmentOptions.MidlineLeft;
        rankText.fontStyle = FontStyles.Bold;
        if (leaderboardEntryFont != null) rankText.font = leaderboardEntryFont;

        var nicknameText = CreateTextElement(entry.transform, nickname,
            leaderboardNicknameFontSize, nickColor,
            new Vector2(-50, 0), new Vector2(300, leaderboardEntryHeight), isPrimary: rank <= 3);
        nicknameText.alignment = TextAlignmentOptions.MidlineLeft;
        if (leaderboardEntryFont != null) nicknameText.font = leaderboardEntryFont;

        var scoreText = CreateTextElement(entry.transform, score.ToString(),
            leaderboardScoreFontSize, rankScoreColor,
            new Vector2(200, 0), new Vector2(120, leaderboardEntryHeight), isPrimary: false);
        scoreText.alignment = TextAlignmentOptions.MidlineRight;
        scoreText.fontStyle = FontStyles.Bold;
        if (leaderboardEntryFont != null) scoreText.font = leaderboardEntryFont;
    }

    private void ShowPlaceholderLeaderboard()
    {
        var placeholderData = new System.Collections.Generic.List<LeaderboardEntry>
        {
            new LeaderboardEntry { nickname = "Cat_Master", score = 2500 },
            new LeaderboardEntry { nickname = "Paw_King", score = 2100 },
            new LeaderboardEntry { nickname = "Meow_Pro", score = 1850 },
            new LeaderboardEntry { nickname = "Nyan_Chan", score = 1650 },
            new LeaderboardEntry { nickname = "Purr_Lord", score = 1420 },
            new LeaderboardEntry { nickname = "Kitty_Ace", score = 1280 },
            new LeaderboardEntry { nickname = "Whisker_99", score = 1050 },
            new LeaderboardEntry { nickname = "Fluffy_Cat", score = 890 },
            new LeaderboardEntry { nickname = "Shadow_Paw", score = 720 },
            new LeaderboardEntry { nickname = "Tiger_Claw", score = 650 },
        };
        
        DisplayLeaderboard(placeholderData);
    }

    // ─── 플로팅 점수 ───

    private void SpawnFloatingScore(int points, Vector3 worldPos)
    {
        StartCoroutine(CoFloatingScore(points, worldPos));
    }

    private IEnumerator CoFloatingScore(int points, Vector3 worldPos)
    {
        var go = new GameObject("FloatingScore");
        go.transform.position = worldPos;

        var tmp = go.AddComponent<TextMeshPro>();
        
        // 커스텀 폰트 적용
        if (floatingScoreFont != null)
            tmp.font = floatingScoreFont;
        
        tmp.text = $"+{points}";
        tmp.fontSize = floatingScoreFontSize;
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

    // ─── 고양이 실루엣 ───

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
        
        // ⭐ 애니메이션을 위해 저장
        _catSilhouette = go;
    }

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

                if (headDist <= headR) inHead = true;

                if (IsInTriangle(fx, fy, s * 0.14f, s * 0.60f, s * 0.06f, s * 0.96f, s * 0.42f, s * 0.70f))
                    inHead = true;
                if (IsInTriangle(fx, fy, s * 0.18f, s * 0.64f, s * 0.12f, s * 0.88f, s * 0.36f, s * 0.70f))
                    inInnerEar = true;

                if (IsInTriangle(fx, fy, s * 0.86f, s * 0.60f, s * 0.94f, s * 0.96f, s * 0.58f, s * 0.70f))
                    inHead = true;
                if (IsInTriangle(fx, fy, s * 0.82f, s * 0.64f, s * 0.88f, s * 0.88f, s * 0.64f, s * 0.70f))
                    inInnerEar = true;

                {
                    float ecx = s * 0.35f, ecy = s * 0.42f;
                    float erx = s * 0.07f, ery = s * 0.05f;
                    float dx = (fx - ecx) / erx;
                    float dy = (fy - ecy) / ery;
                    if (dx * dx + dy * dy <= 1f) inEye = true;
                }

                {
                    float ecx = s * 0.65f, ecy = s * 0.42f;
                    float erx = s * 0.07f, ery = s * 0.05f;
                    float dx = (fx - ecx) / erx;
                    float dy = (fy - ecy) / ery;
                    if (dx * dx + dy * dy <= 1f) inEye = true;
                }

                if (IsInTriangle(fx, fy, cx, s * 0.33f, cx - s * 0.025f, s * 0.37f, cx + s * 0.025f, s * 0.37f))
                    inNose = true;

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

    // ─── 헬퍼 메서드 ───

    private GameObject CreatePanel(string name, Color bgColor, bool trackForTheme, Transform parent = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent != null ? parent : _canvas.transform, false);

        var rt = go.AddComponent<RectTransform>();

        if (bgColor.a > 0.01f)
        {
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            if (trackForTheme)
                _panelBackgrounds.Add(img);
        }

        return go;
    }

    private TextMeshProUGUI CreateTextElement(Transform parent, string text,
        float fontSize, Color color, Vector2 anchoredPos, Vector2 sizeDelta, bool isPrimary)
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

        if (isPrimary)
            _primaryTexts.Add(tmp);

        return tmp;
    }

    /// <summary>
    /// 버튼 클릭 사운드를 재생합니다
    /// </summary>
    private void PlayButtonSound()
    {
        if (buttonClickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.GetComponent<AudioSource>()?.PlayOneShot(buttonClickSound);
        }
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
        img.sprite = CreateRoundedRectSprite(100, 100, 20); // 둥근 네모 스프라이트
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
        
        // 버튼 클릭 시 사운드 재생 + 원래 액션 실행
        btn.onClick.AddListener(() =>
        {
            PlayButtonSound();
            onClick?.Invoke();
        });

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        SetFullStretch(textRt);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        
        // 배경색이 밝으면 검은 텍스트, 어두우면 흰 텍스트
        float brightness = bgColor.r * 0.299f + bgColor.g * 0.587f + bgColor.b * 0.114f;
        tmp.color = brightness > 0.5f ? new Color(0.2f, 0.2f, 0.2f) : Color.white;
        
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return go;
    }

    /// <summary>
    /// 버튼 이미지 색상을 초기화합니다 (Inspector 설정 무시)
    /// </summary>
    private void SetupSerializedButton(Button btn, Color color, UnityEngine.Events.UnityAction onClick)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(onClick);
    }

    private void ResetButtonColor(Button btn, Color color)
    {
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = color;
            
            // ColorBlock도 업데이트
            var colors = btn.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            colors.disabledColor = Color.Lerp(color, Color.gray, 0.5f);
            btn.colors = colors;
            
            Debug.Log($"[UIManager] Button '{btn.name}' color reset to {color}");
        }
    }

    /// <summary>
    /// 둥근 네모 스프라이트를 프로그래매틱하게 생성합니다
    /// </summary>
    private Sprite CreateRoundedRectSprite(int width, int height, int cornerRadius)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        
        // 모든 픽셀을 투명하게 초기화
        var pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // 둥근 네모 그리기
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isInside = false;

                // 모서리 영역 체크
                float dx = 0, dy = 0;

                // 좌상단
                if (x < cornerRadius && y < cornerRadius)
                {
                    dx = cornerRadius - x;
                    dy = cornerRadius - y;
                    isInside = (dx * dx + dy * dy) <= (cornerRadius * cornerRadius);
                }
                // 우상단
                else if (x >= width - cornerRadius && y < cornerRadius)
                {
                    dx = x - (width - cornerRadius - 1);
                    dy = cornerRadius - y;
                    isInside = (dx * dx + dy * dy) <= (cornerRadius * cornerRadius);
                }
                // 좌하단
                else if (x < cornerRadius && y >= height - cornerRadius)
                {
                    dx = cornerRadius - x;
                    dy = y - (height - cornerRadius - 1);
                    isInside = (dx * dx + dy * dy) <= (cornerRadius * cornerRadius);
                }
                // 우하단
                else if (x >= width - cornerRadius && y >= height - cornerRadius)
                {
                    dx = x - (width - cornerRadius - 1);
                    dy = y - (height - cornerRadius - 1);
                    isInside = (dx * dx + dy * dy) <= (cornerRadius * cornerRadius);
                }
                // 모서리가 아닌 영역은 모두 포함
                else
                {
                    isInside = true;
                }

                if (isInside)
                    pixels[y * width + x] = Color.white;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        // 9-slice를 위한 border 설정
        var border = new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius);
        var sprite = Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            border
        );

        return sprite;
    }

    private void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    // ─── 메인 메뉴 애니메이션 ───

    /// <summary>
    /// 메인 메뉴 타이틀 애니메이션 (부드러운 파동 효과)
    /// </summary>
    private IEnumerator CoAnimateMenuTitle()
    {
        if (_titleText == null || _subtitleText == null || _decorationText == null)
            yield break;

        Vector3 titleStartPos = _titleText.rectTransform.anchoredPosition;
        Vector3 subtitleStartPos = _subtitleText.rectTransform.anchoredPosition;
        Vector3 decorationStartPos = _decorationText.rectTransform.anchoredPosition;

        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;

            // 타이틀: 위아래로 부드럽게 흔들리기
            float titleOffset = Mathf.Sin(time * 1.5f) * 15f;
            _titleText.rectTransform.anchoredPosition = titleStartPos + new Vector3(0, titleOffset, 0);
            
            // 약간의 크기 변화
            float titleScale = 1f + Mathf.Sin(time * 1.5f) * 0.05f;
            _titleText.rectTransform.localScale = Vector3.one * titleScale;

            // 부제: 타이틀보다 약간 느리게, 반대 방향
            float subtitleOffset = Mathf.Sin(time * 1.2f + 0.5f) * 8f;
            _subtitleText.rectTransform.anchoredPosition = subtitleStartPos + new Vector3(0, subtitleOffset, 0);

            // 고양이 장식: 좌우로 살짝 흔들리기
            float decorationOffset = Mathf.Sin(time * 2f) * 5f;
            _decorationText.rectTransform.anchoredPosition = decorationStartPos + new Vector3(decorationOffset, 0, 0);
            
            // 회전도 살짝
            float decorationRotation = Mathf.Sin(time * 2f) * 3f;
            _decorationText.rectTransform.localRotation = Quaternion.Euler(0, 0, decorationRotation);

            yield return null;
        }
    }
    
    /// <summary>
    /// 고양이 로고 펄스 애니메이션 (커졌다 작아졌다)
    /// </summary>
    private IEnumerator CoAnimateCatSilhouette()
    {
        if (_catSilhouette == null)
        {
            Debug.LogWarning("[UIManager] ⚠️ CoAnimateCatSilhouette: _catSilhouette is null!");
            _catSilhouetteAnimating = false;
            yield break;
        }
        
        Debug.Log($"[UIManager] 🐱 Cat silhouette animation loop started for '{_catSilhouette.name}'");
        float time = 0f;
        
        while (true)
        {
            if (_catSilhouette == null) // 안전 체크
            {
                Debug.LogWarning("[UIManager] Cat silhouette destroyed during animation!");
                _catSilhouetteAnimating = false;
                yield break;
            }
            
            time += Time.deltaTime;
            
            float scale = 1f + Mathf.Sin(time * catAnimationSpeed) * catAnimationAmplitude;
            _catSilhouette.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
    }

    // ─── 닉네임 입력 ───

    /// <summary>
    /// 닉네임 입력 UI 생성
    /// </summary>
    private void CreateNicknameInput(Transform parent)
    {
        // 컨테이너
        var container = new GameObject("NicknameContainer");
        container.transform.SetParent(parent, false);
        var containerRt = container.AddComponent<RectTransform>();
        containerRt.anchoredPosition = new Vector2(0, -30);
        containerRt.sizeDelta = new Vector2(500, 60);

        // 라벨 "Nickname:"
        var label = new GameObject("Label");
        label.transform.SetParent(container.transform, false);
        var labelRt = label.AddComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0, 0.5f);
        labelRt.anchorMax = new Vector2(0, 0.5f);
        labelRt.pivot = new Vector2(0, 0.5f);
        labelRt.anchoredPosition = new Vector2(0, 0);
        labelRt.sizeDelta = new Vector2(120, 40);

        var labelText = label.AddComponent<TextMeshProUGUI>();
        labelText.text = "Nickname:";
        labelText.fontSize = 24;
        labelText.color = Color.gray;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        _secondaryTexts.Add(labelText);

        // 입력 필드
        var inputFieldGo = new GameObject("InputField");
        inputFieldGo.transform.SetParent(container.transform, false);
        var inputFieldRt = inputFieldGo.AddComponent<RectTransform>();
        inputFieldRt.anchorMin = new Vector2(0, 0.5f);
        inputFieldRt.anchorMax = new Vector2(1, 0.5f);
        inputFieldRt.pivot = new Vector2(0, 0.5f);
        inputFieldRt.anchoredPosition = new Vector2(120, 0);
        inputFieldRt.sizeDelta = new Vector2(-220, 40);

        // 배경
        var inputBg = inputFieldGo.AddComponent<Image>();
        inputBg.color = new Color(1f, 1f, 1f, 0.8f);
        inputBg.sprite = CreateRoundedRectSprite(100, 100, 10);
        inputBg.type = Image.Type.Sliced;

        // TMP InputField
        _nicknameInputField = inputFieldGo.AddComponent<TMP_InputField>();
        _nicknameInputField.textComponent = CreateInputTextComponent(inputFieldGo.transform);
        _nicknameInputField.characterLimit = 12;
        _nicknameInputField.text = _currentNickname;

        // 변경 버튼
        var changeBtn = new GameObject("ChangeButton");
        changeBtn.transform.SetParent(container.transform, false);
        var changeBtnRt = changeBtn.AddComponent<RectTransform>();
        changeBtnRt.anchorMin = new Vector2(1, 0.5f);
        changeBtnRt.anchorMax = new Vector2(1, 0.5f);
        changeBtnRt.pivot = new Vector2(1, 0.5f);
        changeBtnRt.anchoredPosition = new Vector2(0, 0);
        changeBtnRt.sizeDelta = new Vector2(90, 40);

        var changeBtnImg = changeBtn.AddComponent<Image>();
        changeBtnImg.sprite = CreateRoundedRectSprite(100, 100, 10);
        changeBtnImg.color = CAT_EYE_GOLD;
        changeBtnImg.type = Image.Type.Sliced;

        _nicknameChangeButton = changeBtn.AddComponent<Button>();
        _nicknameChangeButton.targetGraphic = changeBtnImg;
        _nicknameChangeButton.onClick.AddListener(OnNicknameChangeClicked);

        var changeBtnLabel = new GameObject("Label");
        changeBtnLabel.transform.SetParent(changeBtn.transform, false);
        var changeBtnLabelRt = changeBtnLabel.AddComponent<RectTransform>();
        SetFullStretch(changeBtnLabelRt);

        var changeBtnText = changeBtnLabel.AddComponent<TextMeshProUGUI>();
        changeBtnText.text = "SAVE";
        changeBtnText.fontSize = 18;
        changeBtnText.color = new Color(0.2f, 0.2f, 0.2f);
        changeBtnText.alignment = TextAlignmentOptions.Center;
        changeBtnText.fontStyle = FontStyles.Bold;

        _secondaryButtons.Add(_nicknameChangeButton);
    }

    /// <summary>
    /// InputField용 텍스트 컴포넌트 생성
    /// </summary>
    private TextMeshProUGUI CreateInputTextComponent(Transform parent)
    {
        var textArea = new GameObject("Text Area");
        textArea.transform.SetParent(parent, false);
        var textAreaRt = textArea.AddComponent<RectTransform>();
        textAreaRt.anchorMin = Vector2.zero;
        textAreaRt.anchorMax = Vector2.one;
        textAreaRt.sizeDelta = new Vector2(-10, -10);
        textAreaRt.anchoredPosition = Vector2.zero;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(textArea.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        SetFullStretch(textRt);

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 20;
        tmp.color = new Color(0.2f, 0.2f, 0.2f);
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.margin = new Vector4(5, 0, 5, 0);

        return tmp;
    }

    /// <summary>
    /// 닉네임 변경 버튼 클릭
    /// </summary>
    private void OnNicknameChangeClicked()
    {
        PlayButtonSound();
        Debug.Log("[UIManager] ⭐ SAVE button clicked!");
        
        if (_nicknameInputField == null)
        {
            Debug.LogError("[UIManager] ❌ Nickname input field is NULL!");
            return;
        }
        
        string newNickname = _nicknameInputField.text.Trim();
        
        if (string.IsNullOrEmpty(newNickname))
        {
            Debug.LogWarning("[UIManager] Empty nickname, cannot save");
            return;
        }

        _currentNickname = newNickname;
        PlayerPrefs.SetString("PlayerNickname", newNickname);
        PlayerPrefs.Save();
        Debug.Log($"[UIManager] Nickname saved: '{_currentNickname}'");
        
        SubmitSessionIfNeeded();
        
        StartCoroutine(CoShowSavedFeedback());
    }

    /// <summary>
    /// 저장 완료 피드백
    /// </summary>
    private IEnumerator CoShowSavedFeedback()
    {
        var btnText = _nicknameChangeButton.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            string original = btnText.text;
            btnText.text = "SAVED!";
            yield return new WaitForSeconds(1f);
            btnText.text = original;
        }
    }
}
