using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// Scene component that holds a DrawingFileManager. Place one in the scene so DrawingFileListUI (and others) can locate it.
/// Uses Application.persistentDataPath + rootPath on all platforms (no extra permissions; works on Quest/Android).
/// </summary>
public class DrawingFileManagerHolder : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("Subpath under Application.persistentDataPath (e.g. Drawings).")]
    public string rootPath = "Drawings";

    [Header("StreamingAssets sync")]
    [Tooltip("If set, replaces persistentDataPath/rootPath with StreamingAssets/Drawings at start (run sync then refresh).")]
    public bool syncFromStreamingAssetsOnStart = true;
    [Tooltip("Subpath under StreamingAssets to copy from (default Drawings).")]
    public string streamingSourceSubpath = SyncStreamingDrawingsToPersistent.DefaultSourceSubpath;

    private DrawingFileManager _manager;

    /// <summary>Drawing file manager for this root path. Created in Awake.</summary>
    public DrawingFileManager Manager => _manager;

    [Header("Folder Display For DrawingFileListUI (Left Column)")]
    public DrawingFileListUI drawingFileListUI;

    private void Awake()
    {
        string root = GetRootPath();
        EnsureDirectoryExists(root);
        _manager = new DrawingFileManager(root);
    }

    private void Start()
    {
        if (syncFromStreamingAssetsOnStart)
            StartCoroutine(SyncThenRefresh());
        else
        {
            if (_manager != null)
                _manager.Refresh();
            if (drawingFileListUI != null)
                drawingFileListUI.InitializeFromDrawingFileManager();
        }
    }

    private IEnumerator SyncThenRefresh()
    {
        string destSubpath = string.IsNullOrEmpty(rootPath) ? SyncStreamingDrawingsToPersistent.DefaultDestSubpath : rootPath.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        yield return SyncStreamingDrawingsToPersistent.Sync(streamingSourceSubpath, destSubpath);
        if (_manager != null)
            _manager.Refresh();
        if (drawingFileListUI != null)
            drawingFileListUI.InitializeFromDrawingFileManager();
    }

    /// <summary>
    /// Creates the root directory and any missing parents if they do not exist.
    /// </summary>
    private static void EnsureDirectoryExists(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("DrawingFileManagerHolder: Could not create directory " + path + ": " + e.Message);
        }
    }

    /// <summary>
    /// Returns the root directory for the drawing files (persistentDataPath + rootPath).
    /// </summary>
    private string GetRootPath()
    {
        string sub = string.IsNullOrEmpty(rootPath) ? null : rootPath.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string root = string.IsNullOrEmpty(sub) ? Application.persistentDataPath : Path.Combine(Application.persistentDataPath, sub);
        Debug.Log("DrawingFileManagerHolder: Root: " + root);
        return root;
    }
}
