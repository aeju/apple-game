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
    private Button _hudThemeToggleButton;
    private TextMeshProUGUI _hudThemeToggleText;
    
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
    private Button _themeToggleButton;
    private TextMeshProUGUI _themeToggleText;

    // 랭킹
    private GameObject _leaderboardPanel;
    private Transform _leaderboardEntriesContainer;

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

        // 초기 테마 적용
        Debug.Log("[UIManager] Applying theme...");
        ApplyTheme();
        Debug.Log("[UIManager] Initialize complete!");
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
                            Debug.Log("[UIManager] 🚪 CLOSE button clicked!");
                            HideLeaderboard();
                        });
                        Debug.Log("[UIManager] ✅ CLOSE button event reconnected!");
                        break;
                    }
                }
            }
        }

        // HUD 요소 재참조
        var topBar = _hudPanel.transform.Find("TopBar");
        if (topBar != null)
        {
            var texts = topBar.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in texts)
            {
                // 숫자만 있는 텍스트 (0 또는 숫자) → Score
                if (txt.name == "Text" && (txt.text == "0" || (int.TryParse(txt.text, out _) && !txt.text.Contains(":"))))
                {
                    // 좌측에 있는 숫자 텍스트만 Score로 인식 (x < 0)
                    var rt = txt.GetComponent<RectTransform>();
                    if (rt != null && rt.anchoredPosition.x < -100)
                    {
                        _scoreText = txt;
                        // ⭐ 점수는 항상 검정색 (다크모드에서도)
                        _scoreText.color = new Color(0.17f, 0.17f, 0.18f);
                        Debug.Log($"[UIManager] Found _scoreText: {txt.text} at position {rt.anchoredPosition}, color set to BLACK");
                    }
                }
                // 시간 형식 (2:00 등) → Timer
                else if (txt.name == "Text" && txt.text.Contains(":"))
                {
                    _timerText = txt;
                    // ⭐ 타이머는 항상 검정색 (다크모드에서도)
                    _timerText.color = new Color(0.17f, 0.17f, 0.18f);
                    Debug.Log($"[UIManager] Found _timerText: {txt.text}, color set to BLACK");
                }
                // SCORE 라벨
                else if (txt.text.Contains("SCORE"))
                {
                    _scoreLabelText = txt;
                    // ⭐ SCORE 라벨도 항상 검정색
                    _scoreLabelText.color = new Color(0.17f, 0.17f, 0.18f);
                    Debug.Log($"[UIManager] Found _scoreLabelText: {txt.text}, color set to BLACK");
                }
            }

            var slider = topBar.GetComponentInChildren<Slider>(true);
            if (slider != null)
            {
                _timerSlider = slider;
                _timerSliderFill = slider.fillRect?.GetComponent<Image>();
            }

            // Combo 텍스트 찾기 (우측 상단)
            foreach (var txt in texts)
            {
                var rt = txt.GetComponent<RectTransform>();
                if (rt != null && rt.anchoredPosition.x > 200 && txt.text == "")
                {
                    _comboText = txt;
                    Debug.Log($"[UIManager] Found _comboText at position {rt.anchoredPosition}");
                    break;
                }
            }
        }

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

        // BGM 컨트롤 재참조 또는 생성
        var bgmControl = _hudPanel.transform.Find("BGMControl");
        if (bgmControl != null)
        {
            Debug.Log("[UIManager] BGMControl found, re-referencing...");
            _bgmToggle = bgmControl.GetComponentInChildren<Toggle>(true);
            if (_bgmToggle != null)
            {
                // 토글 이벤트 재연결
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
            }

            var sliders = bgmControl.GetComponentsInChildren<Slider>(true);
            foreach (var slider in sliders)
            {
                if (slider.name == "BGMVolumeSlider")
                {
                    _bgmVolumeSlider = slider;
                    // 슬라이더 이벤트 재연결
                    _bgmVolumeSlider.onValueChanged.RemoveAllListeners();
                    _bgmVolumeSlider.onValueChanged.AddListener((value) =>
                    {
                        if (_bgmVolumeText != null)
                            _bgmVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
                        
                        if (AudioManager.Instance != null)
                            AudioManager.Instance.SetBGMVolume(value);
                    });
                }
            }

            var bgmTexts = bgmControl.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in bgmTexts)
            {
                if (txt.text.Contains("%"))
                    _bgmVolumeText = txt;
            }
            
            // AudioManager의 현재 볼륨으로 초기화
            if (_bgmVolumeSlider != null && AudioManager.Instance != null)
            {
                float currentVolume = AudioManager.Instance.GetComponent<AudioSource>()?.volume ?? 0.5f;
                _bgmVolumeSlider.value = currentVolume;
                if (_bgmVolumeText != null)
                    _bgmVolumeText.text = $"{Mathf.RoundToInt(currentVolume * 100)}%";
                Debug.Log($"[UIManager] BGM volume re-initialized to {currentVolume * 100}%");
            }
        }
        else
        {
            // BGMControl이 없으면 생성
            Debug.LogWarning("[UIManager] BGMControl not found! Creating new BGM control...");
            CreateBGMControl();
        }

        // 카운트다운 요소 (GameObject 이름으로 안전하게 찾기)
        var countdownNumber = _countdownPanel.transform.Find("CountdownNumber");
        if (countdownNumber != null)
        {
            _countdownText = countdownNumber.GetComponent<TextMeshProUGUI>();
            Debug.Log($"[UIManager] Found countdown text: {_countdownText != null}");
        }
        
        var countdownCat = _countdownPanel.transform.Find("CatFace");
        if (countdownCat != null)
        {
            _countdownCatFace = countdownCat.GetComponent<TextMeshProUGUI>();
            Debug.Log($"[UIManager] Found countdown cat face: {_countdownCatFace != null}");
        }
        
        // 이름으로 못 찾았으면 텍스트 내용으로 폴백
        if (_countdownText == null || _countdownCatFace == null)
        {
            Debug.LogWarning("[UIManager] Countdown elements not found by name, trying fallback search...");
            var cdTexts = _countdownPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            Debug.Log($"[UIManager] Found {cdTexts.Length} TextMeshProUGUI components in CountdownPanel");
            
            foreach (var txt in cdTexts)
            {
                Debug.Log($"[UIManager] Checking text: '{txt.text}' on GameObject: '{txt.gameObject.name}'");
                
                if (_countdownText == null && (txt.text == "3" || txt.text.Length <= 2 && !txt.text.Contains("=")))
                    _countdownText = txt;
                else if (_countdownCatFace == null && txt.text.Contains("="))
                    _countdownCatFace = txt;
            }
            Debug.Log($"[UIManager] Fallback search - text: {_countdownText != null}, cat: {_countdownCatFace != null}");
        }
        
        // 여전히 못 찾았으면 카운트다운 패널 재생성
        if (_countdownText == null || _countdownCatFace == null)
        {
            Debug.LogWarning("[UIManager] Countdown elements not found. Recreating CountdownPanel...");
            if (_countdownPanel != null)
                Destroy(_countdownPanel);
            CreateCountdownPanel();
            Debug.Log("[UIManager] CountdownPanel recreated successfully.");
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
        
        // 필수 요소가 없으면 결과 패널 재생성
        if (_resultScoreText == null || _resultCatFace == null || _resultNewRecordText == null)
        {
            Debug.LogWarning("[UIManager] Result panel elements missing, recreating result panel...");
            Destroy(_resultPanel);
            CreateResultPanel();
        }
        else
        {
            // 닉네임 입력 필드 재참조
            _nicknameInputField = _resultPanel.GetComponentInChildren<TMP_InputField>(true);
            if (_nicknameInputField != null)
            {
                _nicknameInputField.text = _currentNickname;
                
                // 변경 버튼 재연결
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
                // 닉네임 입력 필드가 없으면 생성
                Debug.LogWarning("[UIManager] Nickname input not found, creating...");
                var nicknameContainer = _resultPanel.transform.Find("NicknameContainer");
                if (nicknameContainer != null)
                    Destroy(nicknameContainer.gameObject);
                CreateNicknameInput(_resultPanel.transform);
            }
        }

        // 메인메뉴 요소
        var menuTexts = _mainMenuPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in menuTexts)
        {
            if (txt.text.Contains("Best:"))
                _menuBestScoreText = txt;
            else if (txt.text == "☀" || txt.text == "☾")
                _themeToggleText = txt;
            else if (txt.text.Contains("PAW 10"))
                _titleText = txt;
            else if (txt.text.Contains("Drag paws"))
                _subtitleText = txt;
            else if (txt.text.Contains("=^._.^="))
                _decorationText = txt;
        }
        
        // 타이틀 애니메이션 시작
        if (_titleText != null && _subtitleText != null && _decorationText != null)
        {
            StartCoroutine(CoAnimateMenuTitle());
        }

        // 메인메뉴 버튼 재연결 (중요!)
        var menuButtons = _mainMenuPanel.GetComponentsInChildren<Button>(true);
        Debug.Log($"[UIManager] Found {menuButtons.Length} buttons in MainMenu");
        
        bool hasPlayButton = false;
        bool hasRankingButton = false;
        
        foreach (var btn in menuButtons)
        {
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                Debug.Log($"[UIManager] Button found with label: {label.text}");
                
                if (label.text == "PLAY")
                {
                    hasPlayButton = true;
                    // PLAY 버튼 이벤트 재연결
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        PlayButtonSound();
                        Debug.Log("[UIManager] PLAY button clicked!");
                        GameManager.Instance?.StartGame();
                    });
                    Debug.Log("[UIManager] PLAY button event reconnected!");
                    
                    // 버튼 색상 초기화 (진분홍)
                    ResetButtonColor(btn, DEEP_PINK);
                    
                    // 커스텀 폰트 적용
                    if (menuButtonFont != null)
                        label.font = menuButtonFont;
                    
                    // 폰트 크기 적용
                    label.fontSize = menuButtonFontSize;
                }
                else if (label.text == "RANKING")
                {
                    hasRankingButton = true;
                    // RANKING 버튼 이벤트 재연결
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        PlayButtonSound();
                        Debug.Log("[UIManager] RANKING button clicked!");
                        ShowLeaderboard();
                    });
                    Debug.Log("[UIManager] RANKING button event reconnected!");
                    
                    // 버튼 색상 초기화 (진파랑)
                    ResetButtonColor(btn, DEEP_BLUE);
                    
                    // 커스텀 폰트 적용
                    if (menuButtonFont != null)
                        label.font = menuButtonFont;
                    
                    // 폰트 크기 적용
                    label.fontSize = menuButtonFontSize;
                }
                else if (label.text == "DAY" || label.text == "NIGHT")
                {
                    _themeToggleButton = btn;
                    _themeToggleText = label;
                    
                    // ⭐ 이모지가 아닌 텍스트로 강제 변경
                    label.text = ThemeManager.Instance != null && ThemeManager.Instance.IsLightTheme() ? "DAY" : "NIGHT";
                    
                    // 텍스트 크기 조정 (3배)
                    label.fontSize = 15;
                    
                    // 버튼 크기 조정
                    var btnRt = btn.GetComponent<RectTransform>();
                    if (btnRt != null)
                        btnRt.sizeDelta = new Vector2(120, 50);
                    
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        PlayButtonSound();
                        Debug.Log("[UIManager] Theme toggle clicked!");
                        ThemeManager.Instance?.ToggleTheme();
                    });
                    
                    // 버튼 배경 투명하게
                    var img = btn.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(1f, 1f, 1f, 0f);
                        var colors = btn.colors;
                        colors.normalColor = new Color(1f, 1f, 1f, 0f);
                        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.3f);
                        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                        btn.colors = colors;
                    }
                }
            }
        }
        
        // ⭐ 테마 토글 버튼이 없으면 생성 (메인 메뉴)
        if (_themeToggleButton == null)
        {
            Debug.Log("[UIManager] Theme toggle button not found in MainMenu, creating...");
            
            var themeBtn = new GameObject("ThemeToggle");
            themeBtn.transform.SetParent(_mainMenuPanel.transform, false);
            var themeBtnRt = themeBtn.AddComponent<RectTransform>();
            themeBtnRt.anchorMin = new Vector2(1, 1); // 우측 상단 앵커
            themeBtnRt.anchorMax = new Vector2(1, 1);
            themeBtnRt.pivot = new Vector2(1, 1);
            themeBtnRt.anchoredPosition = new Vector2(-20, -20); // 우측 상단에서 20px 안쪽
            themeBtnRt.sizeDelta = new Vector2(120, 50);

            var themeBtnImg = themeBtn.AddComponent<Image>();
            themeBtnImg.color = new Color(1f, 1f, 1f, 0f); // 투명
            themeBtnImg.type = Image.Type.Sliced;

            var themeBtnComp = themeBtn.AddComponent<Button>();
            var themeBtnColors = themeBtnComp.colors;
            themeBtnColors.normalColor = new Color(1f, 1f, 1f, 0f); // 투명
            themeBtnColors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.3f); // 살짝 보임
            themeBtnColors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.5f); // 눌렀을 때만 보임
            themeBtnComp.colors = themeBtnColors;
            themeBtnComp.onClick.AddListener(() => { PlayButtonSound(); ThemeManager.Instance?.ToggleTheme(); });

            var themeBtnTextGo = new GameObject("Label");
            themeBtnTextGo.transform.SetParent(themeBtn.transform, false);
            var themeBtnTextRt = themeBtnTextGo.AddComponent<RectTransform>();
            SetFullStretch(themeBtnTextRt);
            _themeToggleText = themeBtnTextGo.AddComponent<TextMeshProUGUI>();
            _themeToggleText.text = ThemeManager.Instance != null && ThemeManager.Instance.IsLightTheme() ? "DAY" : "NIGHT";
            _themeToggleText.fontSize = 15;
            _themeToggleText.color = new Color(0.2f, 0.2f, 0.2f);
            _themeToggleText.alignment = TextAlignmentOptions.Center;
            _themeToggleText.fontStyle = FontStyles.Bold;

            _themeToggleButton = themeBtnComp;
            _secondaryButtons.Add(_themeToggleButton);
            
            Debug.Log("[UIManager] ✅ Theme toggle button created in MainMenu!");
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
                
                var playBtnGo = CreateButton(_mainMenuPanel.transform, "PLAY", DEEP_PINK,
                    new Vector2(0, -200), new Vector2(350, 80), menuButtonFontSize, () =>
                    {
                        Debug.Log("[UIManager] PLAY button clicked!");
                        GameManager.Instance?.StartGame();
                    });
                
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
            
            var rankingBtn = CreateButton(_mainMenuPanel.transform, "RANKING", DEEP_BLUE,
                new Vector2(0, -310), new Vector2(350, 80), menuButtonFontSize, () =>
                {
                    ShowLeaderboard();
                });
            
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

        // HUD 버튼 재연결
        var hudButtons2 = _hudPanel.GetComponentsInChildren<Button>(true);
        foreach (var btn in hudButtons2)
        {
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                if (label.text.Contains("HINT"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => 
                    { 
                        Debug.Log("[UIManager] HINT button clicked (hudButtons2)!");
                        GameManager.Instance?.UseHint(); 
                    });
                }
                else if (label.text.Contains("SHUFFLE"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { GameManager.Instance?.UseShuffle(); });
                }
                else if (label.text == "DAY" || label.text == "NIGHT")
                {
                    _hudThemeToggleButton = btn;
                    _hudThemeToggleText = label;
                    
                    // ⭐ 이모지가 아닌 텍스트로 강제 변경
                    label.text = ThemeManager.Instance != null && ThemeManager.Instance.IsLightTheme() ? "DAY" : "NIGHT";
                    
                    // 텍스트 크기 조정 (3배)
                    label.fontSize = 15;
                    
                    // 버튼 크기 조정
                    var btnRt = btn.GetComponent<RectTransform>();
                    if (btnRt != null)
                        btnRt.sizeDelta = new Vector2(120, 50);
                    
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { PlayButtonSound(); ThemeManager.Instance?.ToggleTheme(); });
                    
                    // 버튼 배경 투명하게
                    var img = btn.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(1f, 1f, 1f, 0f);
                        var colors = btn.colors;
                        colors.normalColor = new Color(1f, 1f, 1f, 0f);
                        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.3f);
                        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                        btn.colors = colors;
                    }
                }
            }
        }

        // 결과 화면 버튼 재연결
        var resultButtons = _resultPanel.GetComponentsInChildren<Button>(true);
        foreach (var btn in resultButtons)
        {
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                if (label.text == "RETRY")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.RestartGame(); });
                    
                    // 버튼 색상 초기화
                    ResetButtonColor(btn, CAT_EYE_GOLD);
                }
                else if (label.text == "MENU")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => { PlayButtonSound(); GameManager.Instance?.ReturnToMenu(); });
                    
                    // 버튼 색상 초기화
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

        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.OnThemeChanged.AddListener(OnThemeChanged);
        }

        Debug.Log($"[UIManager] _scoreText is null: {_scoreText == null}");
    }

    // ─── 테마 전환 ───

    private void OnThemeChanged(ThemeManager.Theme theme)
    {
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (ThemeManager.Instance == null)
        {
            Debug.LogWarning("[UIManager] ThemeManager.Instance is null! Cannot apply theme.");
            return;
        }

        bool isLight = ThemeManager.Instance.IsLightTheme();
        Debug.Log($"[UIManager] Applying theme: {(isLight ? "Light" : "Dark")}");

        // 패널 배경
        foreach (var img in _panelBackgrounds)
            if (img != null)
                img.color = ThemeManager.Instance.GetColor("UIPanel");

        // 바 배경
        foreach (var img in _barBackgrounds)
            if (img != null)
                img.color = ThemeManager.Instance.GetColor("HUDBar");

        // ⭐ Score, Timer, "SCORE" 라벨은 항상 검정색 (최우선 처리!)
        Color scoreTimerColor = new Color(0.17f, 0.17f, 0.18f); // 차콜 (항상 검정)
        
        if (_scoreText != null)
        {
            _scoreText.color = scoreTimerColor;
            Debug.Log($"[UIManager] ✅ Set _scoreText color to black: {_scoreText.color}");
        }
        
        if (_timerText != null)
        {
            _timerText.color = scoreTimerColor;
            Debug.Log($"[UIManager] ✅ Set _timerText color to black: {_timerText.color}");
        }
        
        if (_scoreLabelText != null)
        {
            _scoreLabelText.color = scoreTimerColor;
            Debug.Log($"[UIManager] ✅ Set _scoreLabelText color to black: {_scoreLabelText.color}");
        }
        
        // 텍스트 (Score/Timer/ScoreLabel 완전 제외)
        foreach (var txt in _primaryTexts)
        {
            if (txt != null && txt != _scoreText && txt != _timerText && txt != _scoreLabelText)
                txt.color = ThemeManager.Instance.GetColor("TextPrimary");
        }

        foreach (var txt in _secondaryTexts)
        {
            if (txt != null && txt != _scoreText && txt != _timerText && txt != _scoreLabelText)
                txt.color = ThemeManager.Instance.GetColor("TextSecondary");
        }

        // 버튼
        ApplyButtonTheme(_primaryButtons, "BtnPrimary");
        ApplyButtonTheme(_secondaryButtons, "BtnSecondary");
        ApplyButtonTheme(_successButtons, "BtnSuccess");
        
        // ⭐ PLAY/RANKING 버튼은 고정 색상 유지 (테마 영향 안 받음)
        var menuButtons = _mainMenuPanel?.GetComponentsInChildren<Button>(true);
        if (menuButtons != null)
        {
            foreach (var btn in menuButtons)
            {
                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    if (label.text == "PLAY")
                    {
                        ResetButtonColor(btn, DEEP_PINK);
                        Debug.Log("[UIManager] 🎨 PLAY button color restored to DEEP_PINK");
                    }
                    else if (label.text == "RANKING")
                    {
                        ResetButtonColor(btn, DEEP_BLUE);
                        Debug.Log("[UIManager] 🎨 RANKING button color restored to DEEP_BLUE");
                    }
                }
            }
        }
        
        // ⭐ 플레이 화면 버튼들도 고정 색상 유지 (HINT, SHUFFLE, RESTART, HOME)
        var hudButtons = _hudPanel?.GetComponentsInChildren<Button>(true);
        if (hudButtons != null)
        {
            foreach (var btn in hudButtons)
            {
                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    if (label.text.Contains("HINT"))
                    {
                        ResetButtonColor(btn, LIGHT_SKY);
                        Debug.Log("[UIManager] 🎨 HINT button color restored to LIGHT_SKY");
                    }
                    else if (label.text.Contains("SHUFFLE"))
                    {
                        ResetButtonColor(btn, LIGHT_PINK);
                        Debug.Log("[UIManager] 🎨 SHUFFLE button color restored to LIGHT_PINK");
                    }
                    else if (label.text == "RESTART")
                    {
                        ResetButtonColor(btn, CAT_NOSE_PINK);
                        Debug.Log("[UIManager] 🎨 RESTART button color restored to CAT_NOSE_PINK");
                    }
                    else if (label.text == "HOME")
                    {
                        ResetButtonColor(btn, CAT_PAW_CREAM);
                        Debug.Log("[UIManager] 🎨 HOME button color restored to CAT_PAW_CREAM");
                    }
                }
            }
        }

        // 토글 버튼 텍스트 업데이트 (메인 메뉴 + HUD)
        if (_themeToggleText != null)
            _themeToggleText.text = isLight ? "DAY" : "NIGHT";
        
        if (_hudThemeToggleText != null)
            _hudThemeToggleText.text = isLight ? "DAY" : "NIGHT";

        // 카메라 배경 색상 변경
        if (Camera.main != null)
            Camera.main.backgroundColor = ThemeManager.Instance.GetColor("CameraBG");
    }

    private void ApplyButtonTheme(List<Button> buttons, string colorName)
    {
        Color btnColor = ThemeManager.Instance.GetColor(colorName);
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = btnColor;

            var colors = btn.colors;
            colors.normalColor = btnColor;
            colors.highlightedColor = Color.Lerp(btnColor, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(btnColor, Color.black, 0.2f);
            colors.disabledColor = Color.Lerp(btnColor, Color.gray, 0.5f);
            btn.colors = colors;
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
            UpdateMenuBestScore();

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
        CreateCatSilhouette(_mainMenuPanel.transform, new Vector2(0, 300), new Vector2(200, 200));

        // 타이틀
        _titleText = CreateTextElement(_mainMenuPanel.transform, "PAW 10",
            72, CAT_EYE_GOLD, new Vector2(0, 150), new Vector2(800, 100), isPrimary: false);
        _titleText.fontStyle = FontStyles.Bold;

        // 부제
        _subtitleText = CreateTextElement(_mainMenuPanel.transform, "Drag paws that sum to 10!",
            22, Color.gray, new Vector2(0, 80), new Vector2(700, 50), isPrimary: false);
        _secondaryTexts.Add(_subtitleText);

        // 고양이 장식
        _decorationText = CreateTextElement(_mainMenuPanel.transform, "~  =^._.^=  ~",
            28, Color.gray, new Vector2(0, 30), new Vector2(400, 50), isPrimary: false);
        _secondaryTexts.Add(_decorationText);
        
        // 타이틀 애니메이션 시작
        StartCoroutine(CoAnimateMenuTitle());

        // Play 버튼
        var playBtn = CreateButton(_mainMenuPanel.transform, "PLAY", Color.white,
            new Vector2(0, -30), new Vector2(320, 70), 36, () =>
            {
                GameManager.Instance?.StartGame();
            });
        _primaryButtons.Add(playBtn.GetComponent<Button>());

        // Ranking 버튼
        var rankingBtn = CreateButton(_mainMenuPanel.transform, "RANKING", CAT_EYE_GOLD,
            new Vector2(0, -115), new Vector2(280, 60), 28, () =>
            {
                ShowLeaderboard();
            });
        _secondaryButtons.Add(rankingBtn.GetComponent<Button>());

        // 최고기록
        _menuBestScoreText = CreateTextElement(_mainMenuPanel.transform, "Best: 0",
            24, CAT_EYE_GOLD, new Vector2(0, -190), new Vector2(400, 50), isPrimary: false);

        // 테마 토글 버튼 (우측 상단) - 절대 위치 사용
        var themeBtn = new GameObject("ThemeToggle");
        themeBtn.transform.SetParent(_mainMenuPanel.transform, false);
        var themeBtnRt = themeBtn.AddComponent<RectTransform>();
        themeBtnRt.anchorMin = new Vector2(1, 1); // 우측 상단 앵커
        themeBtnRt.anchorMax = new Vector2(1, 1);
        themeBtnRt.pivot = new Vector2(1, 1);
        themeBtnRt.anchoredPosition = new Vector2(-20, -20); // 우측 상단에서 20px 안쪽
        themeBtnRt.sizeDelta = new Vector2(120, 50);

        var themeBtnImg = themeBtn.AddComponent<Image>();
        themeBtnImg.color = new Color(1f, 1f, 1f, 0f); // 투명
        themeBtnImg.type = Image.Type.Sliced;

        var themeBtnComp = themeBtn.AddComponent<Button>();
        var themeBtnColors = themeBtnComp.colors;
        themeBtnColors.normalColor = new Color(1f, 1f, 1f, 0f); // 투명
        themeBtnColors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.3f); // 살짝 보임
        themeBtnColors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.5f); // 눌렀을 때만 보임
        themeBtnComp.colors = themeBtnColors;
        themeBtnComp.onClick.AddListener(() => { PlayButtonSound(); ThemeManager.Instance?.ToggleTheme(); });

        var themeBtnTextGo = new GameObject("Label");
        themeBtnTextGo.transform.SetParent(themeBtn.transform, false);
        var themeBtnTextRt = themeBtnTextGo.AddComponent<RectTransform>();
        SetFullStretch(themeBtnTextRt);
        _themeToggleText = themeBtnTextGo.AddComponent<TextMeshProUGUI>();
        _themeToggleText.text = ThemeManager.Instance != null && ThemeManager.Instance.IsLightTheme() ? "DAY" : "NIGHT";
        _themeToggleText.fontSize = 15;
        _themeToggleText.color = new Color(0.2f, 0.2f, 0.2f);
        _themeToggleText.alignment = TextAlignmentOptions.Center;
        _themeToggleText.fontStyle = FontStyles.Bold;

        _themeToggleButton = themeBtnComp;
        _secondaryButtons.Add(_themeToggleButton);
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

        // 점수 (좌측)
        // ⭐ 처음부터 검정색 (다크모드에서도 검정)
        _scoreText = CreateTextElement(topBar.transform, "0",
            36, new Color(0.17f, 0.17f, 0.18f), new Vector2(-300, -10), new Vector2(200, 60), isPrimary: true);
        _scoreText.alignment = TextAlignmentOptions.Left;
        _scoreText.fontStyle = FontStyles.Bold;

        // ⭐ SCORE 라벨도 처음부터 검정색 (다크모드에서도 검정)
        _scoreLabelText = CreateTextElement(topBar.transform, "SCORE",
            18, new Color(0.17f, 0.17f, 0.18f), new Vector2(-300, -42), new Vector2(200, 30), isPrimary: false);
        _scoreLabelText.alignment = TextAlignmentOptions.Left;

        // 타이머 (중앙) - 숫자 + 슬라이더
        // ⭐ 처음부터 검정색 (다크모드에서도 검정)
        _timerText = CreateTextElement(topBar.transform, "2:00",
            38, new Color(0.17f, 0.17f, 0.18f), new Vector2(0, -8), new Vector2(200, 50), isPrimary: true);
        _timerText.fontStyle = FontStyles.Bold;
        _timerText.alignment = TextAlignmentOptions.Center;

        // 타이머 슬라이더 (원작 스타일)
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

        // 슬라이더 배경
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(sliderGo.transform, false);
        var bgRt = bgGo.AddComponent<RectTransform>();
        SetFullStretch(bgRt);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);

        // 슬라이더 Fill Area
        var fillAreaGo = new GameObject("Fill Area");
        fillAreaGo.transform.SetParent(sliderGo.transform, false);
        var fillAreaRt = fillAreaGo.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.sizeDelta = new Vector2(-4, -4);
        fillAreaRt.anchoredPosition = Vector2.zero;

        // 슬라이더 Fill
        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillAreaGo.transform, false);
        var fillRt = fillGo.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;
        fillRt.anchoredPosition = Vector2.zero;
        _timerSliderFill = fillGo.AddComponent<Image>();
        _timerSliderFill.color = new Color(0.4f, 0.8f, 0.5f); // 초록색 (시작)

        _timerSlider.fillRect = fillRt;
        _timerSlider.targetGraphic = _timerSliderFill;

        var timerLabel = CreateTextElement(topBar.transform, "TIME",
            16, CAT_EYE_GOLD, new Vector2(0, -72), new Vector2(200, 25), isPrimary: false);
        timerLabel.alignment = TextAlignmentOptions.Center;

        // 콤보 (우측)
        _comboText = CreateTextElement(topBar.transform, "",
            32, CAT_EYE_GOLD, new Vector2(300, -20), new Vector2(250, 60), isPrimary: false);
        _comboText.alignment = TextAlignmentOptions.Right;

        // 하단 버튼 바
        var bottomBar = CreatePanel("BottomBar", Color.clear, false, _hudPanel.transform);
        var botRt = bottomBar.GetComponent<RectTransform>();
        botRt.anchorMin = new Vector2(0, 0);
        botRt.anchorMax = new Vector2(1, 0);
        botRt.pivot = new Vector2(0.5f, 0);
        botRt.sizeDelta = new Vector2(0, 80);
        botRt.anchoredPosition = new Vector2(0, 10);

        // 힌트 버튼 (연한 하늘색)
        var hintBtnGo = CreateButton(bottomBar.transform, "HINT (3)", LIGHT_SKY,
            new Vector2(-240, 0), new Vector2(140, 55), 20, () =>
            {
                Debug.Log("[UIManager] HINT button clicked (CreateButton)!");
                GameManager.Instance?.UseHint();
            });
        _hintButton = hintBtnGo.GetComponent<Button>();
        _hintButtonText = hintBtnGo.GetComponentInChildren<TextMeshProUGUI>();
        _secondaryButtons.Add(_hintButton);

        // 셔플 버튼 (연한 핑크)
        var shuffleBtnGo = CreateButton(bottomBar.transform, "SHUFFLE (2)", LIGHT_PINK,
            new Vector2(-80, 0), new Vector2(140, 55), 20, () =>
            {
                GameManager.Instance?.UseShuffle();
            });
        _shuffleButton = shuffleBtnGo.GetComponent<Button>();
        _shuffleButtonText = shuffleBtnGo.GetComponentInChildren<TextMeshProUGUI>();
        _successButtons.Add(_shuffleButton);

        // 재시작 버튼 (핑크색)
        var restartBtnGo = CreateButton(bottomBar.transform, "RESTART", CAT_NOSE_PINK,
            new Vector2(80, 0), new Vector2(140, 55), 18, () =>
            {
                GameManager.Instance?.RestartGame();
            });
        _secondaryButtons.Add(restartBtnGo.GetComponent<Button>());

        // 홈 버튼
        var homeBtnGo = CreateButton(bottomBar.transform, "HOME", CAT_PAW_CREAM,
            new Vector2(240, 0), new Vector2(140, 55), 18, () =>
            {
                GameManager.Instance?.ReturnToMenu();
            });
        _secondaryButtons.Add(homeBtnGo.GetComponent<Button>());

        // HUD 테마 토글 버튼 (우측 상단, 메인 메뉴와 동일 위치)
        var hudThemeBtn = new GameObject("HUDThemeToggle");
        hudThemeBtn.transform.SetParent(_hudPanel.transform, false);
        var hudThemeBtnRt = hudThemeBtn.AddComponent<RectTransform>();
        hudThemeBtnRt.anchorMin = new Vector2(1, 1); // 우측 상단 앵커
        hudThemeBtnRt.anchorMax = new Vector2(1, 1);
        hudThemeBtnRt.pivot = new Vector2(1, 1);
        hudThemeBtnRt.anchoredPosition = new Vector2(-20, -20); // 메인 메뉴와 동일 위치
        hudThemeBtnRt.sizeDelta = new Vector2(120, 50);

        var hudThemeBtnImg = hudThemeBtn.AddComponent<Image>();
        hudThemeBtnImg.color = new Color(1f, 1f, 1f, 0f); // 투명
        hudThemeBtnImg.type = Image.Type.Sliced;

        var hudThemeBtnComp = hudThemeBtn.AddComponent<Button>();
        var hudThemeBtnColors = hudThemeBtnComp.colors;
        hudThemeBtnColors.normalColor = new Color(1f, 1f, 1f, 0f); // 투명
        hudThemeBtnColors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.3f); // 살짝 보임
        hudThemeBtnColors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.5f); // 눌렀을 때만 보임
        hudThemeBtnComp.colors = hudThemeBtnColors;
        hudThemeBtnComp.onClick.AddListener(() => { PlayButtonSound(); ThemeManager.Instance?.ToggleTheme(); });

        var hudThemeBtnTextGo = new GameObject("Label");
        hudThemeBtnTextGo.transform.SetParent(hudThemeBtn.transform, false);
        var hudThemeBtnTextRt = hudThemeBtnTextGo.AddComponent<RectTransform>();
        SetFullStretch(hudThemeBtnTextRt);
        _hudThemeToggleText = hudThemeBtnTextGo.AddComponent<TextMeshProUGUI>();
        _hudThemeToggleText.text = ThemeManager.Instance != null && ThemeManager.Instance.IsLightTheme() ? "DAY" : "NIGHT";
        _hudThemeToggleText.fontSize = 15;
        _hudThemeToggleText.color = new Color(0.2f, 0.2f, 0.2f);
        _hudThemeToggleText.alignment = TextAlignmentOptions.Center;
        _hudThemeToggleText.fontStyle = FontStyles.Bold;

        _hudThemeToggleButton = hudThemeBtnComp;
        _secondaryButtons.Add(_hudThemeToggleButton);

        // BGM 컨트롤 생성
        CreateBGMControl();

        _hudPanel.SetActive(false);
    }

    private void CreateBGMControl()
    {
        if (_hudPanel == null)
        {
            Debug.LogError("[UIManager] Cannot create BGM control - HUDPanel is null!");
            return;
        }

        // BGM 컨트롤 패널 (좌측 하단)
        var bgmPanel = CreatePanel("BGMControl", Color.clear, false, _hudPanel.transform);
        var bgmPanelRt = bgmPanel.GetComponent<RectTransform>();
        bgmPanelRt.anchorMin = new Vector2(0, 0);
        bgmPanelRt.anchorMax = new Vector2(0, 0);
        bgmPanelRt.pivot = new Vector2(0, 0);
        bgmPanelRt.anchoredPosition = new Vector2(20, 100); // 버튼 위쪽
        bgmPanelRt.sizeDelta = new Vector2(200, 80);

        // BGM 토글
        var toggleGo = new GameObject("BGMToggle");
        toggleGo.transform.SetParent(bgmPanel.transform, false);
        var toggleRt = toggleGo.AddComponent<RectTransform>();
        toggleRt.anchoredPosition = new Vector2(0, 45);
        toggleRt.sizeDelta = new Vector2(180, 25);

        _bgmToggle = toggleGo.AddComponent<Toggle>();
        _bgmToggle.isOn = true; // 기본값: BGM ON

        // 토글 배경
        var toggleBgGo = new GameObject("Background");
        toggleBgGo.transform.SetParent(toggleGo.transform, false);
        var toggleBgRt = toggleBgGo.AddComponent<RectTransform>();
        SetFullStretch(toggleBgRt);
        var toggleBgImg = toggleBgGo.AddComponent<Image>();
        toggleBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        // 체크마크
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

        // 토글 라벨
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

        // 토글 이벤트
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

        // BGM 볼륨 슬라이더
        var volumeSliderGo = new GameObject("BGMVolumeSlider");
        volumeSliderGo.transform.SetParent(bgmPanel.transform, false);
        var volumeSliderRt = volumeSliderGo.AddComponent<RectTransform>();
        volumeSliderRt.anchoredPosition = new Vector2(0, 10);
        volumeSliderRt.sizeDelta = new Vector2(130, 20);

        _bgmVolumeSlider = volumeSliderGo.AddComponent<Slider>();
        _bgmVolumeSlider.minValue = 0f;
        _bgmVolumeSlider.maxValue = 1f;
        _bgmVolumeSlider.value = 0.5f; // 기본값
        _bgmVolumeSlider.interactable = true;

        // 슬라이더 배경
        var volBgGo = new GameObject("Background");
        volBgGo.transform.SetParent(volumeSliderGo.transform, false);
        var volBgRt = volBgGo.AddComponent<RectTransform>();
        SetFullStretch(volBgRt);
        var volBgImg = volBgGo.AddComponent<Image>();
        volBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        // 슬라이더 Fill Area
        var volFillAreaGo = new GameObject("Fill Area");
        volFillAreaGo.transform.SetParent(volumeSliderGo.transform, false);
        var volFillAreaRt = volFillAreaGo.AddComponent<RectTransform>();
        volFillAreaRt.anchorMin = Vector2.zero;
        volFillAreaRt.anchorMax = Vector2.one;
        volFillAreaRt.sizeDelta = new Vector2(-4, -4);
        volFillAreaRt.anchoredPosition = Vector2.zero;

        // 슬라이더 Fill
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

        // 슬라이더 Handle
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

        // 볼륨 텍스트
        var volumeTextGo = new GameObject("VolumeText");
        volumeTextGo.transform.SetParent(bgmPanel.transform, false);
        var volumeTextRt = volumeTextGo.AddComponent<RectTransform>();
        volumeTextRt.anchoredPosition = new Vector2(150, 10);
        volumeTextRt.sizeDelta = new Vector2(40, 20);
        _bgmVolumeText = volumeTextGo.AddComponent<TextMeshProUGUI>();
        _bgmVolumeText.fontSize = 16;
        _bgmVolumeText.alignment = TextAlignmentOptions.Left;
        _primaryTexts.Add(_bgmVolumeText);

        // 슬라이더 이벤트
        _bgmVolumeSlider.onValueChanged.AddListener((value) =>
        {
            if (_bgmVolumeText != null)
                _bgmVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
            
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetBGMVolume(value);
        });
        
        // AudioManager의 현재 볼륨으로 초기화
        if (AudioManager.Instance != null)
        {
            float currentVolume = AudioManager.Instance.GetComponent<AudioSource>()?.volume ?? 0.5f;
            _bgmVolumeSlider.value = currentVolume;
            _bgmVolumeText.text = $"{Mathf.RoundToInt(currentVolume * 100)}%";
            Debug.Log($"[UIManager] BGM volume initialized to {currentVolume * 100}%");
        }
        else
        {
            // AudioManager 없으면 기본값
            _bgmVolumeSlider.value = 0.5f;
            _bgmVolumeText.text = "50%";
        }
        
        Debug.Log("[UIManager] BGM Control created successfully!");
    }

    private void UpdateScore(int score)
    {
        if (_scoreText != null)
        {
            _scoreText.text = score.ToString();
            // 항상 검정색 유지 (다크모드에서도)
            _scoreText.color = new Color(0.17f, 0.17f, 0.18f);
            Debug.Log($"[UIManager] Score updated to: {score}");
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
        
        // 항상 검정색 유지 (다크모드에서도)
        _timerText.color = new Color(0.17f, 0.17f, 0.18f);

        // 슬라이더 업데이트 (원작 스타일)
        if (_timerSlider != null && GameManager.Instance != null)
        {
            float maxTime = GameManager.Instance.GameDuration;
            float ratio = time / maxTime;
            _timerSlider.value = ratio;

            // 색상 변화 (초록 → 노랑 → 빨강)
            if (_timerSliderFill != null)
            {
                if (ratio > 0.5f)
                {
                    // 초록 → 노랑
                    float t = (ratio - 0.5f) * 2f;
                    _timerSliderFill.color = Color.Lerp(new Color(0.95f, 0.85f, 0.3f), new Color(0.4f, 0.8f, 0.5f), t);
                }
                else if (ratio > 0.25f)
                {
                    // 노랑 → 주황
                    float t = (ratio - 0.25f) * 4f;
                    _timerSliderFill.color = Color.Lerp(new Color(1f, 0.65f, 0.2f), new Color(0.95f, 0.85f, 0.3f), t);
                }
                else
                {
                    // 주황 → 빨강 (+ 깜빡임)
                    float t = ratio * 4f;
                    Color red = new Color(1f, 0.3f, 0.3f);
                    Color orange = new Color(1f, 0.65f, 0.2f);
                    _timerSliderFill.color = Color.Lerp(red, orange, t);
                    
                    // 깜빡임 효과
                    float pulse = 0.7f + 0.3f * Mathf.PingPong(Time.time * 3f, 1f);
                    _timerSliderFill.color *= pulse;
                }
            }
        }

        // 텍스트 색상 (30초 이하 경고)
        if (time <= 30f)
            _timerText.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), CAT_EYE_GOLD, Mathf.PingPong(Time.time * 2f, 1f));
        else
            _timerText.color = ThemeManager.Instance.GetColor("TextPrimary");
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

        _countdownCatFace = CreateTextElement(_countdownPanel.transform, "",
            52, CAT_EYE_GOLD, new Vector2(0, 80), new Vector2(500, 80), isPrimary: false);
        _countdownCatFace.alignment = TextAlignmentOptions.Center;
        _countdownCatFace.gameObject.name = "CatFace"; // 재참조를 위한 명확한 이름

        _countdownText = CreateTextElement(_countdownPanel.transform, "3",
            120, Color.white, new Vector2(0, -30), new Vector2(300, 200), isPrimary: true);
        _countdownText.fontStyle = FontStyles.Bold;
        _countdownText.alignment = TextAlignmentOptions.Center;
        _countdownText.gameObject.name = "CountdownNumber"; // 재참조를 위한 명확한 이름

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
                _countdownText.color = ThemeManager.Instance.GetColor("TextSecondary");
                break;
            case 2:
                _countdownCatFace.text = "=-.o=";
                _countdownText.text = "2";
                _countdownText.color = ThemeManager.Instance.GetColor("TextPrimary");
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

        var retryBtn = CreateButton(_resultPanel.transform, "RETRY", Color.white,
            new Vector2(-180, -170), new Vector2(240, 65), 26, () =>
            {
                GameManager.Instance?.RestartGame();
            });
        _primaryButtons.Add(retryBtn.GetComponent<Button>());

        var menuBtn = CreateButton(_resultPanel.transform, "MENU", Color.white,
            new Vector2(180, -170), new Vector2(240, 65), 26, () =>
            {
                GameManager.Instance?.ReturnToMenu();
            });
        _secondaryButtons.Add(menuBtn.GetComponent<Button>());

        _resultPanel.SetActive(false);
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
        
        // ⭐ 닉네임을 PlayerPrefs에서 다시 로드 (저장된 값 반영)
        if (PlayerPrefs.HasKey("PlayerNickname"))
        {
            _currentNickname = PlayerPrefs.GetString("PlayerNickname");
            Debug.Log($"[UIManager] ✅ Nickname reloaded from PlayerPrefs in PopulateResult: '{_currentNickname}'");
        }
        
        // 닉네임 입력 필드 업데이트
        if (_nicknameInputField != null)
        {
            _nicknameInputField.text = _currentNickname;
            Debug.Log($"[UIManager] ✅ Nickname input field set to: '{_nicknameInputField.text}'");
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
        _leaderboardPanel = CreatePanel("LeaderboardPanel", Color.white, true);
        SetFullStretch(_leaderboardPanel.GetComponent<RectTransform>());

        // 타이틀
        var title = CreateTextElement(_leaderboardPanel.transform, "🏆 LEADERBOARD 🏆",
            48, CAT_EYE_GOLD, new Vector2(0, 350), new Vector2(700, 80), isPrimary: false);
        title.fontStyle = FontStyles.Bold;

        // 스크롤 영역 컨테이너
        var scrollArea = new GameObject("ScrollArea");
        scrollArea.transform.SetParent(_leaderboardPanel.transform, false);
        var scrollAreaRt = scrollArea.AddComponent<RectTransform>();
        scrollAreaRt.anchoredPosition = new Vector2(0, 0);
        scrollAreaRt.sizeDelta = new Vector2(600, 550);

        // 배경
        var scrollBg = scrollArea.AddComponent<Image>();
        scrollBg.color = new Color(1f, 1f, 1f, 0.3f);
        scrollBg.sprite = CreateRoundedRectSprite(100, 100, 15);
        scrollBg.type = Image.Type.Sliced;
        _panelBackgrounds.Add(scrollBg);

        // 엔트리 컨테이너 (실제 랭킹 항목들이 들어갈 곳)
        var entriesContainer = new GameObject("Entries");
        entriesContainer.transform.SetParent(scrollArea.transform, false);
        var entriesRt = entriesContainer.AddComponent<RectTransform>();
        entriesRt.anchorMin = new Vector2(0.5f, 1f);
        entriesRt.anchorMax = new Vector2(0.5f, 1f);
        entriesRt.pivot = new Vector2(0.5f, 1f);
        entriesRt.anchoredPosition = new Vector2(0, -10);
        entriesRt.sizeDelta = new Vector2(580, 0);

        _leaderboardEntriesContainer = entriesContainer.transform;

        // Close 버튼
        var closeBtn = CreateButton(_leaderboardPanel.transform, "CLOSE", Color.white,
            new Vector2(0, -360), new Vector2(280, 65), 28, () =>
            {
                HideLeaderboard();
            });
        _primaryButtons.Add(closeBtn.GetComponent<Button>());

        _leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        if (_leaderboardPanel != null)
        {
            _leaderboardPanel.SetActive(true);
            _mainMenuPanel?.SetActive(false);
            
            Debug.Log("[UIManager] ShowLeaderboard called");
            
            // 로컬 랭킹 우선 표시 (빠른 응답)
            if (LocalLeaderboardManager.Instance != null)
            {
                var localEntries = LocalLeaderboardManager.Instance.GetTopEntries(10);
                Debug.Log($"[UIManager] Local leaderboard has {localEntries.Count} entries");
                
                if (localEntries != null && localEntries.Count > 0)
                {
                    Debug.Log($"[UIManager] ✅ Displaying local leaderboard ({localEntries.Count} entries)");
                    DisplayLeaderboard(localEntries);
                }
                else
                {
                    Debug.LogWarning("[UIManager] No local leaderboard entries, showing empty message");
                    DisplayLeaderboard(new List<LeaderboardEntry>());
                }
            }
            else
            {
                Debug.LogError("[UIManager] ❌ LocalLeaderboardManager.Instance is NULL!");
                DisplayLeaderboard(new List<LeaderboardEntry>());
            }
            
            // Firebase 랭킹도 시도 (백그라운드, 있으면 업데이트)
            if (FirebaseManager.Instance != null)
            {
                Debug.Log("[UIManager] Also trying Firebase leaderboard in background...");
                FirebaseManager.Instance.LoadLeaderboard(OnLeaderboardLoaded);
            }
        }
        else
        {
            Debug.LogError("[UIManager] ❌ Leaderboard panel is NULL!");
        }
    }
    
    private void ShowLoadingMessage()
    {
        // 기존 엔트리 삭제
        foreach (Transform child in _leaderboardEntriesContainer)
        {
            Destroy(child.gameObject);
        }
        
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
            _mainMenuPanel?.SetActive(true);
        }
    }

    private void OnLeaderboardLoaded(System.Collections.Generic.List<LeaderboardEntry> entries)
    {
        // Firebase에서 데이터를 불러오지 못했으면 로컬 랭킹으로 폴백
        if (entries == null || entries.Count == 0)
        {
            Debug.LogWarning("[UIManager] Firebase leaderboard empty or failed, trying local leaderboard...");
            
            if (LocalLeaderboardManager.Instance != null)
            {
                var localEntries = LocalLeaderboardManager.Instance.GetTopEntries(10);
                if (localEntries != null && localEntries.Count > 0)
                {
                    Debug.Log($"[UIManager] Showing local leaderboard ({localEntries.Count} entries)");
                    DisplayLeaderboard(localEntries);
                    return;
                }
            }
        }
        
        DisplayLeaderboard(entries);
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
                "No records yet!\nPlay to set your first score!",
                28, Color.gray, new Vector2(0, -100), new Vector2(550, 200), isPrimary: false);
            emptyMsg.alignment = TextAlignmentOptions.Center;
            
            var containerRt = _leaderboardEntriesContainer.GetComponent<RectTransform>();
            containerRt.sizeDelta = new Vector2(580, 500);
            return;
        }

        Debug.Log($"[UIManager] Creating {entries.Count} leaderboard entries");
        float yOffset = 0f;
        int rank = 1;

        foreach (var entry in entries)
        {
            CreateLeaderboardEntry(rank, entry.nickname, entry.score, yOffset);
            yOffset -= 50f;
            rank++;
        }

        // 컨테이너 높이 조정
        var containerRt2 = _leaderboardEntriesContainer.GetComponent<RectTransform>();
        containerRt2.sizeDelta = new Vector2(580, Mathf.Max(500, -yOffset + 50));
        
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
        entryRt.sizeDelta = new Vector2(0, 45);

        // 배경 (상위 3명은 특별한 색상)
        var bg = entry.AddComponent<Image>();
        if (rank == 1)
            bg.color = new Color(1f, 0.84f, 0f, 0.3f); // 금색
        else if (rank == 2)
            bg.color = new Color(0.75f, 0.75f, 0.75f, 0.3f); // 은색
        else if (rank == 3)
            bg.color = new Color(0.8f, 0.5f, 0.2f, 0.3f); // 동색
        else
            bg.color = new Color(1f, 1f, 1f, 0.1f); // 투명
        
        bg.sprite = CreateRoundedRectSprite(100, 100, 10);
        bg.type = Image.Type.Sliced;

        // Rank
        var rankText = CreateTextElement(entry.transform, $"{rank}.",
            24, rank <= 3 ? CAT_EYE_GOLD : Color.gray, 
            new Vector2(-250, 0), new Vector2(60, 45), isPrimary: false);
        rankText.alignment = TextAlignmentOptions.MidlineLeft;
        rankText.fontStyle = FontStyles.Bold;

        // Nickname
        var nicknameText = CreateTextElement(entry.transform, nickname,
            22, rank <= 3 ? Color.black : Color.gray,
            new Vector2(-50, 0), new Vector2(300, 45), isPrimary: rank <= 3);
        nicknameText.alignment = TextAlignmentOptions.MidlineLeft;

        // Score
        var scoreText = CreateTextElement(entry.transform, score.ToString(),
            24, rank <= 3 ? CAT_EYE_GOLD : Color.gray,
            new Vector2(200, 0), new Vector2(120, 45), isPrimary: false);
        scoreText.alignment = TextAlignmentOptions.MidlineRight;
        scoreText.fontStyle = FontStyles.Bold;
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
        Debug.Log($"[UIManager] Input field text: '{newNickname}'");
        
        string oldNickname = _currentNickname;
        SaveNickname(newNickname);
        
        // 입력 필드 업데이트 (저장된 닉네임으로)
        _nicknameInputField.text = _currentNickname;
        
        // PlayerPrefs 강제 저장 확인
        PlayerPrefs.Save();
        string verified = PlayerPrefs.GetString("PlayerNickname", "NOT_FOUND");
        Debug.Log($"[UIManager] 🔍 PlayerPrefs verification after save: '{verified}'");
        
        // 닉네임이 바뀌었다는 것만 알림 (다음 게임부터 적용)
        if (oldNickname != _currentNickname)
        {
            Debug.Log($"[UIManager] ✅ Nickname changed! '{oldNickname}' → '{_currentNickname}'");
            Debug.Log($"[UIManager] Will be used for next game!");
        }
        else
        {
            Debug.Log($"[UIManager] ℹ️ Nickname unchanged: '{_currentNickname}'");
        }
        
        // 저장 완료 피드백
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
