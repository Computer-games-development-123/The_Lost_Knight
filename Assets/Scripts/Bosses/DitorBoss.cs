using UnityEngine;
using System.Collections;

public class DitorBoss : BossBase
{
    [Header("Ditor Attack Settings")]
    [Tooltip("Distance ranges for different attacks")]
    public float closeRangeMax = 2f;
    public float midRangeMax = 4f;

    [Tooltip("Special attack (combo) settings")]
    public float comboAttackCooldown = 8f;
    public float comboAttackRange = 6f;
    public float comboTelegraphDuration = 0.5f;

    [Tooltip("Attack cooldowns")]
    public float attack1Cooldown = 2f;
    public float attack2Cooldown = 2.5f;
    public float attack3Cooldown = 3f;
    public float postAttackDelay = 1f;

    [Header("Attack Hit Colliders")]
    [Tooltip("Colliders for each attack type")]
    public Collider2D attack1HitCollider;
    public Collider2D attack2HitCollider;
    public Collider2D comboHitPoint1;
    public Collider2D comboHitPoint2;

    [Header("Phase 2 Settings")]
    [Tooltip("Aura object to activate on phase 2")]
    public DialogueData Phase2Dialogue;
    public GameObject Aura;
    public float auraDuration = 1f;
    private bool inTransition = false;

    [Header("Ending Dialogue")]
    [Tooltip("The dialogue that triggers the ending choice (should be the 'Ending' dialogue)")]
    public DialogueData endingDialogue;

    private float lastAttackTime;
    private float lastComboAttackTime;
    private float currentAttackCooldown;
    private bool isAttacking = false;
    private bool isTelegraphing = false;
    private bool isRecovering = false;
    private float distanceToPlayer;
    private bool isMoving = false;
    private bool hasDealtDamageThisAttack = false;

    protected override void Start()
    {
        base.Start();
        lastComboAttackTime = -comboAttackCooldown;
        lastAttackTime = -attack1Cooldown;

        DisableAllHitColliders();
    }

    protected override void Update()
    {
        if (isDead || player == null || inTransition) return;

        distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (currentHP <= maxHP / 2 && !isPhase2)
        {
            EnterPhase2();
        }

        UpdateMovementAnimation();

        BossAI();
    }

    private void UpdateMovementAnimation()
    {
        isMoving = !isAttacking && !isTelegraphing && !isRecovering && Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        if (anim != null)
            anim.SetBool("IsMoving", isMoving);
    }

    protected override void BossAI()
    {
        if (isAttacking || isTelegraphing || isRecovering)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        FacePlayer();

        if (Time.time >= lastComboAttackTime + comboAttackCooldown && distanceToPlayer <= comboAttackRange)
        {
            StartCoroutine(PerformComboAttack());
            return;
        }

        if (Time.time >= lastAttackTime + currentAttackCooldown)
        {
            if (distanceToPlayer <= closeRangeMax)
            {
                StartCoroutine(PerformAttack1());
            }
            else if (distanceToPlayer <= midRangeMax)
            {
                StartCoroutine(PerformAttack2());
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    private void FacePlayer()
    {
        if (player == null || isAttacking || isTelegraphing || isRecovering)
            return;

        float directionToPlayer = player.position.x - transform.position.x;

        if (directionToPlayer > 0 && !facingRight)
            Flip();
        else if (directionToPlayer < 0 && facingRight)
            Flip();
    }

    private void MoveTowardsPlayer()
    {
        if (isAttacking || isTelegraphing || isRecovering)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

            if (!isAttacking && !isTelegraphing && !isRecovering)
            {
                if (direction.x > 0 && !facingRight)
                    Flip();
                else if (direction.x < 0 && facingRight)
                    Flip();
            }
        }
    }

    // Attack 1: Close Range
    private IEnumerator PerformAttack1()
    {
        isAttacking = true;
        hasDealtDamageThisAttack = false;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Attack1");

        lastAttackTime = Time.time;
        currentAttackCooldown = attack1Cooldown;

        // Wait for animation to complete
        yield return new WaitForSeconds(0.6f);

        DisableAllHitColliders();

        isAttacking = false;

        yield return StartCoroutine(PostAttackRecovery());
    }

    // Attack 2: Mid Range
    private IEnumerator PerformAttack2()
    {
        isAttacking = true;
        hasDealtDamageThisAttack = false;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Attack2");

        lastAttackTime = Time.time;
        currentAttackCooldown = attack2Cooldown;

        // Wait for animation to complete
        yield return new WaitForSeconds(0.8f);

        DisableAllHitColliders();

        isAttacking = false;

        yield return StartCoroutine(PostAttackRecovery());
    }

    // Attack 3: Combo Attack with Telegraph
    private IEnumerator PerformComboAttack()
    {
        isTelegraphing = true;
        rb.linearVelocity = Vector2.zero;

        // Telegraph warning - flash effect
        StartCoroutine(TelegraphFlash());

        yield return new WaitForSeconds(comboTelegraphDuration);
        Collider2D col = GetComponent<Collider2D>();
        isTelegraphing = false;
        isAttacking = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;
        hasDealtDamageThisAttack = false;

        if (anim != null)
            anim.SetTrigger("Attack3");

        lastComboAttackTime = Time.time;
        lastAttackTime = Time.time;
        currentAttackCooldown = attack3Cooldown;

        // Wait for animation to complete
        yield return new WaitForSeconds(1.1f);
        col.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        DisableAllHitColliders();

        isAttacking = false;

        yield return StartCoroutine(PostAttackRecovery());
    }

    // Recovery period after attack
    private IEnumerator PostAttackRecovery()
    {
        isRecovering = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(postAttackDelay);

        isRecovering = false;
    }

    // Telegraph flash warning
    private IEnumerator TelegraphFlash()
    {
        float elapsed = 0f;
        Color originalColor = sr.color;

        while (elapsed < comboTelegraphDuration)
        {
            // Flash between white and yellow
            sr.color = Color.Lerp(Color.white, Color.yellow, Mathf.PingPong(elapsed * 8f, 1f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        sr.color = originalColor;
    }

    public void EnableAttack1HitCollider()
    {
        if (attack1HitCollider != null)
        {
            attack1HitCollider.enabled = true;
            Debug.Log("Attack1 hit collider ENABLED");
            StartCoroutine(DisableHitColliderAfterDelay(attack1HitCollider, 0.25f));
        }
    }

    public void EnableAttack2HitCollider()
    {
        if (attack2HitCollider != null)
        {
            attack2HitCollider.enabled = true;
            Debug.Log("Attack2 hit collider ENABLED");
            StartCoroutine(DisableHitColliderAfterDelay(attack2HitCollider, 0.3f));
        }
    }

    public void EnableComboHitPoint1()
    {
        if (comboHitPoint1 != null)
        {
            comboHitPoint1.enabled = true;
            Debug.Log("Combo hit point 1 ENABLED");
            StartCoroutine(DisableHitColliderAfterDelay(comboHitPoint1, 0.25f));
        }
    }

    public void EnableComboHitPoint2()
    {
        if (comboHitPoint2 != null)
        {
            comboHitPoint2.enabled = true;
            Debug.Log("Combo hit point 2 ENABLED");
            StartCoroutine(DisableHitColliderAfterDelay(comboHitPoint2, 0.25f));
        }
    }

    private IEnumerator DisableHitColliderAfterDelay(Collider2D hitCollider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
            Debug.Log($"{hitCollider.name} DISABLED");
        }
    }

    public void DisableCurrentHitCollider()
    {
        DisableAllHitColliders();
    }

    private void DisableAllHitColliders()
    {
        if (attack1HitCollider != null) attack1HitCollider.enabled = false;
        if (attack2HitCollider != null) attack2HitCollider.enabled = false;
        if (comboHitPoint1 != null) comboHitPoint1.enabled = false;
        if (comboHitPoint2 != null) comboHitPoint2.enabled = false;
    }

    public void OnAttackHitPlayer()
    {
        Debug.Log($"OnAttackHitPlayer called. hasDealtDamageThisAttack: {hasDealtDamageThisAttack}");

        if (isDead || hasDealtDamageThisAttack) return;

        // Check if any hit collider is currently active
        if ((attack1HitCollider != null && attack1HitCollider.enabled) ||
            (attack2HitCollider != null && attack2HitCollider.enabled) ||
            (comboHitPoint1 != null && comboHitPoint1.enabled) ||
            (comboHitPoint2 != null && comboHitPoint2.enabled))
        {
            Debug.Log("Dealing damage to player!");
            DealDamageToPlayer();
            hasDealtDamageThisAttack = true;
        }
        else
        {
            Debug.Log("No active hit collider found");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"OnTriggerEnter2D called with: {collision.gameObject.name}, Tag: {collision.tag}");

        if (isDead)
        {
            Debug.Log("Ditor is dead, ignoring trigger");
            return;
        }

        if (hasDealtDamageThisAttack)
        {
            Debug.Log("Already dealt damage this attack, ignoring");
            return;
        }

        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player detected in trigger!");

            if (attack1HitCollider != null && attack1HitCollider.enabled)
            {
                Debug.Log("Attack1 collider is enabled - dealing damage");
                DealDamageToPlayer();
                hasDealtDamageThisAttack = true;
            }
            else if (attack2HitCollider != null && attack2HitCollider.enabled)
            {
                Debug.Log("Attack2 collider is enabled - dealing damage");
                DealDamageToPlayer();
                hasDealtDamageThisAttack = true;
            }
            else if (comboHitPoint1 != null && comboHitPoint1.enabled)
            {
                Debug.Log("Combo1 collider is enabled - dealing damage");
                DealDamageToPlayer();
                hasDealtDamageThisAttack = true;
            }
            else if (comboHitPoint2 != null && comboHitPoint2.enabled)
            {
                Debug.Log("Combo2 collider is enabled - dealing damage");
                DealDamageToPlayer();
                hasDealtDamageThisAttack = true;
            }
            else
            {
                Debug.Log("No enabled hit collider found!");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead || hasDealtDamageThisAttack) return;

        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player in OnTriggerStay2D");

            if (attack1HitCollider != null && attack1HitCollider.enabled)
            {
                Debug.Log("Attack1 collider active (Stay) - dealing damage");
                DealDamageToPlayer();
                hasDealtDamageThisAttack = true;
            }
            else if (attack2HitCollider != null && attack2HitCollider.enabled)
            {
                Debug.Log("Attack2 collider active (Stay) - dealing damage");
                DealDamageToPlayer();
                hasDealtDamageThisAttack = true;
            }
            else if (comboHitPoint1 != null && comboHitPoint1.enabled)
            {
                Debug.Log("Combo1 collider active (Stay) - dealing damage");
                DealDamageToPlayer();
                hasDealtDamageThisAttack = true;
            }
            else if (comboHitPoint2 != null && comboHitPoint2.enabled)
            {
                Debug.Log("Combo2 collider active (Stay) - dealing damage");
                DealDamageToPlayer();
                hasDealtDamageThisAttack = true;
            }
        }
    }

    protected override void Flip()
    {
        if (isAttacking || isTelegraphing || isRecovering)
            return;

        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    protected override void EnterPhase2()
    {
        StartCoroutine(Phase2());
        base.EnterPhase2();

        attack1Cooldown *= 0.5f;
        attack2Cooldown *= 0.5f;
        attack3Cooldown *= 0.5f;
        comboAttackCooldown *= 0.7f;

        Debug.Log("Ditor is now enraged!");
    }

    private IEnumerator Phase2()
    {
        if (isAttacking) yield return new WaitForSeconds(1f);
        inTransition = true;
        if (Phase2Dialogue)
            DialogueManager.Instance.Play(Phase2Dialogue);
        yield return new WaitForSeconds(0.5f);
        Aura.SetActive(true);
        yield return new WaitForSeconds(auraDuration);
        Aura.SetActive(false);
        inTransition = false;
    }

    public override void TakeDamage(int damageAmount)
    {
        if (!inTransition)
            base.TakeDamage(damageAmount);
    }

    protected override void Die()
    {
        isDead = true;

        if (anim != null)
            anim.SetTrigger("Death");

        Debug.Log($"{bossName} defeated!");

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

        Debug.Log("Ditor defeated - ending sequence will start after death dialogue");
    }

    protected override void OnDeathDialogueComplete()
    {
        // Award coins to player
        if (player != null && coinsReward > 0)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddCoins(coinsReward);
                Debug.Log($"Player received {coinsReward} coins for defeating {bossName}!");
            }
            else
            {
                Debug.LogWarning($"PlayerInventory component not found on player - could not award {coinsReward} coins!");
            }
        }

        if (DialogueManager.Instance != null && slainDialogue != null)
        {
            DialogueManager.Instance.Play(slainDialogue);
        }

        if (DialogueManager.Instance != null && endingDialogue != null)
        {
            Debug.Log("Playing ending dialogue...");
            DialogueManager.Instance.Play(endingDialogue, OnEndingDialogueComplete);
        }
        else
        {
            Debug.LogError("DitorBoss: Ending dialogue not assigned!");
        }
    }

    private void OnEndingDialogueComplete()
    {
        Debug.Log("Ending dialogue complete - showing player choice");

        if (EndingChoiceManager.Instance != null)
        {
            EndingChoiceManager.Instance.ShowEndingChoice();
        }
        else
        {
            Debug.LogError("DitorBoss: EndingChoiceManager.Instance not found in scene!");
        }

    }
}