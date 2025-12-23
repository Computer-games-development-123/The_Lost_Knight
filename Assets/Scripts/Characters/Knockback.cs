using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterContext))]
public class Knockback : MonoBehaviour
{
    [Header("Knockback (set velocity)")]
    public float knockbackX = 3f;
    public float knockbackY = 1f;
    public float knockbackDuration = 0.15f;
    public bool zeroGravityDuringKB = true;

    private CharacterContext ctx;
    public bool isKnockback { get; private set; }

    private float originalGravity;

    private void Start()
    {
        if (ctx == null) ctx = GetComponent<CharacterContext>();

        if (ctx == null)
            Debug.LogError($"{name}: Knockback missing CharacterContext", this);
        else if (ctx.RB == null)
            Debug.LogError($"{name}: Knockback missing Rigidbody2D", this);
    }

    public void ApplyKnockback(Vector2 hitSourcePosition)
    {
        if (ctx == null) ctx = GetComponent<CharacterContext>();
        if (ctx == null || ctx.RB == null) return;

        Vector2 dir = ((Vector2)transform.position - hitSourcePosition).normalized;

        if (Mathf.Abs(dir.x) < 0.1f)
            dir.x = ctx.FacingRight ? 1f : -1f;

        StopAllCoroutines();
        StartCoroutine(KnockbackRoutine(dir));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockback = true;

        originalGravity = ctx.RB.gravityScale;
        if (zeroGravityDuringKB) ctx.RB.gravityScale = 0f;

        //ctx.RB.linearVelocity = new Vector2(direction.x * knockbackX, knockbackY);
        ctx.RB.linearVelocity = new Vector2(direction.x * knockbackX, Mathf.Max(ctx.RB.linearVelocity.y, knockbackY));

        yield return new WaitForSeconds(knockbackDuration);

        if (zeroGravityDuringKB) ctx.RB.gravityScale = originalGravity;

        isKnockback = false;
    }
}
