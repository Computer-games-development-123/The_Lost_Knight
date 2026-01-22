using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;

[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [HideInInspector] public int baseSwordDamage = 8;
    [Tooltip("Current sword damage (upgrades at runtime)")]
    public int swordDamage = 8;  // Runtime damage (can be upgraded)

    [Header("Attack Timing")]
    [Tooltip("Minimum time between attacks (0.33s = 3 attacks per second)")]
    public float attackCooldown = 0.33f;

    [Header("Attack Range")]
    public float normalAttackRange = 1.5f;
    public Transform attackPoint;

    [Header("General")]
    public LayerMask enemyLayer;
    [SerializeField] private KeyCode attackKey = KeyCode.X;

    [Header("Animator Params")]
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string attackIndexIntName = "AttackIndex";

    [Header("Fireball")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireballCooldown = 3f;
    public float fireballSpeed = 5f;

    [Header("Breath of Fire")]
    public float breathOfFireCooldown = 3f;
    public float breathOfFireRange = 3f;      // How far the fire breath reaches
    public float breathOfFireWidth = 1.5f;    // How wide the fire breath is
    public int breathDamageAddition = 30;
    public Transform breathOrigin;             // Where the fire breath starts (player's mouth)
    // Note: Breath of Fire is animation-only, damage is dealt via area detection

    // Runtime - Attack State
    private int currentAttackIndex = 0;  // 0, 1, or 2 (for attacks 1, 2, 3)
    private bool isAttacking = false;
    private float lastAttackTime = -999f;  // Time when last attack was started
    private bool canAttack = true;  // Simple flag to allow attacks

    // Runtime - Fire Spells
    private float lastFireballTime = 0f;
    private float lastBreathOfFireTime = 0f;

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

        // Load saved damage from Cloud Save
        _ = LoadDamageFromSave();
    }

    void Update()
    {
        if (!UserInputManager.Instance.IsInputEnabled)
            return;
        // Basic attack with cooldown
        if (Input.GetKeyDown(attackKey))
        {
            TryPerformAttack();
        }

        // Fireball (C key)
        if (Input.GetKeyDown(KeyCode.C) && Time.time >= lastFireballTime + fireballCooldown)
        {
            if (abilities != null && abilities.hasFireballSpell)
            {
                if (anim != null)
                    anim.SetTrigger("FireBall"); // Match Animator parameter name
                lastFireballTime = Time.time;
            }
        }

        // Breath of Fire (V key)
        if (Input.GetKeyDown(KeyCode.V) && Time.time >= lastBreathOfFireTime + breathOfFireCooldown)
        {
            if (abilities != null && abilities.hasBreathOfFire)
            {
                if (anim != null)
                    anim.SetTrigger("FireBreath"); // Match Animator parameter name
                lastBreathOfFireTime = Time.time;
            }
        }
    }

    /// <summary>
    /// Try to perform attack - only succeeds if cooldown has passed
    /// </summary>
    private void TryPerformAttack()
    {
        // Check if player is currently hurt - cannot attack while hurt
        if (movement != null && movement.isHurt)
        {
            Debug.Log("Cannot attack - player is hurt!");
            return;
        }

        // Check if enough time has passed since last attack
        float timeSinceLastAttack = Time.time - lastAttackTime;

        if (timeSinceLastAttack < attackCooldown)
        {
            // Still on cooldown - ignore input
            Debug.Log($"Attack on cooldown. Wait {(attackCooldown - timeSinceLastAttack):F2}s more. Time since last: {timeSinceLastAttack:F2}s");
            return;
        }

        // Additional check - make sure we can attack
        if (!canAttack)
        {
            Debug.Log("Cannot attack - canAttack is false");
            return;
        }

        Debug.Log($"COOLDOWN PASSED! Time since last attack: {timeSinceLastAttack:F2}s (needed {attackCooldown:F2}s)");

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

        // Play attack sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerAttack();
        }

        // Trigger appropriate attack animation
        if (!grounded)
        {
            anim.SetTrigger("JumpAttack");
        }
        else
        {
            anim.SetTrigger(attackTriggerName);
        }

        Debug.Log($"Performing Attack {animatorIndex}, current time: {Time.time:F2}");

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
        Debug.Log($"Attack state reset at {Time.time:F2}");
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

        // All attacks use the same range now
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, normalAttackRange, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                DealDamageToEnemy(enemy);
            }

            Debug.Log($"Hit {hitEnemies.Length} target(s) with Attack {performedAttackIndex + 1} for {swordDamage} damage!");
        }
        else
        {
            Debug.Log($"Attack {performedAttackIndex + 1} missed - no enemies in range");
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

        Debug.Log("Attack animation ended (called by animation event)");
    }

    /// <summary>
    /// Deal damage to a specific enemy
    /// </summary>
    private void DealDamageToEnemy(Collider2D enemy)
    {
        EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
        if (enemyScript != null)
        {
            Vector2 knockDir = (enemy.transform.position - transform.position).normalized;
            enemyScript.TakeDamage(swordDamage, knockDir);
            Debug.Log($"Hit {enemy.name} for {swordDamage} damage!");
            return;
        }

        BossBase bossScript = enemy.GetComponent<BossBase>();
        if (bossScript != null)
        {
            bossScript.TakeDamage(swordDamage);
            Debug.Log($"Hit boss {enemy.name} for {swordDamage} damage!");
            return;
        }

        Debug.LogWarning($"{enemy.name} has NO EnemyBase or BossBase component!");
    }

    // =========================
    // Fireball
    // =========================
    void ShootFireball()
    {
        if (fireballPrefab == null || firePoint == null)
        {
            Debug.LogError("Fireball prefab or fire point not assigned!");
            return;
        }

        bool shootRight = GetComponent<PlayerController>().facingRight;
        Vector2 dir = shootRight ? Vector2.right : Vector2.left;

        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

        Vector3 s = fireball.transform.localScale;
        s.x = Mathf.Abs(s.x) * (shootRight ? 1f : -1f);
        fireball.transform.localScale = s;

        Rigidbody2D rbP = fireball.GetComponent<Rigidbody2D>();
        if (rbP != null)
            rbP.linearVelocity = dir * fireballSpeed;

        Debug.Log($"Fireball fired! Damage: {swordDamage}");
    }

    // =========================
    // Breath of Fire
    // =========================
    // Note: Breath of Fire is just an animation effect
    // This method detects and damages enemies in a cone/area in front of the player
    void ShootBreathOfFire()
    {
        // Determine spawn point (use breathOrigin if set, otherwise attackPoint)
        Transform origin = breathOrigin != null ? breathOrigin : attackPoint;

        if (origin == null)
        {
            Debug.LogError("No origin point for Breath of Fire! Assign breathOrigin or attackPoint.");
            return;
        }

        // Get facing direction
        Vector2 direction = movement != null ? movement.facingDir() : Vector2.right;

        // Calculate the center point of the breath cone
        Vector2 centerPoint = (Vector2)origin.position + direction * (breathOfFireRange / 2f);

        // Detect all enemies in the area
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
            centerPoint,
            new Vector2(breathOfFireRange, breathOfFireWidth),
            0f,
            enemyLayer
        );

        if (hitEnemies.Length > 0)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                // Check if enemy is actually in front of the player (not behind)
                Vector2 toEnemy = (enemy.transform.position - origin.position).normalized;
                float dotProduct = Vector2.Dot(direction, toEnemy);

                if (dotProduct > 0.5f) // Enemy is in front (not behind or to the side)
                {
                    DealBreathOfFireDamage(enemy);
                }
            }

            Debug.Log($"Breath of Fire hit {hitEnemies.Length} enemies for {swordDamage * 3} damage each!");
        }
        else
        {
            Debug.Log("Breath of Fire - no enemies in range");
        }
    }

    /// <summary>
    /// Deal damage to enemy hit by Breath of Fire
    /// </summary>
    private void DealBreathOfFireDamage(Collider2D enemy)
    {
        int breathDamage = swordDamage + breathDamageAddition;

        EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
        if (enemyScript != null)
        {
            Vector2 knockDir = (enemy.transform.position - transform.position).normalized;
            enemyScript.TakeDamage(breathDamage, knockDir);
            Debug.Log($"Breath of Fire hit {enemy.name} for {breathDamage} damage!");
            return;
        }

        BossBase bossScript = enemy.GetComponent<BossBase>();
        if (bossScript != null)
        {
            bossScript.TakeDamage(breathDamage);
            Debug.Log($"Breath of Fire hit boss {enemy.name} for {breathDamage} damage!");
            return;
        }

        Debug.LogWarning($"{enemy.name} has NO EnemyBase or BossBase component!");
    }

    // =========================
    // Damage Persistence System
    // =========================

    /// <summary>
    /// Load damage from Cloud Save when script starts
    /// </summary>
    private async Task LoadDamageFromSave()
    {
        // Wait for GameManager to be ready
        while (GameManager.Instance == null || !GameManager.Instance.IsProgressLoaded)
        {
            await Task.Yield();
        }

        try
        {
            // Try to load saved sword damage from cloud
            var cloudData = await DatabaseManager.LoadData("PlayerSwordDamage");

            if (cloudData.ContainsKey("PlayerSwordDamage"))
            {
                swordDamage = cloudData["PlayerSwordDamage"].Value.GetAs<int>();
                Debug.Log($"Loaded sword damage from cloud: {swordDamage}");
            }
            else
            {
                // No saved damage - use base damage
                swordDamage = baseSwordDamage;
                Debug.Log($"No saved damage found, using base: {baseSwordDamage}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to load sword damage: {e.Message}");
            swordDamage = baseSwordDamage;
        }
    }

    /// <summary>
    /// Save current damage to Cloud Save
    /// </summary>
    private async void SaveDamageToCloud()
    {
        try
        {
            await DatabaseManager.SaveData(("PlayerSwordDamage", swordDamage));
            Debug.Log($"Saved sword damage to cloud: {swordDamage}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save sword damage: {e.Message}");
        }
    }

    /// <summary>
    /// Increase damage by amount (used by upgrades)
    /// Called by: Yoji's free upgrade (4 dmg) and Shop damage upgrades (5 dmg each)
    /// </summary>
    public void IncreaseDamage(int amount)
    {
        swordDamage += amount;

        // Set the upgraded sword flag (for Yoji's initial upgrade check)
        if (swordDamage > baseSwordDamage)
        {
            GameManager.Instance.SetFlag(GameFlag.hasUpgradedSword, true);
            GameManager.Instance.SaveProgress();
        }

        // Save the actual damage value to cloud
        SaveDamageToCloud();

        Debug.Log($"Damage increased by {amount}. New damage: {swordDamage}");
    }

    /// <summary>
    /// Multiply damage (used by Breath of Fire purchase)
    /// </summary>
    public void MultiplyDamage(int multiplier)
    {
        swordDamage *= multiplier;

        // Save the new damage value
        SaveDamageToCloud();

        Debug.Log($"Damage multiplied by {multiplier}. New damage: {swordDamage}");
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
        }

        // Draw Breath of Fire range
        if (breathOrigin != null || attackPoint != null)
        {
            Transform origin = breathOrigin != null ? breathOrigin : attackPoint;
            Vector2 direction = movement != null ? movement.facingDir() : Vector2.right;
            Vector2 centerPoint = (Vector2)origin.position + direction * (breathOfFireRange / 2f);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Orange, semi-transparent
            Gizmos.DrawWireCube(centerPoint, new Vector3(breathOfFireRange, breathOfFireWidth, 1f));
        }
    }
}