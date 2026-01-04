using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int baseSwordDamage = 8;
    [HideInInspector] public int swordDamage = 8;

    [Header("Attack 1 & 2 (Close Range)")]
    public float normalAttackRange = 1.5f;
    public Transform attackPoint;

    [Header("Attack 3 (Dash Attack)")]
    public float dashAttackRange = 2.5f;  // Longer range for thrust
    public float dashDistance = 1.5f;     // How far to dash forward
    public float dashSpeed = 15f;         // Speed of the dash
    public float dashKnockbackMultiplier = 1.5f;  // Extra knockback for dash attack

    [Header("General")]
    public LayerMask enemyLayer;
    [SerializeField] private KeyCode attackKey = KeyCode.X;

    [Header("Animator Params")]
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string attackIndexIntName = "AttackIndex";  // For cycling through animations

    [Header("Wave of Fire")]
    public GameObject waveOfFirePrefab;
    public Transform firePoint;
    public float waveOfFireCooldown = 5f;

    // Runtime
    private int currentAttackIndex = 0;
    private bool isAttacking = false;
    private bool isDashing = false;
    private float lastWaveOfFireTime = 0f;

    private Animator anim;
    private Abilities abilities;
    private PlayerController movement;
    private Rigidbody2D rb;
    private bool grounded => (movement != null) ? movement.isGrounded : true;

    void Awake()
    {
        anim = GetComponent<Animator>();
        abilities = GetComponent<Abilities>();
        movement = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        // ✅ Load saved damage from GameManager
        LoadDamageFromSave();
    }

    void Update()
    {
        // Basic attack
        if (Input.GetKeyDown(attackKey))
        {
            PerformAttack();
        }

        // Wave of Fire
        if (Input.GetKeyDown(KeyCode.C) && Time.time >= lastWaveOfFireTime + waveOfFireCooldown)
        {
            if (abilities != null && abilities.hasWaveOfFire)
            {
                ShootWaveOfFire();
                lastWaveOfFireTime = Time.time;
            }
        }
    }

    private void PerformAttack()
    {
        isAttacking = true;

        // Set the attack index for animator BEFORE cycling (1, 2, or 3)
        int animatorIndex = currentAttackIndex + 1;
        anim.SetInteger(attackIndexIntName, animatorIndex);

        // Trigger attack animation
        if (!grounded)
        {
            anim.SetTrigger("JumpAttack");
        }
        else
        {
            anim.SetTrigger(attackTriggerName);
        }

        //If this is attack 3 (dash attack), perform dash
        if (currentAttackIndex == 2)
        {
            StartDashAttack();
        }

        Debug.Log($"⚔️ Performing Attack {animatorIndex}, next will be Attack {(currentAttackIndex + 1) % 3 + 1}");

        //Cycle to next attack AFTER performing (0 → 1 → 2 → 0...)
        currentAttackIndex = (currentAttackIndex + 1) % 3;
    }

    private void StartDashAttack()
    {
        if (movement == null || rb == null) return;

        isDashing = true;

        // Dash in facing direction
        Vector2 dashDirection = movement.facingDir();
        rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, rb.linearVelocity.y);

        // Stop dash after short time
        Invoke(nameof(StopDash), dashDistance / dashSpeed);
    }

    private void StopDash()
    {
        isDashing = false;
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    public void DealDamage()
    {
        if (!isAttacking) return;

        if (attackPoint == null)
        {
            Debug.LogError("DealDamage called but AttackPoint is NULL!");
            return;
        }

        int performedAttackIndex = (currentAttackIndex - 1 + 3) % 3;

        // Attack 3 (index 2) uses longer range
        float range = (performedAttackIndex == 2) ? dashAttackRange : normalAttackRange;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                bool isDashAttack = (performedAttackIndex == 2);
                DealDamageToEnemy(enemy, isDashAttack);
            }

            Debug.Log($"✅ Hit {hitEnemies.Length} target(s) with Attack {performedAttackIndex + 1} for {swordDamage} damage!");
        }
        else
        {
            Debug.Log($"💨 Attack {performedAttackIndex + 1} missed - no enemies in range");
        }
    }

    public void OnAttackEnd()
    {
        isAttacking = false;

        if (isDashing)
        {
            StopDash();
        }

        Debug.Log("📢 Attack ended");
    }

    private void DealDamageToEnemy(Collider2D enemy, bool extraKnockback)
    {
        EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
        if (enemyScript != null)
        {
            Vector2 knockDir = (enemy.transform.position - transform.position).normalized;

            // ✅ Apply extra knockback for dash attack
            if (extraKnockback)
            {
                knockDir *= dashKnockbackMultiplier;
            }

            enemyScript.TakeDamage(swordDamage, knockDir);
            Debug.Log($"⚔️ Hit {enemy.name} for {swordDamage} damage!");
            return;
        }

        BossBase bossScript = enemy.GetComponent<BossBase>();
        if (bossScript != null)
        {
            bossScript.TakeDamage(swordDamage);
            Debug.Log($"⚔️ Hit boss {enemy.name} for {swordDamage} damage!");
            return;
        }

        Debug.LogWarning($"{enemy.name} has NO EnemyBase or BossBase component!");
    }

    // =========================
    // Wave of Fire
    // =========================
    void ShootWaveOfFire()
    {
        if (waveOfFirePrefab == null || firePoint == null)
        {
            Debug.LogError("Wave of Fire prefab or fire point not assigned!");
            return;
        }

        if (anim != null)
            anim.SetTrigger("WaveOfFire");

        GameObject wave = Instantiate(waveOfFirePrefab, firePoint.position, Quaternion.identity);

        WaveOfFireProjectile projectile = wave.GetComponent<WaveOfFireProjectile>();
        if (projectile != null)
        {
            projectile.direction = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
            projectile.damage = swordDamage * 5;
        }

        Debug.Log($"✨ Wave of Fire fired! Damage: {swordDamage * 5}");
    }


    /// <summary>
    /// Load damage from save when script starts
    /// </summary>
    private void LoadDamageFromSave()
    {
        if (GameManager.Instance == null) return;

        // Check if sword has been upgraded
        bool hasUpgrade = GameManager.Instance.GetFlag(GameFlag.hasUpgradedSword);

        if (hasUpgrade)
        {
            swordDamage = 10;  // Upgraded damage
            Debug.Log("⚔️ Loaded upgraded sword damage: 10");
        }
        else
        {
            swordDamage = baseSwordDamage;  // Base damage (8)
            Debug.Log($"⚔️ Loaded base sword damage: {baseSwordDamage}");
        }
    }

    /// <summary>
    /// Increase damage by amount (used by upgrades)
    /// </summary>
    public void IncreaseDamage(int amount)
    {
        swordDamage += amount;

        // ✅ Save to GameManager flag
        if (swordDamage > baseSwordDamage)
        {
            GameManager.Instance.SetFlag(GameFlag.hasUpgradedSword, true);
            GameManager.Instance.SaveProgress();
        }

        Debug.Log($"⚔️ Damage increased by {amount}. New damage: {swordDamage}");
    }

    /// <summary>
    /// Multiply damage (used by shop upgrades)
    /// </summary>
    public void MultiplyDamage(int multiplier)
    {
        swordDamage *= multiplier;
        Debug.Log($"⚔️ Damage multiplied by {multiplier}. New damage: {swordDamage}");
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            // Draw normal attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, normalAttackRange);

            // Draw dash attack range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, dashAttackRange);
        }
    }
}