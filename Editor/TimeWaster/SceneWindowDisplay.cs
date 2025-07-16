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
    private const string EditorPrefsKeyX = "TimeWasterBox_PosX";
    private const string EditorPrefsKeyY = "TimeWasterBox_PosY";
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
        string timeString = FormatTime(totalTime);
        string label = $"♥ Life wasted waiting: {timeString}";

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(6, 6, 4, 4),
            normal = { textColor = new Color(0.65f, 0.65f, 0.65f) },
            hover = { textColor = new Color(0.85f, 0.85f, 0.85f) }
        };

        Vector2 contentSize = labelStyle.CalcSize(new GUIContent(label));
        float width = Mathf.Max(contentSize.x + 12f, 200f);
        float height = contentSize.y + 8f;

        Vector2 position = GetSavedPosition(sceneView.position, width, height);
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
                {
                    TimeWasterDetailWindow.ShowWindow();
                }
                e.Use();
            }
            isDragging = false;
            hasDragged = false;
        }
        else if (isDragging && e.type == EventType.MouseDrag)
        {
            float dragDistance = Vector2.Distance(initialMousePos, e.mousePosition);
            if (dragDistance > DragThreshold)
            {
                hasDragged = true;
            }

            dragRect.position = e.mousePosition - dragOffset;

            dragRect.x = Mathf.Clamp(dragRect.x, 0, sceneView.position.width - dragRect.width);
            dragRect.y = Mathf.Clamp(dragRect.y, 0, sceneView.position.height - dragRect.height);

            SavePosition(dragRect.position);
            e.Use();
        }

        GUI.Label(dragRect, label, labelStyle);
        Handles.EndGUI();
    }

    private static Vector2 GetSavedPosition(Rect sceneViewRect, float width, float height)
    {
        float defaultX = 10f;
        float defaultY = sceneViewRect.height - height - 32f;

        bool isInitialized = EditorPrefs.GetBool(EditorPrefsKeyInitialized, false);

        if (!isInitialized)
        {
            EditorPrefs.SetBool(EditorPrefsKeyInitialized, true);
            EditorPrefs.SetFloat(EditorPrefsKeyX, defaultX);
            EditorPrefs.SetFloat(EditorPrefsKeyY, defaultY);
            return new Vector2(defaultX, defaultY);
        }

        float savedX = EditorPrefs.GetFloat(EditorPrefsKeyX, defaultX);
        float savedY = EditorPrefs.GetFloat(EditorPrefsKeyY, defaultY);

        float x = Mathf.Clamp(savedX, 0, sceneViewRect.width - width);
        float y = Mathf.Clamp(savedY, 0, sceneViewRect.height - height);

        return new Vector2(x, y);
    }

    private static void SavePosition(Vector2 position)
    {
        EditorPrefs.SetFloat(EditorPrefsKeyX, position.x);
        EditorPrefs.SetFloat(EditorPrefsKeyY, position.y);
    }

    private static string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:D2}:{secs:D2}";
    }
}