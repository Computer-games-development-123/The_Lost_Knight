using UnityEngine;
using System.Collections;

public class GeorgeBoss : BossBase
{
    [Header("George Specific")]
    // We no longer rely on this across scenes – GameManager state is the truth
    public bool isFirstEncounter = true;

    [Tooltip("Canvas or panel shown during George's first encounter taunt.")]
    public GameObject dialogueCanvas;

    [Header("Charge Attack")]
    public float chargeSpeed = 8f;
    public float chargeCooldown = 3f;
    private float lastChargeTime;
    private bool isCharging = false;

    protected override void OnBossStart()
    {
        base.OnBossStart();
        bossName = "George";

        if (GameManager.Instance != null)
        {
            // George is invulnerable until the player gets the special sword upgrade
            isInvulnerable = !GameManager.Instance.hasSpecialSwordUpgrade;
        }
        else
        {
            Debug.LogError("GameManager.Instance is null in GeorgeBoss!");
        }

        // Only run the first-time cinematic if the player still doesn't have the upgrade
        if (isInvulnerable)
        {
            StartCoroutine(FirstEncounterSequence());
        }
        else
        {
            // Make sure any first-encounter UI is hidden in real fights
            if (dialogueCanvas != null)
                dialogueCanvas.SetActive(false);
        }
    }

    IEnumerator FirstEncounterSequence()
    {
        // Small delay so player has time to hit a bit and realize George is invulnerable
        yield return new WaitForSeconds(5f);

        // Show George's taunt dialogue
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(true);
            // Here you can hook a TextMeshPro text and set it to:
            // "HAHA... You're weak."
        }

        yield return new WaitForSeconds(2f);

        // Mark that the player has died to George (for Yoji's next dialogue)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.hasDiedToGeorge = true;
        }

        // Instant kill player
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(9999); // should trigger your normal death/respawn flow
            }
        }

        // Local flag so this specific instance won't run the sequence again
        isFirstEncounter = false;
    }

    protected override void OnInvulnerableHit()
    {
        base.OnInvulnerableHit();
        // Play "clang" sound effect / feedback
        Debug.Log("George is invulnerable! Your attacks have no effect.");
    }

    protected override void BossAI()
    {
        // During the first-encounter cinematic George doesn't fight normally
        if (isInvulnerable)
            return;

        if (player == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Charge attack
        if (Time.time >= lastChargeTime + chargeCooldown && distanceToPlayer > 3f && !isCharging)
        {
            StartCoroutine(ChargeAttack());
        }
        else if (!isCharging)
        {
            // Normal movement / base AI
            base.BossAI();
        }
    }

    IEnumerator ChargeAttack()
    {
        isCharging = true;
        lastChargeTime = Time.time;

        // Wind-up animation
        if (anim != null)
            anim.SetTrigger("ChargeWindup");

        yield return new WaitForSeconds(0.5f);

        // Charge!
        if (player != null)
        {
            Vector2 chargeDirection = (player.position - transform.position).normalized;
            rb.linearVelocity = chargeDirection * chargeSpeed;
        }

        if (anim != null)
            anim.SetTrigger("Charging");

        yield return new WaitForSeconds(1f);

        // Stop charge
        rb.linearVelocity = Vector2.zero;
        isCharging = false;
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        chargeCooldown *= 0.7f; // Charge more frequently
        chargeSpeed *= 1.2f;
    }
}
