using UnityEngine;
using UnityEngine.Events;
using Meta.XR;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;


public class DrawRoadWithTrigger : MonoBehaviour
{
    public Transform leftControllerAnchor;
    public LineRenderer lineRenderer;
    [SerializeField]
    public List<Vector3> points;

    public GameObject carPrefab;
    List<GameObject> cars;
    List<int> carIndices;

    [Header("Drawing Settings")]
    [SerializeField]
    [Tooltip("Delay in seconds between adding points when holding trigger")]
    private float drawingDelay = 0.05f; // 50ms default delay
    
    private float lastDrawTime = 0f;
    private float lastUndoTime = 0f;
    [Header("Drawing Settings")]
    [SerializeField]
    [Tooltip("Delay in seconds between removing points when holding Y")]
    private float removeDelay = 0.1f;

    private float pointDist = 0.05f;

    float carSpeed = 0.65f;



    void Start()
    {
        points = new List<Vector3>();
        cars = new List<GameObject>();
        carIndices = new List<int>();
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && ShouldDrawPoint())
        {
            if (Time.time >= lastDrawTime + drawingDelay)
            {
                UpdateLineRenderer();
                lastDrawTime = Time.time;
            }
        }
        else if (OVRInput.Get(OVRInput.RawButton.Y))
        {
            // Remove/Undo last road point
            if (Time.time >= lastUndoTime + removeDelay)
            {
                if (points.Count == 0) return;

                points.RemoveAt(points.Count - 1);
                lineRenderer.positionCount = points.Count;
                lineRenderer.SetPositions(points.ToArray());
                lastUndoTime = Time.time;
            }
        }
        else
        {
            // Move cars
            MoveCars();
        }
    }

    bool ShouldDrawPoint()
    {
        if (points.Count == 0) return true;
        // Last point far enough from current position
        return Vector3.Distance(leftControllerAnchor.position, points[points.Count - 1]) >= pointDist;
    }

    void MoveCars()
    {   
        // Update list storage of cars + respective indices
        if (cars.Count < points.Count / 20)
        {
            GameObject new_car = Instantiate(carPrefab);
            cars.Add(new_car);
            
            // int randomIdx = Random.Range(0, points.Count);
            carIndices.Add(0);
        }
        else if (cars.Count > points.Count / 20)
        {
            if (carIndices.Count > 0)
            {
                carIndices.RemoveAt(carIndices.Count - 1);
            }
            if (cars.Count > 0)
            {
                GameObject removed = cars[cars.Count - 1];
                cars.RemoveAt(cars.Count - 1);

                Destroy(removed);
            }
        }

        for (int i = 0; i < carIndices.Count; i++)
        {
            if (carIndices[i] >= points.Count)
            {
                carIndices[i] = 1;
                if (points.Count > 0)
                {
                    cars[i].transform.position = points[0];
                }
                continue;
            }

            Vector3 targetPosition = points[carIndices[i]];

            cars[i].transform.position = Vector3.MoveTowards(cars[i].transform.position, targetPosition, carSpeed * Time.deltaTime);
            cars[i].transform.LookAt(targetPosition);

            if (cars[i].transform.position == targetPosition)
            {
                carIndices[i]++;
            }
        }
        
    }

    void UpdateLineRenderer()
    {
        points.Add(leftControllerAnchor.position);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    public bool PointsValid()
    {   
        return (points != null) && (points.Count >= 2); // Need enough points for model
    }

    
}
