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
    public float pogoBounceForce = 15f;
    public bool allowPogoChain = true;    // can chain multiple pogos


    private float lastAttackTime = 0f;
    private float lastWaveOfLightTime = 0f;

    // cached refs
    private PlayerController playerController;
    private Rigidbody2D rb;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get damage from GameManager
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

        // Update facing direction
        UpdateFacingDirection();
    }

    // Decide which attack to use
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

    // === FORWARD ATTACK (existing behaviour) ===
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

        Debug.Log($"[ForwardAttack] Found {hitEnemies.Length} enemies in range");

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"[ForwardAttack] Hit: {enemy.gameObject.name}");
            DealDamageToEnemy(enemy);
        }
    }

    // === DOWNWARD ATTACK ===
    void DownwardAttack()
    {
        if (attackDownPoint == null)
        {
            Debug.LogError("AttackDownPoint is NULL!");
            return;
        }

        if (anim != null)
            anim.SetTrigger("AttackDown");   // make sure this trigger exists in Animator

        // Hitbox below the player
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackDownPoint.position,
            attackRange,
            enemyLayer
        );

        Debug.Log($"[DownwardAttack] Found {hitEnemies.Length} enemies in range");

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"[DownwardAttack] Hit: {enemy.gameObject.name}");
            DealDamageToEnemy(enemy);
        }

        // === POGO LOGIC ===
        // Only pogo if we actually hit something
        if (hitEnemies.Length > 0 && rb != null)
        {
            // Hard-set vertical speed up (clean bounce)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoBounceForce);

            // Allow immediate next attack (so you can chain pogos)
            if (allowPogoChain)
            {
                // Pretend the cooldown already passed
                lastAttackTime = Time.time - attackCooldown;
            }
        }
    }


    // Shared damage logic
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

    // Visualize attack ranges in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if (attackForwardPoint != null)
            Gizmos.DrawWireSphere(attackForwardPoint.position, attackRange);

        if (attackDownPoint != null)
            Gizmos.DrawWireSphere(attackDownPoint.position, attackRange);
    }
}
