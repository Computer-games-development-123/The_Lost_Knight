using UnityEngine;
using System.Collections;

public class FikaBoss : BossBase
{
    [Header("Fika Specific")]
    public GameObject projectilePrefab;
    public Transform[] dashPoints;
    public float dashSpeed = 8f;
    public float projectileCooldown = 2.5f;
    private float lastProjectileTime;
    private bool isDashing = false;
    
    [Header("Dash Settings")]
    public float dashCooldown = 3f;
    private float lastDashTime = 0f;

    [Header("Portals (Direct References)")]
    public string portalBackToHubName = "Forest_Hub_Portal";
    public string portalToNextAreaName = "BlueBattle_Portal"; // Or whatever's next

    protected override void OnBossStart()
    {
        base.OnBossStart();
        bossName = "Fika";
        maxHP = 120;
        currentHP = maxHP;
        moveSpeed = 3f;
        
        Debug.Log($"✅ {bossName} initialized: HP={currentHP}, Damage={damage}");
    }

    protected override void BossAI()
    {
        if (player == null || isDashing) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (Time.time >= lastProjectileTime + projectileCooldown && distanceToPlayer > 4f && distanceToPlayer < 10f)
        {
            ShootProjectile();
        }
        else if (distanceToPlayer < 6f && Time.time >= lastDashTime + dashCooldown && Random.value > 0.95f)
        {
            StartCoroutine(DashAttack());
        }
        else
        {
            CirclePlayer();
        }
    }

    void ShootProjectile()
    {
        lastProjectileTime = Time.time;

        if (projectilePrefab != null && player != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            Vector2 direction = (player.position - spawnPos).normalized;
            
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            int projectileLayer = LayerMask.NameToLayer("EnemyProjectile");
            if (projectileLayer != -1)
            {
                projectile.layer = projectileLayer;
            }

            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * 7f;
            }
            
            EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
            if (projScript != null)
            {
                projScript.damage = damage;
            }

            Destroy(projectile, 3f);
        }
    }

    void CirclePlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer < 3f)
        {
            Vector2 awayDirection = (transform.position - player.position).normalized;
            rb.linearVelocity = awayDirection * moveSpeed * 0.8f;
        }
        else if (distanceToPlayer > 8f)
        {
            Vector2 towardDirection = (player.position - transform.position).normalized;
            rb.linearVelocity = towardDirection * moveSpeed * 0.5f;
        }
        else
        {
            Vector2 offset = new Vector2(Mathf.Sin(Time.time * 1.5f), 0) * 4f;
            Vector2 targetPosition = (Vector2)player.position + offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed * 0.7f;
        }
        
        if (rb.linearVelocity.x > 0)
            spriteRenderer.flipX = false;
        else if (rb.linearVelocity.x < 0)
            spriteRenderer.flipX = true;
    }

    IEnumerator DashAttack()
    {
        isDashing = true;
        lastDashTime = Time.time;

        if (anim != null)
            anim.SetTrigger("DashWindup");
        
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.3f);

        Vector2 dashDirection = (player.position - transform.position).normalized;
        rb.linearVelocity = dashDirection * dashSpeed;

        if (anim != null)
            anim.SetTrigger("Dash");

        yield return new WaitForSeconds(0.25f);

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.3f);
        
        isDashing = false;
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        
        projectileCooldown *= 0.7f;
        dashSpeed *= 1.2f;
        dashCooldown *= 0.8f;
        
        Debug.Log($"{bossName} entered Phase 2!");
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"💀 {bossName} defeated!");

        if (anim != null)
            anim.SetTrigger("Death");

        if (waveManager != null)
        {
            waveManager.OnBossDied(this);
        }

        if (DialogueManager.Instance != null && deathDialogue != null)
        {
            DialogueManager.Instance.Play(deathDialogue, OnDeathDialogueComplete);
        }
        else
        {
            OnDeathDialogueComplete();
        }
    }

    private void OnDeathDialogueComplete()
    {
        Debug.Log($"✅ Fika defeated - spawning portals...");

        // ✅ Update GameManager - NO MORE ACT FLAGS!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFikaDefeated();
        }

        // ✅ Spawn portals directly (same as George)
        GameObject portalBack = FindObjectInHierarchy(portalBackToHubName);
        GameObject portalNext = FindObjectInHierarchy(portalToNextAreaName);

        if (portalBack != null)
        {
            portalBack.SetActive(true);
            Debug.Log($"🌀 Portal spawned: {portalBackToHubName}");
        }

        if (portalNext != null)
        {
            portalNext.SetActive(true);
            Debug.Log($"🌀 Portal spawned: {portalToNextAreaName}");
        }

        Destroy(gameObject, 2f);
    }

    private GameObject FindObjectInHierarchy(string objectName)
    {
        if (string.IsNullOrEmpty(objectName)) return null;

        GameObject obj = GameObject.Find(objectName);
        if (obj != null) return obj;

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.hideFlags == HideFlags.None && go.scene.isLoaded)
            {
                if (go.name == objectName)
                {
                    return go;
                }
            }
        }

        return null;
    }
}