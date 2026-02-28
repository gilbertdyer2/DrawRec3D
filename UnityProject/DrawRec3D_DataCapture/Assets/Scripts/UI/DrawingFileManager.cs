using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DrawingFileManager
{
    string FOLDER_ROOT;
    private Dictionary<string, List<string>> filePathsByFolder = new Dictionary<string, List<string>>();

    public DrawingFileManager(string root)
    {
        FOLDER_ROOT = string.IsNullOrEmpty(root) ? root : root.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        // Refresh() is called by Holder in Start().
    }

    /// <summary>Immediate subfolder name -> list of full paths to .json files in that folder.</summary>
    public IReadOnlyDictionary<string, List<string>> FilePathsByFolder => filePathsByFolder;

    public void Refresh()
    {
        Debug.Log("DrawingFileManager: Refreshing! FOLDER_ROOT: " + FOLDER_ROOT);
        filePathsByFolder.Clear();
        if (string.IsNullOrEmpty(FOLDER_ROOT) || !Directory.Exists(FOLDER_ROOT))
        {
            Debug.Log("DrawingFileManager: FOLDER_ROOT is null or does not exist! FOLDER_ROOT: " + FOLDER_ROOT);
            return;
        }
        Debug.Log("DrawingFileManager: FOLDER_ROOT exists! FOLDER_ROOT: " + FOLDER_ROOT);
        string[] dirs = Directory.GetDirectories(FOLDER_ROOT);
        Debug.Log("DrawingFileManager: GetDirectories returned " + dirs.Length + " subfolder(s).");
        foreach (string dir in dirs)
        {
            string folderName = Path.GetFileName(dir);
            string[] jsonFiles = GetJsonFilesCaseInsensitive(dir);
            filePathsByFolder[folderName] = new List<string>(jsonFiles);
            Debug.Log("DrawingFileManager: Found " + jsonFiles.Length + " JSON files in folder: " + folderName + " (path: " + dir + ")");
        }
    }

    /// <summary>
    /// Gets all .json files in the directory (matches .json and .JSON on case-sensitive filesystems e.g. Android).
    /// </summary>
    private static string[] GetJsonFilesCaseInsensitive(string directory)
    {
        string[] all = null;
        try
        {
            all = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
            Debug.Log("DrawingFileManager: GetFiles(\"" + directory + "\", \"*.*\") returned " + (all?.Length ?? 0) + " file(s).");
            if (all != null && all.Length > 0)
            {
                int show = Mathf.Min(3, all.Length);
                for (int i = 0; i < show; i++)
                    Debug.Log("DrawingFileManager:   file[" + i + "] = " + Path.GetFileName(all[i]));
                if (all.Length > show)
                    Debug.Log("DrawingFileManager:   ... and " + (all.Length - show) + " more.");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("DrawingFileManager: GetFiles threw for \"" + directory + "\": " + e.GetType().Name + " - " + e.Message);
            return new string[0];
        }
        if (all == null) return new string[0];
        var list = new List<string>();
        foreach (string path in all)
        {
            string ext = Path.GetExtension(path);
            if (string.Equals(ext, ".json", System.StringComparison.OrdinalIgnoreCase))
                list.Add(path);
        }
        if (list.Count != (all?.Length ?? 0))
            Debug.Log("DrawingFileManager: After .json filter: " + list.Count + " of " + (all?.Length ?? 0) + " files.");
        return list.ToArray();
    }
}
