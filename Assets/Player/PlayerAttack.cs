using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.6f;
    public int damage = 1;
    public LayerMask enemyLayer;
    public KeyCode attackKey = KeyCode.X;

    // we will read facingDir from here
    private PlayerController playerController;

    void Start()
    {
        // get reference to PlayerController on the same GameObject
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            Attack();
        }
    }

    void Attack()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("⚠️ No Attack Point assigned!");
            return;
        }

        // ✅ flip attackPoint left/right according to facingDir
        if (playerController != null)
        {
            Vector3 localPos = attackPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * playerController.facingDir; // 1 = right, -1 = left
            attackPoint.localPosition = localPos;
        }

        // now check for enemies
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        Debug.Log($"ATTACK triggered with key {attackKey} — hits: {hits.Length}");

        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(damage);
                Debug.Log($"Hit enemy: {hit.name}");
            }
            else
            {
                Debug.Log("Hit something without Enemy component: " + hit.name);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
