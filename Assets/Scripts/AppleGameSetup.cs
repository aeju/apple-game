using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 씬 부트스트래퍼. 이 컴포넌트를 빈 GameObject에 추가하면
/// 게임에 필요한 모든 오브젝트를 자동으로 생성합니다.
///
/// 사용법:
/// 1. Unity에서 SampleScene을 엽니다
/// 2. Hierarchy의 모든 오브젝트를 삭제합니다
/// 3. 빈 GameObject를 만들고 이 스크립트를 추가합니다
/// 4. Play를 누르면 모든 것이 자동으로 세팅됩니다
/// </summary>
public class AppleGameSetup : MonoBehaviour
{
    [Header("Grid Settings")]
    public int Columns = 10;
    public int Rows = 17;
    public float CellSize = 0.5f;

    [Header("Game Settings")]
    public float GameDuration = 120f;
    public int TargetSum = 10;

    private void Awake()
    {
        Debug.Log("[AppleGameSetup] Awake called. Application.isPlaying: " + Application.isPlaying);

        SetupCamera();
        SetupLight2D();
        SetupManagers();
        SetupUI();
        SetupEventSystem();
        SetupBackground();

        Debug.Log("[AppleGameSetup] Setup complete!");
    }

    /// <summary>
    /// Edit 모드에서 Scene에 오브젝트를 생성합니다 (Editor에서 호출)
    /// </summary>
    public void GenerateInEditMode()
    {
        Debug.Log("[AppleGameSetup] Generating objects in Edit mode...");
        
        SetupCamera();
        SetupLight2D();
        SetupManagers();
        SetupUI();
        SetupEventSystem();
        SetupBackground();
        
        Debug.Log("[AppleGameSetup] Generation complete!");
    }

    /// <summary>
    /// 생성된 오브젝트를 삭제합니다 (Editor에서 호출)
    /// </summary>
    public void ClearGeneratedObjects()
    {
        Debug.Log("[AppleGameSetup] Clearing generated objects...");

        // Camera
        var cameras = FindObjectsOfType<Camera>(true);
        foreach (var cam in cameras)
        {
            if (cam.name == "Main Camera")
                DestroyImmediate(cam.gameObject);
        }

        // Light
        var lights = FindObjectsOfType<UnityEngine.Rendering.Universal.Light2D>(true);
        foreach (var light in lights)
        {
            if (light.name == "Global Light 2D")
                DestroyImmediate(light.gameObject);
        }

        // Managers
        string[] managerNames = { "ThemeManager", "GameManager", "GridManager", 
                                  "ScoreManager", "DragSelector", "AudioManager" };
        foreach (var name in managerNames)
        {
            var obj = GameObject.Find(name);
            if (obj != null) DestroyImmediate(obj);
        }

        // UI
        var canvas = GameObject.Find("Canvas");
        if (canvas != null) DestroyImmediate(canvas);

        var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem != null && eventSystem.name == "EventSystem")
            DestroyImmediate(eventSystem.gameObject);

        // Background
        var bg = GameObject.Find("GridBackground");
        if (bg != null) DestroyImmediate(bg);

        Debug.Log("[AppleGameSetup] Clear complete!");
    }

    /// <summary>
    /// 카메라를 설정합니다. 기존 카메라가 있으면 재사용합니다.
    /// </summary>
    private void SetupCamera()
    {
        // 기존 카메라 찾기
        Camera cam = Camera.main;

        // 기존 카메라가 없으면 새로 생성
        if (cam == null)
        {
            // 비활성화된 카메라가 있는지도 확인
            Camera[] allCams = FindObjectsOfType<Camera>(true);
            if (allCams.Length > 0)
            {
                cam = allCams[0];
                cam.gameObject.SetActive(true);
                cam.gameObject.tag = "MainCamera";
            }
            else
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
        }

        // 중복 카메라 비활성화
        Camera[] cameras = FindObjectsOfType<Camera>(true);
        foreach (var c in cameras)
        {
            if (c != cam)
                c.gameObject.SetActive(false);
        }

        // URP 카메라 데이터 추가 (없으면)
        if (cam.GetComponent<UniversalAdditionalCameraData>() == null)
            cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

        // AudioListener 확인
        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();

        // 카메라 설정 - 모든 것을 원점(0,0)에 맞춤
        cam.orthographic = true;

        // 그리드 전체가 보이도록 ortho size 계산 (화면 비율에 따라 동적 조정)
        float gridWidth = (Columns - 1) * CellSize;  // 4.5 (10칸 × 0.5)
        float gridHeight = (Rows - 1) * CellSize;    // 8.0 (17칸 × 0.5)
        
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float gridAspect = gridWidth / gridHeight;
        
        // 화면 비율에 따라 카메라 크기 조정
        if (screenAspect > gridAspect)
        {
            // 화면이 더 넓음 (가로 모드) - 세로를 기준으로
            cam.orthographicSize = gridHeight * 0.5f + 2.0f; // 상하 여백 포함
        }
        else
        {
            // 화면이 더 좁음 (세로 모드, 모바일) - 가로를 기준으로
            cam.orthographicSize = (gridWidth * 0.5f + 1.0f) / screenAspect; // 좌우 여백 최소화
        }

        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.backgroundColor = new Color(0.96f, 0.94f, 0.91f); // 밝은 테마 기본 배경
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    /// <summary>
    /// URP 2D 렌더링에 필요한 Global Light 2D를 생성합니다.
    /// 이 라이트가 없으면 스프라이트가 보이지 않습니다!
    /// </summary>
    private void SetupLight2D()
    {
        // 기존 2D 라이트 확인 (비활성화된 것 포함)
        Light2D existingLight = FindObjectOfType<Light2D>(true);

        if (existingLight != null)
        {
            // 기존 라이트가 비활성화되어 있으면 활성화
            existingLight.gameObject.SetActive(true);
        }
        else
        {
            // 새 Global Light 2D 생성
            var lightGo = new GameObject("Global Light 2D");
            var light2D = lightGo.AddComponent<Light2D>();
            light2D.lightType = Light2D.LightType.Global;
            light2D.intensity = 1f;
            light2D.color = Color.white;
        }
    }

    /// <summary>
    /// 게임 매니저들을 생성합니다.
    /// </summary>
    private void SetupManagers()
    {
        // ThemeManager를 제일 먼저 생성 (다른 매니저들이 테마를 참조할 수 있도록)
        if (GameObject.Find("ThemeManager") == null)
        {
            var go = new GameObject("ThemeManager");
            var tm = go.AddComponent<ThemeManager>();
            if (!Application.isPlaying)
                tm.SendMessage("Awake"); // Edit 모드에서 Awake 강제 호출
        }

        if (GameObject.Find("GameManager") == null)
        {
            var go = new GameObject("GameManager");
            var gm = go.AddComponent<GameManager>();
            gm.GameDuration = GameDuration;
            gm.TargetSum = TargetSum;
            if (!Application.isPlaying)
                gm.SendMessage("Awake");
        }

        if (GameObject.Find("GridManager") == null)
        {
            var go = new GameObject("GridManager");
            var grid = go.AddComponent<GridManager>();
            grid.Columns = Columns;
            grid.Rows = Rows;
            grid.CellSize = CellSize;
            if (!Application.isPlaying)
                grid.SendMessage("Awake");
        }

        if (GameObject.Find("ScoreManager") == null)
        {
            var go = new GameObject("ScoreManager");
            var sm = go.AddComponent<ScoreManager>();
            if (!Application.isPlaying)
                sm.SendMessage("Awake");
        }

        if (GameObject.Find("DragSelector") == null)
        {
            var go = new GameObject("DragSelector");
            var ds = go.AddComponent<DragSelector>();
            if (!Application.isPlaying)
                ds.SendMessage("Awake");
        }

        if (GameObject.Find("AudioManager") == null)
        {
            var go = new GameObject("AudioManager");
            var am = go.AddComponent<AudioManager>();
            if (!Application.isPlaying)
                am.SendMessage("Awake");
        }

        if (GameObject.Find("FirebaseManager") == null)
        {
            var go = new GameObject("FirebaseManager");
            var fm = go.AddComponent<FirebaseManager>();
            if (!Application.isPlaying)
                fm.SendMessage("Awake");
        }

        if (GameObject.Find("LocalLeaderboardManager") == null)
        {
            var go = new GameObject("LocalLeaderboardManager");
            var llm = go.AddComponent<LocalLeaderboardManager>();
            if (!Application.isPlaying)
                llm.SendMessage("Awake");
        }
    }

    /// <summary>
    /// UI 캔버스와 UIManager를 생성합니다.
    /// </summary>
    private void SetupUI()
    {
        // 이미 존재하는지 확인
        var existingCanvas = GameObject.Find("Canvas");
        Canvas canvasComponent = null;
        UIManager uiManagerComponent = null;

        if (existingCanvas != null)
        {
            Debug.Log("[AppleGameSetup] Canvas already exists in scene.");
            canvasComponent = existingCanvas.GetComponent<Canvas>();
            uiManagerComponent = existingCanvas.GetComponent<UIManager>();

            if (canvasComponent != null)
            {
                // 카메라 재설정 (Play 모드에서 중요)
                canvasComponent.renderMode = RenderMode.ScreenSpaceCamera;
                canvasComponent.worldCamera = Camera.main;
                canvasComponent.planeDistance = 1f;
                Debug.Log("[AppleGameSetup] Canvas camera set to: " + Camera.main);
            }
        }
        else
        {
            Debug.Log("[AppleGameSetup] Creating new Canvas...");
            
            // 메인 UI 캔버스 - ScreenSpaceCamera 모드
            var canvasGo = new GameObject("Canvas");
            canvasComponent = canvasGo.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceCamera;
            canvasComponent.worldCamera = Camera.main;
            canvasComponent.planeDistance = 1f;
            canvasComponent.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            uiManagerComponent = canvasGo.AddComponent<UIManager>();
            
            // Edit 모드에서는 Awake가 자동으로 호출되지 않으므로 수동 호출
            if (!Application.isPlaying)
            {
                uiManagerComponent.SendMessage("Awake");
            }
        }

        // UIManager 초기화 (항상 호출)
        if (uiManagerComponent != null && canvasComponent != null)
        {
            Debug.Log("[AppleGameSetup] Initializing UIManager...");
            uiManagerComponent.Initialize(canvasComponent);
        }
        else
        {
            Debug.LogError("[AppleGameSetup] Failed to get Canvas or UIManager component!");
        }
    }

    /// <summary>
    /// EventSystem이 없으면 생성합니다.
    /// </summary>
    private void SetupEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }

    /// <summary>
    /// 배경을 생성하고 테마 변경을 구독합니다.
    /// </summary>
    private void SetupBackground()
    {
        var bgGo = new GameObject("GridBackground");
        var bgSr = bgGo.AddComponent<SpriteRenderer>();

        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        bgSr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4);

        float gridWidth = (Columns - 1) * CellSize + CellSize;
        float gridHeight = (Rows - 1) * CellSize + CellSize;

        bgGo.transform.position = new Vector3(0f, 0f, 0.5f);
        bgGo.transform.localScale = new Vector3(gridWidth, gridHeight, 1f);
        bgSr.sortingOrder = -1;

        // 초기 테마 색상 적용
        if (ThemeManager.Instance != null)
        {
            bgSr.color = ThemeManager.Instance.GetColor("GridBG");

            // 테마 변경 시 배경 색상도 변경
            ThemeManager.Instance.OnThemeChanged.AddListener((theme) =>
            {
                if (bgSr != null)
                    bgSr.color = ThemeManager.Instance.GetColor("GridBG");
            });
        }
        else
        {
            bgSr.color = new Color(1.0f, 0.98f, 0.95f, 0.8f); // 밝은 테마 기본
        }
    }
}
