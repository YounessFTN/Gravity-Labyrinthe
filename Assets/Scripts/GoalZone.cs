using UnityEngine;
using UnityEngine.Events;

public class GoalZone : MonoBehaviour
{
    public UnityEvent onPlayerReached;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            onPlayerReached.Invoke();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.4f);
        Gizmos.DrawCube(transform.position, Vector3.one * 2f);
        Gizmos.color = new Color(0f, 1f, 0.3f, 1f);
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    }
}
