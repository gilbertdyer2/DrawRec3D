using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Test script: finds JSON drawing files in a folder (e.g. Assets/ObjectRecognizer/RuntimeDrawings_JSON),
/// loads them via LoadDrawingFromFile, and displays the current one using DrawPoints3D.
/// Use Next/Previous to cycle through drawings.
/// </summary>
public class TestLoadDrawingViewer : MonoBehaviour
{
    [Header("Paths")]
    [Tooltip("Used only when no DrawingFileManagerHolder in scene. Folder under Application.dataPath to search for .json files.")]
    public string pathToJSONFolder = "DrawRec3D/SampleDrawings";

    [Header("Display")]
    [Tooltip("Assign the DrawPoints3D component that should show the loaded points.")]
    public DrawPoints3D drawPoints3D;

    [Header("Optional: Cycle Drawings With L/R Arrow Keys")]
    [Tooltip("Right Arrow = next, Left Arrow = previous drawing.")]
    [SerializeField] private bool useKeyboardCycle = true;

    [Header("Size (float, updates after a new drawing is loaded)")]
    [Tooltip("Scales the drawing uniformly to match the furthest point away from the origin to the radius.")]
    public float radius;

    private readonly List<string> _jsonFilePaths = new List<string>();
    private int _currentIndex = -1;

    private void Start()
    {
        RefreshFileList();
        if (_jsonFilePaths.Count > 0)
        {
            _currentIndex = 0;
            LoadAndDisplayCurrent();
        }
        else
        {
            Debug.LogWarning($"TestLoadDrawingViewer: No .json files found in {GetFullJsonFolderPath()}. Add JSONs from SaveDrawingToFile to test.");
        }
    }

    private void Update()
    {
        if (_jsonFilePaths.Count == 0 || !useKeyboardCycle) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            _currentIndex = (_currentIndex + 1) % _jsonFilePaths.Count;
            LoadAndDisplayCurrent();
        }
        else if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _jsonFilePaths.Count - 1;
            LoadAndDisplayCurrent();
        }
    }

    /// <summary>
    /// Rescans the JSON folder and repopulates the file list. Call to pick up new files at runtime.
    /// Uses DrawingFileManagerHolder in scene if present (same path as app: Downloads on Quest, etc.); otherwise falls back to folder under Application.dataPath.
    /// </summary>
    public void RefreshFileList()
    {
        _jsonFilePaths.Clear();

        var holder = FindObjectOfType<DrawingFileManagerHolder>();
        if (holder != null)
        {
            holder.Manager.Refresh();
            var byFolder = holder.Manager.FilePathsByFolder;
            foreach (var kv in byFolder)
            {
                foreach (string path in kv.Value)
                    _jsonFilePaths.Add(path);
            }
            if (_jsonFilePaths.Count > 0)
                Debug.Log($"TestLoadDrawingViewer: Found {_jsonFilePaths.Count} JSON file(s) via DrawingFileManagerHolder.");
            return;
        }
        else
            Debug.Log("TestLoadDrawingViewer: No DrawingFileManagerHolder in scene. Falling back to folder under Application.dataPath.");
            

        string folder = GetFullJsonFolderPath();
        if (!Directory.Exists(folder))
        {
            Debug.LogWarning($"TestLoadDrawingViewer: No DrawingFileManagerHolder in scene and folder does not exist: {folder}");
            return;
        }

        string[] files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string f in files)
            _jsonFilePaths.Add(f);

        if (_jsonFilePaths.Count > 0)
            Debug.Log($"TestLoadDrawingViewer: Found {_jsonFilePaths.Count} JSON file(s) in {folder}");
    }

    /// <summary>
    /// Loads the drawing at _currentIndex and assigns its points to DrawPoints3D.
    /// </summary>
    private void LoadAndDisplayCurrent()
    {
        if (drawPoints3D == null)
        {
            Debug.LogError("TestLoadDrawingViewer: DrawPoints3D reference is not set.");
            return;
        }

        if (_currentIndex < 0 || _currentIndex >= _jsonFilePaths.Count)
            return;

        string path = _jsonFilePaths[_currentIndex];
        if (!LoadDrawingFromFile.LoadDrawing(path, out List<Vector3> points))
        {
            Debug.LogError($"TestLoadDrawingViewer: Failed to load {path}");
            return;
        }

        // drawPoints3D.points = points;
        drawPoints3D.points = GesturePreprocessing.SetFirstAsOrigin(points);
        NormalizeToCube(drawPoints3D.points, radius);
        Debug.Log($"TestLoadDrawingViewer: Displaying {_jsonFilePaths.Count} file(s), current [{_currentIndex + 1}/{_jsonFilePaths.Count}] {Path.GetFileName(path)} ({points.Count} points)");
    }

    private string GetFullJsonFolderPath()
    {
        return Path.Combine(Application.dataPath, pathToJSONFolder);
    }

    
    /// <summary>
    /// Normalizes the list of points in-place so they fit inside a cube of the given size,
    /// centered at the origin. Uses uniform scaling so the shape is preserved with no distortion.
    /// </summary>
    /// <param name="points">Points to normalize (modified in place).</param>
    /// <param name="cubeSize">Side length of the cube (default 10). The longest axis of the bounding box is scaled to this size.</param>
    public static void NormalizeToCube(List<Vector3> points, float cubeSize = 4f)
    {
        if (points == null || points.Count == 0 || cubeSize <= 0f) return;

        Vector3 min = points[0], max = points[0];
        for (int i = 1; i < points.Count; i++)
        {
            Vector3 p = points[i];
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        Vector3 center = (min + max) * 0.5f;
        Vector3 extent = max - min;
        float maxExtent = Mathf.Max(extent.x, extent.y, extent.z);
        float scale = maxExtent > 0.0001f ? cubeSize / maxExtent : 1f;

        for (int i = 0; i < points.Count; i++)
            points[i] = (points[i] - center) * scale;
    }
}
