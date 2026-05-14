using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    public Transform cameraTransform;

    private Rigidbody rb;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float cameraPitch = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()

    {

        Vector3 gravityDirection = Physics.gravity.normalized;

        Vector3 move =

            transform.forward * moveInput.y +

            transform.right * moveInput.x;

        Vector3 currentFallVelocity =

            Vector3.Project(rb.linearVelocity, gravityDirection);

        rb.linearVelocity = move * moveSpeed + currentFallVelocity;

    }

    void Update()
    {
        // Horizontal rotation (player body)
        transform.Rotate(Vector3.up * (lookInput.x * mouseSensitivity));

        // Vertical rotation (camera)
        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        cameraTransform.localEulerAngles =
            new Vector3(cameraPitch, 0f, 0f);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
    
    public void OnGravityLeft()
    {
    }
}