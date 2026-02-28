using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Single instance in the scene. Clears its children and instantiates one prefab per drawing path; contents are updated at runtime via SetDrawingPaths (e.g. when the user selects a folder).
/// </summary>
public class FolderContentsList : MonoBehaviour
{
    [Tooltip("Prefab to instantiate for each drawing JSON (e.g. a list row or button).")]
    public GameObject itemPrefab;


    private DrawingFileManager drawingFileManager;
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
    /// Clears all current children, then instantiates one item prefab per path and initializes via IDrawingFileItem.
    /// </summary>
    /// <param name="folderName">Folder name for display (passed to each item).</param>
    /// <param name="drawingPaths">Full paths to drawing JSON files in this folder.</param>
    public void SetDrawingPaths(string folderName, IList<string> drawingPaths)
    {
        Debug.Log("FolderContentsList: SetDrawingPaths called with folderName: " + folderName + " and drawingPaths: " + drawingPaths.Count);
        ClearChildren();
        if (itemPrefab == null || drawingPaths == null) return;

        foreach (string filePath in drawingPaths)
        {
            string displayName = Path.GetFileNameWithoutExtension(filePath);
            GameObject go = Instantiate(itemPrefab, transform);
            go.name = displayName;

            // var item = go.GetComponent<IDrawingFileItem>();
            // if (item != null)
            //     item.SetDrawingFile(folderName, filePath, displayName);

            var item = go.GetComponent<FolderButton>();
            if (item != null)
            {

                item.Initialize(displayName, filePath, false, this, GetOrFindManager());
                // item.SetText(displayName);
                // item.SetFilepath(folderName + "/" + filePath);
                // item.isDirectory = false;
            }
        }
    }

    /// <summary>
    /// Removes all children under this transform.
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
