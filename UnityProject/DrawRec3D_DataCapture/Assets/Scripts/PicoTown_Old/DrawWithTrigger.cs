using UnityEngine;
using UnityEngine.Events;
using Meta.XR;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;


public class DrawWithTrigger : MonoBehaviour
{
    public Transform rightControllerAnchor;
    public LineRenderer lineRenderer;
    [SerializeField]
    public List<Vector3> points;

    [Header("Drawing Settings")]
    [SerializeField]
    [Tooltip("Delay in seconds between adding points when holding trigger")]
    private float drawingDelay = 0.01f; // 10ms default delay
    
    private float lastDrawTime = 0f;
    bool cleared = false;
    // public UnityEvent OnConfirmDrawing;
    // public UnityEvent OnInvalidDrawing;
    public InstantPlacementController placementController;

    private void Update()
    {
        // Draw with trigger
        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            
            if (cleared)
            {
                points = new List<Vector3>();
                ShowDrawing();
                cleared = false;
                UpdateLineRenderer();
            }
            else if (Time.time >= lastDrawTime + drawingDelay)
            {
                UpdateLineRenderer();
                lastDrawTime = Time.time;
                
            }
        }
        else if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            if (cleared) return;
            Debug.Log("Pressed X");
            // Clear drawing
            points = new List<Vector3>();
            UpdateLineRenderer();

            HideDrawing();
            cleared = true;
        }
        else if (OVRInput.GetDown(OVRInput.RawButton.A))
        {   
            if (cleared) return;
            
            Debug.Log($"Pressed A, curPoints: {points.Count}");
            
            if (PointsValid())
            {
                bool status = placementController.TryPlace(points);

                if (status == true)
                {
                    HideDrawing();
                    cleared = true;
                }
            }

            
        }
    }

    void HideDrawing()
    {
        lineRenderer.enabled = false;
    }

    void ShowDrawing()
    {
        lineRenderer.enabled = true;
    }

    void UpdateLineRenderer()
    {
        if (!lineRenderer.enabled)
        {
            Debug.Log("building drawing lineRenderer not enabled!");
        }
        points.Add(rightControllerAnchor.position);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }


    public bool PointsValid()
    {   
        // if (points.Count < 128)
        // {
        //     OnInvalidDrawing?.Invoke(); // msg 
        // }
        return (points != null) && (points.Count >= 64); // Need enough points for model, 64 not enough but 128 is good
    }


}
