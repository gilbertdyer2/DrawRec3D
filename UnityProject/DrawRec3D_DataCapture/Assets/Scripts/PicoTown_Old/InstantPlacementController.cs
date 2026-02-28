using UnityEngine;
using Meta.XR;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class InstantPlacementController : MonoBehaviour
{
    public Transform rightControllerAnchor;
    public GameObject prefabToPlace;
    public EnvironmentRaycastManager raycastManager;

    public ShapeDefinitions shapeDefinitions;


    // Adapted from https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-environment-raycast/
    public bool TryPlace(List<Vector3> points)
    {
        // if (!drawer.PointsValid())
        // {
        //     Debug.Log("Could not place building from drawing. Not enough points or points was null.");
        //     return;
        // }
        
        // Send a ray downwards at the center of the drawing, towards the mesh
        Vector3 rayStart = GetGroundCenter(points);
        Ray ray = new Ray(rayStart, Vector3.down);

        Debug.Log("Sending raycast, trying to place...");
        if (raycastManager.Raycast(ray, out var hit))
        {
            GameObject buildingToPlace = shapeDefinitions.GetMatch(points);

            var objectToPlace = Instantiate(buildingToPlace, hit.point, Quaternion.LookRotation(Vector3.zero, Vector3.up));
            Debug.Log("Placed building");

            // If no MRUK component is present in the scene, we add an OVRSpatialAnchor component
            // to the instantiated prefab to anchor it in the physical space and prevent drift.
            if (MRUK.Instance?.IsWorldLockActive != true)
            {
                objectToPlace.AddComponent<OVRSpatialAnchor>();
            }

            return true;
        } 
        else
        {
            Debug.Log("Raycast did not hit!");
        }

        return false;
    }

    // Gets the bounds the XZ plane of points and returns the center of the rectangular boundary
    //      - The returned y value is the max y value of the points
    public Vector3 GetGroundCenter(List<Vector3> points)
    {
        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minZ = float.PositiveInfinity;
        float maxZ = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;

        foreach (Vector3 p in points)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.z < minZ) minZ = p.z;
            if (p.z > maxZ) maxZ = p.z;
            if (p.y > maxY) maxY = p.y;
        }

        float centerX = (minX + maxX) * 0.5f;
        float centerZ = (minZ + maxZ) * 0.5f;

        return new Vector3(centerX, maxY, centerZ);
    }
}