using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(CharacterContext))]
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Settings")]
    public EnemyData enemyData;

    [Header("Fallback Stats (if no EnemyData)")]
    public int fallbackMaxHP = 10;
    public int fallbackDamage = 5;
    public float fallbackMoveSpeed = 2f;
    public int fallbackCoinsDropped = 1;

    [Header("Jumper Settings")]
    public float jumperJumpForce = 15f;
    public float jumperJumpCooldown = 1.5f;
    private float lastJumpTime = 0f;

    [Header("Ranged Attack Settings (for Ranged type)")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float attackRange = 8f;
    public float attackCooldown = 2f;
    public float projectileSpeed = 8f;
    private float lastAttackTime = 0f;

    [Header("References")]
    public WaveManager waveManager;

    [Header("Runtime")]
    public CharacterContext ctx;
    public Transform player;
    protected Collider2D col;
    protected bool isDead = false;

    // Stats from data / fallback
    public int MaxHP => enemyData != null ? enemyData.baseMaxHP : fallbackMaxHP;
    public int Damage => enemyData != null ? enemyData.baseDamage : fallbackDamage;
    public float MoveSpeed => enemyData != null ? enemyData.moveSpeed : fallbackMoveSpeed;
    public int CoinsDropped => enemyData != null ? enemyData.coinsDropped : fallbackCoinsDropped;

    // Ground check for jumper
    protected bool IsGrounded => Physics2D.OverlapCircle(
        transform.position + Vector3.down * 0.5f,
        0.1f,
        LayerMask.GetMask("Ground")
    );

    protected void Awake()
    {
        ctx = GetComponent<CharacterContext>();
        col = GetComponent<Collider2D>();

        if (ctx == null || ctx.CS == null)
        {
            ctx.CS = GetComponent<CharacterStats>();
        }

        ApplyDataToCharacterStats();

        ctx.CS.OnDied += HandleDied;
    }

    protected virtual void OnDestroy()
    {
        if (ctx != null && ctx.CS != null)
            ctx.CS.OnDied -= HandleDied;
    }

    protected void Start()
    {
        if (ctx == null || ctx.CS == null) ctx = GetComponent<CharacterContext>();
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        else Debug.LogWarning($"{name}: Could not find Player!");

        Debug.Log($"{name} spawned with {ctx.CS.currentHP} HP");
    }

    private void ApplyDataToCharacterStats()
    {
        // CharacterStats already has data field, but EnemyData is separate.
        // We override runtime fields here so it's consistent.
        ctx.CS.currentHP = MaxHP;
        ctx.CS.damage = Damage;
    }

    protected virtual void Update()
    {
        if (player == null) Debug.LogWarning($"{name}: player is null");
        //if (isDead) Debug.LogWarning($"{name}: isDead true");

        if (isDead || player == null) return;

        if (ctx != null && ctx.KB != null && ctx.KB.isKnockback)
            return;

        if (enemyData != null)
            ExecuteBehavior(enemyData.behaviorType);
        else
            WalkerBehavior();
    }

    #region Behaviors

    protected virtual void ExecuteBehavior(EnemyData.EnemyBehaviorType behaviorType)
    {
        switch (behaviorType)
        {
            case EnemyData.EnemyBehaviorType.Walker:
                WalkerBehavior();
                break;
            case EnemyData.EnemyBehaviorType.FastWalker:
                FastWalkerBehavior();
                break;
            case EnemyData.EnemyBehaviorType.Jumper:
                JumperBehavior();
                break;
            case EnemyData.EnemyBehaviorType.Ranged:
                RangedBehavior();
                break;
            case EnemyData.EnemyBehaviorType.Elite:
                EliteBehavior();
                break;
        }
    }

    protected virtual void WalkerBehavior()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        if (ctx.RB.bodyType == RigidbodyType2D.Dynamic)
            ctx.RB.linearVelocity = new Vector2(direction.x * MoveSpeed, ctx.RB.linearVelocity.y);
        UpdateFacing(direction.x);
    }

    protected virtual void FastWalkerBehavior()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        ctx.RB.linearVelocity = new Vector2(direction.x * MoveSpeed * 1.5f, ctx.RB.linearVelocity.y);
        UpdateFacing(direction.x);
    }

    protected virtual void JumperBehavior()
    {
        WalkerBehavior();

        if (IsGrounded)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            bool canJump = distanceToPlayer < 5f
                           && Time.time >= lastJumpTime + jumperJumpCooldown
                           && Random.value > 0.90f;

            if (canJump)
            {
                ctx.RB.linearVelocity = new Vector2(ctx.RB.linearVelocity.x, jumperJumpForce);
                lastJumpTime = Time.time;
            }
        }
    }

    protected virtual void RangedBehavior()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        float optimalMinRange = 5f;
        float optimalMaxRange = 7f;

        if (distanceToPlayer < optimalMinRange)
        {
            Vector2 direction = (transform.position - player.position).normalized;
            ctx.RB.linearVelocity = new Vector2(direction.x * MoveSpeed * 1.2f, ctx.RB.linearVelocity.y);
            UpdateFacing(direction.x);
        }
        else if (distanceToPlayer > optimalMaxRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            ctx.RB.linearVelocity = new Vector2(direction.x * MoveSpeed * 0.8f, ctx.RB.linearVelocity.y);
            UpdateFacing(direction.x);
        }
        else
        {
            ctx.RB.linearVelocity = new Vector2(0, ctx.RB.linearVelocity.y);
        }

        if (Time.time >= lastAttackTime + attackCooldown && distanceToPlayer <= attackRange)
        {
            ShootProjectile();
            lastAttackTime = Time.time;
        }
    }

    protected virtual void ShootProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        Vector3 spawnPos = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position + Vector3.up * 0.5f;

        Vector2 direction = (player.position - spawnPos).normalized;

        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
            projRb.linearVelocity = direction * projectileSpeed;

        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
            projScript.damage = Damage;

        Destroy(projectile, 5f);
    }

    protected virtual void EliteBehavior()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        ctx.RB.linearVelocity = new Vector2(direction.x * MoveSpeed * 1.2f, ctx.RB.linearVelocity.y);
        UpdateFacing(direction.x);
    }

    protected void UpdateFacing(float directionX)
    {
        if (ctx == null) return;

        if (directionX > 0) ctx.SetFacing(true);
        else if (directionX < 0) ctx.SetFacing(false);
    }

    #endregion

    #region Death handling (from CharacterStats)

    private void HandleDied(CharacterStats stats)
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{name} died.");

        // Stop movement
        if (ctx != null && ctx.RB != null)
            ctx.RB.linearVelocity = Vector2.zero;

        // Disable collider
        if (col != null)
            col.enabled = false;

        // Report to waves
        if (waveManager != null)
            waveManager.OnEnemyDied(this);
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        playerStats.EarnCoins(enemyData.coinsDropped);
        // Destroy after animation time (adjust as needed)
        Destroy(gameObject, 1f);
    }

    #endregion

    #region Collision - Player Damage

    private void OnCollisionEnter2D(Collision2D collision) => DealContactDamage(collision.gameObject);
    private void OnTriggerEnter2D(Collider2D collision) => DealContactDamage(collision.gameObject);

    private void DealContactDamage(GameObject other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null)
                ps.TakeDamage(Damage, transform.position);
        }
    }

    #endregion
}
