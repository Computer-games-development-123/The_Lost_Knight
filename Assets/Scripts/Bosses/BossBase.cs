using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossBase : MonoBehaviour
{
    [Header("Boss Stats")]
    public string bossName = "Boss";
    public int maxHP = 100;
    public int damage = 10;
    public float moveSpeed = 3f;
    public bool isInvulnerable = false;

    [Header("Rewards")]
    [Tooltip("Coins awarded to player when this boss is defeated")]
    public int coinsReward = 100;

    [Header("Dialogues")]
    public DialogueData spawnDialogue;
    public DialogueData deathDialogue;
    public DialogueData slainDialogue;

    [Header("References")]
    public WaveManager waveManager;
    public Transform player;

    [SerializeField] protected int currentHP;
    protected SpriteRenderer sr;
    protected Rigidbody2D rb;
    protected Animator anim;
    protected bool isDead = false;
    protected bool isPhase2 = false;
    public bool facingRight = true;
    public int CurrentHP => currentHP;

    protected virtual void Start()
    {
        currentHP = maxHP;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        OnBossStart();

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
    }

    protected virtual void OnBossStart()
    {
        Debug.Log($"{bossName} battle started!");
        DialogueManager.Instance.Play(spawnDialogue);
    }

    protected virtual void BossAI()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

            if (direction.x > 0 && !facingRight)
                Flip();
            else if (direction.x < 0 && facingRight)
                Flip();

        }
    }
    protected virtual void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
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

        StartCoroutine(FlashRed());
        Debug.Log($"{bossName} took {damageAmount} damage. HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected IEnumerator FlashRed()
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }
    }

    protected virtual void OnInvulnerableHit()
    {
        StartCoroutine(FlashRed());
        Debug.Log($"{bossName} is invulnerable - no damage taken!");
    }

    protected virtual void EnterPhase2()
    {
        isPhase2 = true;
        moveSpeed *= 1.5f;
        damage = Mathf.RoundToInt(damage * 1.3f);
        Debug.Log($"{bossName} entered Phase 2!");
    }

    protected virtual void DealDamageToPlayer()
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamage(damage);
    }

    protected virtual void Die()
    {
        isDead = true;

        if (anim != null)
            anim.SetTrigger("Death");

        Debug.Log($"{bossName} defeated!");

        if (waveManager != null)
        {
            waveManager.OnBossDied(this);
        }

        if (DialogueManager.Instance != null && deathDialogue != null)
        {
            DialogueManager.Instance.Play(deathDialogue, OnDeathDialogueComplete);
        }
        else
        {
            OnDeathDialogueComplete();
        }
        Destroy(gameObject, 3f);
    }

    protected virtual void OnDeathDialogueComplete()
    {
        // Award coins to player
        if (player != null && coinsReward > 0)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddCoins(coinsReward);
                Debug.Log($"Player received {coinsReward} coins for defeating {bossName}!");
            }
            else
            {
                Debug.LogWarning($"PlayerInventory component not found on player - could not award {coinsReward} coins!");
            }
        }

        // Play slain dialogue if available
        if (DialogueManager.Instance != null && slainDialogue != null)
        {
            DialogueManager.Instance.Play(slainDialogue);
        }

        Destroy(gameObject, 2f);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }


}