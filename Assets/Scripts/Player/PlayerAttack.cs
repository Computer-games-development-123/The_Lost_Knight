using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int swordDamage = 8;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Input")]
    [SerializeField] private KeyCode attackKey = KeyCode.X;

    [Header("Combo (old logic, no listeners)")]
    [SerializeField] private bool enableComboSystem = true;
    [SerializeField] private float comboWindow = 0.35f;
    [SerializeField] private int maxComboSteps = 3;

    [Header("Animator Params (your animator)")]
    [SerializeField] private string attackTriggerName = "Attack";         // Trigger
    [SerializeField] private string jumpAttackTriggerName = "JumpAttack"; // Trigger
    [SerializeField] private string comboIntName = "Combo";               // int

    // [Header("Movement During Attack (optional)")]
    // [SerializeField] private bool lockMovementDuringAttack = false;
    // [SerializeField, Range(0f, 1f)] private float attackMoveMultiplier = 0.2f;

    [Header("Wave of Fire")]
    public GameObject waveOfFirePrefab;
    public Transform firePoint;
    public float waveOfFireCooldown = 5f;

    [Header("Attack Point")]
    public Transform attackPoint;

    // Runtime (same as old)
    private int comboStep = 0;
    private float comboTimer = 0f;
    private bool hitConfirmedThisStep = false;
    private bool isAttacking = false;

    private float lastWaveOfFireTime = 0f;

    private Animator anim;
    private Abilities abilities;
    private PlayerController movement;
    private bool grounded => (movement != null) ? movement.isGrounded : true;

    void Awake()
    {
        anim = GetComponent<Animator>();
        abilities = GetComponent<Abilities>();
        movement = GetComponent<PlayerController>(); // optional
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            if (enableComboSystem) RegisterAttackInput();
            else StartAttackAnim(1);
        }

        if (enableComboSystem && comboStep > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                ResetCombo();
        }

        if (Input.GetKeyDown(KeyCode.C) && Time.time >= lastWaveOfFireTime + waveOfFireCooldown)
        {
            if (abilities != null && abilities.hasWaveOfFire)
            {
                ShootWaveOfFire();
                lastWaveOfFireTime = Time.time;
            }
        }
    }

    // =========================
    // Combo logic
    // =========================
    private void RegisterAttackInput()
    {
        if (anim == null) return;

        if (comboStep == 0)
        {
            comboStep = 1;
            comboTimer = comboWindow;
            hitConfirmedThisStep = false;

            StartAttackAnim(comboStep);
            return;
        }

        if (comboTimer > 0f)
        {
            // must confirm hit from previous step
            if (!hitConfirmedThisStep)
                return;

            hitConfirmedThisStep = false;

            comboStep = Mathf.Clamp(comboStep + 1, 1, maxComboSteps);
            comboTimer = comboWindow;

            StartAttackAnim(comboStep);
        }
    }

    private void StartAttackAnim(int step)
    {
        isAttacking = true;

        // if (movement != null)
        // {
        //     if (lockMovementDuringAttack)
        //         movement.MovementLocked = true;
        //     else
        //         movement.MovementMultiplier = attackMoveMultiplier;
        // }

        // Update combo int first
        anim.SetInteger(comboIntName, step);

        // Choose ground vs air trigger
        if (!grounded)
        {
            // Air attack
            anim.SetTrigger(jumpAttackTriggerName);
        }
        else
        {
            // Ground attack
            anim.SetTrigger(attackTriggerName);
        }
    }

    private void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;
        hitConfirmedThisStep = false;
        isAttacking = false;

        if (anim != null)
            anim.SetInteger(comboIntName, 0);

        // if (movement != null)
        // {
        //     movement.MovementLocked = false;
        //     movement.MovementMultiplier = 1f;
        // }

        Debug.Log("🔄 Combo reset");
    }

    public void DealDamage()
    {
        if (!isAttacking) return;

        if (attackPoint == null)
        {
            Debug.LogError("DealDamage called but AttackPoint is NULL!");
            return;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            foreach (Collider2D enemy in hitEnemies)
                DealDamageToEnemy(enemy);

            hitConfirmedThisStep = true;

            Debug.Log($"✅ DealDamage hit {hitEnemies.Length} target(s) - combo can continue!");
        }
        else
        {
            Debug.Log("💨 DealDamage missed - no enemies in range");
        }
    }

    public void OnAttackEnd()
    {
        // if (movement != null)
        // {
        //     movement.MovementLocked = false;
        //     movement.MovementMultiplier = 1f;
        // }

        isAttacking = false;

        if (comboTimer <= 0f)
            ResetCombo();

        Debug.Log("📢 OnAttackEnd animation event called");
    }

    private void DealDamageToEnemy(Collider2D enemy)
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
    // Upgrade Methods
    // =========================
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
