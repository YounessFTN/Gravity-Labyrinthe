using UnityEngine;
using UnityEngine.Events;

public class GoalZone : MonoBehaviour
{
    public UnityEvent onPlayerReached;

    void Awake()
    {
        // Vérifie qu'on a bien un collider trigger — sans lui OnTriggerEnter ne fire jamais
        var col = GetComponent<Collider>();
        if (col == null)
        {
            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius    = 2f;
            sphere.isTrigger = true;
            Debug.Log("[GoalZone] Aucun collider trouvé → SphereCollider trigger auto-ajouté (rayon 2).");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.Log($"[GoalZone] Collider '{col.GetType().Name}' existait mais n'était pas trigger → corrigé.");
        }
        else
        {
            Debug.Log($"[GoalZone] Collider trigger OK : {col.GetType().Name}");
        }

        Debug.Log($"[GoalZone] Instance StartAndSensitivityUI au Awake : {(StartAndSensitivityUI.Instance != null ? "OK" : "NULL (normal si le Player n'est pas encore Awake)")}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[GoalZone] OnTriggerEnter déclenché par : '{other.name}'  tag='{other.tag}'  a PlayerController={other.GetComponent<PlayerController>() != null || other.GetComponentInParent<PlayerController>() != null}");

        // Accepte le tag "Player" OU la présence du composant PlayerController (au cas où le tag manque)
        bool isPlayer = other.CompareTag("Player")
                     || other.GetComponent<PlayerController>() != null
                     || other.GetComponentInParent<PlayerController>() != null;

        if (!isPlayer)
        {
            Debug.Log("[GoalZone] → pas reconnu comme joueur, ignoré.");
            return;
        }

        Debug.Log("[GoalZone] → joueur détecté ! Invoque les events.");
        onPlayerReached?.Invoke();

        var ui = StartAndSensitivityUI.Instance;

        if (ui == null)
        {
            Debug.Log("[GoalZone] Instance nulle → création de l'UI à la volée.");
            var pc = other.GetComponent<PlayerController>()
                  ?? other.GetComponentInParent<PlayerController>();
            ui = StartAndSensitivityUI.EnsureInstance(pc);
        }

        if (ui != null)
        {
            Debug.Log("[GoalZone] → appel ShowVictory()");
            ui.ShowVictory();
        }
        else
        {
            Debug.LogError("[GoalZone] → impossible de créer StartAndSensitivityUI.");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.4f);
        Gizmos.DrawCube(transform.position, Vector3.one * 2f);
        Gizmos.color = new Color(0f, 1f, 0.3f, 1f);
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    }
}
