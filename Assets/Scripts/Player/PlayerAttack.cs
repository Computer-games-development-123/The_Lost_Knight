using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int baseSwordDamage = 8;
<<<<<<< HEAD
    [HideInInspector] public int swordDamage = 8;  // Runtime damage (can be upgraded)
    
    [Header("Attack Timing")]
    [Tooltip("Minimum time between attacks (0.33s = 3 attacks per second)")]
    public float attackCooldown = 0.33f;
    
=======
    public int swordDamage = 8;

>>>>>>> 9930c28 (Added dialogues to Fika and fix bugs)
    [Header("Attack 1 & 2 (Close Range)")]
    public float normalAttackRange = 1.5f;
    public Transform attackPoint;
    
    [Header("Attack 3 (Dash Attack)")]
    public float dashAttackRange = 2.5f;
    public float dashDistance = 1.5f;
    public float dashSpeed = 15f;
    public float dashKnockbackMultiplier = 1.5f;
    
    [Header("General")]
    public LayerMask enemyLayer;
    [SerializeField] private KeyCode attackKey = KeyCode.X;

    [Header("Animator Params")]
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string attackIndexIntName = "AttackIndex";

    [Header("Wave of Fire")]
    public GameObject waveOfFirePrefab;
    public Transform firePoint;
    public float waveOfFireCooldown = 5f;

    // Runtime - Attack State
    private int currentAttackIndex = 0;  // 0, 1, or 2 (for attacks 1, 2, 3)
    private bool isAttacking = false;
    private bool isDashing = false;
    private float lastAttackTime = -999f;  // Time when last attack was started
    private bool canAttack = true;  // Simple flag to allow attacks
    
    // Runtime - Wave of Fire
    private float lastWaveOfFireTime = 0f;

    // Components
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
        
        // Load saved damage from GameManager
        LoadDamageFromSave();
    }

    void Update()
    {
        // Basic attack with cooldown
        if (Input.GetKeyDown(attackKey))
        {
            TryPerformAttack();
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

    /// <summary>
    /// Try to perform attack - only succeeds if cooldown has passed
    /// </summary>
    private void TryPerformAttack()
    {
        // Check if enough time has passed since last attack
        float timeSinceLastAttack = Time.time - lastAttackTime;
        
        if (timeSinceLastAttack < attackCooldown)
        {
            // Still on cooldown - ignore input
            Debug.Log($"⏱️ Attack on cooldown. Wait {(attackCooldown - timeSinceLastAttack):F2}s more. Time since last: {timeSinceLastAttack:F2}s");
            return;
        }

        // Additional check - make sure we can attack
        if (!canAttack)
        {
            Debug.Log("🚫 Cannot attack - canAttack is false");
            return;
        }

        Debug.Log($"✅ COOLDOWN PASSED! Time since last attack: {timeSinceLastAttack:F2}s (needed {attackCooldown:F2}s)");

        // Cooldown passed - perform attack
        PerformAttack();
    }

    /// <summary>
    /// Execute the current attack in the combo sequence
    /// </summary>
    private void PerformAttack()
    {
        // Mark that we're attacking
        isAttacking = true;
        canAttack = false;  // Temporarily disable attacks
        lastAttackTime = Time.time;

        // Set the attack index for animator (1, 2, or 3)
        int animatorIndex = currentAttackIndex + 1;
        anim.SetInteger(attackIndexIntName, animatorIndex);

        // Trigger appropriate attack animation
        if (!grounded)
        {
            anim.SetTrigger("JumpAttack");
        }
        else
        {
            anim.SetTrigger(attackTriggerName);
        }

        // If this is attack 3 (dash attack), perform dash
        if (currentAttackIndex == 2)
        {
            StartDashAttack();
        }

        Debug.Log($"⚔️ Performing Attack {animatorIndex}, current time: {Time.time:F2}");

        // Cycle to next attack for the next input (0 → 1 → 2 → 0...)
        currentAttackIndex = (currentAttackIndex + 1) % 3;

        // Auto-reset attack state after cooldown (backup in case animation event doesn't fire)
        Invoke(nameof(ResetAttackState), attackCooldown);
    }

    /// <summary>
    /// Reset attack state - called either by animation event or as backup timer
    /// </summary>
    private void ResetAttackState()
    {
        isAttacking = false;
        canAttack = true;
        Debug.Log($"🔓 Attack state reset at {Time.time:F2}");
    }

    /// <summary>
    /// Dash forward for attack 3
    /// </summary>
    private void StartDashAttack()
    {
        if (movement == null || rb == null) return;

        isDashing = true;
        
        // Dash in facing direction
        Vector2 dashDirection = movement.facingDir();
        rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, rb.linearVelocity.y);

        // Stop dash after short time
        float dashDuration = dashDistance / dashSpeed;
        Invoke(nameof(StopDash), dashDuration);
    }

    private void StopDash()
    {
        isDashing = false;
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // ==========================================
    // Animation Events (called by Unity Animator)
    // ==========================================

    /// <summary>
    /// Called by Animation Event - deals damage at the right frame
    /// </summary>
    public void DealDamage()
    {
        if (!isAttacking) return;

        if (attackPoint == null)
        {
            Debug.LogError("DealDamage called but AttackPoint is NULL!");
            return;
        }

        // Determine which attack was just performed
        // currentAttackIndex has already moved forward, so we check the previous one
        int performedAttackIndex = (currentAttackIndex - 1 + 3) % 3;
        
        // Attack 3 (index 2) uses longer range
        float range = (performedAttackIndex == 2) ? dashAttackRange : normalAttackRange;
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                // Apply extra knockback if it's the dash attack (attack 3)
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

    /// <summary>
    /// Called by Animation Event - marks end of attack animation
    /// </summary>
    public void OnAttackEnd()
    {
        // Cancel the backup reset timer since animation event fired properly
        CancelInvoke(nameof(ResetAttackState));
        
        ResetAttackState();
        
        if (isDashing)
        {
            StopDash();
        }

        Debug.Log("🔵 Attack animation ended (called by animation event)");
    }

    /// <summary>
    /// Deal damage to a specific enemy
    /// </summary>
    private void DealDamageToEnemy(Collider2D enemy, bool extraKnockback)
    {
        EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
        if (enemyScript != null)
        {
            Vector2 knockDir = (enemy.transform.position - transform.position).normalized;
            
            // Apply extra knockback for dash attack
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

    // =========================
    // Damage Persistence System
    // =========================
    
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
            Debug.Log("⚔️ Loaded upgraded sword damage, damage forced - set to: 10");
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
        
        // Save to GameManager flag
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

    // =========================
    // Debug Visualization
    // =========================
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