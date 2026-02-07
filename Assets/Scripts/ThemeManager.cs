using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 밝은 테마 / 다크 테마 전환을 관리합니다.
/// 친구 고양이(검정 고양이)는 밝은 배경에서 더 돋보입니다!
/// </summary>
public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    public enum Theme { Light, Dark }

    [Header("Current Theme")]
    public Theme CurrentTheme = Theme.Light;

    [Header("Events")]
    public UnityEvent<Theme> OnThemeChanged = new UnityEvent<Theme>();

    // ─── 밝은 테마 색상 ───
    public static class LightTheme
    {
        public static readonly Color Background = new Color(0.96f, 0.94f, 0.91f);      // 따뜻한 크림
        public static readonly Color GridBG = new Color(1.0f, 0.98f, 0.95f, 0.8f);     // 아이보리
        public static readonly Color CameraBG = new Color(0.93f, 0.90f, 0.87f);        // 베이지
        
        public static readonly Color UIPanel = new Color(1.0f, 1.0f, 1.0f, 0.95f);     // 화이트
        public static readonly Color HUDBar = new Color(1.0f, 0.98f, 0.96f, 0.92f);    // 밝은 크림
        
        public static readonly Color TextPrimary = new Color(0.17f, 0.17f, 0.18f);     // 차콜
        public static readonly Color TextSecondary = new Color(0.40f, 0.40f, 0.42f);   // 그레이
        public static readonly Color TextAccent = new Color(0.95f, 0.78f, 0.10f);      // 황금 (고양이 눈)
        
        public static readonly Color BtnPrimary = new Color(1.0f, 0.72f, 0.29f);       // 따뜻한 오렌지
        public static readonly Color BtnSecondary = new Color(0.85f, 0.85f, 0.87f);    // 라이트 그레이
        public static readonly Color BtnSuccess = new Color(0.60f, 0.80f, 0.65f);      // 소프트 그린
        
        public static readonly Color SelectionFill = new Color(0.95f, 0.78f, 0.10f, 0.15f);
        public static readonly Color SelectionBorder = new Color(0.95f, 0.78f, 0.10f, 0.7f);
        public static readonly Color SelectionSuccess = new Color(0.95f, 0.85f, 0.10f);
        public static readonly Color SelectionFail = new Color(0.95f, 0.35f, 0.35f);
    }

    // ─── 다크 테마 색상 ───
    public static class DarkTheme
    {
        public static readonly Color Background = new Color(0.04f, 0.04f, 0.06f);      // 딥 블랙
        public static readonly Color GridBG = new Color(0.08f, 0.08f, 0.12f, 0.5f);    // 미드나잇
        public static readonly Color CameraBG = new Color(0.04f, 0.04f, 0.06f);        // 딥 블랙
        
        public static readonly Color UIPanel = new Color(0.08f, 0.08f, 0.10f, 0.94f);  // 검정
        public static readonly Color HUDBar = new Color(0.06f, 0.06f, 0.08f, 0.88f);   // 차콜
        
        public static readonly Color TextPrimary = new Color(1.0f, 1.0f, 1.0f);        // 화이트
        public static readonly Color TextSecondary = new Color(0.70f, 0.70f, 0.72f);   // 라이트 그레이
        public static readonly Color TextAccent = new Color(0.95f, 0.78f, 0.10f);      // 황금 (고양이 눈)
        
        public static readonly Color BtnPrimary = new Color(0.90f, 0.72f, 0.12f);      // 황금
        public static readonly Color BtnSecondary = new Color(0.30f, 0.30f, 0.35f);    // 다크 그레이
        public static readonly Color BtnSuccess = new Color(0.40f, 0.65f, 0.45f);      // 딥 그린
        
        public static readonly Color SelectionFill = new Color(0.95f, 0.78f, 0.10f, 0.10f);
        public static readonly Color SelectionBorder = new Color(0.95f, 0.78f, 0.10f, 0.6f);
        public static readonly Color SelectionSuccess = new Color(0.95f, 0.85f, 0.10f);
        public static readonly Color SelectionFail = new Color(1.0f, 0.35f, 0.35f);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        // 저장된 테마 불러오기
        int savedTheme = PlayerPrefs.GetInt("GameTheme", 0); // 0=Light, 1=Dark
        CurrentTheme = (Theme)savedTheme;
    }

    /// <summary>
    /// 테마를 전환합니다.
    /// </summary>
    public void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == Theme.Light ? Theme.Dark : Theme.Light;
        SaveTheme();
        OnThemeChanged?.Invoke(CurrentTheme);
    }

    /// <summary>
    /// 특정 테마로 설정합니다.
    /// </summary>
    public void SetTheme(Theme theme)
    {
        if (CurrentTheme == theme) return;
        CurrentTheme = theme;
        SaveTheme();
        OnThemeChanged?.Invoke(CurrentTheme);
    }

    /// <summary>
    /// 테마를 저장합니다.
    /// </summary>
    private void SaveTheme()
    {
        PlayerPrefs.SetInt("GameTheme", (int)CurrentTheme);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 현재 테마의 색상을 가져옵니다.
    /// </summary>
    public Color GetColor(string colorName)
    {
        var theme = CurrentTheme == Theme.Light ? typeof(LightTheme) : typeof(DarkTheme);
        var field = theme.GetField(colorName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (field != null)
            return (Color)field.GetValue(null);
        return Color.white;
    }

    /// <summary>
    /// 밝은 테마인지 확인합니다.
    /// </summary>
    public bool IsLightTheme()
    {
        return CurrentTheme == Theme.Light;
    }
}
