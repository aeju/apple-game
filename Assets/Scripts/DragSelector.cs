using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 마우스/터치 드래그로 사과를 선택하는 메카닉을 처리합니다.
/// 실시간 합계 표시 기능을 포함합니다.
/// </summary>
public class DragSelector : MonoBehaviour
{
    public static DragSelector Instance { get; private set; }

    [Header("Sum Text Settings")]
    [Tooltip("드래그 점수 텍스트에 사용할 TMP 폰트 (null이면 기본 폰트 사용)")]
    [SerializeField] private TMP_FontAsset sumTextFont = null;
    
    [Tooltip("드래그 점수 텍스트 크기 (기본값: 4.5)")]
    [SerializeField] private float sumTextFontSize = 4.5f;

    private bool _isDragging;
    private Vector3 _dragStart;
    private Vector3 _dragEnd;

    // 선택 시각 요소
    private SpriteRenderer _fillRenderer;
    private LineRenderer _borderRenderer;
    private TextMeshPro _sumText;

    // 현재 선택 상태
    private List<Apple> _currentSelection = new List<Apple>();
    private int _currentSum;

    private Camera _cam;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        CreateVisuals();
    }

    private void Start()
    {
        _cam = Camera.main;

        // Play 모드에서만 테마 변경 구독
        if (Application.isPlaying && ThemeManager.Instance != null)
        {
            ThemeManager.Instance.OnThemeChanged.AddListener(OnThemeChanged);
            ApplyThemeColors(); // 초기 색상 적용
        }
    }

    private void OnThemeChanged(ThemeManager.Theme theme)
    {
        ApplyThemeColors();
    }

    private void ApplyThemeColors()
    {
        if (ThemeManager.Instance == null) return;

        // 기본 선택 영역 색상
        if (_fillRenderer != null)
            _fillRenderer.color = ThemeManager.Instance.GetColor("SelectionFill");

        if (_borderRenderer != null)
        {
            Color borderColor = ThemeManager.Instance.GetColor("SelectionBorder");
            _borderRenderer.startColor = borderColor;
            _borderRenderer.endColor = borderColor;
        }
    }

    /// <summary>
    /// 선택 영역 시각 요소를 생성합니다.
    /// </summary>
    private void CreateVisuals()
    {
        // 반투명 채우기
        var fillGo = new GameObject("SelectionFill");
        fillGo.transform.SetParent(transform);
        _fillRenderer = fillGo.AddComponent<SpriteRenderer>();
        _fillRenderer.sprite = CreateSquareSprite();
        _fillRenderer.color = new Color(0.95f, 0.78f, 0.10f, 0.15f);
        _fillRenderer.sortingOrder = 10;
        _fillRenderer.enabled = false;

        // 테두리
        var borderGo = new GameObject("SelectionBorder");
        borderGo.transform.SetParent(transform);
        _borderRenderer = borderGo.AddComponent<LineRenderer>();
        _borderRenderer.positionCount = 5;
        _borderRenderer.loop = false;
        _borderRenderer.startWidth = 0.04f;
        _borderRenderer.endWidth = 0.04f;
        _borderRenderer.useWorldSpace = true;
        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default")
                  ?? Shader.Find("Sprites/Default")
                  ?? Shader.Find("Hidden/Internal-Colored");
        _borderRenderer.material = new Material(shader);
        _borderRenderer.startColor = new Color(0.95f, 0.78f, 0.10f, 0.7f);
        _borderRenderer.endColor = new Color(0.95f, 0.78f, 0.10f, 0.7f);
        _borderRenderer.sortingOrder = 11;
        _borderRenderer.enabled = false;

        // 실시간 합계 텍스트
        var sumGo = new GameObject("SumLabel");
        sumGo.transform.SetParent(transform);
        _sumText = sumGo.AddComponent<TextMeshPro>();
        
        // 커스텀 폰트 적용
        if (sumTextFont != null)
            _sumText.font = sumTextFont;
        
        _sumText.fontSize = sumTextFontSize;
        _sumText.fontStyle = FontStyles.Bold;
        _sumText.alignment = TextAlignmentOptions.Center;
        _sumText.sortingOrder = 12;
        _sumText.rectTransform.sizeDelta = new Vector2(2f, 1f);
        _sumText.enabled = false;
    }

    private static Sprite CreateSquareSprite()
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4);
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        if (GridManager.Instance == null || GridManager.Instance.IsProcessing) return;

        var pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 screenPos = pointer.position.ReadValue();
        Vector3 worldPos = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos.z = 0f;

        // 드래그 시작
        if (pointer.press.wasPressedThisFrame)
        {
            // UI 위에서 클릭한 경우 무시
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            _isDragging = true;
            _dragStart = worldPos;
            _dragEnd = worldPos;
            ShowVisuals(true);
        }

        // 드래그 중
        if (_isDragging && pointer.press.isPressed)
        {
            _dragEnd = worldPos;
            UpdateSelectionVisuals();
        }

        // 드래그 종료
        if (_isDragging && pointer.press.wasReleasedThisFrame)
        {
            _isDragging = false;
            TryConfirmSelection();
            ShowVisuals(false);
            ClearHighlights();
        }
    }

    /// <summary>
    /// 선택 영역 시각 요소를 업데이트합니다.
    /// </summary>
    private void UpdateSelectionVisuals()
    {
        Vector3 min = Vector3.Min(_dragStart, _dragEnd);
        Vector3 max = Vector3.Max(_dragStart, _dragEnd);

        // 채우기 위치/크기
        Vector3 center = (min + max) * 0.5f;
        Vector3 size = max - min;
        size.x = Mathf.Max(size.x, 0.1f);
        size.y = Mathf.Max(size.y, 0.1f);

        _fillRenderer.transform.position = new Vector3(center.x, center.y, 0f);
        _fillRenderer.transform.localScale = new Vector3(size.x, size.y, 1f);

        // 둥근 테두리 생성
        Vector3[] roundedRect = CreateRoundedRectangle(min, max, 0.15f, 8);
        _borderRenderer.positionCount = roundedRect.Length;
        _borderRenderer.SetPositions(roundedRect);

        // 선택된 사과 업데이트
        ClearHighlights();
        _currentSelection = GridManager.Instance.GetApplesInWorldRect(min, max);
        _currentSum = GridManager.Instance.SumOfApples(_currentSelection);

        // 하이라이트
        foreach (var apple in _currentSelection)
            apple.Highlight(true);

        // 합계 텍스트 및 색상
        int target = GameManager.Instance.TargetSum;

        if (_currentSelection.Count > 0)
        {
            _sumText.enabled = true;
            _sumText.transform.position = new Vector3(max.x + 0.5f, max.y + 0.3f, -0.02f);
            _sumText.text = _currentSum.ToString();

            if (_currentSum == target)
            {
                // 성공 - 테마별 성공 색상
                Color successColor = ThemeManager.Instance != null ?
                    ThemeManager.Instance.GetColor("SelectionSuccess") :
                    new Color(0.95f, 0.85f, 0.10f);
                _sumText.color = successColor;
                _fillRenderer.color = new Color(successColor.r, successColor.g, successColor.b, 0.25f);
                SetBorderColor(successColor);
            }
            else if (_currentSum > target)
            {
                // 초과 - 테마별 실패 색상
                Color failColor = ThemeManager.Instance != null ?
                    ThemeManager.Instance.GetColor("SelectionFail") :
                    new Color(1f, 0.35f, 0.35f);
                _sumText.color = failColor;
                _fillRenderer.color = new Color(failColor.r, failColor.g, failColor.b, 0.15f);
                SetBorderColor(failColor);
            }
            else
            {
                // 진행 중 - 부드러운 황금색
                _sumText.color = new Color(0.95f, 0.78f, 0.10f, 0.8f);
                _fillRenderer.color = ThemeManager.Instance != null ?
                    ThemeManager.Instance.GetColor("SelectionFill") :
                    new Color(0.95f, 0.78f, 0.10f, 0.12f);
                Color borderColor = ThemeManager.Instance != null ?
                    ThemeManager.Instance.GetColor("SelectionBorder") :
                    new Color(0.95f, 0.78f, 0.10f, 0.7f);
                SetBorderColor(borderColor);
            }
        }
        else
        {
            _sumText.enabled = false;
            _fillRenderer.color = ThemeManager.Instance != null ?
                ThemeManager.Instance.GetColor("SelectionFill") :
                new Color(0.95f, 0.78f, 0.10f, 0.08f);
            Color borderColor = ThemeManager.Instance != null ?
                ThemeManager.Instance.GetColor("SelectionBorder") :
                new Color(0.95f, 0.78f, 0.10f, 0.4f);
            borderColor.a *= 0.5f;
            SetBorderColor(borderColor);
        }
    }

    /// <summary>
    /// 선택을 확정합니다.
    /// </summary>
    private void TryConfirmSelection()
    {
        if (_currentSelection.Count == 0) return;

        int target = GameManager.Instance.TargetSum;
        if (_currentSum == target)
        {
            // 성공!
            int count = _currentSelection.Count;

            // 점수 팝업 위치 계산
            Vector3 center = Vector3.zero;
            foreach (var a in _currentSelection)
                center += a.transform.position;
            center /= _currentSelection.Count;

            ScoreManager.Instance?.AddScore(count, center);
            AudioManager.Instance?.PlaySFX("meow");

            // 사과 제거
            var applesToRemove = new List<Apple>(_currentSelection);
            GridManager.Instance.RemoveApples(applesToRemove, () =>
            {
                // 모든 사과 제거 확인
                if (GridManager.Instance.ActiveAppleCount() == 0)
                {
                    GameManager.Instance?.EndGame();
                }
            });
        }
        else
        {
            // 실패 피드백 (화난 고양이)
            AudioManager.Instance?.PlaySFX("hiss");
        }
    }

    private void ClearHighlights()
    {
        foreach (var apple in _currentSelection)
            if (apple != null && apple.IsActive)
                apple.Highlight(false);
        _currentSelection.Clear();
    }

    private void ShowVisuals(bool show)
    {
        _fillRenderer.enabled = show;
        _borderRenderer.enabled = show;
        if (!show) _sumText.enabled = false;
    }

    private void SetBorderColor(Color color)
    {
        _borderRenderer.startColor = color;
        _borderRenderer.endColor = color;
    }

    /// <summary>
    /// 둥근 사각형의 포인트를 생성합니다.
    /// </summary>
    private Vector3[] CreateRoundedRectangle(Vector3 min, Vector3 max, float cornerRadius, int segmentsPerCorner)
    {
        float z = -0.01f;
        
        float width = max.x - min.x;
        float height = max.y - min.y;
        float maxRadius = Mathf.Min(width, height) * 0.5f;
        cornerRadius = Mathf.Min(cornerRadius, maxRadius);

        List<Vector3> points = new List<Vector3>();

        Vector2 bottomLeft = new Vector2(min.x + cornerRadius, min.y + cornerRadius);
        Vector2 bottomRight = new Vector2(max.x - cornerRadius, min.y + cornerRadius);
        Vector2 topRight = new Vector2(max.x - cornerRadius, max.y - cornerRadius);
        Vector2 topLeft = new Vector2(min.x + cornerRadius, max.y - cornerRadius);

        AddCornerPoints(points, bottomLeft, cornerRadius, 180f, 270f, segmentsPerCorner, z);
        points.Add(new Vector3(bottomRight.x, min.y, z));
        AddCornerPoints(points, bottomRight, cornerRadius, 270f, 360f, segmentsPerCorner, z);
        points.Add(new Vector3(max.x, topRight.y, z));
        AddCornerPoints(points, topRight, cornerRadius, 0f, 90f, segmentsPerCorner, z);
        points.Add(new Vector3(topLeft.x, max.y, z));
        AddCornerPoints(points, topLeft, cornerRadius, 90f, 180f, segmentsPerCorner, z);
        points.Add(new Vector3(min.x, bottomLeft.y, z));
        points.Add(points[0]);

        return points.ToArray();
    }

    /// <summary>
    /// 원호 모서리 포인트를 추가합니다.
    /// </summary>
    private void AddCornerPoints(List<Vector3> points, Vector2 center, float radius, float startAngle, float endAngle, int segments, float z)
    {
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            points.Add(new Vector3(x, y, z));
        }
    }
}
