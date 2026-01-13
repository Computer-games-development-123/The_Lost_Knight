using UnityEngine;

/// <summary>
/// Lightning projectile that falls from Philip's portal
/// Damages player on contact
/// </summary>
public class LightningProjectile : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 15;

    [Header("Movement")]
    [SerializeField] private float fallSpeed = 10f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 5f;

    private Rigidbody2D rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f; // No gravity, we control the fall
            rb.linearVelocity = Vector2.down * fallSpeed;
        }

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Hit player
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // If not found on this object, search parent (in case hit attack point)
            if (playerHealth == null)
            {
                playerHealth = other.GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Lightning struck player for {damage} damage!");
            }

            hasHit = true;
            DestroyLightning();
        }
        // Hit ground
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            hasHit = true;
            DestroyLightning();
        }
    }

    private void DestroyLightning()
    {
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Could add explosion effect here

        // Destroy after a brief delay (in case you want to add particle effects)
        Destroy(gameObject, 0.1f);
    }
}
