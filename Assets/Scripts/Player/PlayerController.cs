using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 20f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("I-Frames")]
    public float iFrameDuration = 1.5f;

    [Header("Screen Clamp")]
    public float edgeBuffer = 0.5f;
    private Camera cam;
    private float halfHeight;
    private float halfWidth;

    [Header("Knockback")]
    public float knockbackForce = 7f;
    public float knockbackUpForce = 2f;
    public float knockbackDuration = 0.15f;
    private bool isKnockback = false;

    [Header("Abilities")]
    public float teleportDistance = 4.5f;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private bool isInvulnerable;
    private float invulnerabilityTimer;
    private bool facingRight = true;
    private PlayerHealth playerHealth;

    [Header("Potions")]
    [SerializeField] private float healAmountPerPotion = 10f;

    public bool IsInvulnerable => isInvulnerable;
    public bool IsGrounded => isGrounded;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        cam = Camera.main;
    }

    void Update()
    {
        // Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float moveInput = 0f;

        // === ONLY control movement when NOT in knockback ===
        if (!isKnockback)
        {
            // Movement Input
            moveInput = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

            // Flip Sprite
            if (moveInput > 0 && !facingRight)
                Flip();
            else if (moveInput < 0 && facingRight)
                Flip();

            // Jump (blocked during knockback)
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }

        // Use Potion (allowed during knockback)
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (GameManager.Instance != null && playerHealth != null)
            {
                // Only heal if we actually have potions
                if (GameManager.Instance.HasPotions())
                {
                    // Optional: don’t waste potions at full HP
                    if (!playerHealth.IsAtFullHealth)
                    {
                        GameManager.Instance.UsePotion();              // consume 1 potion
                        playerHealth.Heal(healAmountPerPotion);        // restore HP
                        Debug.Log("Used a potion to heal the player.");
                    }
                    else
                    {
                        Debug.Log("HP already full, not using a potion.");
                    }
                }
                else
                {
                    Debug.Log("No potions left!");
                }
            }
        }


        // Teleport Ability (you can decide if you want to block this during knockback)
        if (Input.GetKeyDown(KeyCode.LeftAlt) && GameManager.Instance != null && GameManager.Instance.hasTeleport)
        {
            Teleport();
        }

        // Update I-Frames
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0)
            {
                isInvulnerable = false;
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.white;
            }
            else
            {
                // Flicker effect
                if (spriteRenderer != null)
                {
                    float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                    spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
                }
            }
        }

        // Animations – use actual velocity, not input
        if (anim != null)
        {
            float speed = Mathf.Abs(rb.linearVelocity.x);
            anim.SetFloat("Speed", speed);
            anim.SetBool("IsGrounded", isGrounded);
        }
    }

    void LateUpdate()
    {
        KeepPlayerInsideScreen();
    }

    private void KeepPlayerInsideScreen()
    {
        if (cam == null) return;

        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        Vector3 camPos = cam.transform.position;
        Vector3 pos = transform.position;

        float minX = camPos.x - halfWidth + edgeBuffer;
        float maxX = camPos.x + halfWidth - edgeBuffer;
        float minY = camPos.y - halfHeight + edgeBuffer;
        float maxY = camPos.y + halfHeight - edgeBuffer;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }

    public Vector2 facingDir()
    {
        return facingRight ? Vector2.right : Vector2.left;
    }

    public void TakeDamage(int damageAmount)
    {
        // Optional: respect i-frames
        if (isInvulnerable)
            return;

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
        }
        else
        {
            Debug.LogWarning("PlayerHealth component not found!");
            return;
        }

        // Hurt feedback
        PlayHurtAnimation();
        TriggerInvulnerability();
    }

    public void TakeDamage(int damageAmount, Vector2 hitSourcePosition)
    {
        // Re-use normal damage logic
        TakeDamage(damageAmount);

        // Then apply knockback
        ApplyKnockback(hitSourcePosition);
    }

    public void TriggerInvulnerability()
    {
        isInvulnerable = true;
        invulnerabilityTimer = iFrameDuration;
    }

    public void ApplyKnockback(Vector2 hitSourcePosition)
    {
        // Direction from hit source to player
        Vector2 dir = ((Vector2)transform.position - hitSourcePosition).normalized;

        // Ensure horizontal push
        if (Mathf.Abs(dir.x) < 0.1f)
            dir.x = facingRight ? 1f : -1f;

        StartCoroutine(KnockbackRoutine(dir));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockback = true;

        // Cancel current motion first
        rb.linearVelocity = Vector2.zero;

        // Apply impulse knockback (big difference!)
        rb.AddForce(new Vector2(direction.x * knockbackForce, knockbackUpForce), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockback = false;
    }

    public void PlayHurtAnimation()
    {
        if (anim != null)
            anim.SetTrigger("Hurt");
    }

    public void PlayDeathAnimation()
    {
        if (anim != null)
            anim.SetTrigger("Death");
    }

    void Teleport()
    {
        Vector2 teleportDirection = facingRight ? Vector2.right : Vector2.left;
        Vector2 newPosition = (Vector2)transform.position + (teleportDirection * teleportDistance);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, teleportDirection, teleportDistance, groundLayer);

        if (hit.collider == null)
        {
            transform.position = newPosition;
            TriggerInvulnerability();
            Debug.Log("Teleported!");
        }
        else
        {
            Debug.Log("Can't teleport - obstacle in the way!");
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
