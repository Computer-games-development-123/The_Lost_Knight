using UnityEngine;
using System.Collections;

public class FikaBoss : BossBase
{
    [Header("Fika Specific")]
    public GameObject projectilePrefab;
    public Transform[] dashPoints;
    public float dashSpeed = 12f;
    public float projectileCooldown = 2f;
    private float lastProjectileTime;
    private bool isDashing = false;

    protected override void OnBossStart()
    {
        base.OnBossStart();
        bossName = "Fika";
        maxHP = 120;
        currentHP = maxHP;
        moveSpeed = 5f;
    }

    protected override void BossAI()
    {
        if (player == null || isDashing) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Ranged attack
        if (Time.time >= lastProjectileTime + projectileCooldown && distanceToPlayer > 4f)
        {
            ShootProjectile();
        }
        // Dash attack
        else if (distanceToPlayer < 6f && Random.value > 0.98f)
        {
            StartCoroutine(DashAttack());
        }
        else
        {
            // Circle around player
            CirclePlayer();
        }
    }

    void ShootProjectile()
    {
        lastProjectileTime = Time.time;

        if (projectilePrefab != null && player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * 8f;
            }

            Destroy(projectile, 3f);
        }
    }

    void CirclePlayer()
    {
        if (player == null) return;

        Vector2 offset = new Vector2(Mathf.Sin(Time.time * 2f), 0) * 4f;
        Vector2 targetPosition = (Vector2)player.position + offset;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        rb.linearVelocity = direction * moveSpeed;
    }

    IEnumerator DashAttack()
    {
        isDashing = true;

        Vector2 dashDirection = (player.position - transform.position).normalized;
        rb.linearVelocity = dashDirection * dashSpeed;

        if (anim != null)
            anim.SetTrigger("Dash");

        yield return new WaitForSeconds(0.3f);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        projectileCooldown *= 0.6f; // Shoot more frequently
        dashSpeed *= 1.3f;
    }
}