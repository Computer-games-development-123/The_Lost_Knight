using UnityEngine;

/// <summary>
/// Simple projectile fired by ranged enemies.
/// Deals damage to player on contact, then destroys itself.
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public int damage = 5;
    public float lifetime = 5f; // Auto-destroy after this time

    [Header("Visual")]
    public bool rotateTowardsDirection = true;
    public float rotationSpeed = 360f;

    private void Start()
    {
        // Auto-destroy after lifetime expires
        Destroy(gameObject, lifetime);

        // Optional: Rotate sprite to face movement direction
        if (rotateTowardsDirection)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity != Vector2.zero)
            {
                float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hit player - deal damage
        if (collision.CompareTag("Player"))
        {
            CharacterStats ps = collision.GetComponent<CharacterStats>();
            if (ps != null)
            {
                ps.TakeDamage(damage, transform.position);
                Debug.Log($"Projectile hit player for {damage} damage!");
            }

            // Destroy projectile on hit
            Destroy(gameObject);
            return;
        }

        // Hit ground/walls - destroy
        if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            Debug.Log("Projectile hit obstacle!");
            Destroy(gameObject);
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Same logic for collision (in case trigger is not used)
        if (collision.gameObject.CompareTag("Player"))
        {
            CharacterStats ps = collision.gameObject.GetComponent<CharacterStats>();
            if (ps != null)
            {
                ps.TakeDamage(damage, transform.position);
                Debug.Log($"Projectile hit player for {damage} damage!");
            }

            Destroy(gameObject);
            return;
        }

        // Hit anything else - destroy
        Debug.Log($"Projectile hit {collision.gameObject.name}!");
        Destroy(gameObject);
    }
}