using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int swordDamage = 10;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer;

    [Header("Wave of Light Settings")]
    public GameObject waveOfLightPrefab;
    public Transform firePoint;
    public float waveOfLightCooldown = 3f;

    [Header("References")]
    public Animator anim;
    public Transform attackPoint;

    private float lastAttackTime = 0f;
    private float lastWaveOfLightTime = 0f;

    void Update()
    {
        // Get damage from GameManager
        if (GameManager.Instance != null)
        {
            swordDamage = GameManager.Instance.swordDamage;
        }

        if (Input.GetKeyDown(KeyCode.X) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.C) && Time.time >= lastWaveOfLightTime + waveOfLightCooldown)
        {
            if (GameManager.Instance != null && GameManager.Instance.hasWaveOfLight)
            {
                ShootWaveOfLight();
                lastWaveOfLightTime = Time.time;
            }
            else
            {
                Debug.Log("Wave of Light not unlocked yet!");
            }
        }


        // Update facing direction
        UpdateFacingDirection();
    }

    void Attack()
    {
        if (attackPoint == null)
        {
            Debug.LogError("AttackPoint is NULL!");
            return;
        }

        Debug.Log($"Attacking at {attackPoint.position} with range {attackRange}, layer: {enemyLayer.value}");

        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        Debug.Log($"Found {hitEnemies.Length} enemies in range");

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"Hit: {enemy.gameObject.name}");

            EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(swordDamage);
                continue;
            }
            BossBase bossScript = enemy.GetComponent<BossBase>();
            if (bossScript != null)
            {
                bossScript.TakeDamage(swordDamage);
                continue;
            }
            Debug.LogWarning($"{enemy.name} has NO EnemyBase or BossBase component!");
        }
    }

    void ShootWaveOfLight()
    {
        if (waveOfLightPrefab == null || firePoint == null)
        {
            Debug.LogError("Wave of Light prefab or fire point not assigned!");
            return;
        }

        // Play special attack animation
        if (anim != null)
        {
            anim.SetTrigger("WaveOfLight");
        }

        // Instantiate projectile
        GameObject wave = Instantiate(waveOfLightPrefab, firePoint.position, Quaternion.identity);

        // Set projectile direction based on facing direction
        WaveOfLightProjectile projectile = wave.GetComponent<WaveOfLightProjectile>();
        if (projectile != null)
        {
            projectile.direction = new Vector2(Mathf.Sign(transform.localScale.x), 0f);

            // Set damage from GameManager
            if (GameManager.Instance != null)
            {
                projectile.damage = GameManager.Instance.swordDamage * 5; // Wave does 5x sword damage
            }
        }

        Debug.Log("Wave of Light fired!");
    }

    void UpdateFacingDirection()
    {
        // Get horizontal input
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    // Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}