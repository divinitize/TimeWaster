using UnityEngine;
using UnityEditor;

public class TimeWasterDetailWindow : EditorWindow
{
    private Vector2 scrollPosition;

    [MenuItem("Window/Time Waster Stats")]
    public static void ShowWindow()
    {
        var window = GetWindow<TimeWasterDetailWindow>("Time Waster Stats");
        window.minSize = new Vector2(350, 250);
        window.maxSize = new Vector2(600, 500);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Unity Time Waster Tracker", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        Rect line = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(line, new Color(0.3f, 0.3f, 0.3f));
        GUILayout.Space(10);

        float totalTime = TimeWasterTracker.GetTotalWastedTime();
        var categories = TimeWasterTracker.GetWaitingCategories();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUIStyle totalStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
        };

        GUILayout.Label($"Total time wasted: {FormatTime(totalTime)}", totalStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(15);
        GUILayout.Label("Breakdown by Category:", EditorStyles.boldLabel);
        GUILayout.Space(5);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (var category in categories)
        {
            if (category.Value > 0f)
            {
                float percentage = totalTime > 0 ? (category.Value / totalTime) * 100f : 0f;

                GUILayout.BeginHorizontal();

                GUILayout.Label($"{category.Key}:", GUILayout.Width(120));
                GUILayout.Label(FormatTime(category.Value), GUILayout.Width(60));
                GUILayout.Label($"({percentage:F1}%)", GUILayout.Width(50));

                Rect progressRect = GUILayoutUtility.GetRect(100, 12);
                EditorGUI.ProgressBar(progressRect, percentage / 100f, "");

                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
        }

        GUILayout.EndScrollView();
        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Refresh", GUILayout.Height(28)))
        {
            Repaint();
        }

        GUILayout.Space(10);

        GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f);
        if (GUILayout.Button("Reset Timer", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("Reset Timer",
                "Are you sure you want to reset the time waster tracker? This will clear all recorded data.",
                "Yes, Reset", "Cancel"))
            {
                TimeWasterTracker.ResetTimer();
                Repaint();
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIStyle footerStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = Color.gray }
        };
        GUILayout.Label("Data persists between editor sessions", footerStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:D2}:{secs:D2}";
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }
}
