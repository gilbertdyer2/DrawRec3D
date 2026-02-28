using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implement on a prefab used as a row/item in the drawing file list. Called once per instantiated item.
/// </summary>
public interface IDrawingFileItem
{
    void SetDrawingFile(string folderName, string filePath, string displayName);
}

/// <summary>
/// Implement on a folder row prefab. Called once per folder so the row can update the shared FolderContentsList when selected.
/// </summary>
public interface IFolderRowItem
{
    void SetFolder(string folderName, IList<string> paths, FolderContentsList folderContentList);
}

/// <summary>
/// Initializes prefab instances under this GameObject for each subfolder and JSON in a DrawingFileManager (for UI display).
/// </summary>
public class DrawingFileListUI : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Prefab to instantiate for each folder (one row per folder). Implement IFolderRowItem so the row can update folderContentList when selected.")]
    public GameObject itemPrefab;

    [Header("Drawing List UI (single instance)")]
    [Tooltip("Single FolderContentsList whose children are updated at runtime when a folder is selected. Assign in the scene.")]
    public FolderContentsList folderContentList;

    [Header("Source")]
    [Tooltip("Drawing file manager to read from. If unset, one is located in the scene (DrawingFileManagerHolder).")]
    public DrawingFileManager drawingFileManager;

    
    /// <summary>
    /// Returns the assigned drawing file manager, or the one from DrawingFileManagerHolder in the scene if not assigned.
    /// </summary>
    private DrawingFileManager GetOrFindManager()
    {
        if (drawingFileManager != null) return drawingFileManager;
        var holder = FindObjectOfType<DrawingFileManagerHolder>();
        if (holder != null) return holder.Manager;
        return null;
    }

    /// <summary>
    /// Builds the list: clears existing children, then instantiates one prefab per folder and populates each row's FolderContentsList.
    /// Uses drawingFileManager if assigned; otherwise finds DrawingFileManagerHolder in the scene.
    /// </summary>
    public void InitializeFromDrawingFileManager()
    {
        DrawingFileManager manager = GetOrFindManager();
        if (manager == null)
        {
            Debug.LogWarning("DrawingFileListUI: No DrawingFileManager assigned and no DrawingFileManagerHolder in scene.");
            return;
        }
        Debug.Log("DrawingFileListUI: Found DrawingFileManager in scene.");
        InitializeFromDrawingFileManager(manager);
    }

    /// <summary>
    /// Builds the list: clears existing children, then instantiates one prefab per folder and passes the shared folderContentList to each row via IFolderRowItem.
    /// The single folderContentList is updated at runtime when a folder row is selected.
    /// </summary>
    /// <param name="manager">Source of folder -> JSON paths.</param>
    public void InitializeFromDrawingFileManager(DrawingFileManager manager)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("DrawingFileListUI: No item prefab assigned.");
            return;
        }
        if (manager == null) return;

        ClearChildren();

        var byFolder = manager.FilePathsByFolder;
        foreach (var kv in byFolder)
        {
            string folderName = kv.Key;
            List<string> paths = kv.Value;

            GameObject row = Instantiate(itemPrefab, transform);
            row.name = folderName;

            // var folderRow = row.GetComponent<IFolderRowItem>();
            // if (folderRow != null && folderContentList != null)
            //     folderRow.SetFolder(folderName, paths, folderContentList);

            var folderButtonUI = row.GetComponent<FolderButton>();
            if (folderButtonUI != null)
            {
                folderButtonUI.Initialize(folderName, folderName, true, folderContentList, manager);
                folderButtonUI.drawingFileManager = manager;
            }
        }
    }

    /// <summary>
    /// Removes all children under this transform (e.g. before rebuilding the list).
    /// </summary>
    public void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
