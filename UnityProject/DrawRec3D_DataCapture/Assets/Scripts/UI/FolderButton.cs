using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FolderButton : MonoBehaviour
{
    public string filepath = "NONE";
    public TextMeshProUGUI displayText;
    public bool isDirectory = true;
    
    // Reference to folder contents list, used only for directories
    public FolderContentsList folderContentsList;

    // Reference to drawing display, used only for json drawing files
    public DrawingJSONViewer drawingJSONViewer;

    // Reference to drawing file manager
    public DrawingFileManager drawingFileManager;


    public void Initialize(string text, 
                           string filepath, 
                           bool isDirectory, 
                           FolderContentsList folderContentsList,
                           DrawingFileManager drawingFileManager
                           )
    {
        SetText(text);
        SetFilepath(filepath);
        this.isDirectory = isDirectory;
        this.folderContentsList = folderContentsList;
        this.drawingFileManager = drawingFileManager;
        FindDrawingJSONViewer();
    }


    public void FindDrawingJSONViewer()
    {
        drawingJSONViewer = FindObjectOfType<DrawingJSONViewer>();
    }

    public void SetAsDrawing()
    {
        // Precautionary: Try to find drawingJSONViewer in the scene once if not set
        if (drawingJSONViewer == null)
        {
            FindDrawingJSONViewer();
            if (drawingJSONViewer == null)
            {
                // Debug.Log("FolderButton: DrawingJSONViewer not found");
                return;
            }
        }
        
        drawingJSONViewer.DisplayDrawing(filepath);
    }

    public void SetText(string text)
    {
        displayText.text = text;
    }

    public void SetFilepath(string path)
    {
        filepath = path;
    }

    public void FolderButtonToggled(bool toggledOn)
    {
        if (toggledOn)
        {
            if (isDirectory)
            {
                if (drawingFileManager == null)
                {
                    Debug.LogError("FolderButton: DrawingFileManager not set");
                    return;
                }
                Debug.Log("FolderButton: Setting drawing paths for folder: " + filepath);
                // Set the folder contents list UI to the current folder
                folderContentsList.SetDrawingPaths(filepath, drawingFileManager.FilePathsByFolder[filepath]);
            }
            else // JSON drawing file
            {
                // Set the drawing JSON viewer to this button's drawing
                SetAsDrawing();
            }
        }
        else if (!toggledOn)
        {
            
        }
        
    }
}
