using UnityEngine;
using System.Collections;

public class PhilipBoss : BossBase
{
    [Header("Philip Specific")]
    public GameObject shockwavePrefab;
    public float slamCooldown = 4f;
    public int slamDamage = 15;
    private float lastSlamTime;
    private bool isSlaming = false;

    protected override void OnBossStart()
    {
        base.OnBossStart();
        bossName = "Philip";
        maxHP = 150;
        currentHP = maxHP;
        moveSpeed = 2f;
        damage = 12;
    }

    protected override void BossAI()
    {
        if (player == null || isSlaming) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Ground slam attack
        if (Time.time >= lastSlamTime + slamCooldown && distanceToPlayer < 5f)
        {
            StartCoroutine(GroundSlam());
        }
        else
        {
            // Slow advance
            base.BossAI();
        }
    }

    IEnumerator GroundSlam()
    {
        isSlaming = true;
        lastSlamTime = Time.time;

        // Jump up
        rb.linearVelocity = new Vector2(0, 10f);

        if (anim != null)
            anim.SetTrigger("Jump");

        yield return new WaitForSeconds(0.5f);

        // Slam down
        rb.linearVelocity = new Vector2(0, -15f);

        yield return new WaitUntil(() => rb.linearVelocity.y == 0);

        // Create shockwave
        if (shockwavePrefab != null)
        {
            Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        }

        // Damage player if nearby
        if (player != null && Vector2.Distance(transform.position, player.position) < 4f)
        {
            player.GetComponent<PlayerController>()?.TakeDamage(slamDamage);
        }

        if (anim != null)
            anim.SetTrigger("SlamImpact");

        yield return new WaitForSeconds(1f);

        isSlaming = false;
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        slamCooldown *= 0.7f;
        slamDamage = Mathf.RoundToInt(slamDamage * 1.5f);
    }
}