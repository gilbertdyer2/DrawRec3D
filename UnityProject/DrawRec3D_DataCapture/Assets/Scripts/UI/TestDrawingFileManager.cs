using System.IO;
using UnityEngine;

/// <summary>
/// Play-mode test for DrawingFileManager: logs folders and JSON file counts from the root path.
/// </summary>
public class TestDrawingFileManager : MonoBehaviour
{
    [Header("Path (fallback)")]
    [Tooltip("Used only if no DrawingFileManagerHolder is in the scene. Root folder under Application.dataPath.")]
    public string rootPathUnderAssets = "Drawings";

    [Header("Optional: Re-run Refresh with key")]
    [Tooltip("Press this key to call Refresh() again and re-log (Input System).")]
    [SerializeField] private bool refreshWithKey = true;

    private DrawingFileManager _manager;

    private void Start()
    {
        var holder = FindObjectOfType<DrawingFileManagerHolder>();
        if (holder != null)
        {
            _manager = holder.Manager;
            Debug.Log("TestDrawingFileManager: Using manager from DrawingFileManagerHolder in scene.");
        }
        else
        {
            string root = Path.Combine(Application.dataPath, rootPathUnderAssets);
            _manager = new DrawingFileManager(root);
            Debug.Log("TestDrawingFileManager: No holder in scene; created manager from rootPathUnderAssets.");
        }
        LogResults();
    }

    private void Update()
    {
        if (refreshWithKey && UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            _manager.Refresh();
            Debug.Log("TestDrawingFileManager: Refresh() called.");
            LogResults();
        }
    }

    private void LogResults()
    {
        var byFolder = _manager.FilePathsByFolder;
        if (byFolder.Count == 0)
        {
            Debug.Log($"TestDrawingFileManager: No subfolders (or root missing). Root = {Path.Combine(Application.dataPath, rootPathUnderAssets)}");
            return;
        }

        Debug.Log($"TestDrawingFileManager: Found {byFolder.Count} folder(s):");
        foreach (var kv in byFolder)
        {
            Debug.Log($"  [{kv.Key}] {kv.Value.Count} .json file(s)");
            int maxShow = 3;
            for (int i = 0; i < kv.Value.Count && i < maxShow; i++)
                Debug.Log($"    - {Path.GetFileName(kv.Value[i])}");
            if (kv.Value.Count > maxShow)
                Debug.Log($"    ... and {kv.Value.Count - maxShow} more");
        }
    }
}
