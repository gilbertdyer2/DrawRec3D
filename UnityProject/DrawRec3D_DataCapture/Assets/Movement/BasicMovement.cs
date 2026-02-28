using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Basic first-person movement: WASD, sprint (Shift), jump (Space), fly mode (V) with up/down (Space/Ctrl).
/// Uses CharacterController. Assign a camera as forward reference for movement direction.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class BasicMovement : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Forward direction for movement (e.g. Main Camera). If unset, uses this transform.")]
    public Transform forwardReference;

    [Header("Ground movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Fly mode")]
    public float flySpeed = 12f;
    public float flyVerticalSpeed = 8f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _flyMode;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (forwardReference == null)
            forwardReference = transform;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Toggle fly mode with V
        if (keyboard.vKey.wasPressedThisFrame)
            _flyMode = !_flyMode;

        if (_flyMode)
            UpdateFly(keyboard);
        else
            UpdateGround(keyboard);
    }

    private void UpdateGround(Keyboard keyboard)
    {
        Vector3 input = GetMoveInput(keyboard);
        Vector3 dir = (forwardReference.forward * input.z + forwardReference.right * input.x).normalized;
        if (dir.sqrMagnitude > 0.01f)
        {
            float speed = keyboard.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
            _velocity.x = dir.x * speed;
            _velocity.z = dir.z * speed;
        }
        else
        {
            _velocity.x = 0f;
            _velocity.z = 0f;
        }

        if (_controller.isGrounded)
        {
            if (keyboard.spaceKey.wasPressedThisFrame)
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            else
                _velocity.y = -2f;
        }
        else
        {
            _velocity.y += gravity * Time.deltaTime;
        }

        _controller.Move(_velocity * Time.deltaTime);
    }

    private void UpdateFly(Keyboard keyboard)
    {
        Vector3 input = GetMoveInput(keyboard);
        float up = 0f;
        if (keyboard.spaceKey.isPressed) up += 1f;
        if (keyboard.leftCtrlKey.isPressed) up -= 1f;

        Vector3 dir = (forwardReference.forward * input.z + forwardReference.right * input.x).normalized;
        Vector3 move = dir * flySpeed + Vector3.up * (up * flyVerticalSpeed);
        _controller.Move(move * Time.deltaTime);
    }

    private static Vector3 GetMoveInput(Keyboard keyboard)
    {
        float x = 0f, z = 0f;
        if (keyboard.wKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed) z -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;
        if (keyboard.aKey.isPressed) x -= 1f;
        return new Vector3(x, 0f, z);
    }
}
