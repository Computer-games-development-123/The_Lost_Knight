using UnityEngine;

/// <summary>
/// Player Attack - Combat ONLY
/// NO pogo attack (removed)
/// NO downward attack (removed)
/// NO teleport (use Abilities.cs)
/// NO i-frames (use Invulnerability.cs)
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour, IHitConfirmListener
{
    [Header("Attack Settings")]
    [Tooltip("Synced from PlayerState automatically")]
    public int swordDamage = 8; 
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer;

    [Header("Combo System")]
    [SerializeField] private bool enableComboSystem = true;
    [SerializeField] private float comboWindow = 0.35f;
    [SerializeField] private int maxComboSteps = 3;
    private int comboStep = 0;
    private float comboTimer = 0f;
    private bool hitConfirmedThisStep = false;

    [Header("Hit Confirmation")]
    [SerializeField] private HitConfirmBroadcaster hitBroadcaster;

    [Header("Wave of Light")]
    public GameObject waveOfLightPrefab;
    public Transform firePoint;
    public float waveOfLightCooldown = 5f;

    [Header("Attack Point")]
    public Transform attackPoint;

    private float lastAttackTime = 0f;
    private float lastWaveOfLightTime = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private AnimatorDriver animDriver;
    private Abilities abilities;

    private bool isAttacking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        animDriver = GetComponent<AnimatorDriver>();
    }

    void Start()
    {
        abilities = GetComponent<Abilities>();
        
        // Register with HitConfirmBroadcaster if available
        if (hitBroadcaster == null)
            hitBroadcaster = GetComponentInChildren<HitConfirmBroadcaster>();
        
        if (hitBroadcaster == null)
            hitBroadcaster = GetComponentInParent<HitConfirmBroadcaster>();
        
        if (hitBroadcaster != null && enableComboSystem)
        {
            hitBroadcaster.Register(this);
            Debug.Log("✅ PlayerAttack registered with HitConfirmBroadcaster - Combo system enabled!");
        }
        else if (enableComboSystem)
        {
            Debug.LogWarning("⚠️ HitConfirmBroadcaster not found - Combo system disabled!");
            enableComboSystem = false;
        }
        
        Debug.Log($"PlayerAttack started with damage: {swordDamage}");
    }

    void OnDestroy()
    {
        if (hitBroadcaster != null)
            hitBroadcaster.Unregister(this);
    }

    void Update()
    {
        // Combo timer countdown
        if (enableComboSystem && comboStep > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                ResetCombo();
        }

        // Attack (X key)
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (enableComboSystem)
            {
                RegisterComboAttack();
            }
            else
            {
                // Simple attack (no combo)
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;
                }
            }
        }

        // Wave of Light (C key)
        if (Input.GetKeyDown(KeyCode.C) && Time.time >= lastWaveOfLightTime + waveOfLightCooldown)
        {
            if (abilities != null && abilities.hasWaveOfLight)
            {
                ShootWaveOfLight();
                lastWaveOfLightTime = Time.time;
            }
        }
    }

    #region Combo System
    
    private void RegisterComboAttack()
    {
        if (comboStep == 0)
        {
            // Start new combo
            comboStep = 1;
            comboTimer = comboWindow;
            hitConfirmedThisStep = false;
            isAttacking = true;
            
            Attack();
            
            if (animDriver != null)
            {
                animDriver.anim.SetInteger("ComboStep", comboStep);
            }
        }
        else if (comboTimer > 0f)
        {
            // Continue combo only if previous hit confirmed
            if (!hitConfirmedThisStep)
            {
                Debug.Log("⚠️ Combo blocked - previous hit missed!");
                return;
            }
            
            hitConfirmedThisStep = false;
            comboStep = Mathf.Clamp(comboStep + 1, 1, maxComboSteps);
            comboTimer = comboWindow;
            isAttacking = true;
            
            Attack();
            
            if (animDriver != null)
            {
                animDriver.anim.SetInteger("ComboStep", comboStep);
            }
            
            Debug.Log($"🎯 Combo step {comboStep}!");
        }
    }

    private void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;
        hitConfirmedThisStep = false;
        isAttacking = false;
        
        if (animDriver != null)
        {
            animDriver.anim.SetInteger("ComboStep", 0);
        }
        
        Debug.Log("🔄 Combo reset");
    }

    // Called by HitConfirmBroadcaster when attack hits enemy
    public void OnHitConfirmed(GameObject target)
    {
        if (!isAttacking || !enableComboSystem) return;
        
        hitConfirmedThisStep = true;
        Debug.Log($"✅ Hit confirmed on {target.name} - combo can continue!");
    }

    #endregion

    void Attack()
    {
        if (attackPoint == null)
        {
            Debug.LogError("AttackPoint is NULL!");
            return;
        }

        // Trigger animation
        if (animDriver != null)
            animDriver.Attack();
        else if (anim != null)
            anim.SetTrigger("Attack");

        // Deal damage
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            DealDamageToEnemy(enemy);
        }
    }

    void DealDamageToEnemy(Collider2D enemy)
    {
        EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
        if (enemyScript != null)
        {
            Vector2 knockDir = (enemy.transform.position - transform.position).normalized;
            enemyScript.TakeDamage(swordDamage, knockDir);
            
            // Notify hit confirmation system
            if (hitBroadcaster != null && enableComboSystem)
            {
                hitBroadcaster.NotifyHit(enemy.gameObject);
            }
            
            Debug.Log($"⚔️ Hit {enemy.name} for {swordDamage} damage!");
            return;
        }

        BossBase bossScript = enemy.GetComponent<BossBase>();
        if (bossScript != null)
        {
            bossScript.TakeDamage(swordDamage);
            
            // Notify hit confirmation system
            if (hitBroadcaster != null && enableComboSystem)
            {
                hitBroadcaster.NotifyHit(enemy.gameObject);
            }
            
            Debug.Log($"⚔️ Hit boss {enemy.name} for {swordDamage} damage!");
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

        // Trigger animation
        if (animDriver != null)
            animDriver.WaveOfLight();
        else if (anim != null)
            anim.SetTrigger("WaveOfLight");

        // Spawn projectile
        GameObject wave = Instantiate(waveOfLightPrefab, firePoint.position, Quaternion.identity);

        WaveOfLightProjectile projectile = wave.GetComponent<WaveOfLightProjectile>();
        if (projectile != null)
        {
            projectile.direction = new Vector2(Mathf.Sign(transform.localScale.x), 0f);

            // Damage is 5x normal sword damage
            projectile.damage = swordDamage * 5;
        }

        Debug.Log($"✨ Wave of Light fired! Damage: {swordDamage * 5}");
    }

    #region Upgrade Methods (Called by StoreController)
    
    /// <summary>
    /// Increase sword damage by a fixed amount
    /// Called by StoreController when purchasing damage upgrade
    /// </summary>
    public void IncreaseDamage(int amount)
    {
        swordDamage += amount;
        Debug.Log($"⚔️ Damage increased by {amount}. New damage: {swordDamage}");
    }

    /// <summary>
    /// Multiply sword damage
    /// Called by StoreController when purchasing Sword of Light
    /// </summary>
    public void MultiplyDamage(int multiplier)
    {
        swordDamage *= multiplier;
        Debug.Log($"⚔️ Damage multiplied by {multiplier}. New damage: {swordDamage}");
    }
    
    #endregion

    void OnDrawGizmosSelected()
    {
        // Attack range (red)
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}