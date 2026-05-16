using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float minMouseSensitivity = 0.2f;
    public float maxMouseSensitivity = 8f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.1f;
    public bool controlsEnabled = true;
    public bool autoCreateStartUI = true;

    public Transform cameraTransform;

    private Rigidbody rb;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpRequested = false;

    private float cameraPitch = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (autoCreateStartUI)
            StartAndSensitivityUI.EnsureInstance(this);
        else
            LockCursor(true);
    }

    void FixedUpdate()
    {
        if (!controlsEnabled)
        {
            rb.linearVelocity = Vector3.Project(rb.linearVelocity, Physics.gravity.normalized);
            jumpRequested = false;
            return;
        }

        Vector3 gravityDirection = Physics.gravity.normalized;

        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        Vector3 currentFallVelocity = Vector3.Project(rb.linearVelocity, gravityDirection);

        rb.linearVelocity = move * moveSpeed + currentFallVelocity;

        if (jumpRequested)
        {
            rb.linearVelocity += -gravityDirection * jumpForce;
            jumpRequested = false;
        }
    }

    void Update()
    {
        if (!controlsEnabled)
            return;

        // Horizontal rotation (player body)
        transform.Rotate(Vector3.up * (lookInput.x * mouseSensitivity));

        // Vertical rotation (camera)
        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        cameraTransform.localEulerAngles =
            new Vector3(cameraPitch, 0f, 0f);

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && IsGrounded())
            jumpRequested = true;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, minMouseSensitivity, maxMouseSensitivity);
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
        jumpRequested = false;
    }

    public void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && IsGrounded())
            jumpRequested = true;
    }

    bool IsGrounded()
    {
        LayerMask groundMask = ~(1 << gameObject.layer);

        CapsuleCollider col = GetComponent<CapsuleCollider>();
        float radius = col != null ? col.radius : 0.3f;
        float halfHeight = col != null ? col.height / 2f : 0.5f;

        Vector3 gravityDirection = Physics.gravity.normalized;
        Vector3 feetCenter = transform.position + gravityDirection * (halfHeight - radius);

        return Physics.CheckSphere(feetCenter, radius + groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    public void OnGravityLeft()
    {
    }
}
