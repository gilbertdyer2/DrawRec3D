using System.Collections.Generic;
using UnityEngine;

public static class GesturePreprocessing
{
    /// <summary>
    /// Uniformly sample a point list to exactly N points.
    /// - If points.Count > N: uniform downsample
    /// - If points.Count < N: randomly repeat some points
    /// </summary>
    public static List<Vector3> SamplePointsFixed(List<Vector3> points, int N = 128, int? seed = null)
    {
        if (points == null || points.Count == 0)
        {
            Debug.LogError("Cannot sample from an empty point list.");
            return new List<Vector3>();
        }

        // Set random seed if provided
        if (seed.HasValue)
        {
            Random.InitState(seed.Value);
        }

        int L = points.Count;

        // Case 1: More than N, downsample uniformly
        if (L > N)
        {
            List<Vector3> sampled = new List<Vector3>(N);
            for (int i = 0; i < N; i++)
            {
                // Linearly interpolate indices
                float index = i * (L - 1) / (float)(N - 1);
                sampled.Add(points[Mathf.FloorToInt(index)]);
            }
            return sampled;
        }

        // Case 2: Less than N, pad with random repeats
        if (L < N)
        {
            List<Vector3> padded = new List<Vector3>(points);
            int needed = N - L;
            
            for (int i = 0; i < needed; i++)
            {
                int randomIndex = Random.Range(0, L);
                padded.Add(points[randomIndex]);
            }
            
            return padded;
        }

        // Case 3: Already exactly N
        return new List<Vector3>(points);
    }

    /// <summary>
    /// Treat first point as origin (0,0,0) - Subtracts first point value from every point.
    /// </summary>
    public static List<Vector3> SetFirstAsOrigin(List<Vector3> points)
    {
        if (points == null || points.Count == 0)
        {
            Debug.LogError("Cannot set origin on empty point list.");
            return new List<Vector3>();
        }

        List<Vector3> pointsCopy = new List<Vector3>(points.Count);
        Vector3 origin = points[0];

        for (int i = 0; i < points.Count; i++)
        {
            pointsCopy.Add(points[i] - origin);
        }

        return pointsCopy;
    }
}