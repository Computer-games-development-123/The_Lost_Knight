using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossBase : MonoBehaviour
{
    [Header("Boss Stats")]
    public string bossName = "Boss";
    public int maxHP = 100;
    public int damage = 10;
    public float moveSpeed = 3f;
    public bool isInvulnerable = false;

    [Header("Health Bar Asset")]
    [SerializeField] private Image healthFill;

    [Header("Dialogues")]
    public DialogueData spawnDialogue;
    public DialogueData deathDialogue;

    [Header("References")]
    public WaveManager waveManager;
    public Transform player;

    protected int currentHP;
    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer spriteRenderer;
    protected bool isDead = false;
    protected bool isPhase2 = false;

    protected virtual void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        OnBossStart();
        if (DialogueManager.Instance != null && spawnDialogue != null)
        {
            DialogueManager.Instance.Play(spawnDialogue);
        }
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;

        BossAI();

        // Check for phase 2
        if (currentHP <= maxHP / 2 && !isPhase2)
        {
            EnterPhase2();
        }
        Refresh();
    }

    private void Refresh()
    {
        if (healthFill != null)
        {
            float maxHp = Mathf.Max(1f, maxHP);
            healthFill.fillAmount = currentHP / maxHp;
        }
    }

    protected virtual void OnBossStart()
    {
        // Override in specific boss scripts
        Debug.Log($"{bossName} battle started!");
    }

    protected virtual void BossAI()
    {
        // Basic AI - move toward player
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

            if (direction.x > 0)
                spriteRenderer.flipX = false;
            else if (direction.x < 0)
                spriteRenderer.flipX = true;
        }
    }

    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        if (isInvulnerable)
        {
            Debug.Log($"{bossName} is invulnerable!");
            OnInvulnerableHit();
            return;
        }

        currentHP -= damageAmount;

        if (anim != null)
            anim.SetTrigger("Hurt");

        Debug.Log($"{bossName} took {damageAmount} damage. HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void OnInvulnerableHit()
    {
        // Override for special invulnerability logic (like George's first encounter)
    }

    protected virtual void EnterPhase2()
    {
        isPhase2 = true;
        moveSpeed *= 1.5f;
        damage = Mathf.RoundToInt(damage * 1.3f);
        Debug.Log($"{bossName} entered Phase 2!");
    }

    protected virtual void Die()
    {
        isDead = true;

        if (anim != null)
            anim.SetTrigger("Death");

        Debug.Log($"{bossName} defeated!");

        // Notify WaveManager
        if (waveManager != null)
        {
            waveManager.OnBossDied(this);
        }

        Destroy(gameObject, 3f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamageToPlayer(collision.gameObject);
        }
    }

    // Helper method to deal damage to player - can be called from collision or attacks
    protected void DealDamageToPlayer(GameObject playerObject)
    {
        CharacterStats playerHealth = playerObject.GetComponent<CharacterStats>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage, transform.position);
        }
    }
}
