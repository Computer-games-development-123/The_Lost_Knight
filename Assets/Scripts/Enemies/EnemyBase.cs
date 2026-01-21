using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Settings")]
    public EnemyData enemyData;

    [Header("Fallback Stats (if no EnemyData)")]
    public int fallbackMaxHP = 10;
    public int fallbackDamage = 5;
    public float fallbackMoveSpeed = 2f;
    public int fallbackCoinsDropped = 1;

    [Header("Knockback Settings")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.12f;
    private bool isKnocked = false;

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

    [Header("Sprite direction")]
    public bool isFacingLeft = false;

    [Header("waiting for entring animation")]
    public bool hasEntringAnimation = false;

    // Runtime variables
    protected int currentHP;
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Transform player;
    protected bool isDead = false;
    protected SpriteRenderer spriteRenderer;
    protected Collider2D col;

    // Properties for easy access
    public int MaxHP => enemyData != null ? enemyData.maxHP : fallbackMaxHP;
    public int Damage => enemyData != null ? enemyData.damage : fallbackDamage;
    public float MoveSpeed => enemyData != null ? enemyData.moveSpeed : fallbackMoveSpeed;
    public int CoinsDropped => enemyData != null ? enemyData.coinsDropped : fallbackCoinsDropped;

    // Ground check for jumper
    protected bool IsGrounded => Physics2D.OverlapCircle(
        transform.position + Vector3.down * 0.5f,
        0.1f,
        LayerMask.GetMask("Ground")
    );

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        if (hasEntringAnimation) return;
        currentHP = MaxHP;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Could not find Player!");
        }

        if (enemyData != null)
        {
            if (enemyData.enemySprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = enemyData.enemySprite;
            }
            if (enemyData.animatorController != null && anim != null)
            {
                anim.runtimeAnimatorController = enemyData.animatorController;
            }
        }

        Debug.Log($"{gameObject.name} spawned with {currentHP} HP");
    }

    protected virtual void Update()
    {
        if (isDead || player == null || isKnocked) return;

        if (enemyData != null)
        {
            ExecuteBehavior(enemyData.behaviorType);
        }
        else
        {
            WalkerBehavior();
        }

        UpdateAnimations();
    }

    public void OnEntringAnimationEnd()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        hasEntringAnimation = false;
        Start();
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;

        float speed = Mathf.Abs(rb.linearVelocity.x);
        anim.SetFloat("Speed", speed);
        anim.SetBool("IsMoving", speed > 0.1f);

        if (enemyData != null && enemyData.behaviorType == EnemyData.EnemyBehaviorType.Jumper)
        {
            anim.SetBool("IsGrounded", IsGrounded);
        }
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
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * MoveSpeed, rb.linearVelocity.y);

        UpdateFacing(direction.x);
    }

    protected virtual void FastWalkerBehavior()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * MoveSpeed * 1.5f, rb.linearVelocity.y);

        UpdateFacing(direction.x);
    }

    protected virtual void JumperBehavior()
    {
        WalkerBehavior();

        if (player != null && IsGrounded)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            bool canJump = distanceToPlayer < 5f
                        && Time.time >= lastJumpTime + jumperJumpCooldown
                        && Random.value > 0.90f;

            if (canJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumperJumpForce);
                lastJumpTime = Time.time;
                Debug.Log($"{gameObject.name} jumped!");
            }
        }
    }

    protected virtual void RangedBehavior()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        float optimalMinRange = 5f;
        float optimalMaxRange = 7f;

        if (distanceToPlayer < optimalMinRange)
        {
            Vector2 direction = (transform.position - player.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * MoveSpeed * 1.2f, rb.linearVelocity.y);
            UpdateFacing(direction.x);
        }
        else if (distanceToPlayer > optimalMaxRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * MoveSpeed * 0.8f, rb.linearVelocity.y);
            UpdateFacing(direction.x);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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

        Vector3 spawnPos;
        if (projectileSpawnPoint != null)
        {
            spawnPos = projectileSpawnPoint.position;
        }
        else
        {
            spawnPos = transform.position + Vector3.up * 0.5f;
        }

        Vector2 direction = (player.position - spawnPos).normalized;

        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        int projectileLayer = LayerMask.NameToLayer("EnemyProjectile");
        if (projectileLayer != -1)
        {
            projectile.layer = projectileLayer;
        }

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * projectileSpeed;
        }

        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.damage = Damage;
        }

        Debug.Log($"{gameObject.name} shot projectile from {spawnPos} toward player!");

        Destroy(projectile, 5f);
    }

    protected virtual void EliteBehavior()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * MoveSpeed * 1.2f, rb.linearVelocity.y);

        UpdateFacing(direction.x);
    }

    protected void UpdateFacing(float directionX)
    {
        if (spriteRenderer == null) return;
        if (isFacingLeft) directionX *= -1;
        if (directionX > 0)
            spriteRenderer.flipX = false;
        else if (directionX < 0)
            spriteRenderer.flipX = true;
    }

    #endregion

    #region Damage System
    public virtual void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (isDead) return;

        currentHP -= damage;

        if (anim != null)
            anim.SetTrigger("Hurt");

        StartCoroutine(FlashRed());
        StartCoroutine(ApplyKnockback(hitDirection));

        if (currentHP <= 0)
            Die();
    }

    private System.Collections.IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnocked = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector2.zero;
        isKnocked = false;
    }

    protected System.Collections.IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} died!");

        rb.linearVelocity = Vector2.zero;

        if (col != null)
        {
            col.enabled = false;
        }

        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        // Drop coins - give to player's inventory
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerInventory inventory = playerObj.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddCoins(CoinsDropped);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Player has no PlayerInventory component!");
            }
        }

        if (waveManager != null)
        {
            waveManager.OnEnemyDied(this);
        }

        Destroy(gameObject, 1f);
    }

    #endregion

    #region Collision - Player Damage

    private float lastDamageTime = 0f;
    private float damageCooldown = 0.5f; // Only damage player every 0.5 seconds

    private void OnCollisionStay2D(Collision2D collision)
    {
        DealContactDamage(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        DealContactDamage(collision.gameObject);
    }

    private void DealContactDamage(GameObject other)
    {
        if (isDead) return;

        // Cooldown check to prevent spamming damage every frame
        if (Time.time < lastDamageTime + damageCooldown)
            return;

        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(Damage, transform.position);
                lastDamageTime = Time.time;
                Debug.Log($"{gameObject.name} dealt {Damage} contact damage to Player!");
                return;
            }

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Damage);
                lastDamageTime = Time.time;
                Debug.Log($"{gameObject.name} dealt {Damage} contact damage to Player!");
                return;
            }
        }
    }

    #endregion

    #region Debug

    protected virtual void OnDrawGizmosSelected()
    {
        if (enemyData != null && enemyData.behaviorType == EnemyData.EnemyBehaviorType.Ranged)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 5f);
            Gizmos.DrawWireSphere(transform.position, 7f);

            if (projectileSpawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(projectileSpawnPoint.position, 0.2f);
            }
            else
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.2f);
            }
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 5f);
        }
    }

    #endregion
}