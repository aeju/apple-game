using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 개별 사과 오브젝트. 숫자, 시각 상태, 애니메이션을 관리합니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Apple : MonoBehaviour
{
    public int Number { get; private set; }
    public int GridX { get; set; }
    public int GridY { get; set; }
    public bool IsActive { get; private set; } = true;

    private SpriteRenderer _sr;
    private TextMeshPro _label;
    private Color _baseColor;
    private Vector3 _normalScale;

    private static Sprite _circleSprite;

    // 숫자별 색상 (1~9)
    public static readonly Color[] NumberColors =
    {
        new Color(0.55f, 0.87f, 0.55f), // 1 - 연두
        new Color(0.30f, 0.75f, 0.42f), // 2 - 초록
        new Color(0.97f, 0.85f, 0.35f), // 3 - 노랑
        new Color(1.00f, 0.62f, 0.22f), // 4 - 주황
        new Color(0.93f, 0.28f, 0.22f), // 5 - 빨강
        new Color(0.76f, 0.16f, 0.16f), // 6 - 진빨강
        new Color(0.60f, 0.32f, 0.78f), // 7 - 보라
        new Color(0.35f, 0.55f, 0.90f), // 8 - 파랑
        new Color(0.22f, 0.33f, 0.70f), // 9 - 진파랑
    };

    /// <summary>
    /// 원형 스프라이트를 생성(캐싱)합니다.
    /// </summary>
    public static Sprite GetCircleSprite()
    {
        if (_circleSprite != null) return _circleSprite;

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;
        float radius = center - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                {
                    float edgeSmooth = Mathf.Clamp01((radius - dist) * 0.5f + 0.5f);
                    tex.SetPixel(x, y, new Color(1, 1, 1, edgeSmooth));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        return _circleSprite;
    }

    /// <summary>
    /// 사과를 초기화합니다.
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
        _sr.sprite = GetCircleSprite();
        _sr.sortingOrder = 1;
        _baseColor = NumberColors[number - 1];
        _sr.color = _baseColor;

        // 숫자 텍스트
        var child = transform.Find("Label");
        if (child != null)
        {
            _label = child.GetComponent<TextMeshPro>();
        }
        else
        {
            var go = new GameObject("Label");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -0.05f, -0.01f);
            _label = go.AddComponent<TextMeshPro>();
        }

        _label.text = number.ToString();
        _label.fontSize = 5.5f;
        _label.fontStyle = FontStyles.Bold;
        _label.alignment = TextAlignmentOptions.Center;
        _label.color = Color.white;
        _label.sortingOrder = 2;
        _label.rectTransform.sizeDelta = Vector2.one;

        _normalScale = Vector3.one * 0.45f;
        transform.localScale = _normalScale;
    }

    /// <summary>
    /// 선택 하이라이트를 설정합니다.
    /// </summary>
    public void Highlight(bool on)
    {
        if (!IsActive) return;
        _sr.color = on ? Color.Lerp(_baseColor, Color.white, 0.4f) : _baseColor;
        transform.localScale = on ? Vector3.one * 0.50f : _normalScale;
    }

    /// <summary>
    /// 힌트 하이라이트를 설정합니다.
    /// </summary>
    public void ShowHint(bool on)
    {
        if (!IsActive) return;
        _sr.color = on ? Color.Lerp(_baseColor, new Color(1f, 0.95f, 0.3f), 0.55f) : _baseColor;
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
            float ease = 1f - Mathf.Pow(1f - p, 3f); // ease-out cubic
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
