using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 9f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("I-Frames")]
    public float iFrameDuration = 1.5f;

    [Header("Abilities")]
    public float teleportDistance = 4.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float lastAttackTime;
    private bool isInvulnerable;
    private float invulnerabilityTimer;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Movement Input
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Flip Sprite
        if (moveInput > 0 && !facingRight)
            Flip();
        else if (moveInput < 0 && facingRight)
            Flip();

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Use Potion
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.Instance?.UsePotion();
        }

        // Teleport Ability
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

        // Animations
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveInput));
            anim.SetBool("IsGrounded", isGrounded);
        }
    }

    public Vector2 facingDir()
    {
        return facingRight ? Vector2.right : Vector2.left;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvulnerable) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentHP -= damageAmount;
            isInvulnerable = true;
            invulnerabilityTimer = iFrameDuration;

            if (anim != null)
                anim.SetTrigger("Hurt");

            Debug.Log($"Player took {damageAmount} damage. HP: {GameManager.Instance.currentHP}/{GameManager.Instance.maxHP}");

            if (GameManager.Instance.currentHP <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        if (anim != null)
            anim.SetTrigger("Death");
        
        Debug.Log("Player died!");
        
        if (GameManager.Instance != null)
        {
            Invoke("CallGameManagerDeath", 2f);
        }
    }

    void CallGameManagerDeath()
    {
        GameManager.Instance?.OnPlayerDied();
    }

    void Teleport()
    {
        Vector2 teleportDirection = facingRight ? Vector2.right : Vector2.left;
        Vector2 newPosition = (Vector2)transform.position + (teleportDirection * teleportDistance);
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, teleportDirection, teleportDistance, groundLayer);
        
        if (hit.collider == null)
        {
            transform.position = newPosition;
            isInvulnerable = true;
            invulnerabilityTimer = iFrameDuration;
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