using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GravityController : MonoBehaviour
{
    public float gravityStrength = 9.81f;
    public float rotationDuration = 0.5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnGravityLeft(InputValue value)
    {
        if (!value.isPressed)
            return;

        Vector3 desiredGravity = GetNearestAxis(-transform.right);
        StartCoroutine(ChangeGravity(desiredGravity));
    }
    
    public void OnGravityRight(InputValue value)
    {
        if (!value.isPressed)
            return;

        Vector3 desiredGravity = GetNearestAxis(transform.right);
        StartCoroutine(ChangeGravity(desiredGravity));
    }
    
    public void OnGravityFront(InputValue value)
    {
        if (!value.isPressed)
            return;

        Vector3 desiredGravity = GetNearestAxis(transform.forward);
        StartCoroutine(ChangeGravity(desiredGravity));
    }

    IEnumerator ChangeGravity(Vector3 newGravityDirection)
    {
        Physics.gravity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;

        Quaternion startRotation = transform.rotation;

        Vector3 newUp = -newGravityDirection;

        Quaternion targetRotation =
            Quaternion.FromToRotation(transform.up, newUp) * transform.rotation;

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.rotation = targetRotation;

        Physics.gravity = newGravityDirection.normalized * gravityStrength;
    }

    Vector3 GetNearestAxis(Vector3 direction)
    {
        direction.Normalize();

        Vector3[] axes =
        {
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back
        };

        Vector3 bestAxis = axes[0];
        float bestDot = Vector3.Dot(direction, bestAxis);

        foreach (Vector3 axis in axes)
        {
            float dot = Vector3.Dot(direction, axis);

            if (dot > bestDot)
            {
                bestDot = dot;
                bestAxis = axis;
            }
        }

        return bestAxis;
    }
}