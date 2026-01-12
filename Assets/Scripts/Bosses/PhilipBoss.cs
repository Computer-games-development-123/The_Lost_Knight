using UnityEngine;
using System.Collections;

public class PhilipBoss : BossBase
{
    [Header("Philip Specific - Portal Attack")]
    public GameObject portalPrefab; // The portal that appears above Philip
    public GameObject lightningPrefab; // The lightning projectile that drops on player
    public Transform portalSpawnPoint; // Where portal spawns above Philip
    public float portalAttackCooldown = 5f;
    public float portalAttackDuration = 2f;
    
    [Header("Philip Specific - Melee Attack")]
    public Transform meleeAttackPoint;
    public float meleeAttackRange = 2f;
    public float meleeAttackCooldown = 3f;
    
    [Header("Philip Specific - Movement")]
    public float floatSpeed = 2f;
    public float attackRange = 6f; // Distance to start attacking
    public float meleeRange = 2.5f; // Distance for melee attacks
    
    [Header("Phase 2 Changes")]
    public float phase2PortalCooldownMultiplier = 0.6f;
    public float phase2MeleeCooldownMultiplier = 0.7f;
    
    private float lastPortalAttackTime = -999f;
    private float lastMeleeAttackTime = -999f;
    private bool isAttacking = false;
    private GameObject activePortal;
    
    private enum AttackType { None, Melee, Portal }
    private AttackType currentAttack = AttackType.None;

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

    protected override void Update()
    {
        base.Update();
        
        // Debug: Manual attack testing
        if (Input.GetKeyDown(KeyCode.V)) StartCoroutine(PerformPortalAttack());
        if (Input.GetKeyDown(KeyCode.B)) StartCoroutine(PerformMeleeAttack());
    }

    protected override void BossAI()
    {
        if (isDead || player == null || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Face the player
        FacePlayer();

        // Decide which attack to use based on distance and cooldowns
        if (distanceToPlayer <= meleeRange && Time.time >= lastMeleeAttackTime + meleeAttackCooldown)
        {
            // Close enough for melee
            StartCoroutine(PerformMeleeAttack());
        }
        else if (distanceToPlayer <= attackRange && Time.time >= lastPortalAttackTime + portalAttackCooldown)
        {
            // In attack range, use portal attack
            StartCoroutine(PerformPortalAttack());
        }
        else if (distanceToPlayer > meleeRange)
        {
            // Move towards player
            FloatTowardsPlayer();
        }
        else
        {
            // Wait for cooldowns - play idle
            if (anim != null)
                anim.SetBool("IsMoving", false);
            
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
    }

    private void FloatTowardsPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction.x * floatSpeed, rb.linearVelocity.y);
        }

        if (anim != null)
        {
            anim.SetBool("IsMoving", true);
        }
    }

    private void FacePlayer()
    {
        if (player == null) return;

        float direction = player.position.x - transform.position.x;
        
        if (direction > 0 && !facingRight)
            Flip();
        else if (direction < 0 && facingRight)
            Flip();
    }

    private IEnumerator PerformMeleeAttack()
    {
        isAttacking = true;
        currentAttack = AttackType.Melee;
        lastMeleeAttackTime = Time.time;

        // Stop movement
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetBool("IsMoving", false);

        // Trigger attack animation
        if (anim != null)
            anim.SetTrigger("Attack1");

        // Wait for animation to reach damage frame (adjust timing as needed)
        yield return new WaitForSeconds(0.4f);

        // Deal damage
        DealMeleeDamage();

        // Wait for animation to finish
        yield return new WaitForSeconds(0.6f);

        isAttacking = false;
        currentAttack = AttackType.None;
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
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Philip's melee attack hit for {damage} damage!");
                }
            }
        }
    }

    private IEnumerator PerformPortalAttack()
    {
        isAttacking = true;
        currentAttack = AttackType.Portal;
        lastPortalAttackTime = Time.time;

        // Stop movement
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetBool("IsMoving", false);

        // Trigger portal opening animation
        if (anim != null)
            anim.SetTrigger("Attack2");

        // Wait a moment for animation to start
        yield return new WaitForSeconds(0.3f);

        // Spawn portal above Philip
        if (portalPrefab != null && portalSpawnPoint != null)
        {
            activePortal = Instantiate(portalPrefab, portalSpawnPoint.position, Quaternion.identity);
            activePortal.transform.SetParent(transform); // Follow Philip
        }

        // Wait a moment, then spawn lightning above player
        yield return new WaitForSeconds(0.5f);

        if (player != null && lightningPrefab != null)
        {
            // Spawn lightning above player's current position
            Vector3 lightningSpawnPos = new Vector3(player.position.x, player.position.y + 5f, player.position.z);
            GameObject lightning = Instantiate(lightningPrefab, lightningSpawnPos, Quaternion.identity);
            
            // Give lightning downward velocity
            Rigidbody2D lightningRb = lightning.GetComponent<Rigidbody2D>();
            if (lightningRb != null)
            {
                lightningRb.linearVelocity = Vector2.down * 10f;
            }

            Destroy(lightning, 3f);
        }

        // Keep portal open for duration
        yield return new WaitForSeconds(portalAttackDuration);

        // Close portal animation
        if (anim != null)
            anim.SetTrigger("ClosePortal");

        yield return new WaitForSeconds(0.3f);

        // Destroy portal
        if (activePortal != null)
        {
            Destroy(activePortal);
            activePortal = null;
        }

        yield return new WaitForSeconds(0.4f);

        isAttacking = false;
        currentAttack = AttackType.None;
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();

        // Reduce attack cooldowns
        portalAttackCooldown *= phase2PortalCooldownMultiplier;
        meleeAttackCooldown *= phase2MeleeCooldownMultiplier;
        floatSpeed *= 1.3f;

        Debug.Log("Philip entered Phase 2 - The Bringer of Death awakens!");
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

        // Destroy any active portal
        if (activePortal != null)
        {
            Destroy(activePortal);
            activePortal = null;
        }

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

    private void OnDrawGizmosSelected()
    {
        // Draw melee attack range
        if (meleeAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRange);
        }

        // Draw attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw melee range
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}