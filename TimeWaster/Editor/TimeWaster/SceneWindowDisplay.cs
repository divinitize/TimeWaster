using UnityEngine;
using UnityEditor;
using System;

[InitializeOnLoad]
public class SceneWindowDisplay
{
    private static Rect dragRect;
    private static bool isDragging;
    private static bool hasDragged;
    private static Vector2 dragOffset;
    private static Vector2 initialMousePos;

    private const string EditorPrefsKeyOffsetX = "TimeWasterBox_OffsetX";
    private const string EditorPrefsKeyOffsetY = "TimeWasterBox_OffsetY";
    private const string EditorPrefsKeyInitialized = "TimeWasterBox_Initialized";

    private const float DragThreshold = 3f;

    static SceneWindowDisplay()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();

        float totalTime = TimeWasterTracker.GetTotalWastedTime();
        string label = $"♥ Life wasted waiting: {FormatTime(totalTime)}";

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(6, 6, 4, 4),
            normal = { textColor = new Color(0.65f, 0.65f, 0.65f) },
            hover = { textColor = new Color(0.85f, 0.85f, 0.85f) }
        };

        Vector2 contentSize = labelStyle.CalcSize(new GUIContent(label));
        float width = Mathf.Max(contentSize.x + 12f, 200f);
        float height = contentSize.y + 8f;

        Vector2 position = GetDisplayPosition(sceneView.position, width, height);
        dragRect = new Rect(position.x, position.y, width, height);

        EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.Pan);

        Event e = Event.current;

        if (e.type == EventType.MouseDown && dragRect.Contains(e.mousePosition))
        {
            isDragging = true;
            hasDragged = false;
            initialMousePos = e.mousePosition;
            dragOffset = e.mousePosition - dragRect.position;
            e.Use();
        }
        else if (e.type == EventType.MouseUp)
        {
            if (isDragging)
            {
                if (!hasDragged && dragRect.Contains(e.mousePosition))
                    TimeWasterDetailWindow.ShowWindow();

                e.Use();
            }

            isDragging = false;
            hasDragged = false;
        }
        else if (isDragging && e.type == EventType.MouseDrag)
        {
            if (Vector2.Distance(initialMousePos, e.mousePosition) > DragThreshold)
                hasDragged = true;

            Vector2 newPos = e.mousePosition - dragOffset;

            // Clamp within Scene view
            newPos.x = Mathf.Clamp(newPos.x, 0, sceneView.position.width - width);
            newPos.y = Mathf.Clamp(newPos.y, 0, sceneView.position.height - height);

            SaveOffset(sceneView.position, newPos, width, height);
            e.Use();
        }

        GUI.Label(dragRect, label, labelStyle);

        Handles.EndGUI();
    }

    private static Vector2 GetDisplayPosition(Rect sceneViewRect, float width, float height)
    {
        float sceneWidth = sceneViewRect.width;
        float sceneHeight = sceneViewRect.height;

        bool isInitialized = EditorPrefs.GetBool(EditorPrefsKeyInitialized, false);
        if (!isInitialized)
        {
            float defaultOffsetX = 10f;
            float defaultOffsetY = 32f;

            EditorPrefs.SetFloat(EditorPrefsKeyOffsetX, defaultOffsetX);
            EditorPrefs.SetFloat(EditorPrefsKeyOffsetY, defaultOffsetY);
            EditorPrefs.SetBool(EditorPrefsKeyInitialized, true);
        }

        float offsetX = EditorPrefs.GetFloat(EditorPrefsKeyOffsetX, 10f);
        float offsetY = EditorPrefs.GetFloat(EditorPrefsKeyOffsetY, 32f);

        // Bottom-left: X is offset from left, Y is offset from bottom
        float x = offsetX;
        float y = sceneHeight - height - offsetY;

        return new Vector2(x, y);
    }

    private static void SaveOffset(Rect sceneViewRect, Vector2 position, float width, float height)
    {
        float offsetX = position.x;
        float offsetY = sceneViewRect.height - position.y - height;

        EditorPrefs.SetFloat(EditorPrefsKeyOffsetX, offsetX);
        EditorPrefs.SetFloat(EditorPrefsKeyOffsetY, offsetY);
    }

    private static string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:D2}:{secs:D2}";
    }
}
