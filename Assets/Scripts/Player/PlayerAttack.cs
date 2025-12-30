using UnityEngine;

/// <summary>
/// Player Attack - SIMPLIFIED VERSION
/// ✅ NO HitConfirmBroadcaster needed
/// ✅ Detects hits directly when attacking
/// ✅ 0.35s attack cooldown (prevents spam)
/// ✅ Miss resets to Attack 1
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int swordDamage = 8;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Attack Timing")]
    [Tooltip("Time between attacks - prevents spam (0.35s = ~3 attacks/second)")]
    public float attackCooldown = 0.35f;

    [Header("Combo System")]
    [SerializeField] private bool enableComboSystem = true;
    [Tooltip("Time window to input next attack after hitting")]
    [SerializeField] private float comboWindow = 0.8f;
    [SerializeField] private int maxComboSteps = 3;

    private int comboStep = 0;
    private float comboTimer = 0f;
    private bool hitSomethingThisAttack = false; // ✅ Simple flag instead of broadcaster
    private float lastAttackTime = -999f;

    [Header("Wave of Fire")]
    public GameObject waveOfFirePrefab;
    public Transform firePoint;
    public float waveOfFireCooldown = 5f;

    [Header("Attack Point")]
    public Transform attackPoint;

    private float lastWaveOfFireTime = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private AnimatorDriver animDriver;
    private Abilities abilities;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        animDriver = GetComponent<AnimatorDriver>();
    }

    void Start()
    {
        abilities = GetComponent<Abilities>();
        Debug.Log($"PlayerAttack started - Damage: {swordDamage}, Cooldown: {attackCooldown}s");
    }

    void Update()
    {
        // Combo timer countdown
        if (enableComboSystem && comboStep > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }

        // Attack (X key)
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Check cooldown first
            if (Time.time < lastAttackTime + attackCooldown)
            {
                Debug.Log($"⏱️ Attack on cooldown! Wait {(lastAttackTime + attackCooldown - Time.time):F2}s");
                return;
            }

            if (enableComboSystem)
            {
                TryComboAttack();
            }
            else
            {
                PerformAttack();
            }
        }

        // Wave of Fire (C key)
        if (Input.GetKeyDown(KeyCode.C) && Time.time >= lastWaveOfFireTime + waveOfFireCooldown)
        {
            if (abilities != null && abilities.hasWaveOfFire)
            {
                ShootWaveOfFire();
                lastWaveOfFireTime = Time.time;
            }
        }
    }

    #region Combo System

    private void TryComboAttack()
    {
        if (comboStep == 0)
        {
            // Start new combo (Attack 1)
            comboStep = 1;
            comboTimer = comboWindow;

            PerformAttack();

            Debug.Log("🎯 Combo started - Attack 1/3");
        }
        else if (comboStep > 0 && comboStep < maxComboSteps)
        {
            // ✅ Check if previous attack hit something
            if (!hitSomethingThisAttack)
            {
                // MISSED! Reset to attack 1
                Debug.Log("❌ Combo broken - missed target! Restarting combo...");
                ResetCombo();

                // Start new combo
                comboStep = 1;
                comboTimer = comboWindow;

                PerformAttack();

                Debug.Log("🎯 Combo restarted - Attack 1/3");
                return;
            }

            // HIT! Continue combo
            comboStep++;
            comboTimer = comboWindow;

            PerformAttack();

            Debug.Log($"🎯 Combo continued - Attack {comboStep}/3");

            if (comboStep >= maxComboSteps)
            {
                Debug.Log("✨ COMBO COMPLETE!");
            }
        }
        else if (comboStep >= maxComboSteps)
        {
            // Combo finished - start new one
            Debug.Log("🔄 Combo finished - starting new combo");
            ResetCombo();
            TryComboAttack();
        }
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        hitSomethingThisAttack = false; // ✅ Reset hit flag

        if (attackPoint == null)
        {
            Debug.LogError("AttackPoint is NULL!");
            return;
        }

        // Trigger the correct animation based on combo step
        if (anim != null)
        {
            if (comboStep == 1)
            {
                anim.SetTrigger("Attack");
                Debug.Log("🎬 Playing Attack 1 animation");
            }
            else if (comboStep == 2)
            {
                anim.SetTrigger("Attack2");
                Debug.Log("🎬 Playing Attack 2 animation");
            }
            else if (comboStep == 3)
            {
                anim.SetTrigger("Attack3");
                Debug.Log("🎬 Playing Attack 3 animation");
            }
        }
        else if (animDriver != null)
        {
            animDriver.Attack();
        }

        // ✅ Deal damage and check if we hit something
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                DealDamageToEnemy(enemy);
            }

            // ✅ We hit something! Set the flag
            hitSomethingThisAttack = true;
            Debug.Log($"✅ Hit {hitEnemies.Length} target(s) - combo can continue!");
        }
        else
        {
            Debug.Log("💨 Attack missed - no enemies in range");
        }
    }

    private void ResetCombo()
    {
        if (comboStep > 0)
        {
            Debug.Log("🔄 Combo reset");
        }

        comboStep = 0;
        comboTimer = 0f;
        hitSomethingThisAttack = false;
    }

    #endregion

    void DealDamageToEnemy(Collider2D enemy)
    {
        EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
        if (enemyScript != null)
        {
            Vector2 knockDir = (enemy.transform.position - transform.position).normalized;
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

    void ShootWaveOfFire()
    {
        if (waveOfFirePrefab == null || firePoint == null)
        {
            Debug.LogError("Wave of Fire prefab or fire point not assigned!");
            return;
        }

        if (animDriver != null)
            animDriver.WaveOfFire();
        else if (anim != null)
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

    #region Upgrade Methods

    public void IncreaseDamage(int amount)
    {
        swordDamage += amount;
        Debug.Log($"⚔️ Damage increased by {amount}. New damage: {swordDamage}");
    }

    public void MultiplyDamage(int multiplier)
    {
        swordDamage *= multiplier;
        Debug.Log($"⚔️ Damage multiplied by {multiplier}. New damage: {swordDamage}");
    }

    #endregion

    #region Animation Event Receivers

    // Called by animation events
    public void DealDamage()
    {
        Debug.Log("📢 DealDamage animation event called");
    }

    public void OnAttackEnd()
    {
        Debug.Log("📢 OnAttackEnd animation event called");
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}