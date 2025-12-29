using UnityEngine;
using System.Collections;

/// <summary>
/// Modular knockback system - can be used by any character
/// </summary>
public class Knockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForce = 7f;
    public float knockbackUpForce = 2f;
    public float knockbackDuration = 0.15f;

    private Rigidbody2D rb;
    private bool isKnockback = false;

    public bool IsKnockback => isKnockback;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(Vector2 hitSourcePosition)
    {
        if (rb == null) return;

        Vector2 dir = ((Vector2)transform.position - hitSourcePosition).normalized;

        if (Mathf.Abs(dir.x) < 0.1f)
        {
            // Default to facing direction if hit from directly above/below
            dir.x = transform.localScale.x > 0 ? 1f : -1f;
        }

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
}