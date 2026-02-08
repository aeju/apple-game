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
    private Coroutine _hintCoroutine = null; // 힌트 애니메이션 코루틴
    private GameObject _outlineObject = null; // 힌트 효과용 외곽선 오브젝트
    private SpriteRenderer _outlineSR = null; // 외곽선 SpriteRenderer

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
    /// 힌트 하이라이트를 설정합니다. (고양이 눈 색상 글로우 + 외곽선)
    /// </summary>
    public void ShowHint(bool on)
    {
        if (!IsActive) return;
        
        // 기존 힌트 애니메이션 중지
        if (_hintCoroutine != null)
        {
            StopCoroutine(_hintCoroutine);
            _hintCoroutine = null;
        }
        
        if (on)
        {
            // 외곽선 오브젝트 생성
            CreateOutline();
            
            // 펄스 애니메이션 시작
            _hintCoroutine = StartCoroutine(CoHintPulse());
        }
        else
        {
            // 외곽선 제거
            DestroyOutline();
            
            // 원래 상태로 복원
            transform.localScale = _normalScale;
            _sr.color = _baseColor;
        }
    }
    
    /// <summary>
    /// 힌트 외곽선 오브젝트 생성
    /// </summary>
    private void CreateOutline()
    {
        if (_outlineObject != null) return; // 이미 존재하면 무시
        
        _outlineObject = new GameObject("HintOutline");
        _outlineObject.transform.SetParent(transform, false);
        _outlineObject.transform.localPosition = Vector3.zero;
        _outlineObject.transform.localRotation = Quaternion.identity;
        
        _outlineSR = _outlineObject.AddComponent<SpriteRenderer>();
        _outlineSR.sprite = _sr.sprite; // 동일한 sprite 사용
        _outlineSR.sortingOrder = _sr.sortingOrder - 1; // 본체보다 뒤에 렌더링
        
        // 외곽선 색상 (검정색)
        Color outlineColor = new Color(0f, 0f, 0f, 0.8f);
        _outlineSR.color = outlineColor;
        
        // 약간 더 크게 (외곽선 효과 - 매우 얇게)
        _outlineObject.transform.localScale = Vector3.one * 1.0375f;
        
        Debug.Log($"[Apple {Number}] Hint outline created!");
    }
    
    /// <summary>
    /// 힌트 외곽선 오브젝트 제거
    /// </summary>
    private void DestroyOutline()
    {
        if (_outlineObject != null)
        {
            Destroy(_outlineObject);
            _outlineObject = null;
            _outlineSR = null;
        }
    }
    
    /// <summary>
    /// 힌트 펄스 애니메이션 (크기 + 발광 + 외곽선)
    /// </summary>
    private IEnumerator CoHintPulse()
    {
        float pulseSpeed = 3f; // 펄스 속도
        float minScale = 1.0f;
        float maxScale = 1.25f; // 최대 25% 확대
        
        Color glowColor = new Color(1f, 0.95f, 0.3f); // 황금빛 발광
        float glowIntensity = _isUsingCustomSprite ? 0.4f : 0.6f; // 커스텀 이미지는 약간 덜 밝게
        
        // 외곽선 색상 (검정색)
        Color outlineColorBase = new Color(0f, 0f, 0f, 1f);
        
        while (true)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            
            // 크기 펄스
            float scale = Mathf.Lerp(minScale, maxScale, t);
            transform.localScale = _normalScale * scale;
            
            // 색상 펄스 (발광 효과)
            if (_isUsingCustomSprite)
            {
                // 커스텀 이미지: 밝게 빛나는 효과 (원본 색상 유지하면서)
                float brightness = 1f + (t * 0.3f); // 최대 30% 밝게
                _sr.color = new Color(brightness, brightness, brightness, 1f);
            }
            else
            {
                // 프로그래밍 발바닥: 황금빛으로 빛남
                _sr.color = Color.Lerp(_baseColor, glowColor, t * glowIntensity);
            }
            
            // 외곽선 펄스 (투명도 변화) ✨
            if (_outlineSR != null)
            {
                // t에 따라 외곽선이 켜졌다 꺼졌다
                float outlineAlpha = Mathf.Lerp(0.3f, 0.95f, t); // 30% ~ 95% 투명도
                Color outlineColor = outlineColorBase;
                outlineColor.a = outlineAlpha;
                _outlineSR.color = outlineColor;
                
                // 외곽선도 약간 크기 변화 (매우 얇은 두께)
                float outlineScale = Mathf.Lerp(1.0375f, 1.0875f, t);
                _outlineObject.transform.localScale = Vector3.one * outlineScale;
            }
            
            yield return null;
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
        // 폭신 터지기 효과 🎉
        
        // 1단계: 스쿼시 (눌림)
        float squashDur = 0.08f;
        Vector3 startScale = transform.localScale;
        Color startColor = _sr.color;
        Color startLabelColor = _label != null ? _label.color : Color.white;
        
        for (float t = 0; t < squashDur; t += Time.deltaTime)
        {
            float p = t / squashDur;
            float squash = 1f - 0.2f * Mathf.Sin(p * Mathf.PI);
            transform.localScale = startScale * squash;
            yield return null;
        }
        
        // 2단계: 폭신 터지기 (부풀어 오름 + 파티클)
        float popDur = 0.15f;
        
        // 작은 발바닥 파티클 생성
        SpawnPawParticles();
        
        // 반짝이 파티클 생성
        SpawnSparkleParticles();
        
        for (float t = 0; t < popDur; t += Time.deltaTime)
        {
            float p = t / popDur;
            
            // 크게 부풀어 오름
            float scale = 1f + 0.6f * p;
            transform.localScale = startScale * scale;
            
            // 밝아지며 사라짐
            float alpha = 1f - p * p; // ease-in
            float brightness = 1f + 0.5f * p; // 밝아짐
            _sr.color = new Color(startColor.r * brightness, startColor.g * brightness, startColor.b * brightness, alpha);
            
            if (_label != null)
                _label.color = new Color(startLabelColor.r, startLabelColor.g, startLabelColor.b, alpha);
            
            yield return null;
        }

        onDone?.Invoke();
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 작은 발바닥 파티클 생성
    /// </summary>
    private void SpawnPawParticles()
    {
        int particleCount = Random.Range(4, 7); // 4-6개
        
        for (int i = 0; i < particleCount; i++)
        {
            // 파티클 생성
            var particleGo = new GameObject("PawParticle");
            particleGo.transform.position = transform.position;
            particleGo.transform.localScale = transform.localScale * 0.3f; // 작게
            
            var sr = particleGo.AddComponent<SpriteRenderer>();
            sr.sprite = _sr.sprite;
            sr.color = _baseColor;
            sr.sortingOrder = _sr.sortingOrder + 1;
            
            // 랜덤 방향으로 날아가기
            float angle = (360f / particleCount) * i + Random.Range(-20f, 20f);
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
            float distance = Random.Range(0.8f, 1.5f);
            
            // 자체 파괴 컴포넌트 추가
            var autoDestroy = particleGo.AddComponent<ParticleAutoDestroy>();
            autoDestroy.StartFloat(direction * distance, Random.Range(0.4f, 0.6f));
        }
    }
    
    /// <summary>
    /// 반짝이 파티클 생성
    /// </summary>
    private void SpawnSparkleParticles()
    {
        int sparkleCount = Random.Range(5, 9); // 5-8개
        
        for (int i = 0; i < sparkleCount; i++)
        {
            var sparkleGo = new GameObject("Sparkle");
            sparkleGo.transform.position = transform.position + new Vector3(
                Random.Range(-0.2f, 0.2f),
                Random.Range(-0.2f, 0.2f),
                0
            );
            
            // 별 모양 스프라이트 생성 (간단한 다이아몬드)
            var sr = sparkleGo.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSparkleSprite();
            sr.color = new Color(1f, 0.95f, 0.3f, 1f); // 황금색
            sr.sortingOrder = _sr.sortingOrder + 2;
            
            float size = Random.Range(0.1f, 0.2f);
            sparkleGo.transform.localScale = Vector3.one * size;
            
            // 랜덤 방향으로
            Vector3 direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
            float distance = Random.Range(0.5f, 1.2f);
            
            // 자체 파괴 컴포넌트 추가
            var autoDestroy = sparkleGo.AddComponent<ParticleAutoDestroy>();
            autoDestroy.StartFloat(direction * distance, Random.Range(0.3f, 0.5f));
        }
    }
    
    /// <summary>
    /// 별 모양 스프라이트 생성
    /// </summary>
    private Sprite CreateSparkleSprite()
    {
        int size = 16;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        
        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        // 간단한 다이아몬드 모양
        int center = size / 2;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - center);
                int dy = Mathf.Abs(y - center);
                
                if (dx + dy < center)
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
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
