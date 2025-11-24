using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GeorgeBossController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;

    private Transform player;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector2 dir = player.position - transform.position;
        float dist = dir.magnitude;

        if (dist > stopDistance)
        {
            dir.Normalize();
            rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }
}
