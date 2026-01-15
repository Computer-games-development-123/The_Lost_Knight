using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProjectileDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask playerMask;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private float destroyFallbackDelay = 1.2f;

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
        if (((1 << other.gameObject.layer) & playerMask) == 0)
            return;

        hasHit = true;

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
                Debug.Log($"Projectile dealt {damage} damage to player!");
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Try to find EnemyBase first
            EnemyBase eh = other.GetComponent<EnemyBase>();
            if (eh == null)
            {
                eh = other.GetComponentInParent<EnemyBase>();
            }
            if (eh != null)
            {
                eh.TakeDamage(damage, transform.position);
                Debug.Log($"Projectile dealt {damage} damage to enemy!");
            }
            else
            {
                // If not an EnemyBase, try BossBase 
                BossBase boss = other.GetComponent<BossBase>();
                if (boss == null)
                {
                    boss = other.GetComponentInParent<BossBase>();
                }
                if (boss != null)
                {
                    boss.TakeDamage(damage);
                    Debug.Log($"Projectile dealt {damage} damage to boss!");
                }
                else
                {
                    Debug.LogWarning("Hit enemy layer but couldn't find EnemyBase or BossBase component!");
                }
            }
        }
        else
        {
            Debug.LogWarning("Hit player/enemy layer but couldn't find Health component!");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (col != null)
            col.enabled = false;

        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
        {
            animator.SetTrigger(hitTrigger);

            Invoke(nameof(DestroySelf), destroyFallbackDelay);
        }
        else
        {
            DestroySelf();
        }
    }
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