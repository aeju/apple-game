using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 숫자 라벨 색상 모드
/// </summary>
public enum LabelColorMode
{
    Auto,       // 자동 색상 (밝은 패드=어두운 글자, 어두운 패드=밝은 글자)
    Uniform,    // 모든 숫자에 같은 색상 적용
    Individual  // 각 숫자마다 다른 색상 적용
}

/// <summary>
/// 고양이 발바닥(肉球) 오브젝트. 숫자, 시각 상태, 애니메이션을 관리합니다.
/// 검정 고양이 헌정 게임 - 발바닥 모양 스프라이트를 사용합니다.
/// Inspector에서 외부 이미지를 할당할 수 있습니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Apple : MonoBehaviour
{
    [Header("External Sprites (Optional - 할당 시 프로그래밍 생성 무시)")]
    [Tooltip("숫자 1~9에 해당하는 스프라이트를 순서대로 할당하세요 (256x256 PNG 권장)")]
    [SerializeField] private Sprite[] customSprites = new Sprite[9];
    
    [Header("Number Label Settings")]
    [Tooltip("숫자 라벨 표시 여부 (체크 해제 시 숫자 숨김)")]
    [SerializeField] private bool showNumberLabel = true;
    
    [Tooltip("숫자 라벨에 사용할 TMP 폰트 (null이면 기본 폰트 사용)")]
    [SerializeField] private TMP_FontAsset customFont = null;
    
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

    public int Number { get; private set; }
    public int GridX { get; set; }
    public int GridY { get; set; }
    public bool IsActive { get; private set; } = true;

    private SpriteRenderer _sr;
    private TextMeshPro _label;
    private Color _baseColor;
    private Vector3 _normalScale;
    private bool _isUsingCustomSprite = false; // 커스텀 이미지 사용 여부

    private static Sprite[] _pawSprites = new Sprite[9];
    private static Sprite[] _customSpritesCache = null;

    // 숫자별 색상 (1~9) - 고양이 테마 따뜻한 톤
    public static readonly Color[] NumberColors =
    {
        new Color(0.60f, 0.82f, 0.60f), // 1 - 부드러운 세이지
        new Color(0.30f, 0.68f, 0.42f), // 2 - 포레스트 그린
        new Color(0.95f, 0.78f, 0.15f), // 3 - 황금 앰버 (고양이 눈)
        new Color(0.95f, 0.55f, 0.18f), // 4 - 따뜻한 오렌지
        new Color(0.85f, 0.25f, 0.22f), // 5 - 크림슨
        new Color(0.62f, 0.15f, 0.18f), // 6 - 다크 크림슨
        new Color(0.52f, 0.28f, 0.72f), // 7 - 딥 퍼플
        new Color(0.28f, 0.48f, 0.82f), // 8 - 스틸 블루
        new Color(0.18f, 0.25f, 0.60f), // 9 - 미드나잇 네이비
    };

    // 발바닥 패드 색상 (1~9) - 숫자별 다양한 육구 색상
    private static readonly Color[] PadColors =
    {
        new Color(1.0f, 0.80f, 0.85f), // 1 - 연한 분홍 (새끼 고양이)
        new Color(1.0f, 0.75f, 0.80f), // 2 - 밝은 분홍
        new Color(1.0f, 0.85f, 0.70f), // 3 - 복숭아색
        new Color(0.95f, 0.70f, 0.75f), // 4 - 로즈 핑크
        new Color(0.12f, 0.12f, 0.15f), // 5 - 검정 (친구 고양이!) ★
        new Color(0.35f, 0.25f, 0.30f), // 6 - 다크 브라운
        new Color(0.28f, 0.22f, 0.32f), // 7 - 딥 퍼플 브라운
        new Color(0.30f, 0.30f, 0.35f), // 8 - 차콜
        new Color(0.20f, 0.20f, 0.28f), // 9 - 미드나잇 그레이
    };

    /// <summary>
    /// 숫자별로 다른 스프라이트를 반환합니다.
    /// 외부 이미지가 할당되어 있으면 그것을 사용하고, 없으면 프로그래밍 방식으로 생성합니다.
    /// 1-4: 작은 귀여운 발바닥 (밝은 패드)
    /// 5: 친구 고양이 발바닥 (검정 패드, 특별 디자인!)
    /// 6-9: 어른 고양이 발바닥 (진한 패드)
    /// </summary>
    public Sprite GetSprite(int number)
    {
        int idx = number - 1;

        // 외부 이미지가 할당되어 있으면 사용
        if (customSprites != null && customSprites.Length > idx && customSprites[idx] != null)
        {
            // Static 캐시에 저장 (다른 Apple 오브젝트도 사용)
            if (_customSpritesCache == null)
                _customSpritesCache = new Sprite[9];
            _customSpritesCache[idx] = customSprites[idx];
            return customSprites[idx];
        }

        // 캐시된 커스텀 스프라이트 확인
        if (_customSpritesCache != null && _customSpritesCache[idx] != null)
            return _customSpritesCache[idx];

        // 프로그래밍 방식으로 생성된 스프라이트 확인
        if (_pawSprites[idx] != null) return _pawSprites[idx];

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float s = size;
        Color padColor = PadColors[idx];

        // 숫자별 사이즈 조정 (1-4: 작음, 5: 중간+특별, 6-9: 큼)
        float sizeScale = 1.0f;
        if (number <= 4) sizeScale = 0.88f;      // 새끼 고양이 발
        else if (number == 5) sizeScale = 1.05f; // 친구 고양이 발 (특별!)
        else sizeScale = 0.95f;                   // 어른 고양이 발

        // 초기화
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, Color.clear);

        // 발바닥 그리기 (숫자별 변형)
        for (int py = 0; py < size; py++)
        {
            for (int px = 0; px < size; px++)
            {
                float fx = px;
                float fy = py;
                bool inPaw = false;
                Color pixelColor = Color.clear;

                // 메인 패드 위치/크기 조정 (숫자별 변형)
                float mainCx = s * 0.5f;
                float mainCy = s * 0.36f;
                float mainRx = s * 0.30f * sizeScale;
                float mainRy = s * 0.23f * sizeScale;

                // 숫자별 메인 패드 모양 변형
                if (number == 5)
                {
                    // 친구 고양이 - 하트 모양에 가까운 패드
                    mainRx *= 0.95f;
                    mainRy *= 1.05f;
                }
                else if (number <= 4)
                {
                    // 새끼 고양이 - 둥글둥글
                    mainRy *= 1.08f;
                }

                // 메인 패드 (큰 젤리)
                {
                    float dx = (fx - mainCx) / mainRx;
                    float dy = (fy - mainCy) / mainRy;
                    float d = dx * dx + dy * dy;
                    if (d <= 1f)
                    {
                        inPaw = true;
                        float edgeSmooth = Mathf.Clamp01((1f - Mathf.Sqrt(d)) * 3f);
                        pixelColor = new Color(padColor.r, padColor.g, padColor.b, edgeSmooth);
                    }
                }

                // 토 빈 (발가락) - 숫자별 위치 약간 다름
                float toeScale = sizeScale;
                float toeY1 = s * (0.68f + (number % 3) * 0.01f);
                float toeY2 = s * (0.76f + (number % 2) * 0.01f);
                float toeY3 = s * (0.68f + ((number + 1) % 3) * 0.01f);

                // 토 빈 1 (왼쪽)
                {
                    float cx = s * 0.24f, cy = toeY1, r = s * 0.11f * toeScale;
                    float d = Vector2.Distance(new Vector2(fx, fy), new Vector2(cx, cy));
                    if (d <= r)
                    {
                        inPaw = true;
                        float edgeSmooth = Mathf.Clamp01((r - d) * 2f);
                        pixelColor = new Color(padColor.r, padColor.g, padColor.b, edgeSmooth);
                    }
                }

                // 토 빈 2 (가운데)
                {
                    float cx = s * 0.50f, cy = toeY2, r = s * 0.115f * toeScale;
                    float d = Vector2.Distance(new Vector2(fx, fy), new Vector2(cx, cy));
                    if (d <= r)
                    {
                        inPaw = true;
                        float edgeSmooth = Mathf.Clamp01((r - d) * 2f);
                        pixelColor = new Color(padColor.r, padColor.g, padColor.b, edgeSmooth);
                    }
                }

                // 토 빈 3 (오른쪽)
                {
                    float cx = s * 0.76f, cy = toeY3, r = s * 0.11f * toeScale;
                    float d = Vector2.Distance(new Vector2(fx, fy), new Vector2(cx, cy));
                    if (d <= r)
                    {
                        inPaw = true;
                        float edgeSmooth = Mathf.Clamp01((r - d) * 2f);
                        pixelColor = new Color(padColor.r, padColor.g, padColor.b, edgeSmooth);
                    }
                }

                // 숫자 5 특별 효과: 작은 노란 눈 (메인 패드 위에)
                if (number == 5 && inPaw)
                {
                    float eyeY = s * 0.42f;
                    float eyeSize = s * 0.022f;
                    
                    // 왼쪽 눈
                    float d1 = Vector2.Distance(new Vector2(fx, fy), new Vector2(s * 0.43f, eyeY));
                    if (d1 <= eyeSize)
                    {
                        Color eyeGold = new Color(0.95f, 0.78f, 0.10f);
                        pixelColor = Color.Lerp(pixelColor, eyeGold, 0.85f);
                    }
                    
                    // 오른쪽 눈
                    float d2 = Vector2.Distance(new Vector2(fx, fy), new Vector2(s * 0.57f, eyeY));
                    if (d2 <= eyeSize)
                    {
                        Color eyeGold = new Color(0.95f, 0.78f, 0.10f);
                        pixelColor = Color.Lerp(pixelColor, eyeGold, 0.85f);
                    }
                }

                if (inPaw && pixelColor.a > 0.01f)
                    tex.SetPixel(px, py, pixelColor);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        _pawSprites[idx] = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        return _pawSprites[idx];
    }

    /// <summary>
    /// 발바닥을 초기화합니다.
    /// </summary>
    public void Init(int number, int gx, int gy)
    {
        Number = number;
        GridX = gx;
        GridY = gy;
        IsActive = true;
        gameObject.SetActive(true);

        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
        _sr.sprite = GetSprite(number);
        _sr.sortingOrder = 1;
        
        // 커스텀 이미지 사용 여부 확인
        int idx = number - 1;
        _isUsingCustomSprite = (customSprites != null && customSprites.Length > idx && customSprites[idx] != null) ||
                               (_customSpritesCache != null && _customSpritesCache[idx] != null);
        
        // 커스텀 이미지는 원본 색상 유지, 프로그래밍 발바닥은 색상 적용
        _baseColor = _isUsingCustomSprite ? Color.white : NumberColors[number - 1];
        _sr.color = _baseColor;

        // 스프라이트 크기 정규화 (외부 이미지 크기가 다를 경우 대응)
        if (_sr.sprite != null)
        {
            // 목표: 모든 스프라이트가 동일한 월드 크기를 가지도록
            float targetWorldSize = 0.45f; // 목표 크기 (그리드 셀의 90%)
            
            // 스프라이트의 실제 크기 계산 (bounds)
            Bounds spriteBounds = _sr.sprite.bounds;
            float maxDimension = Mathf.Max(spriteBounds.size.x, spriteBounds.size.y);
            
            // 정규화된 스케일 계산
            float normalizedScale = targetWorldSize / maxDimension;
            _normalScale = Vector3.one * normalizedScale;
            transform.localScale = _normalScale;
            
            Debug.Log($"[Apple {number}] Sprite bounds: {spriteBounds.size}, scale: {normalizedScale}");
        }
        else
        {
            _normalScale = Vector3.one * 0.45f;
            transform.localScale = _normalScale;
        }

        // 숫자 텍스트 (메인 패드 중앙에 박혀있게)
        if (showNumberLabel)
        {
            var child = transform.Find("Label");
            if (child != null)
            {
                _label = child.GetComponent<TextMeshPro>();
            }
            else
            {
                var go = new GameObject("Label");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, labelYOffset, -0.01f);
                _label = go.AddComponent<TextMeshPro>();
            }

            // 커스텀 폰트 적용
            if (customFont != null)
            {
                _label.font = customFont;
            }

            _label.text = number.ToString();
            _label.fontSize = labelFontSize;
            _label.fontStyle = FontStyles.Bold;
            _label.alignment = TextAlignmentOptions.Center;
            
            // 숫자 색상 결정 (모드에 따라)
            _label.color = GetLabelColor(number);
            
            _label.sortingOrder = 2;
            _label.rectTransform.sizeDelta = Vector2.one;
        }
        else
        {
            // 숫자 라벨 숨김 옵션 활성화 시 기존 라벨 제거
            var child = transform.Find("Label");
            if (child != null)
            {
                Destroy(child.gameObject);
            }
            _label = null;
        }
    }

    /// <summary>
    /// 숫자 라벨의 색상을 결정합니다.
    /// </summary>
    private Color GetLabelColor(int number)
    {
        int colorIdx = number - 1;
        
        switch (labelColorMode)
        {
            case LabelColorMode.Uniform:
                // 모든 숫자에 같은 색상
                return uniformLabelColor;
            
            case LabelColorMode.Individual:
                // 각 숫자마다 다른 색상
                if (individualLabelColors != null && individualLabelColors.Length > colorIdx && individualLabelColors[colorIdx].a > 0.01f)
                {
                    return individualLabelColors[colorIdx];
                }
                // 색상이 없으면 Auto로 폴백
                goto case LabelColorMode.Auto;
            
            case LabelColorMode.Auto:
            default:
                // 자동 색상: 패드가 밝으면 어두운 글자, 어두우면 밝은 글자
                if (number <= 4)
                    return new Color(0.25f, 0.25f, 0.30f, 0.9f); // 밝은 패드에는 어두운 글자
                else if (number == 5)
                    return new Color(0.95f, 0.78f, 0.10f); // 친구 고양이는 황금 눈 색!
                else
                    return new Color(0.95f, 0.95f, 1.0f, 0.95f); // 어두운 패드에는 밝은 글자
        }
    }

    /// <summary>
    /// 선택 하이라이트를 설정합니다. (황금빛 글로우)
    /// </summary>
    public void Highlight(bool on)
    {
        if (!IsActive) return;
        
        // 커스텀 이미지는 색상 변경하지 않고 스케일만 변경
        if (_isUsingCustomSprite)
        {
            transform.localScale = on ? _normalScale * 1.11f : _normalScale;
        }
        else
        {
            _sr.color = on ? Color.Lerp(_baseColor, new Color(1f, 0.9f, 0.6f), 0.4f) : _baseColor;
            transform.localScale = on ? Vector3.one * 0.50f : _normalScale;
        }
    }

    /// <summary>
    /// 힌트 하이라이트를 설정합니다. (고양이 눈 색상 글로우)
    /// </summary>
    public void ShowHint(bool on)
    {
        if (!IsActive) return;
        
        // 커스텀 이미지는 색상 변경하지 않음
        if (!_isUsingCustomSprite)
        {
            _sr.color = on ? Color.Lerp(_baseColor, new Color(0.95f, 0.78f, 0.10f), 0.55f) : _baseColor;
        }
    }

    /// <summary>
    /// 제거 애니메이션을 실행합니다.
    /// </summary>
    public void Remove(System.Action onDone = null)
    {
        IsActive = false;
        StartCoroutine(CoRemove(onDone));
    }

    private IEnumerator CoRemove(System.Action onDone)
    {
        float dur = 0.25f;
        Vector3 startScale = transform.localScale;
        Color startColor = _sr.color;
        Color startLabelColor = _label != null ? _label.color : Color.white;

        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float p = t / dur;
            transform.localScale = startScale * (1f + 0.35f * p);

            float alpha = 1f - p;
            _sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            if (_label != null)
                _label.color = new Color(startLabelColor.r, startLabelColor.g, startLabelColor.b, alpha);

            yield return null;
        }

        onDone?.Invoke();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 목표 위치로 이동 애니메이션을 실행합니다.
    /// </summary>
    public void MoveTo(Vector3 target, float dur = 0.2f, System.Action onDone = null)
    {
        StartCoroutine(CoMove(target, dur, onDone));
    }

    private IEnumerator CoMove(Vector3 target, float dur, System.Action onDone)
    {
        Vector3 start = transform.position;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / dur);
            float ease = 1f - Mathf.Pow(1f - p, 3f);
            transform.position = Vector3.Lerp(start, target, ease);
            yield return null;
        }
        transform.position = target;
        onDone?.Invoke();
    }

    /// <summary>
    /// 등장 애니메이션을 실행합니다.
    /// </summary>
    public void Appear(float delay = 0f)
    {
        StartCoroutine(CoAppear(delay));
    }

    private IEnumerator CoAppear(float delay)
    {
        transform.localScale = Vector3.zero;
        SetAlpha(0f);

        if (delay > 0f) yield return new WaitForSeconds(delay);

        float dur = 0.25f;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / dur);
            float bounce = 1f + 0.15f * Mathf.Sin(p * Mathf.PI);
            transform.localScale = _normalScale * p * bounce;
            SetAlpha(p);
            yield return null;
        }

        transform.localScale = _normalScale;
        SetAlpha(1f);
    }

    private void SetAlpha(float a)
    {
        if (_sr != null)
        {
            var c = _sr.color;
            c.a = a;
            _sr.color = c;
        }
        if (_label != null)
        {
            var c = _label.color;
            c.a = a;
            _label.color = c;
        }
    }
}
