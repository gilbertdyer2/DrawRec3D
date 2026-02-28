using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoints : MonoBehaviour
{
    [SerializeField]
    private List<Vector3> points;
    [SerializeField]
    private List<float> timestamps;
    private float startTime;
    public string currentDrawingName;

    public GameObject leftController;
    public GameObject rightController;

    private GameObject brush;
    [SerializeField]
    private bool drawing;
    [SerializeField]
    private LineRenderer lineRenderer;

    public float segmentLength;
    [SerializeField]
    private GameObject trackPathPrefab;

    [SerializeField]
    private SaveDrawingToFile saveDrawingToFile;
    bool currentDrawingSaved = false;

    void Start()
    {
        drawing = false;
    }

    void Update()
    {
        if (drawing)
        {
            drawSegment();
        }
    }

    public void SaveDrawingAsFile()
    {
        if (points.Count == 0)
        {
            Debug.Log("SavePoints: Tried to save empty drawing!");
            return;
        }
        saveDrawingToFile.SaveDrawing(currentDrawingName, points);
        currentDrawingSaved = true;
    }


    private void drawSegment()
    {
        Vector3 brushPos = brush.transform.position;

        if (points.Count == 0)
        {
            float curTime = Time.time;

            startTime = curTime;
            points.Add(brushPos);
            timestamps.Add(curTime - startTime); // 0.0

        }
        // Draw if past segment length
        else if (Vector3.Distance(points[points.Count - 1], brushPos) >= segmentLength)
        {
            float curTime = Time.time;

            points.Add(brushPos);
            timestamps.Add(curTime - startTime);
        }

        
        // Update visuals
        lineRenderer.positionCount = points.Count;
        if (points.Count > 0) lineRenderer.SetPositions(points.ToArray());
    }

    private void beginDraw()
    {
        Debug.Log("Beginning Draw");
        lineRenderer.positionCount = 0;;
        drawing = true;
        currentDrawingSaved = false;
        points = new List<Vector3>();
        timestamps = new List<float>();
    }

    private void endDraw()
    {
        Debug.Log("Ending Draw");
        brush = null;
        drawing = false;

        if (points.Count >= 2)
        {
        
        }
        
    }

    // Controller-specific method calls
    public void beginDrawLeft()
    {
        if (drawing) return;

        brush = leftController;
        beginDraw();
    }

    public void endDrawLeft()
    {
        if (!drawing || brush != leftController) return;
        
        endDraw();
    }
    
    public void beginDrawRight()
    {
        if (drawing) return;

        brush = rightController;
        beginDraw();
    }
    
    public void endDrawRight()
    {
        if (!drawing || brush != rightController) return;

        endDraw();
    }
}
