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

    [Header("Attack Points")]
    public Transform attackForwardPoint;
    public Transform attackDownPoint;

    [Header("Pogo Settings")]
    public float pogoAttackRange = 2f;  // BIGGER hitbox for pogo
    public float pogoBounceForce = 15f;
    public bool allowPogoChain = true;
    public float pogoForgivenessWindow = 0.1f; // Can pogo slightly early

    private float lastAttackTime = 0f;
    private float lastWaveOfLightTime = 0f;

    private PlayerController playerController;
    private Rigidbody2D rb;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            swordDamage = GameManager.Instance.swordDamage;
        }

        // Normal / downward attack
        if (Input.GetKeyDown(KeyCode.X) && Time.time >= lastAttackTime + attackCooldown)
        {
            HandleAttack();
            lastAttackTime = Time.time;
        }

        // Wave of Light
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

        UpdateFacingDirection();
    }

    void HandleAttack()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool isGrounded = playerController != null && playerController.IsGrounded;

        // In air + pressing DOWN => downward attack
        if (!isGrounded && verticalInput < -0.5f)
        {
            DownwardAttack();
        }
        else
        {
            ForwardAttack();
        }
    }

    void ForwardAttack()
    {
        if (attackForwardPoint == null)
        {
            Debug.LogError("AttackForwardPoint is NULL!");
            return;
        }

        if (anim != null)
            anim.SetTrigger("Attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackForwardPoint.position, attackRange, enemyLayer);

        Debug.Log($"[ForwardAttack] Found {hitEnemies.Length} enemies");

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"[ForwardAttack] Hit: {enemy.gameObject.name}");
            DealDamageToEnemy(enemy);
        }
    }

    void DownwardAttack()
    {
        if (attackDownPoint == null)
        {
            Debug.LogError("AttackDownPoint is NULL!");
            return;
        }

        if (anim != null)
            anim.SetTrigger("AttackDown");

        // BIGGER hitbox for pogo (easier to hit)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackDownPoint.position,
            pogoAttackRange,  // Use the bigger range
            enemyLayer
        );

        Debug.Log($"[DownwardAttack/Pogo] Found {hitEnemies.Length} enemies in range {pogoAttackRange}");

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"[DownwardAttack/Pogo] Hit: {enemy.gameObject.name}");
            DealDamageToEnemy(enemy);
        }

        // POGO LOGIC - Only if we hit something
        if (hitEnemies.Length > 0 && rb != null)
        {
            PerformPogo();
        }
    }

    void PerformPogo()
    {
        // Hard-set vertical speed up (clean bounce)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoBounceForce);

        Debug.Log($"🦘 POGO! Bouncing with force {pogoBounceForce}");

        // Allow immediate next attack (chain pogos)
        if (allowPogoChain)
        {
            lastAttackTime = Time.time - attackCooldown;
        }
    }

    void DealDamageToEnemy(Collider2D enemy)
    {
        EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
        if (enemyScript != null)
        {
            Vector2 knockDir = (enemy.transform.position - transform.position).normalized;
            enemyScript.TakeDamage(swordDamage, knockDir);
            return;
        }

        BossBase bossScript = enemy.GetComponent<BossBase>();
        if (bossScript != null)
        {
            bossScript.TakeDamage(swordDamage);
            return;
        }

        Debug.LogWarning($"{enemy.name} has NO EnemyBase or BossBase component!");
    }

    void ShootWaveOfLight()
    {
        if (waveOfLightPrefab == null || firePoint == null)
        {
            Debug.LogError("Wave of Light prefab or fire point not assigned!");
            return;
        }

        if (anim != null)
        {
            anim.SetTrigger("WaveOfLight");
        }

        GameObject wave = Instantiate(waveOfLightPrefab, firePoint.position, Quaternion.identity);

        WaveOfLightProjectile projectile = wave.GetComponent<WaveOfLightProjectile>();
        if (projectile != null)
        {
            projectile.direction = new Vector2(Mathf.Sign(transform.localScale.x), 0f);

            if (GameManager.Instance != null)
            {
                projectile.damage = GameManager.Instance.swordDamage * 5;
            }
        }

        Debug.Log("Wave of Light fired!");
    }

    void UpdateFacingDirection()
    {
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

    void OnDrawGizmosSelected()
    {
        // Forward attack (red)
        Gizmos.color = Color.red;
        if (attackForwardPoint != null)
            Gizmos.DrawWireSphere(attackForwardPoint.position, attackRange);

        // Pogo attack (green - BIGGER)
        Gizmos.color = Color.green;
        if (attackDownPoint != null)
            Gizmos.DrawWireSphere(attackDownPoint.position, pogoAttackRange);
    }
}