using UnityEngine;
using System.Collections;

public class GeorgeBoss : BossBase
{
    [Header("George Dialogues")]
    public DialogueData firstEncounterDialogue;
    public DialogueData secondEncounterDialogue;

    [Header("First Encounter Settings")]
    public int hitsToTriggerTaunt = 5;
    private int invulnerableHitCount = 0;
    private bool firstEncounterSequenceStarted = false;

    [Header("Attack Settings")]
    public float attackCooldown = 4f;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    [Header("Range Detection")]
    public float closeRangeThreshold = 2f;
    public Transform attackPoint;
    public float attackRange = 1f;

    [Header("Flying Settings")]
    public float flySpeed = 6f;
    private bool isFlying = false;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;

    [Header("Boundaries")]
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded = false;

    protected override void Update()
    {
        // Ground detection
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (anim != null)
            {
                anim.SetBool("IsGrounded", isGrounded);
            }
        }

        // Boundary enforcement
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;

        base.Update();
    }

    protected override void OnBossStart()
    {
        //base.OnBossStart();
        bossName = "George";

        bool hasUpgrade = false;

        if (GameManager.Instance != null)
        {
            hasUpgrade = GameManager.Instance.GetFlag(GameFlag.hasUpgradedSword);
        }
        isInvulnerable = !hasUpgrade;
        if (hasUpgrade) DialogueManager.Instance.Play(secondEncounterDialogue);
        else DialogueManager.Instance.Play(spawnDialogue);

        ResetInvulnerableHitCount();
    }


    private void ResetInvulnerableHitCount()
    {
        invulnerableHitCount = 0;
        firstEncounterSequenceStarted = false;
    }

    protected override void OnInvulnerableHit()
    {
        base.OnInvulnerableHit();

        if (!isInvulnerable) return;

        invulnerableHitCount++;
        Debug.Log($"Hit {invulnerableHitCount}/{hitsToTriggerTaunt}");

        if (!firstEncounterSequenceStarted && invulnerableHitCount >= hitsToTriggerTaunt)
        {
            firstEncounterSequenceStarted = true;
            StartCoroutine(FirstEncounterTauntAndKillPlayer());
        }
    }

    private IEnumerator FirstEncounterTauntAndKillPlayer()
    {
        isAttacking = false;
        isFlying = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetTrigger("Tentacle");
            anim.SetFloat("Speed", 0);
        }

        if (DialogueManager.Instance != null && firstEncounterDialogue != null)
        {
            bool done = false;
            DialogueManager.Instance.Play(firstEncounterDialogue, () => done = true);

            while (!done)
                yield return null;
        }

        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(9999);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDiedToGeorge();
        }

    }

    protected override void BossAI()
    {
        if (isDead || player == null) return;
        if (isAttacking || isFlying) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (Time.time >= lastAttackTime + attackCooldown && isGrounded && distanceToPlayer <= closeRangeThreshold)
        {
            StartCoroutine(AttackSequence());
        }
        else if (isGrounded)
        {
            WalkTowardsPlayer();
        }
    }

    private void WalkTowardsPlayer()
    {
        if (player == null) return;

        float moveDirection = Mathf.Sign(player.position.x - transform.position.x);

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveDirection * walkSpeed, rb.linearVelocity.y);
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(walkSpeed));
            anim.SetBool("IsMoving", true);
        }

        if (moveDirection > 0 && !facingRight)
            Flip();
        else if (moveDirection < 0 && facingRight)
            Flip();
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetFloat("Speed", 0);
            anim.SetBool("IsMoving", false);
        }

        yield return StartCoroutine(CloseRangeAttack());
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FlyAway());

        isAttacking = false;
    }

    IEnumerator CloseRangeAttack()
    {
        if (anim != null)
            anim.SetTrigger("Punch");

        yield return new WaitForSeconds(0.3f);

        if (attackPoint != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                    DealDamageToPlayer();
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator FlyAway()
    {
        isFlying = true;

        if (anim != null)
        {
            anim.SetBool("IsGrounded", false);
            anim.SetFloat("Speed", flySpeed); // Speed > 0 for fly animation
            anim.SetBool("IsMoving", true);
        }

        float direction = Mathf.Sign(transform.position.x - player.position.x);
        if (direction == 0) direction = 1;

        float targetX = Mathf.Clamp(transform.position.x + (direction * 5f), minX, maxX);
        float startX = transform.position.x;
        float distance = Mathf.Abs(targetX - startX);

        // Fly up
        float timer = 0f;
        while (timer < 0.5f)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(direction * flySpeed, flySpeed);
            timer += Time.deltaTime;
            yield return null;
        }

        // Fly horizontal
        while (Mathf.Abs(transform.position.x - startX) < distance &&
               transform.position.x > minX && transform.position.x < maxX)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(direction * flySpeed, 0);
            yield return null;
        }

        // Descend
        float descentTimer = 0f;
        while (!isGrounded && descentTimer < 3f)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.down * 8f;
            descentTimer += Time.deltaTime;
            yield return null;
        }

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("IsGrounded", true);
            anim.SetFloat("Speed", 0);
            anim.SetBool("IsMoving", false);
        }

        isFlying = false;
    }

    protected override void OnDeathDialogueComplete()
    {
        // Call base to handle coins and slain dialogue
        base.OnDeathDialogueComplete();

        // George-specific death logic
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGeorgeDefeated();
            GameManager.Instance.SaveProgress();
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        isAttacking = false;
        isFlying = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetFloat("Speed", 0);
        }
        base.Die();
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();

        attackCooldown *= 0.75f;
        walkSpeed *= 1.3f;
        flySpeed *= 1.2f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeRangeThreshold);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Vector3 tl = new Vector3(minX, maxY, 0);
        Vector3 tr = new Vector3(maxX, maxY, 0);
        Vector3 bl = new Vector3(minX, minY, 0);
        Vector3 br = new Vector3(maxX, minY, 0);

        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
    }
}