using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float maxChaseDistance = 20f;   // עד איזה מרחק רודף אחרי השחקן

    private Rigidbody2D rb;
    private Transform player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // למצוא את השחקן לפי Tag
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;

        // אם השחקן רחוק מדי, לא זזים
        if (Mathf.Abs(dx) > maxChaseDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // כיוון התנועה (שמאלה/ימינה)
        float dir = Mathf.Sign(dx);

        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }
}
