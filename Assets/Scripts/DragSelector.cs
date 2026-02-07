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
        _fillRenderer.color = new Color(1f, 1f, 1f, 0.15f);
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
        // URP 호환 셰이더 탐색 (fallback 포함)
        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default")
                  ?? Shader.Find("Sprites/Default")
                  ?? Shader.Find("Hidden/Internal-Colored");
        _borderRenderer.material = new Material(shader);
        _borderRenderer.startColor = Color.white;
        _borderRenderer.endColor = Color.white;
        _borderRenderer.sortingOrder = 11;
        _borderRenderer.enabled = false;

        // 실시간 합계 텍스트
        var sumGo = new GameObject("SumLabel");
        sumGo.transform.SetParent(transform);
        _sumText = sumGo.AddComponent<TextMeshPro>();
        _sumText.fontSize = 4.5f;
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

        // 테두리
        float z = -0.01f;
        _borderRenderer.SetPositions(new Vector3[]
        {
            new Vector3(min.x, min.y, z),
            new Vector3(max.x, min.y, z),
            new Vector3(max.x, max.y, z),
            new Vector3(min.x, max.y, z),
            new Vector3(min.x, min.y, z)
        });

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
                _sumText.color = new Color(0.3f, 1f, 0.3f); // 초록
                _fillRenderer.color = new Color(0.3f, 1f, 0.3f, 0.2f);
                SetBorderColor(new Color(0.3f, 1f, 0.3f));
            }
            else if (_currentSum > target)
            {
                _sumText.color = new Color(1f, 0.4f, 0.4f); // 빨강
                _fillRenderer.color = new Color(1f, 0.3f, 0.3f, 0.15f);
                SetBorderColor(new Color(1f, 0.4f, 0.4f));
            }
            else
            {
                _sumText.color = Color.white;
                _fillRenderer.color = new Color(1f, 1f, 1f, 0.15f);
                SetBorderColor(Color.white);
            }
        }
        else
        {
            _sumText.enabled = false;
            _fillRenderer.color = new Color(1f, 1f, 1f, 0.12f);
            SetBorderColor(new Color(1f, 1f, 1f, 0.6f));
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
            AudioManager.Instance?.PlaySFX("success");

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
            // 실패 피드백
            AudioManager.Instance?.PlaySFX("fail");
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
}
