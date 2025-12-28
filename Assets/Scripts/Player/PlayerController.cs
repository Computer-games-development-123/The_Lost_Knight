using UnityEngine;
using System.Collections;

/// <summary>
/// Player Controller - Movement, Jump, Knockback ONLY
/// NO teleport (use Abilities.cs)
/// NO i-frames (use Invulnerability.cs)
/// NO attack (use PlayerAttack.cs)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 20f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Knockback")]
    public float knockbackForce = 7f;
    public float knockbackUpForce = 2f;
    public float knockbackDuration = 0.15f;
    private bool isKnockback = false;

    [Header("Potions")]
    [SerializeField] private float healAmountPerPotion = 10f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private bool facingRight = true;
    private PlayerHealth playerHealth;
    private PlayerInventory playerInventory;
    private Invulnerability invulnerability;
    private AnimatorDriver animDriver;

    public bool IsGrounded => isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
        playerInventory = GetComponent<PlayerInventory>();
        invulnerability = GetComponent<Invulnerability>();
        animDriver = GetComponent<AnimatorDriver>();
    }

    void Update()
    {
        // Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // === ONLY control movement when NOT in knockback ===
        if (!isKnockback)
        {
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
                
                if (animDriver != null)
                    animDriver.Jump();
            }
        }

        // Use Potion (allowed during knockback)
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (playerInventory != null)
            {
                // PlayerInventory.UsePotion() handles all checks internally
                playerInventory.UsePotion(healAmountPerPotion);
            }
            else
            {
                Debug.LogWarning("⚠️ PlayerInventory not found!");
            }
        }

        // Animations
        if (anim != null)
        {
            float speed = Mathf.Abs(rb.linearVelocity.x);
            anim.SetFloat("Speed", speed);
            anim.SetBool("Grounded", isGrounded);
            anim.SetFloat("YVelocity", rb.linearVelocity.y);
        }
    }

    public Vector2 facingDir()
    {
        return facingRight ? Vector2.right : Vector2.left;
    }

    /// <summary>
    /// Called by enemies/hazards to damage player
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        // Check invulnerability component
        if (invulnerability != null && invulnerability.IsInvulnerable)
        {
            Debug.Log("Player is invulnerable - damage blocked");
            return;
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
        }

        // Trigger i-frames via Invulnerability component
        if (invulnerability != null)
        {
            invulnerability.Trigger();
        }

        // Play hurt animation
        if (animDriver != null)
            animDriver.Hurt();
        else if (anim != null)
            anim.SetTrigger("Hurt");
    }

    /// <summary>
    /// Called by enemies to damage + knockback player
    /// </summary>
    public void TakeDamage(int damageAmount, Vector2 hitSourcePosition)
    {
        TakeDamage(damageAmount);
        ApplyKnockback(hitSourcePosition);
    }

    public void ApplyKnockback(Vector2 hitSourcePosition)
    {
        Vector2 dir = ((Vector2)transform.position - hitSourcePosition).normalized;

        // Ensure horizontal push
        if (Mathf.Abs(dir.x) < 0.1f)
            dir.x = facingRight ? 1f : -1f;

        StartCoroutine(KnockbackRoutine(dir));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockback = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(direction.x * knockbackForce, knockbackUpForce), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockback = false;
    }

    public void PlayHurtAnimation()
    {
        if (animDriver != null)
            animDriver.Hurt();
        else if (anim != null)
            anim.SetTrigger("Hurt");
    }

    public void PlayDeathAnimation()
    {
        if (animDriver != null)
            animDriver.Death();
        else if (anim != null)
            anim.SetTrigger("Death");
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