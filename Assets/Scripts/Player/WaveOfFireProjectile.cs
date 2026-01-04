using UnityEngine;

public class WaveOfFireProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public int damage = 50;
    public float lifetime = 3f;
    public LayerMask enemyLayer;

    [HideInInspector]
    public Vector2 direction = Vector2.right;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if hit enemy
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            EnemyBase enemy = collision.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, transform.position);
            }

            BossBase boss = collision.GetComponent<BossBase>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }

            // Destroy projectile after hit (or not, depending on your design)
            // Destroy(gameObject);
        }
    }
}