using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveDrawingToFile : MonoBehaviour
{
    [Header("Folder location for .json files of drawings saved to the headset")]
    public string rootFolderName = "drawingData";

    /// <summary>
    /// Saves a 3D drawing represented by a list of Vector3 points as a .json file
    /// Used to save drawings for data preprocessing in Python.
    /// </summary>
    /// <param name="drawingName">Name of the drawing (e.g. "cube", "pyramid", "house").</param>
    /// <param name="positions">List of points that make up the drawing.</param>
    /// <param name="subfolderName">Optional for organization, save drawings to a named subdirectory of the root folder instead</param>
    public void SaveDrawing(string drawingName, List<Vector3> positions, string subfolderPath = "")
    {
        
        Debug.Log($"SaveDrawingToFile: Attempting to save drawing ({drawingName})");
        if (positions == null || positions.Count == 0)
        {
            Debug.LogError("SaveDrawingToFile: Cannot save empty drawing.");
            return;
        }

        List<DrawingPoint> DrawingPoints = new List<DrawingPoint>(positions.Count);
        for (int i = 0; i < positions.Count; i++)
        {
            DrawingPoints.Add(new DrawingPoint(positions[i]));
        }

        // Create or get root and class folders
        string rootPath = Application.persistentDataPath + "/" + rootFolderName;
        if (!Directory.Exists(rootPath))
            Directory.CreateDirectory(rootPath);
        if (subfolderPath != "")
            rootPath = rootPath + "/" + subfolderPath;
        
        string classPath = rootPath + "/" + drawingName;
        if (!Directory.Exists(classPath))
            Directory.CreateDirectory(classPath);
        // Arbitrary unique name for the file
        string fileName = drawingName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".json";
        string filePath = classPath + "/" + fileName;
        
        // JSON utils require we convert data to serializable form 
        DrawingWrapper wrapper = new DrawingWrapper
        {
            drawingName = drawingName,
            size = positions.Count,
            points = new List<SerializablePoint>()
        };

        foreach (DrawingPoint p in DrawingPoints)
        {
            wrapper.points.Add(new SerializablePoint
            {
                x = p.position.x,
                y = p.position.y,
                z = p.position.z,
            });
        }

        string json = JsonUtility.ToJson(wrapper, prettyPrint: true);
        File.WriteAllText(filePath, json);

        Debug.Log($"SaveDrawingToFile: Saved drawing ({drawingName}) to: {filePath}");
    }

    // ----- Serializable helper classes ----- //
    [Serializable]
    private class DrawingWrapper
    {
        public string drawingName;
        public int size;
        public List<SerializablePoint> points;
    }

    [Serializable]
    private class SerializablePoint
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public struct DrawingPoint
    {
        public Vector3 position;

        public DrawingPoint(Vector3 pos)
        {
            position = pos;
        }
    }
}
