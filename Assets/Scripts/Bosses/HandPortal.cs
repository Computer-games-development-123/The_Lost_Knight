using UnityEngine;

public class HandPortal : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 20;
    [SerializeField] private LayerMask playerMask;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph == null)
            {
                ph = other.GetComponentInParent<PlayerHealth>();
            }
            if (ph != null)
            {
                ph.TakeDamage(damage);
                Debug.Log($"HandPortal dealt {damage} damage to player!");
            }
        }
    }
}
