using UnityEngine;
using System.Collections;

public class DitorBoss : BossBase
{
    [Header("Ditor Specific")]
    public GameObject[] summonPrefabs;
    public GameObject darkBeamPrefab;
    public Transform[] teleportPoints;
    public float summonCooldown = 8f;
    public float beamCooldown = 5f;
    private float lastSummonTime;
    private float lastBeamTime;
    private int phase = 1;

    protected override void OnBossStart()
    {
        base.OnBossStart();
        bossName = "Ditor";
        maxHP = 200;
        currentHP = maxHP;
        moveSpeed = 4f;
        damage = 15;
    }

    protected override void BossAI()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Summon minions
        if (Time.time >= lastSummonTime + summonCooldown)
        {
            SummonMinions();
        }

        // Dark beam attack
        if (Time.time >= lastBeamTime + beamCooldown && distanceToPlayer < 8f)
        {
            StartCoroutine(DarkBeamAttack());
        }

        // Teleport if player gets too close
        if (distanceToPlayer < 2f && Random.value > 0.95f)
        {
            Teleport();
        }
        else
        {
            // Maintain distance
            MaintainDistance();
        }
    }

    void SummonMinions()
    {
        lastSummonTime = Time.time;

        int summonCount = phase == 1 ? 2 : 4;

        for (int i = 0; i < summonCount; i++)
        {
            if (summonPrefabs.Length > 0)
            {
                Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 3f;
                GameObject minion = Instantiate(summonPrefabs[Random.Range(0, summonPrefabs.Length)], spawnPos, Quaternion.identity);
            }
        }

        Debug.Log($"Ditor summoned {summonCount} minions!");
    }

    IEnumerator DarkBeamAttack()
    {
        lastBeamTime = Time.time;

        if (anim != null)
            anim.SetTrigger("BeamCharge");

        yield return new WaitForSeconds(1f);

        // Fire beam
        if (darkBeamPrefab != null && player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            GameObject beam = Instantiate(darkBeamPrefab, transform.position, Quaternion.identity);
            
            // Set beam direction
            beam.transform.right = direction;
            
            Destroy(beam, 2f);
        }

        if (anim != null)
            anim.SetTrigger("BeamFire");
    }

    void Teleport()
    {
        if (teleportPoints.Length > 0)
        {
            Transform teleportPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
            transform.position = teleportPoint.position;

            if (anim != null)
                anim.SetTrigger("Teleport");

            Debug.Log("Ditor teleported!");
        }
    }

    void MaintainDistance()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 direction;

        if (distanceToPlayer < 5f)
        {
            // Move away
            direction = (transform.position - player.position).normalized;
        }
        else if (distanceToPlayer > 8f)
        {
            // Move closer
            direction = (player.position - transform.position).normalized;
        }
        else
        {
            // Strafe
            direction = new Vector2(-(player.position - transform.position).normalized.y, (player.position - transform.position).normalized.x);
        }

        rb.linearVelocity = direction * moveSpeed;
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        phase = 2;
        summonCooldown *= 0.6f;
        beamCooldown *= 0.7f;
        Debug.Log("Ditor entered Phase 2 - Desperation mode!");
    }
}   