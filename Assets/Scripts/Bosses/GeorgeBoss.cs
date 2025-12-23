using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GeorgeBoss : BossBase
{
    [Header("Flying Boss Specific")]
    public bool isFirstEncounter = true;

    [Header("Boss Dialogues")]
    public DialogueData firstEncounterDialogue;
    public GameObject dialogueCanvas;

    [Header("Attack Settings")]
    public float attackCooldown = 4f;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    [Header("Range Detection")]
    public float closeRangeThreshold = 2f;
    public Transform attackPoint; // Point from where the attack originates
    public float attackRange = 1f; // Range of the attack from the attack point

    [Header("Flying Settings")]
    public float flySpeed = 6f;
    public float flyHeight = 3f;
    public float flyDuration = 1f;
    private bool isFlying = false;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;

    [Header("Boundaries")]
    public float minX = -8f; // Left boundary
    public float maxX = 8f;  // Right boundary
    public float minY = -4f;  // Bottom boundary
    public float maxY = 4f;  // Top boundary

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Player Reference")]
    public CharacterContext playerCtx;

    private bool isGrounded = false;

    protected override void Start()
    {
        base.Start();

        // Get player context if not assigned
        if (playerCtx == null && player != null)
        {
            playerCtx = player.GetComponent<CharacterContext>();
        }
    }

    protected override void Update()
    {
        // Check if grounded every frame
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Enforce boundaries - keep boss within screen bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;

        base.Update();
    }

    protected override void OnBossStart()
    {
        base.OnBossStart();
        bossName = "FlyingBoss";

        if (GameManager.Instance != null)
        {
            bool hasUpgrade = playerCtx != null && playerCtx.AB != null && playerCtx.AB.hasUpgradedSword;
            //isInvulnerable = !hasUpgrade;

            if (!hasUpgrade)
            {
                spawnDialogue = null;
            }
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null in FlyingBoss! Assuming first encounter (invulnerable).");
            //isInvulnerable = true;
            spawnDialogue = null;
        }

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    protected override void BossAI()
    {
        if (isDead || player == null) return;

        // Don't do anything while attacking or flying
        if (isAttacking || isFlying)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Attack only when: cooldown ready, grounded, AND close enough to player
        if (Time.time >= lastAttackTime + attackCooldown && isGrounded && distanceToPlayer <= closeRangeThreshold)
        {
            StartCoroutine(AttackSequence(distanceToPlayer));
        }
        // Walk towards player when not attacking or flying
        else if (isGrounded)
        {
            WalkTowardsPlayer();
        }
    }

    private void WalkTowardsPlayer()
    {
        if (player == null) return;

        // Calculate horizontal direction only (don't fly vertically)
        float horizontalDirection = (player.position.x - transform.position.x);
        float moveDirection = Mathf.Sign(horizontalDirection);

        // Walk horizontally towards player
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveDirection * walkSpeed, rb.linearVelocity.y);
        }

        if (anim != null)
        {
            anim.SetBool("IsMoving", true);
        }

        // Flip sprite based on direction
        if (moveDirection < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (moveDirection > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    IEnumerator AttackSequence(float distanceToPlayer)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        Debug.Log("⚔️ Starting attack sequence");

        // Stop movement during attack
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetBool("IsMoving", false);

        // Choose attack based on distance
        if (distanceToPlayer <= closeRangeThreshold)
        {
            yield return StartCoroutine(CloseRangeAttack());
        }
        else
        {
            yield return StartCoroutine(RangedAttack());
        }

        Debug.Log("⏳ Waiting 1 second after attack...");
        // Wait 1 second after attack, then fly away
        yield return new WaitForSeconds(1f);

        Debug.Log("🚀 Now flying away...");
        // Fly away and land (this sets isFlying to true, then false when done)
        yield return StartCoroutine(FlyAway());

        Debug.Log("✅ Attack sequence complete");
        // After landing, both isFlying and isAttacking will be false
        // Boss will resume walking towards player in BossAI()
        isAttacking = false;
    }

    IEnumerator CloseRangeAttack()
    {
        Debug.Log("🗡️ FlyingBoss: Close Range Attack!");

        if (anim != null)
            anim.SetTrigger("Punch");

        // Attack windup
        yield return new WaitForSeconds(0.3f);

        // Use attackPoint to check for hits
        if (attackPoint != null)
        {
            // Detect enemies in range of attack point
            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

            foreach (Collider2D hit in hitPlayers)
            {
                if (hit.CompareTag("Player"))
                {
                    // Use the boss's damage value, not the player's
                    DealDamageToPlayer(hit.gameObject);
                    Debug.Log("💥 Close attack hit player!");
                }
            }
        }
        else
        {
            // Fallback to distance check if attackPoint is not set
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer <= closeRangeThreshold)
                {
                    DealDamageToPlayer(player.gameObject);
                    Debug.Log("💥 Close attack hit player!");
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator RangedAttack()
    {
        Debug.Log("🏹 FlyingBoss: Ranged Attack!");

        if (anim != null)
            anim.SetTrigger("Tentacle");

        // Attack windup
        yield return new WaitForSeconds(0.4f);

        // Spawn projectile towards player
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // You can spawn a projectile here
            // Example: Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            // Set projectile direction and damage

            Debug.Log("🎯 Ranged projectile fired!");
        }

        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator FlyAway()
    {
        isFlying = true;
        Debug.Log("🦅 FlyingBoss: Starting fly away!");

        if (anim != null)
        {
            anim.SetBool("IsGrounded", false);
            anim.SetBool("IsMoving", true);
        }

        // Calculate direction away from player (horizontal only)
        float directionAwayFromPlayer = Mathf.Sign(transform.position.x - player.position.x);
        if (directionAwayFromPlayer == 0) directionAwayFromPlayer = 1; // Default right if directly above/below

        // Target position: 5 units away horizontally, but clamp within boundaries
        float targetX = transform.position.x + (directionAwayFromPlayer * 5f);
        targetX = Mathf.Clamp(targetX, minX, maxX);

        float startX = transform.position.x;
        float distanceToFly = Mathf.Abs(targetX - startX);

        Debug.Log($"⬆️ Flying away from player. Current X: {startX}, Target X: {targetX}, Distance: {distanceToFly}");

        // PHASE 1: Fly up and away horizontally
        float flyUpDuration = 0.5f; // Fly up for 0.5 seconds
        float flyTimer = 0f;

        while (flyTimer < flyUpDuration)
        {
            if (rb != null)
            {
                // Move up and away from player
                rb.linearVelocity = new Vector2(directionAwayFromPlayer * flySpeed, flySpeed);
            }

            flyTimer += Time.deltaTime;
            yield return null;
        }

        // PHASE 2: Continue flying horizontally until we reach target distance or boundary
        while (Mathf.Abs(transform.position.x - startX) < distanceToFly &&
               transform.position.x > minX && transform.position.x < maxX)
        {
            if (rb != null)
            {
                // Keep flying horizontally away
                rb.linearVelocity = new Vector2(directionAwayFromPlayer * flySpeed, 0);
            }
            yield return null;
        }

        Debug.Log($"⬇️ Reached target or boundary, now descending. Current X: {transform.position.x}");

        // PHASE 3: Descend until grounded (with safety timeout)
        float descendSpeed = 8f;
        float maxDescentTime = 3f;
        float descentTimer = 0f;

        while (!isGrounded && descentTimer < maxDescentTime)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.down * descendSpeed;
            }

            descentTimer += Time.deltaTime;
            yield return null;
        }

        if (!isGrounded)
        {
            Debug.LogWarning("⚠️ Boss didn't detect ground in time! Forcing landing.");
        }

        // PHASE 4: Stop and set grounded
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("IsGrounded", true);
            anim.SetBool("IsMoving", false);
        }

        isFlying = false;
        Debug.Log("✅ FlyingBoss: Landed, ready to walk again");
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"💀 {bossName} defeated!");

        isAttacking = false;
        isFlying = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Death");

        if (waveManager != null)
        {
            waveManager.OnBossDied(this);
        }

        // Play death dialogue, then spawn portal
        if (DialogueManager.Instance != null && deathDialogue != null)
        {
            Debug.Log($"Playing {bossName} death dialogue...");
            DialogueManager.Instance.Play(deathDialogue, OnDeathDialogueComplete);
        }
        else
        {
            Debug.LogWarning($"⚠️ No death dialogue for {bossName}! Proceeding to post-death...");
            OnDeathDialogueComplete();
        }
    }

    private void OnDeathDialogueComplete()
    {
        Debug.Log($"Death dialogue complete for {bossName}. Handling post-death...");

        // Update GameManager (customize based on your act/level system)
        if (GameManager.Instance != null)
        {
            // Example: GameManager.Instance.OnFlyingBossDefeated();
            GameManager.Instance.SaveProgress();
            Debug.Log("✅ Boss cleared!");
        }
        else
        {
            Debug.LogError("❌ GameManager not found! Cannot mark boss as cleared!");
        }

        // Destroy boss after delay
        Destroy(gameObject, 2f);
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();

        // Make boss more aggressive in Phase 2
        attackCooldown *= 0.75f; // Attack more frequently
        walkSpeed *= 1.3f;
        flySpeed *= 1.2f;

        Debug.Log($"{bossName} entered Phase 2!");
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize close range threshold
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeRangeThreshold);

        // Visualize attack point and range
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        // Visualize ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Visualize boundaries
        Gizmos.color = Color.yellow;
        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
