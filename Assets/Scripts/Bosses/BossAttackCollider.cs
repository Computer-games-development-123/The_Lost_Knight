using UnityEngine;

public class BossAttackCollider : MonoBehaviour
{
    [Tooltip("Reference to the boss script")]
    public DitorBoss ditorBoss;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"BossAttackCollider hit: {collision.gameObject.name}, Tag: {collision.tag}");

        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player hit by attack collider!");
            if (ditorBoss != null)
            {
                ditorBoss.OnAttackHitPlayer();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (ditorBoss != null)
            {
                ditorBoss.OnAttackHitPlayer();
            }
        }
    }
}