using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Editor/testing helper: loads a single JSON drawing from a path under Assets and asks DrawingRecognizer for a match.
/// Use in editor only (path is under Application.dataPath).
/// </summary>
public class TestDrawingMatchComparer : MonoBehaviour
{
    [Tooltip("Path relative to Assets folder, e.g. ObjectRecognizer/RuntimeDrawings_JSON/test.json")]
    public string pathUnderAssets = "ObjectRecognizer/RuntimeDrawings_JSON/test.json";

    [Tooltip("If unset, finds DrawingRecognizer in scene at runtime.")]
    public DrawingRecognizer drawingRecognizer;

    [Tooltip("Key to run load + match in editor (only in Play mode). Uses Input System. T key is used to avoid invalid Key indexer issues.")]
    public bool useTestKey = true;

    private void Update()
    {
        if (!useTestKey) return;
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.tKey.wasPressedThisFrame)
        {
            Debug.Log("TestDrawingMatchComparer: T key pressed, loading and matching drawing.");
            TestLoadAndMatch();
        }
    }

    /// <summary>
    /// Loads the JSON at pathUnderAssets (relative to Assets) and gets match from DrawingRecognizer. Call from context menu or when testKey is pressed.
    /// </summary>
    [ContextMenu("Test Load And Match")]
    public void TestLoadAndMatch()
    {
#if !UNITY_EDITOR
        Debug.LogWarning("TestDrawingMatchComparer: Intended for editor testing only.");
        return;
#else
        string fullPath = Path.Combine(Application.dataPath, pathUnderAssets);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"TestDrawingMatchComparer: File not found: {fullPath}");
            return;
        }

        if (!LoadDrawingFromFile.LoadDrawing(fullPath, out List<Vector3> points))
        {
            Debug.LogError("TestDrawingMatchComparer: Failed to load drawing.");
            return;
        }

        if (drawingRecognizer == null)
            drawingRecognizer = FindObjectOfType<DrawingRecognizer>();
        if (drawingRecognizer == null)
        {
            Debug.LogError("TestDrawingMatchComparer: No DrawingRecognizer in scene.");
            return;
        }

        string match = drawingRecognizer.GetMatch(points);
        Debug.Log($"[TestDrawingMatchComparer] Loaded {Path.GetFileName(fullPath)} ({points.Count} points) -> match: '{match}'");
#endif
    }
}
