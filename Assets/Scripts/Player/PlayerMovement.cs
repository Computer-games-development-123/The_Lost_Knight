using UnityEngine;

[RequireComponent(typeof(CharacterContext))]
public class PlayerMovement : MonoBehaviour
{
    public CharacterData data;
    private CharacterContext ctx;

    [Header("Movement")]
    public float MoveSpeed => (data != null) ? data.moveSpeed : 5f;
    public float jumpForce = 20f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    public bool IsGrounded { get; private set; }

    public bool MovementLocked { get; set; }
    public float MovementMultiplier { get; set; } = 1f;

    private float moveInput;
    private bool jumpPressed;

    private void Awake()
    {
        ctx = GetComponent<CharacterContext>();
    }

    private void Update()
    {
        if (groundCheck != null)
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (MovementLocked || (ctx.KB != null && ctx.KB.isKnockback))
        {
            moveInput = 0f;
            jumpPressed = false;
            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;

        if (moveInput > 0.01f) ctx.SetFacing(true);
        else if (moveInput < -0.01f) ctx.SetFacing(false);
    }

    private void FixedUpdate()
    {
        if (MovementLocked || (ctx.KB != null && ctx.KB.isKnockback))
            return;

        float finalSpeed = moveInput * MoveSpeed * Mathf.Clamp01(MovementMultiplier);
        ctx.RB.linearVelocity = new Vector2(finalSpeed, ctx.RB.linearVelocity.y);

        if (jumpPressed && IsGrounded)
        {
            ctx.RB.linearVelocity = new Vector2(ctx.RB.linearVelocity.x, jumpForce);
            if (ctx.AD != null) ctx.AD.Jump();
        }

        jumpPressed = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
