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

    [Header("Dialogues")]
    public DialogueData spawnDialogue;
    public DialogueData deathDialogue;

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

    // Health bar UI (auto-found)
    // private Image healthBarFill;
    // private TextMeshProUGUI bossNameText;
    // private GameObject healthBarCanvas;

    // Public property for external access
    public int CurrentHP => currentHP;

    protected virtual void Start()
    {
        currentHP = maxHP;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Auto-find health bar UI in scene
        //FindHealthBarUI();

        OnBossStart();

        // Show health bar
        // ShowHealthBar();

        // if (DialogueManager.Instance != null && spawnDialogue != null)
        // {
        //     DialogueManager.Instance.Play(spawnDialogue);
        // }
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

        // Update health bar
        //UpdateHealthBar();

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
        // Override for special invulnerability logic
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

        // Hide health bar
        //HideHealthBar();

        if (anim != null)
            anim.SetTrigger("Death");

        Debug.Log($"{bossName} defeated!");

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
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }

    #region Health Bar Methods

    /// <summary>
    /// Auto-find health bar UI in the scene
    /// Looks for BossHealthCanvas or BossHealthBar objects
    /// </summary>
    // private void FindHealthBarUI()
    // {
    //     // Try to find BossHealthCanvas
    //     GameObject canvas = GameObject.Find("BossHealthCanvas");
    //     if (canvas == null)
    //         canvas = GameObject.Find("BossHealthBar");

    //     if (canvas != null)
    //     {
    //         healthBarCanvas = canvas;

    //         // Find Fill image (look for Image with fillAmount)
    //         Image[] images = canvas.GetComponentsInChildren<Image>(true);
    //         foreach (Image img in images)
    //         {
    //             // Look for the fill bar (has Image Type = Filled)
    //             if (img.type == Image.Type.Filled && img.name.ToLower().Contains("fill"))
    //             {
    //                 healthBarFill = img;
    //                 Debug.Log($"✅ Found health bar fill: {img.name}");
    //                 break;
    //             }
    //         }

    //         // Find boss name text
    //         TextMeshProUGUI[] texts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
    //         foreach (TextMeshProUGUI text in texts)
    //         {
    //             // Look for text named BossName or similar
    //             if (text.name.ToLower().Contains("name") || text.name.ToLower().Contains("boss"))
    //             {
    //                 bossNameText = text;
    //                 Debug.Log($"✅ Found boss name text: {text.name}");
    //                 break;
    //             }
    //         }

    //         if (healthBarFill == null)
    //             Debug.LogWarning("⚠️ Could not find health bar Fill image!");

    //         if (bossNameText == null)
    //             Debug.LogWarning("⚠️ Could not find boss name text!");
    //     }
    //     else
    //     {
    //         Debug.LogError("❌ Could not find BossHealthCanvas in scene!");
    //     }
    // }

    // private void ShowHealthBar()
    // {
    //     // Set boss name
    //     if (bossNameText != null)
    //     {
    //         bossNameText.text = bossName;
    //     }

    //     // Set health to full
    //     if (healthBarFill != null)
    //     {
    //         healthBarFill.fillAmount = 1f;
    //     }

    //     // Show canvas
    //     if (healthBarCanvas != null)
    //     {
    //         healthBarCanvas.SetActive(true);
    //         Debug.Log($"🌟 Health bar shown for {bossName}");
    //     }
    // }

    // private void UpdateHealthBar()
    // {
    //     if (healthBarFill != null)
    //     {
    //         float fillAmount = (float)currentHP / maxHP;
    //         healthBarFill.fillAmount = fillAmount;
    //     }
    // }

    // private void HideHealthBar()
    // {
    //     if (healthBarCanvas != null)
    //     {
    //         healthBarCanvas.SetActive(false);
    //         Debug.Log("🚫 Health bar hidden");
    //     }
    // }

    #endregion
}