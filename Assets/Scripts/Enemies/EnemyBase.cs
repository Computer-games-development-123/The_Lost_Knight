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

    [Header("References")]
    public WaveManager waveManager;

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

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        // Initialize HP
        currentHP = MaxHP;

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Could not find Player!");
        }

        // Apply visuals from EnemyData if available
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
        if (isDead || player == null) return;

        // Execute behavior based on type
        if (enemyData != null)
        {
            ExecuteBehavior(enemyData.behaviorType);
        }
        else
        {
            // Default to walker behavior
            WalkerBehavior();
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
        // Base movement
        WalkerBehavior();
        
        // TODO: Add jump logic
        // Example: Jump when close to player
    }

    protected virtual void RangedBehavior()
    {
        // TODO: Implement ranged attack
        // Stay at distance and shoot
    }

    protected virtual void EliteBehavior()
    {
        // Elite enemies are stronger walkers
        WalkerBehavior();
    }

    protected void UpdateFacing(float directionX)
    {
        if (spriteRenderer == null) return;
        
        if (directionX > 0)
            spriteRenderer.flipX = false;
        else if (directionX < 0)
            spriteRenderer.flipX = true;
    }

    #endregion

    #region Damage System

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        
        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHP}/{MaxHP}");

        // Play hurt animation
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        // Visual feedback - flash red
        StartCoroutine(FlashRed());

        if (currentHP <= 0)
        {
            Die();
        }
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

        // Stop movement
        rb.linearVelocity = Vector2.zero;
        
        // Disable collider to prevent further interactions
        if (col != null)
        {
            col.enabled = false;
        }

        // Play death animation
        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        // Drop coins
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(CoinsDropped);
        }

        // Notify WaveManager
        if (waveManager != null)
        {
            waveManager.OnEnemyDied(this);
        }

        // Destroy after delay (for death animation)
        Destroy(gameObject, 1f);
    }

    #endregion

    #region Collision - Player Damage

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DealContactDamage(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Optional: Deal damage while staying in contact
        // DealContactDamage(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DealContactDamage(collision.gameObject);
    }

    private void DealContactDamage(GameObject other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            // Try PlayerController first
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(Damage);
                Debug.Log($"{gameObject.name} dealt {Damage} contact damage to Player!");
                return;
            }

            // Fallback to PlayerHealth
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Damage);
                Debug.Log($"{gameObject.name} dealt {Damage} contact damage to Player (via PlayerHealth)!");
                return;
            }

            Debug.LogWarning($"{gameObject.name}: Player has no damage-receiving component!");
        }
    }

    #endregion

    #region Debug

    protected virtual void OnDrawGizmosSelected()
    {
        // Visualize enemy detection range (optional)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }

    #endregion
}