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
        SetupCamera();
        SetupLight2D();
        SetupManagers();
        SetupUI();
        SetupEventSystem();
        SetupBackground();
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

        // 그리드 전체가 보이도록 ortho size 계산
        float gridHeight = (Rows - 1) * CellSize; // 8.0
        cam.orthographicSize = gridHeight * 0.5f + 1.5f; // 5.5 (상하 여백 포함)

        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.backgroundColor = new Color(0.08f, 0.10f, 0.14f);
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
        if (GameManager.Instance == null)
        {
            var go = new GameObject("GameManager");
            var gm = go.AddComponent<GameManager>();
            gm.GameDuration = GameDuration;
            gm.TargetSum = TargetSum;
        }

        if (GridManager.Instance == null)
        {
            var go = new GameObject("GridManager");
            var grid = go.AddComponent<GridManager>();
            grid.Columns = Columns;
            grid.Rows = Rows;
            grid.CellSize = CellSize;
        }

        if (ScoreManager.Instance == null)
        {
            var go = new GameObject("ScoreManager");
            go.AddComponent<ScoreManager>();
        }

        if (DragSelector.Instance == null)
        {
            var go = new GameObject("DragSelector");
            go.AddComponent<DragSelector>();
        }

        if (AudioManager.Instance == null)
        {
            var go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
        }
    }

    /// <summary>
    /// UI 캔버스와 UIManager를 생성합니다.
    /// </summary>
    private void SetupUI()
    {
        if (UIManager.Instance != null) return;

        // 메인 UI 캔버스 - ScreenSpaceCamera 모드
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 1f;
        canvas.sortingOrder = 100;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var uiManager = canvasGo.AddComponent<UIManager>();
        uiManager.Initialize(canvas);
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
    /// 배경을 생성합니다.
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
        bgSr.color = new Color(0.15f, 0.18f, 0.22f, 0.6f);
        bgSr.sortingOrder = -1;
    }
}
