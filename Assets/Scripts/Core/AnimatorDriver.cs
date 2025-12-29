using UnityEngine;

[RequireComponent(typeof(CharacterContext))]
[RequireComponent(typeof(Animator))]
public class AnimatorDriver : MonoBehaviour
{
    private CharacterContext ctx;
    private PlayerController movement;
    public Animator anim;

    private void Awake()
    {
        ctx = GetComponent<CharacterContext>();
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (movement == null || anim == null) return;

        // Speed animation
        float speed = Mathf.Abs(ctx.RB.linearVelocity.x);
        anim.SetFloat("Speed", speed);
        
        // Vertical velocity
        anim.SetFloat("YVelocity", ctx.RB.linearVelocity.y);
        
        // Grounded state
        anim.SetBool("Grounded", movement.IsGrounded);
    }

    // Animation triggers
    public void Attack() => anim.SetTrigger("Attack");
    public void AttackDown() => anim.SetTrigger("AttackDown");
    public void Jump() => anim.SetTrigger("Jump");
    public void Hurt() => anim.SetTrigger("Hurt");
    public void Death() => anim.SetTrigger("Death");
    public void WaveOfFire() => anim.SetTrigger("WaveOfFire");
}