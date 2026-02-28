using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Orbits the camera around a target point. Use WASD or drag with the mouse (right button) to rotate; scroll wheel to zoom.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Point to orbit around. If unset, uses world origin.")]
    public Transform target;

    [Header("Orbit")]
    [Tooltip("Distance from target.")]
    public float radius = 5f;
    [Tooltip("Clamp vertical angle (degrees). Prevents flipping over the top.")]
    public float minPitch = -89f;
    public float maxPitch = 89f;
    [Tooltip("Min/max radius for scroll zoom.")]
    public float minRadius = 0.5f;
    public float maxRadius = 100f;

    [Header("Mouse")]
    [Tooltip("Use right mouse button to drag and orbit.")]
    public bool orbitWithMouse = true;
    [Tooltip("Mouse sensitivity (degrees per pixel).")]
    public float mouseSensitivity = 0.3f;
    [Tooltip("Scroll wheel zoom: radius change per scroll step.")]
    public float scrollZoomSpeed = 1f;
    [Tooltip("When Shift is held, zoom and orbit speed is multiplied by this (e.g. 0.25 = finer control).")]
    [Range(0.01f, 1f)]
    public float shiftKeyMovementMultiplier = 0.25f;

    [Header("Keys (WASD)")]
    [Tooltip("Degrees per second when using WASD.")]
    public float keyOrbitSpeed = 90f;
    [Tooltip("Q = raise camera height, E = lower. Units per second.")]
    public float heightKeySpeed = 2f;

    private float _yawDeg;
    private float _pitchDeg;
    private float _heightOffset;
    private bool _anglesInitialized;

    private void Start()
    {
        if (target == null)
            target = new GameObject("OrbitCameraTarget").transform;
        InitializeAnglesFromPosition();
        // radius = Mathf.Clamp(radius, minRadius, maxRadius);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        // Mouse drag (right button)
        if (orbitWithMouse && mouse != null && mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();

            // Apply shift key multiplier
            if (keyboard != null && keyboard.leftShiftKey.isPressed)
                delta *= shiftKeyMovementMultiplier;

            _yawDeg -= delta.x * mouseSensitivity;
            _pitchDeg -= delta.y * mouseSensitivity;
        }
        // Scroll zoom (slower when Shift held)
        if (mouse != null)
        {
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.001f)
            {
                float speed = scrollZoomSpeed;
                if (keyboard != null && keyboard.leftShiftKey.isPressed)
                    speed *= shiftKeyMovementMultiplier;
                radius -= scroll * speed;
                radius = Mathf.Clamp(radius, minRadius, maxRadius);
            }
        }
        // WASD orbit
        if (keyboard != null)
        {
            float dt = keyOrbitSpeed * Time.deltaTime;
            // Apply shift key multiplier
            if (keyboard != null && keyboard.leftShiftKey.isPressed)
                dt *= shiftKeyMovementMultiplier;
            
            // WASD
            if (keyboard.aKey.isPressed) _yawDeg += dt;
            if (keyboard.dKey.isPressed) _yawDeg -= dt;
            if (keyboard.wKey.isPressed) _pitchDeg += dt;
            if (keyboard.sKey.isPressed) _pitchDeg -= dt;
            // Q/E height
            float hDt = heightKeySpeed * Time.deltaTime;

            // Shift key multiplier
            if (keyboard != null && keyboard.leftShiftKey.isPressed)
                hDt *= shiftKeyMovementMultiplier;
            
            if (keyboard.qKey.isPressed) _heightOffset += hDt;
            if (keyboard.eKey.isPressed) _heightOffset -= hDt;
        }

        _pitchDeg = Mathf.Clamp(_pitchDeg, minPitch, maxPitch);

        Vector3 targetPoint = GetTargetPoint();
        Vector3 position = GetOrbitPosition(targetPoint);
        transform.position = position;
        transform.LookAt(targetPoint);
    }

    /// <summary>
    /// Set orbit angles (degrees). Yaw = horizontal, pitch = vertical.
    /// </summary>
    public void SetAngles(float yawDeg, float pitchDeg)
    {
        _yawDeg = yawDeg;
        _pitchDeg = Mathf.Clamp(pitchDeg, minPitch, maxPitch);
        _anglesInitialized = true;
    }

    /// <summary>
    /// Set the orbit radius.
    /// </summary>
    public void SetRadius(float r)
    {
        radius = Mathf.Clamp(r, minRadius, maxRadius);
    }

    private void InitializeAnglesFromPosition()
    {
        if (_anglesInitialized) return;
        Vector3 offset = transform.position - target.position;
        if (offset.sqrMagnitude < 0.0001f)
        {
            _yawDeg = 0f;
            _pitchDeg = 0f;
        }
        else
        {
            float r = offset.magnitude;
            _pitchDeg = Mathf.Asin(Mathf.Clamp(offset.y / r, -1f, 1f)) * Mathf.Rad2Deg;
            _yawDeg = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
            radius = r;
        }
        _anglesInitialized = true;
    }

    private Vector3 GetTargetPoint()
    {
        return target.position + Vector3.up * _heightOffset;
    }

    private Vector3 GetOrbitPosition(Vector3 center)
    {
        float yawRad = _yawDeg * Mathf.Deg2Rad;
        float pitchRad = _pitchDeg * Mathf.Deg2Rad;
        float cosP = Mathf.Cos(pitchRad);
        float x = Mathf.Sin(yawRad) * cosP;
        float y = Mathf.Sin(pitchRad);
        float z = Mathf.Cos(yawRad) * cosP;
        return center + radius * new Vector3(x, y, z);
    }
}
