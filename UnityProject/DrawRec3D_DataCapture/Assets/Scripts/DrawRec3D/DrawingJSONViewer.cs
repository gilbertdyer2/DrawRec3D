using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Loads a JSON drawing from a specified file path and displays it using DrawPoints3D.
/// Does not manage file lists; call DisplayDrawing(path) to show a drawing by path.
/// </summary>
public class DrawingJSONViewer : MonoBehaviour
{
    [Header("Display")]
    [Tooltip("Assign the DrawPoints3D component that should show the loaded points.")]
    public DrawPoints3D drawPoints3D;

    [Header("Normalization")]
    [Tooltip("Side length of the cube used to normalize points (uniform scale, shape preserved).")]
    public float normalizeCubeSize = 10f;
    

    /// <summary>
    /// Loads the drawing at the given path and displays it. Uses the same pipeline as TestLoadDrawingViewer (LoadDrawingFromFile, SetFirstAsOrigin, NormalizeToCube).
    /// </summary>
    /// <param name="path">Full path to the .json drawing file.</param>
    /// <returns>True if the drawing was loaded and displayed; false otherwise.</returns>
    public bool DisplayDrawing(string path)
    {
        if (drawPoints3D == null)
        {
            Debug.LogError("DrawingJSONViewer: DrawPoints3D reference is not set.");
            return false;
        }

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("DrawingJSONViewer: DisplayDrawing called with null or empty path.");
            return false;
        }

        if (!LoadDrawingFromFile.LoadDrawing(path, out List<Vector3> points))
        {
            Debug.LogError($"DrawingJSONViewer: Failed to load {path}");
            return false;
        }

        drawPoints3D.points = GesturePreprocessing.SetFirstAsOrigin(points);
        NormalizeToCube(drawPoints3D.points, normalizeCubeSize);
        Debug.Log($"DrawingJSONViewer: Displaying {Path.GetFileName(path)} ({drawPoints3D.points.Count} points)");

        return true;
    }

    /// <summary>
    /// Clears the displayed drawing by removing all points from DrawPoints3D.
    /// </summary>
    public void ClearDrawing()
    {
        if (drawPoints3D == null)
        {
            Debug.LogError("DrawingJSONViewer: DrawPoints3D reference is not set.");
            return;
        }
        drawPoints3D.points.Clear();
    }

    /// <summary>
    /// Normalizes the list of points in-place so they fit inside a cube of the given size,
    /// centered at the origin. Uses uniform scaling so the shape is preserved.
    /// </summary>
    public static void NormalizeToCube(List<Vector3> points, float cubeSize = 10f)
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
