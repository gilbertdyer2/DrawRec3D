using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Shape used for each point.
/// </summary>
public enum PointShape
{
    Cube,
    Sphere
}

/// <summary>
/// Draws individual 3D points (not connected by lines) from a List&lt;Vector3&gt; at runtime.
/// Uses a MeshFilter + MeshRenderer so rendering works in URP (Graphics.DrawMesh from OnRenderObject does not).
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DrawPoints3D : MonoBehaviour
{
    [Tooltip("List of 3D positions to draw as points (local space).")]
    public List<Vector3> points = new List<Vector3>();

    [Header("Appearance")]
    [Tooltip("Shape of each point: Cube (square) or Sphere (dot).")]
    public PointShape pointShape = PointShape.Cube;
    [Tooltip("Size of each point (cube half-extent / sphere radius).")]
    [Min(0.001f)]
    public float pointSize = 0.05f;

    [Tooltip("Color of the points (used when no material is assigned).")]
    public Color color = Color.white;

    [Tooltip("Optional material. If not set, uses URP Unlit with the Color field.")]
    public Material material;

    [Header("Quest controller origin (optional)")]
    [Tooltip("When left controller grip is held, drawn points origin is set to this transform's position.")]
    public Transform leftControllerAnchor;
    [Tooltip("When right controller grip is held, drawn points origin is set to this transform's position.")]
    public Transform rightControllerAnchor;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Material _materialInstance;
    private Mesh _combinedMesh;
    private int _lastPointCount = -1;
    private float _lastPointSize = -1f;
    private PointShape _lastPointShape = (PointShape)(-1);

    // Single cube: 24 vertices, 36 indices (12 tris). Reused to build combined mesh.
    private static readonly Vector3[] CubeVertices = new Vector3[]
    {
        new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),  new Vector3(0.5f, -0.5f, 0.5f),  new Vector3(0.5f, 0.5f, 0.5f),  new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, 0.5f),  new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),  new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f),   new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),  new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f),  new Vector3(-0.5f, 0.5f, 0.5f)
    };

    private static readonly int[] CubeTriangles = new int[]
    {
        0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7,
        8, 10, 9, 8, 11, 10, 12, 14, 13, 12, 15, 14,
        16, 18, 17, 16, 19, 18, 20, 22, 21, 20, 23, 22
    };

    private const int CubeVertexCount = 24;
    private const int CubeIndexCount = 36;

    // Cached sphere mesh (Unity's built-in sphere) used for PointShape.Sphere.
    private static Mesh _sphereMesh;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        if (material != null)
        {
            _materialInstance = material;
        }
        else
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogError("DrawPoints3D: No suitable shader found. Assign a Material in the inspector.");
                return;
            }
            _materialInstance = new Material(shader);
            _materialInstance.color = color;
            if (_materialInstance.HasProperty("_BaseColor"))
                _materialInstance.SetColor("_BaseColor", color);
        }

        _meshRenderer.sharedMaterial = _materialInstance;
        _meshRenderer.enabled = true;
    }

    private void LateUpdate()
    {
        UpdateOriginFromControllerGrip();

        if (points == null || _meshFilter == null || _meshRenderer == null || _materialInstance == null)
            return;

        int count = points.Count;
        if (count == 0)
        {
            if (_combinedMesh != null) { Destroy(_combinedMesh); _combinedMesh = null; }
            _meshFilter.sharedMesh = null;
            _meshRenderer.enabled = false;
            _lastPointCount = 0;
            _lastPointShape = (PointShape)(-1);
            return;
        }

        // Rebuild mesh only when count, size, or shape changed
        if (count == _lastPointCount && Mathf.Approximately(pointSize, _lastPointSize) && pointShape == _lastPointShape)
            return;
        _lastPointCount = count;
        _lastPointSize = pointSize;
        _lastPointShape = pointShape;

        _meshRenderer.enabled = true;
        if (_combinedMesh != null)
            Destroy(_combinedMesh);
        _combinedMesh = BuildCombinedMesh();
        _meshFilter.sharedMesh = _combinedMesh;
    }

    private void OnDestroy()
    {
        if (_combinedMesh != null)
            Destroy(_combinedMesh);
        if (material == null && _materialInstance != null)
            Destroy(_materialInstance);
    }

    private Mesh BuildCombinedMesh()
    {
        int count = points.Count;
        Vector3[] srcVerts;
        int[] srcIndices;
        float shapeScale;

        if (pointShape == PointShape.Sphere)
        {
            // Lazy-init the sphere mesh once using Unity's built-in sphere.
            if (_sphereMesh == null)
            {
                GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _sphereMesh = temp.GetComponent<MeshFilter>().sharedMesh;
                Object.Destroy(temp);
            }

            if (_sphereMesh == null)
            {
                Debug.LogError("DrawPoints3D: Sphere mesh not available.");
                return null;
            }

            srcVerts = _sphereMesh.vertices;
            srcIndices = _sphereMesh.triangles;
            // Built-in sphere has radius ~0.5 at scale 1; multiply by 2 so pointSize ~= sphere radius.
            shapeScale = pointSize * 2f;
        }
        else
        {
            srcVerts = CubeVertices;
            srcIndices = CubeTriangles;
            shapeScale = pointSize;
        }

        int vertsPerPoint = srcVerts.Length;
        int indicesPerPoint = srcIndices.Length;

        int totalVerts = count * vertsPerPoint;
        int totalIndices = count * indicesPerPoint;

        var vertices = new Vector3[totalVerts];
        var indices = new int[totalIndices];

        for (int i = 0; i < count; i++)
        {
            Vector3 p = points[i];
            int vBase = i * vertsPerPoint;
            int iBase = i * indicesPerPoint;

            for (int v = 0; v < vertsPerPoint; v++)
                vertices[vBase + v] = p + srcVerts[v] * shapeScale;

            for (int t = 0; t < indicesPerPoint; t++)
                indices[iBase + t] = srcIndices[t] + vBase;
        }

        var mesh = new Mesh();
        mesh.name = "DrawPoints3D_Combined";
        mesh.indexFormat = totalVerts > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        // Bounds that encompass all points for correct culling
        Vector3 min = points[0], max = points[0];
        for (int i = 1; i < count; i++)
        {
            Vector3 pt = points[i];
            min = Vector3.Min(min, pt);
            max = Vector3.Max(max, pt);
        }
        float padding = pointSize * 2f;
        mesh.bounds = new Bounds(
            (min + max) * 0.5f,
            (max - min) + Vector3.one * padding
        );
        mesh.RecalculateNormals();

        return mesh;
    }

    private void UpdateOriginFromControllerGrip()
    {
        if (leftControllerAnchor != null && OVRInput.Get(OVRInput.RawButton.LHandTrigger))
        {
            transform.position = leftControllerAnchor.position;
            return;
        }
        if (rightControllerAnchor != null && OVRInput.Get(OVRInput.RawButton.RHandTrigger))
        {
            transform.position = rightControllerAnchor.position;
        }
    }
}
