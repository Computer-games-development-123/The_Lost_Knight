using UnityEngine;
using System.Collections;

public class PhilipBoss : BossBase
{
    [Header("Portal Attack (Prefab-based)")]
    public GameObject portalPrefab;
    public Transform portalSpawnPoint;
    public float portalAttackCooldown = 10f;
    public float portalTelegraphDelay = 0.25f;
    public float portalLifetime = 1.2f;

    [Header("Melee Attack")]
    public Transform meleeAttackPoint;
    public float meleeAttackRange = 2f;
    public float meleeAttackCooldown = 2f;

    [Header("Movement")]
    public float floatSpeed = 2f;
    public float attackRange = 6f;
    public float meleeRange = 2.5f;

    private float lastPortalAttackTime = -999f;
    private float lastMeleeAttackTime = -999f;
    private bool isAttacking = false;

    private GameObject activePortal;

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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(GameFlag.YojiDead, true);
            GameManager.Instance.SaveProgress();
        }

        if (StoreStateManager.Instance != null)
        {
            StoreStateManager.Instance.SetStoreState(StoreStateManager.StoreState.PostPhilip);
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
        else if (dist >= attackRange && Time.time >= lastPortalAttackTime + portalAttackCooldown)
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
        if (isAttacking) yield break;

        isAttacking = true;
        lastPortalAttackTime = Time.time;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null)
        {
            anim.SetBool("IsMoving", false);

            anim.SetBool("IsSpecial", true);

            anim.SetTrigger("Attack2");
        }

        yield return new WaitForSeconds(portalTelegraphDelay);

        GameObject portal = null;

        if (portalPrefab != null && portalSpawnPoint != null)
        {
            portal = Instantiate(portalPrefab, portalSpawnPoint.position, Quaternion.identity);

            Animator portalAnim = portal.GetComponentInChildren<Animator>(true);
            if (portalAnim != null)
                portalAnim.SetTrigger("Open");

            Destroy(portal, portalLifetime);
        }
        else
        {
            Debug.LogWarning("PhilipBoss: Portal prefab or spawn point missing!");
        }

        float t = 0f;
        while (t < portalLifetime)
        {
            if (isDead) yield break;

            if (rb != null) rb.linearVelocity = Vector2.zero;
            t += Time.deltaTime;
            yield return null;
        }

        if (anim != null)
        {
            anim.SetTrigger("ClosePortal");

            yield return new WaitForSeconds(0.6f);
        }

        isAttacking = false;
    }


    private IEnumerator PerformMeleeAttack()
    {
        if (isAttacking) yield break;

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

        Collider2D[] hits = Physics2D.OverlapCircleAll(meleeAttackPoint.position, meleeAttackRange);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph == null) ph = hit.GetComponentInParent<PlayerHealth>();

            if (ph != null)
                ph.TakeDamage(damage);
        }
    }

    public override void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        if (isInvulnerable)
        {
            Debug.Log($"{bossName} is invulnerable!");
            OnInvulnerableHit();
            return;
        }

        currentHP -= damageAmount;

        if (anim != null && !isAttacking)
        {
            anim.SetTrigger("Hurt");
        }


        StartCoroutine(FlashRed());
        Debug.Log($"{bossName} took {damageAmount} damage. HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        isAttacking = false;
        StopAllCoroutines();

        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (activePortal != null)
        {
            Destroy(activePortal);
            activePortal = null;
        }

        if (anim != null) anim.SetTrigger("Death");

        if (waveManager != null) waveManager.OnBossDied(this);

        if (DialogueManager.Instance != null && deathDialogue != null)
            DialogueManager.Instance.Play(deathDialogue, OnDeathDialogueComplete);
        else
            OnDeathDialogueComplete();
    }

    protected override void OnDeathDialogueComplete()
    {
        base.OnDeathDialogueComplete();
        if (GameManager.Instance != null) GameManager.Instance.OnPhilipDefeated();
    }
}
