using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// AppleGameSetup의 Custom Editor
/// Inspector에 버튼을 추가하여 Edit 모드에서 UI를 생성/삭제할 수 있습니다.
/// </summary>
[CustomEditor(typeof(AppleGameSetup))]
public class AppleGameSetupEditor : Editor
{
    private static readonly string[] PanelNames = {
        "MainMenuPanel", "HUDPanel", "CountdownPanel", "ResultPanel", "LeaderboardPanel"
    };

    private static readonly string[] PanelLabels = {
        "Main Menu", "HUD", "Countdown", "Result", "Leaderboard"
    };

    private static readonly Color[] PanelColors = {
        new Color(0.6f, 0.85f, 0.6f),
        new Color(0.6f, 0.75f, 0.9f),
        new Color(0.9f, 0.85f, 0.6f),
        new Color(0.85f, 0.6f, 0.85f),
        new Color(0.9f, 0.7f, 0.5f)
    };

    private bool _showPanelTools = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AppleGameSetup setup = (AppleGameSetup)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Scene Setup Tools", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Edit 모드에서 게임 오브젝트들을 생성/삭제할 수 있습니다.\n" +
            "생성 후 Scene view와 Inspector에서 자유롭게 편집하세요!",
            MessageType.Info);

        EditorGUILayout.Space(5);

        // Generate All 버튼
        GUI.backgroundColor = new Color(0.5f, 0.8f, 0.5f);
        if (GUILayout.Button("Generate All Objects in Scene", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "Generate Scene Objects",
                "Scene에 모든 게임 오브젝트를 생성합니다.\n" +
                "이미 생성된 오브젝트가 있으면 건너뜁니다.\n\n계속하시겠습니까?",
                "Generate", "Cancel"))
            {
                setup.GenerateInEditMode();
                EditorUtility.DisplayDialog("Complete", "Scene에 오브젝트가 생성되었습니다!", "OK");
            }
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(5);

        // Clear All 버튼
        GUI.backgroundColor = new Color(1f, 0.6f, 0.5f);
        if (GUILayout.Button("Clear Generated Objects", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(
                "Clear Generated Objects",
                "생성된 모든 오브젝트를 삭제합니다.\n" +
                "주의: 수동으로 추가한 오브젝트도 삭제될 수 있습니다!\n\n계속하시겠습니까?",
                "Delete", "Cancel"))
            {
                setup.ClearGeneratedObjects();
                EditorUtility.DisplayDialog("Complete", "생성된 오브젝트가 삭제되었습니다.", "OK");
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        // 개별 패널 도구
        _showPanelTools = EditorGUILayout.Foldout(_showPanelTools, "Panel Tools (개별 패널 생성/삭제)", true, EditorStyles.foldoutHeader);
        if (_showPanelTools)
        {
            DrawPanelTools();
        }

        EditorGUILayout.Space(10);

        // 상태 표시
        DrawStatusSection();
    }

    private void DrawPanelTools()
    {
        var uiManager = FindUIManager();

        if (uiManager == null)
        {
            EditorGUILayout.HelpBox(
                "UIManager가 없습니다.\n먼저 'Generate All Objects'로 기본 구조를 생성하세요.",
                MessageType.Warning);
            return;
        }

        EditorGUILayout.HelpBox(
            "개별 패널을 생성하거나 삭제할 수 있습니다.\n" +
            "생성된 패널은 Hierarchy에서 자유롭게 편집 가능합니다.\n" +
            "⚠ (재)생성 시 기존 패널의 수정 사항은 초기화됩니다!",
            MessageType.Info);

        Canvas canvas = uiManager.GetComponent<Canvas>();

        EditorGUILayout.Space(3);

        for (int i = 0; i < PanelNames.Length; i++)
        {
            bool exists = canvas != null && canvas.transform.Find(PanelNames[i]) != null;

            EditorGUILayout.BeginHorizontal();

            string statusIcon = exists ? "●" : "○";
            Color statusColor = exists ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            var statusStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = statusColor },
                fixedWidth = 18
            };
            EditorGUILayout.LabelField(statusIcon, statusStyle, GUILayout.Width(18));
            EditorGUILayout.LabelField(PanelLabels[i], GUILayout.Width(100));

            GUI.backgroundColor = PanelColors[i];
            if (GUILayout.Button(exists ? "Regenerate" : "Generate", GUILayout.Height(22)))
            {
                string action = exists ? "재생성" : "생성";
                if (!exists || EditorUtility.DisplayDialog(
                    $"{PanelLabels[i]} Panel",
                    $"{PanelLabels[i]} 패널을 {action}합니다.\n" +
                    (exists ? "기존 패널의 수정 사항이 초기화됩니다!\n\n" : "\n") +
                    "계속하시겠습니까?",
                    action, "Cancel"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, $"Generate {PanelNames[i]}");
                    uiManager.EditorGeneratePanel(PanelNames[i]);
                    EditorUtility.SetDirty(canvas.gameObject);
                }
            }

            GUI.backgroundColor = new Color(1f, 0.65f, 0.55f);
            GUI.enabled = exists;
            if (GUILayout.Button("Delete", GUILayout.Width(60), GUILayout.Height(22)))
            {
                if (EditorUtility.DisplayDialog(
                    $"Delete {PanelLabels[i]}",
                    $"{PanelLabels[i]} 패널을 삭제합니다.\n\n계속하시겠습니까?",
                    "Delete", "Cancel"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, $"Delete {PanelNames[i]}");
                    uiManager.EditorDeletePanel(PanelNames[i]);
                    EditorUtility.SetDirty(canvas.gameObject);
                }
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawStatusSection()
    {
        EditorGUILayout.LabelField("Current State:", EditorStyles.boldLabel);

        bool hasCamera = Camera.main != null;
        bool hasCanvas = GameObject.Find("Canvas") != null;
        bool hasManagers = GameObject.Find("GameManager") != null;

        Canvas canvas = null;
        if (hasCanvas)
            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Camera: {(hasCamera ? "✓" : "✗")}");
        EditorGUILayout.LabelField($"Canvas: {(hasCanvas ? "✓" : "✗")}");
        EditorGUILayout.LabelField($"Managers: {(hasManagers ? "✓" : "✗")}");

        if (canvas != null)
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Panels:", EditorStyles.miniLabel);
            for (int i = 0; i < PanelNames.Length; i++)
            {
                bool exists = canvas.transform.Find(PanelNames[i]) != null;
                EditorGUILayout.LabelField($"  {PanelLabels[i]}: {(exists ? "✓" : "✗")}");
            }
        }
        EditorGUILayout.EndVertical();
    }

    private UIManager FindUIManager()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return null;
        return canvas.GetComponent<UIManager>();
    }
}
