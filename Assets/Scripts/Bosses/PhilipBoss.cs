using UnityEngine;
using System.Collections;

public class PhilipBoss : BossBase
{
    [Header("Portal Attack (Prefab-based)")]
    public GameObject portalPrefab;          // Prefab של הפורטל
    public Transform portalSpawnPoint;        // נקודה מעל פיליפ
    public float portalAttackCooldown = 5f;
    public float portalTelegraphDelay = 0.25f;
    public float portalLifetime = 1.2f;

    [Header("Melee Attack")]
    public Transform meleeAttackPoint;
    public float meleeAttackRange = 2f;
    public float meleeAttackCooldown = 3f;

    [Header("Movement")]
    public float floatSpeed = 2f;
    public float attackRange = 6f;
    public float meleeRange = 2.5f;

    private float lastPortalAttackTime = -999f;
    private float lastMeleeAttackTime = -999f;
    private bool isAttacking = false;

    protected override void Start()
    {
        base.Start();
        if (anim == null) anim = GetComponent<Animator>();
        floatSpeed = moveSpeed;
    }

    protected override void OnBossStart()
    {
        base.OnBossStart();

        bossName = "Philip, Bringer of Death";

        // Set Yoji as dead when Philip appears
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(GameFlag.YojiDead, true);
            GameManager.Instance.SaveProgress();
            Debug.Log("Philip has killed Yoji - YojiDead flag set");
        }

        // Update store to free (PostPhilip state)
        if (StoreStateManager.Instance != null)
        {
            StoreStateManager.Instance.SetStoreState(StoreStateManager.StoreState.PostPhilip);
            Debug.Log("Store is now free - Yoji's Legacy");
        }
    }

    protected override void BossAI()
    {
        if (isDead || player == null || isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);
        FacePlayer();

        if (dist <= meleeRange && Time.time >= lastMeleeAttackTime + meleeAttackCooldown)
        {
            StartCoroutine(PerformMeleeAttack());
        }
        else if (dist <= attackRange && Time.time >= lastPortalAttackTime + portalAttackCooldown)
        {
            StartCoroutine(PerformPortalAttack());
        }
        else if (dist > meleeRange)
        {
            FloatTowardsPlayer();
        }
        else
        {
            if (anim != null) anim.SetBool("IsMoving", false);
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator PerformPortalAttack()
    {
        isAttacking = true;
        lastPortalAttackTime = Time.time;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("IsMoving", false);
        if (anim != null) anim.SetTrigger("Attack2");

        yield return new WaitForSeconds(portalTelegraphDelay);

        if (portalPrefab == null || portalSpawnPoint == null)
        {
            Debug.LogWarning("PhilipBoss: Portal prefab or spawn point missing!");
        }
        else
        {
            GameObject portal = Instantiate(
                portalPrefab,
                portalSpawnPoint.position,
                Quaternion.identity
            );

            Animator portalAnim = portal.GetComponentInChildren<Animator>();
            if (portalAnim != null)
                portalAnim.SetTrigger("Open");

            Destroy(portal, portalLifetime);
        }

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    private IEnumerator PerformMeleeAttack()
    {
        isAttacking = true;
        lastMeleeAttackTime = Time.time;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("IsMoving", false);
        if (anim != null) anim.SetTrigger("Attack1");

        yield return new WaitForSeconds(0.4f);
        DealMeleeDamage();
        yield return new WaitForSeconds(0.6f);

        isAttacking = false;
    }

    private void FloatTowardsPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        if (rb != null)
            rb.linearVelocity = new Vector2(dir.x * floatSpeed, rb.linearVelocity.y);
        if (anim != null)
            anim.SetBool("IsMoving", true);
    }

    private void FacePlayer()
    {
        float dx = player.position.x - transform.position.x;
        if (dx > 0 && !facingRight) Flip();
        else if (dx < 0 && facingRight) Flip();
    }

    private void DealMeleeDamage()
    {
        if (meleeAttackPoint == null)
        {
            Debug.LogWarning("Melee attack point not assigned!");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            meleeAttackPoint.position,
            meleeAttackRange
        );

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Philip melee hit for {damage} damage!");
            }
        }
    }

    protected override void OnDeathDialogueComplete()
    {
        // Call base to handle coins and slain dialogue
        base.OnDeathDialogueComplete();

        Debug.Log($"Philip defeated - unlocking final area...");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhilipDefeated();
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{bossName} defeated!");

        // Stop all movement and attacks
        isAttacking = false;
        StopAllCoroutines();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

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

}
