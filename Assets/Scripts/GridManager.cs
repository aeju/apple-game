using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 사과 그리드를 생성하고 관리합니다. 중력, 힌트, 셔플 기능을 포함합니다.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int Columns = 10;
    public int Rows = 17;
    public float CellSize = 0.5f;

    [Header("Custom Sprites (Optional - 양털공/커스텀 이미지)")]
    [Tooltip("숫자 1~9에 해당하는 스프라이트를 순서대로 할당하세요.\n비어있으면 프로그래밍 방식으로 발바닥을 생성합니다.\n(256x256 PNG 권장)")]
    [SerializeField] private Sprite[] numberSprites = new Sprite[9];
    
    [Header("Number Label Settings")]
    [Tooltip("숫자 라벨 표시 여부 (체크 해제 시 숫자 숨김)")]
    [SerializeField] private bool showNumberLabel = true;
    
    [Tooltip("숫자 라벨에 사용할 TMP 폰트 (null이면 기본 폰트 사용)")]
    [SerializeField] private TMPro.TMP_FontAsset customFont = null;
    
    [Tooltip("숫자 라벨 폰트 크기 (기본값: 5.5)")]
    [SerializeField] private float labelFontSize = 5.5f;
    
    [Tooltip("숫자 라벨 Y축 위치 오프셋 (기본값: -0.11)")]
    [SerializeField] private float labelYOffset = -0.11f;
    
    [Header("Label Color Settings")]
    [Tooltip("색상 모드: Auto(자동), Uniform(같은색), Individual(각각 다른색)")]
    [SerializeField] private LabelColorMode labelColorMode = LabelColorMode.Auto;
    
    [Tooltip("모든 숫자에 적용할 단일 색상 (Uniform 모드에서만 사용)")]
    [SerializeField] private Color uniformLabelColor = Color.white;
    
    [Tooltip("숫자 1~9의 개별 텍스트 색상 (Individual 모드에서만 사용)")]
    [SerializeField] private Color[] individualLabelColors = new Color[9];

    private Apple[,] _grid;
    private Transform _container;
    private Vector3 _gridOrigin;
    private bool _isProcessing;

    public bool IsProcessing => _isProcessing;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    /// <summary>
    /// 그리드 원점으로부터 (x, y) 셀의 월드 좌표를 계산합니다.
    /// </summary>
    public Vector3 GridToWorld(int x, int y)
    {
        return _gridOrigin + new Vector3(x * CellSize, y * CellSize, 0f);
    }

    /// <summary>
    /// 새로운 그리드를 생성합니다.
    /// </summary>
    public void GenerateGrid()
    {
        ClearGrid();

        if (_container == null)
        {
            _container = new GameObject("AppleContainer").transform;
            _container.SetParent(transform);
        }

        _grid = new Apple[Columns, Rows];

        // 그리드를 원점(0,0) 중심으로 배치
        _gridOrigin = new Vector3(
            -(Columns - 1) * CellSize * 0.5f,
            -(Rows - 1) * CellSize * 0.5f,
            0f
        );

        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                var go = new GameObject($"Apple_{x}_{y}");
                go.AddComponent<SpriteRenderer>();
                go.transform.SetParent(_container);
                go.transform.position = GridToWorld(x, y);

                var apple = go.AddComponent<Apple>();
                
                // Reflection으로 private 필드 설정 (Inspector에서 설정된 경우)
                var bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                
                // 커스텀 스프라이트 할당
                if (numberSprites != null && numberSprites.Length == 9)
                {
                    var field = typeof(Apple).GetField("customSprites", bindingFlags);
                    if (field != null)
                        field.SetValue(apple, numberSprites);
                }
                
                // 숫자 라벨 표시 여부 설정
                var showLabelField = typeof(Apple).GetField("showNumberLabel", bindingFlags);
                if (showLabelField != null)
                    showLabelField.SetValue(apple, showNumberLabel);
                
                // TMP 폰트 설정 할당
                if (customFont != null)
                {
                    var fontField = typeof(Apple).GetField("customFont", bindingFlags);
                    if (fontField != null)
                        fontField.SetValue(apple, customFont);
                }
                
                // 폰트 크기 설정
                var fontSizeField = typeof(Apple).GetField("labelFontSize", bindingFlags);
                if (fontSizeField != null)
                    fontSizeField.SetValue(apple, labelFontSize);
                
                // Y 오프셋 설정
                var offsetField = typeof(Apple).GetField("labelYOffset", bindingFlags);
                if (offsetField != null)
                    offsetField.SetValue(apple, labelYOffset);
                
                // 라벨 색상 모드 설정
                var colorModeField = typeof(Apple).GetField("labelColorMode", bindingFlags);
                if (colorModeField != null)
                    colorModeField.SetValue(apple, labelColorMode);
                
                // 단일 색상 설정 (Uniform 모드)
                var uniformColorField = typeof(Apple).GetField("uniformLabelColor", bindingFlags);
                if (uniformColorField != null)
                    uniformColorField.SetValue(apple, uniformLabelColor);
                
                // 개별 색상 설정 (Individual 모드)
                if (individualLabelColors != null && individualLabelColors.Length == 9)
                {
                    var individualColorsField = typeof(Apple).GetField("individualLabelColors", bindingFlags);
                    if (individualColorsField != null)
                        individualColorsField.SetValue(apple, individualLabelColors);
                }
                
                int num = GenerateNumber();
                apple.Init(num, x, y);

                _grid[x, y] = apple;

                // 등장 애니메이션 (왼쪽 아래부터 순서대로)
                float delay = (x + y) * 0.008f;
                apple.Appear(delay);
            }
        }
    }

    /// <summary>
    /// 1~9 사이의 숫자를 생성합니다.
    /// 가중 랜덤으로 합 10 조합이 잘 생기도록 분포를 조절합니다.
    /// </summary>
    private int GenerateNumber()
    {
        // 가중치: 1~4와 6~9가 많을수록 합 10 조합이 풍부해짐
        // 5는 5+5=10 조합만 가능하므로 비중을 낮춤
        int[] weights = { 12, 12, 12, 12, 8, 12, 12, 12, 12 }; // 1~9
        int totalWeight = 0;
        foreach (int w in weights) totalWeight += w;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative) return i + 1;
        }
        return 5; // fallback
    }

    /// <summary>
    /// 그리드를 초기화합니다.
    /// </summary>
    public void ClearGrid()
    {
        if (_container != null)
        {
            for (int i = _container.childCount - 1; i >= 0; i--)
                Destroy(_container.GetChild(i).gameObject);
        }
        _grid = null;
        _isProcessing = false;
    }

    /// <summary>
    /// 월드 좌표 사각형 안에 있는 활성 사과 목록을 반환합니다.
    /// </summary>
    public List<Apple> GetApplesInWorldRect(Vector2 min, Vector2 max)
    {
        var result = new List<Apple>();
        if (_grid == null) return result;

        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                var apple = _grid[x, y];
                if (apple == null || !apple.IsActive) continue;

                Vector3 pos = apple.transform.position;
                if (pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y)
                    result.Add(apple);
            }
        }
        return result;
    }

    /// <summary>
    /// 사과 리스트의 숫자 합을 계산합니다.
    /// </summary>
    public int SumOfApples(List<Apple> apples)
    {
        int sum = 0;
        foreach (var a in apples) sum += a.Number;
        return sum;
    }

    /// <summary>
    /// 사과들을 제거하고 중력을 적용합니다.
    /// </summary>
    public void RemoveApples(List<Apple> apples, System.Action onComplete = null)
    {
        StartCoroutine(CoRemoveAndGravity(apples, onComplete));
    }

    private IEnumerator CoRemoveAndGravity(List<Apple> apples, System.Action onComplete)
    {
        _isProcessing = true;

        // 사과 제거 애니메이션
        int pending = apples.Count;
        foreach (var apple in apples)
        {
            _grid[apple.GridX, apple.GridY] = null;
            apple.Remove(() => pending--);
        }

        // 모든 제거 애니메이션 완료 대기
        while (pending > 0) yield return null;
        yield return new WaitForSeconds(0.05f);

        // 중력 적용
        yield return StartCoroutine(CoApplyGravity());

        _isProcessing = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 열별로 사과를 아래로 떨어뜨립니다.
    /// </summary>
    private IEnumerator CoApplyGravity()
    {
        int animating = 0;

        for (int x = 0; x < Columns; x++)
        {
            int writeY = 0;
            for (int readY = 0; readY < Rows; readY++)
            {
                var apple = _grid[x, readY];
                if (apple != null && apple.IsActive)
                {
                    if (readY != writeY)
                    {
                        _grid[x, readY] = null;
                        _grid[x, writeY] = apple;
                        apple.GridY = writeY;

                        float dur = 0.1f + 0.03f * (readY - writeY);
                        animating++;
                        apple.MoveTo(GridToWorld(x, writeY), dur, () => animating--);
                    }
                    writeY++;
                }
            }
        }

        while (animating > 0) yield return null;
    }

    /// <summary>
    /// 목표 합을 만족하는 유효한 사각 선택이 있는지 찾습니다.
    /// </summary>
    public (Vector2Int min, Vector2Int max)? FindValidSelection(int targetSum)
    {
        if (_grid == null) return null;

        // 작은 영역부터 탐색 (플레이어에게 유용한 힌트를 위해)
        for (int w = 1; w <= Columns; w++)
        {
            for (int h = 1; h <= Rows; h++)
            {
                for (int x1 = 0; x1 <= Columns - w; x1++)
                {
                    for (int y1 = 0; y1 <= Rows - h; y1++)
                    {
                        int x2 = x1 + w - 1;
                        int y2 = y1 + h - 1;

                        int sum = 0;
                        int count = 0;

                        for (int xx = x1; xx <= x2; xx++)
                        {
                            for (int yy = y1; yy <= y2; yy++)
                            {
                                var a = _grid[xx, yy];
                                if (a != null && a.IsActive)
                                {
                                    sum += a.Number;
                                    count++;
                                }
                            }
                            if (sum > targetSum) break;
                        }

                        if (count > 0 && sum == targetSum)
                            return (new Vector2Int(x1, y1), new Vector2Int(x2, y2));
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 유효한 움직임이 존재하는지 확인합니다.
    /// </summary>
    public bool HasValidMoves(int targetSum)
    {
        return FindValidSelection(targetSum).HasValue;
    }

    /// <summary>
    /// 힌트를 표시합니다 (유효한 선택 영역 하이라이트).
    /// </summary>
    public void ShowHint(int targetSum)
    {
        var hint = FindValidSelection(targetSum);
        if (hint == null) return;

        var (min, max) = hint.Value;
        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                if (_grid[x, y] != null && _grid[x, y].IsActive)
                    _grid[x, y].ShowHint(true);

        StartCoroutine(CoClearHintAfterDelay(min, max, 2f));
    }

    private IEnumerator CoClearHintAfterDelay(Vector2Int min, Vector2Int max, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_grid == null) yield break;

        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                if (x < Columns && y < Rows && _grid[x, y] != null && _grid[x, y].IsActive)
                    _grid[x, y].ShowHint(false);
    }

    /// <summary>
    /// 남은 사과들의 위치를 섞습니다.
    /// </summary>
    public void ShuffleApples()
    {
        if (_grid == null) return;

        var activeApples = new List<Apple>();
        var positions = new List<(int x, int y)>();

        for (int x = 0; x < Columns; x++)
            for (int y = 0; y < Rows; y++)
                if (_grid[x, y] != null && _grid[x, y].IsActive)
                {
                    activeApples.Add(_grid[x, y]);
                    positions.Add((x, y));
                }

        // Fisher-Yates 셔플
        for (int i = activeApples.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = activeApples[i];
            activeApples[i] = activeApples[j];
            activeApples[j] = temp;
        }

        // 새 위치에 재배치
        _grid = new Apple[Columns, Rows];
        for (int i = 0; i < activeApples.Count; i++)
        {
            var (x, y) = positions[i];
            var apple = activeApples[i];
            _grid[x, y] = apple;
            apple.GridX = x;
            apple.GridY = y;
            apple.MoveTo(GridToWorld(x, y), 0.35f);
        }
    }

    /// <summary>
    /// 활성 사과 수를 반환합니다.
    /// </summary>
    public int ActiveAppleCount()
    {
        int count = 0;
        if (_grid == null) return 0;
        for (int x = 0; x < Columns; x++)
            for (int y = 0; y < Rows; y++)
                if (_grid[x, y] != null && _grid[x, y].IsActive)
                    count++;
        return count;
    }
}
