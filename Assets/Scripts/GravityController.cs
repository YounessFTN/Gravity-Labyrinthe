using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GravityController : MonoBehaviour
{
    public float gravityStrength = 9.81f;
    public float rotationDuration = 0.5f;

    public float gravityCooldown = 3f;
    public GravityStatusUI gravityStatusUI;
    public bool autoCreateGravityStatusUI = true;

    private Rigidbody rb;
    private bool canChangeGravity = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (gravityStatusUI == null && autoCreateGravityStatusUI)
            gravityStatusUI = GravityStatusUI.EnsureInstance();

        if (gravityStatusUI != null)
        {
            gravityStatusUI.Bind(this);
            gravityStatusUI.SetGravity(GetCurrentGravityDirection(), gravityStrength);
            gravityStatusUI.SetReady();
        }
    }

    public void OnGravityLeft(InputValue value)
    {
        TryChangeGravity(value, -transform.right);
    }

    public void OnGravityRight(InputValue value)
    {
        TryChangeGravity(value, transform.right);
    }

    public void OnGravityFront(InputValue value)
    {
        TryChangeGravity(value, transform.forward);
    }

    void TryChangeGravity(InputValue value, Vector3 direction)
    {
        if (!value.isPressed || !canChangeGravity)
            return;

        Vector3 desiredGravity = GetNearestAxis(direction);
        StartCoroutine(ChangeGravity(desiredGravity));
    }

    IEnumerator ChangeGravity(Vector3 newGravityDirection)
    {
        canChangeGravity = false;

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
            float t = Mathf.Clamp01(elapsed / rotationDuration);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            if (gravityStatusUI != null)
                gravityStatusUI.SetTransitionProgress(t);

            yield return null;
        }

        transform.rotation = targetRotation;
        Physics.gravity = newGravityDirection.normalized * gravityStrength;

        if (gravityStatusUI != null)
            gravityStatusUI.SetGravity(newGravityDirection, gravityStrength);

        float cooldownRemaining = gravityCooldown;
        while (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;

            if (gravityStatusUI != null)
                gravityStatusUI.SetCooldown(Mathf.Max(0f, cooldownRemaining), gravityCooldown);

            yield return null;
        }

        canChangeGravity = true;

        if (gravityStatusUI != null)
            gravityStatusUI.SetReady();
    }

    Vector3 GetCurrentGravityDirection()
    {
        if (Physics.gravity.sqrMagnitude > 0.001f)
            return GetNearestAxis(Physics.gravity);

        return Vector3.down;
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
