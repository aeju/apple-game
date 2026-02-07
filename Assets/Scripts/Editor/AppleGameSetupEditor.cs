using UnityEngine;
using UnityEditor;

/// <summary>
/// AppleGameSetup의 Custom Editor
/// Inspector에 버튼을 추가하여 Edit 모드에서 UI를 생성/삭제할 수 있습니다.
/// </summary>
[CustomEditor(typeof(AppleGameSetup))]
public class AppleGameSetupEditor : Editor
{
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

        // Generate 버튼
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

        // Clear 버튼
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

        // 상태 표시
        EditorGUILayout.LabelField("Current State:", EditorStyles.boldLabel);
        bool hasCamera = Camera.main != null;
        bool hasCanvas = GameObject.Find("Canvas") != null;
        bool hasManagers = GameObject.Find("GameManager") != null;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Camera: {(hasCamera ? "✓ Exists" : "✗ Missing")}");
        EditorGUILayout.LabelField($"Canvas: {(hasCanvas ? "✓ Exists" : "✗ Missing")}");
        EditorGUILayout.LabelField($"Managers: {(hasManagers ? "✓ Exists" : "✗ Missing")}");
        EditorGUILayout.EndVertical();
    }
}
