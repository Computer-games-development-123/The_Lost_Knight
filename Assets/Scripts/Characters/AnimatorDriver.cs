using UnityEditor.Animations;
using UnityEngine;

[RequireComponent(typeof(CharacterContext))]
[RequireComponent(typeof(Animator))]
public class AnimatorDriver : MonoBehaviour
{
    protected CharacterContext ctx;
    protected PlayerMovement movement;
    public Animator anim;
    private FormSwitcher fs;
    private void Awake()
    {
        ctx = GetComponent<CharacterContext>();
        anim = GetComponent<Animator>();
        fs = GetComponent<FormSwitcher>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (movement == null) return;

        float speed01 = Mathf.Abs(ctx.RB.linearVelocity.x) / Mathf.Max(0.01f, movement.MoveSpeed);

        anim.SetFloat("Speed", speed01);
        anim.SetFloat("YVelocity", ctx.RB.linearVelocity.y);
        anim.SetBool("Grounded", movement.IsGrounded);
    }

    public virtual void Attack() => anim.SetTrigger("Attack");
    public void SpecialAttack() => anim.SetTrigger("SpAttack");
    public void Attacking(bool b) => anim.SetBool("Attacking", b);
    public void Combo(int num) => anim.SetInteger("Combo", num);
    public void AttackDown() => anim.SetTrigger("AttackDown");
    public void SetBlock(bool b) => anim.SetBool("Block", b);
    public void Jump() => anim.SetTrigger("JumpTrig");
    public void JumpAttack() => anim.SetTrigger("JumpAttack");
    public void Transform() => fs?.StartTransformation();
    public void Hurt() => anim.SetTrigger("Hurt");
    public void Death() => anim.SetTrigger("Death");
}
/* block animation
if (Input.GetKey(KeyCode.C))
    pad.SetBlock(true);
else
    pad.SetBlock(false);
*/