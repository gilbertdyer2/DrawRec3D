using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Replaces the contents of persistentDataPath/[destSubpath] with StreamingAssets/[sourceSubpath].
/// Call Sync() (coroutine) before using DrawingFileManager so the app uses the bundled drawings.
/// On Android, uses AssetManager to list all files in StreamingAssets/sourceSubpath and copies each via UnityWebRequest.
/// </summary>
public static class SyncStreamingDrawingsToPersistent
{
    public const string DefaultSourceSubpath = "Drawings";
    public const string DefaultDestSubpath = "Drawings";

    /// <summary>
    /// Replaces persistentDataPath/destSubpath with contents of StreamingAssets/sourceSubpath. Run as a coroutine.
    /// </summary>
    public static IEnumerator Sync(string sourceSubpath = DefaultSourceSubpath, string destSubpath = DefaultDestSubpath)
    {
        string sourceRoot = Path.Combine(Application.streamingAssetsPath, sourceSubpath.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        string destRoot = Path.Combine(Application.persistentDataPath, destSubpath.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

#if UNITY_ANDROID && !UNITY_EDITOR
        yield return SyncAndroid(sourceRoot, destRoot, sourceSubpath);
#else
        SyncStandalone(sourceRoot, destRoot);
        yield break;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static IEnumerator SyncAndroid(string sourceRoot, string destRoot, string sourceSubpath)
    {
        List<string> relativePaths = ListStreamingAssetsRecursive(sourceSubpath);
        if (relativePaths == null || relativePaths.Count == 0)
        {
            Debug.Log("SyncStreamingDrawingsToPersistent: No files found in StreamingAssets/" + sourceSubpath);
            yield break;
        }
        ClearDestination(destRoot);
        foreach (string rel in relativePaths)
        {
            string fileUrl = sourceRoot + "/" + rel.Replace('\\', '/');
            string destPath = Path.Combine(destRoot, rel);
            yield return CopyOneFileAndroid(fileUrl, destPath);
        }
        Debug.Log("SyncStreamingDrawingsToPersistent: Copied " + relativePaths.Count + " file(s) to " + destRoot);
    }

    /// <summary>
    /// Lists all file paths under StreamingAssets/assetPath using Android AssetManager (no manifest needed).
    /// </summary>
    private static List<string> ListStreamingAssetsRecursive(string assetPath)
    {
        var result = new List<string>();
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var assets = activity.Call<AndroidJavaObject>("getAssets"))
            {
                CollectAssetPaths(assets, assetPath, "", result);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("SyncStreamingDrawingsToPersistent: ListStreamingAssetsRecursive failed: " + e.Message);
        }
        return result;
    }

    private static void CollectAssetPaths(AndroidJavaObject assets, string assetPath, string prefix, List<string> result)
    {
        string[] names = assets.Call<string[]>("list", assetPath);
        if (names == null || names.Length == 0) return;
        foreach (string name in names)
        {
            string relative = string.IsNullOrEmpty(prefix) ? name : prefix + "/" + name;
            string childPath = string.IsNullOrEmpty(assetPath) ? name : assetPath + "/" + name;
            string[] childList = assets.Call<string[]>("list", childPath);
            if (childList != null && childList.Length > 0)
                CollectAssetPaths(assets, childPath, relative, result);
            else
                result.Add(relative);
        }
    }

    private static IEnumerator CopyOneFileAndroid(string fileUrl, string destPath)
    {
        using (var req = UnityWebRequest.Get(fileUrl))
        {
            req.SendWebRequest();
            while (!req.isDone)
                yield return null;

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("SyncStreamingDrawingsToPersistent: Failed to load " + fileUrl + ": " + req.error);
                yield break;
            }

            byte[] data = req.downloadHandler?.data;
            if (data == null || data.Length == 0) yield break;

            try
            {
                string dir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(destPath, data);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("SyncStreamingDrawingsToPersistent: Failed to write " + destPath + ": " + e.Message);
            }
        }
    }
#endif

    private static void SyncStandalone(string sourceRoot, string destRoot)
    {
        if (!Directory.Exists(sourceRoot))
        {
            Debug.LogWarning("SyncStreamingDrawingsToPersistent: Source not found: " + sourceRoot);
            return;
        }
        ClearDestination(destRoot);
        int count = CopyDirectoryRecursive(sourceRoot, destRoot);
        Debug.Log("SyncStreamingDrawingsToPersistent: Copied " + count + " file(s) to " + destRoot);
    }

    private static void ClearDestination(string destRoot)
    {
        try
        {
            if (Directory.Exists(destRoot))
                Directory.Delete(destRoot, true);
            Directory.CreateDirectory(destRoot);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("SyncStreamingDrawingsToPersistent: Could not clear destination " + destRoot + ": " + e.Message);
        }
    }

    private static int CopyDirectoryRecursive(string sourceDir, string destDir)
    {
        int count = 0;
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            try { File.Copy(file, destFile, true); count++; }
            catch (System.Exception e) { Debug.LogWarning("SyncStreamingDrawingsToPersistent: Copy failed " + file + ": " + e.Message); }
        }
        foreach (string sub in Directory.GetDirectories(sourceDir))
        {
            string destSub = Path.Combine(destDir, Path.GetFileName(sub));
            if (!Directory.Exists(destSub)) Directory.CreateDirectory(destSub);
            count += CopyDirectoryRecursive(sub, destSub);
        }
        return count;
    }
}
