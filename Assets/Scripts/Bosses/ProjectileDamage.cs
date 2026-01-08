using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProjectileDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask playerMask; // Set this to the Player layer

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private float destroyFallbackDelay = 1.2f; // If there's no Animation Event

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;

    private bool hasHit = false;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<Collider2D>();
        if (animator == null) animator = GetComponent<Animator>();

        // Recommended for projectiles
        if (col != null) col.isTrigger = true;
        if (rb != null) rb.gravityScale = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Only hit player by layer (recommended)
        if (((1 << other.gameObject.layer) & playerMask) == 0)
            return;

        hasHit = true;

        // 1) Damage player - search for PlayerHealth in parent if not found on self
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        
        // If not found (for example, we hit the attack point), search in parent
        if (ph == null)
        {
            ph = other.GetComponentInParent<PlayerHealth>();
        }
        
        if (ph != null)
        {
            ph.TakeDamage(damage);
            Debug.Log($"Projectile dealt {damage} damage to player!");
        }
        else
        {
            Debug.LogWarning("Hit player layer but couldn't find PlayerHealth component!");
        }

        // 2) Stop movement and prevent repeated hits
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (col != null)
            col.enabled = false;

        // 3) Play hit animation
        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
        {
            animator.SetTrigger(hitTrigger);

            // Backup for deletion if you didn't add an Animation Event at the end of the clip
            Invoke(nameof(DestroySelf), destroyFallbackDelay);
        }
        else
        {
            // If there's no animator - delete immediately
            DestroySelf();
        }
    }

    // Call this at the end of the Hit animation using an Animation Event
    public void OnHitAnimationFinished()
    {
        DestroySelf();
    }

    private void DestroySelf()
    {
        CancelInvoke();
        Destroy(gameObject);
    }
}