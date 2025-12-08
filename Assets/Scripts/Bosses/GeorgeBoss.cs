using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GeorgeBoss : BossBase
{
    [Header("George Specific")]
    public bool isFirstEncounter = true;

    [Header("George Dialogues")]
    public DialogueData firstEncounterDialogue;
    public GameObject dialogueCanvas;

    [Header("First Encounter Settings")]
    public int hitsToTriggerTaunt = 5;
    private int invulnerableHitCount = 0;
    private bool firstEncounterSequenceStarted = false;

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
            bool hasUpgrade = GameManager.Instance.hasSpecialSwordUpgrade;

            // First encounter (no upgrade yet) -> George is invulnerable
            isInvulnerable = !hasUpgrade;

            // If it's *not* first encounter, we want spawnDialogue (second intro) to play.
            if (!hasUpgrade)
            {
                // First encounter: no spawnDialogue yet
                spawnDialogue = null;
            }
        }
        else
        {
            // When testing this scene directly, assume "first encounter"
            Debug.LogWarning("GameManager.Instance is null in GeorgeBoss! Assuming first encounter (invulnerable).");
            isInvulnerable = true;
            spawnDialogue = null;
        }

        // Hide any old canvas-based dialogue
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }


    protected override void OnInvulnerableHit()
    {
        base.OnInvulnerableHit();

        if (!isInvulnerable) return;

        invulnerableHitCount++;
        Debug.Log($"George was hit while invulnerable {invulnerableHitCount}/{hitsToTriggerTaunt}");

        if (!firstEncounterSequenceStarted && invulnerableHitCount >= hitsToTriggerTaunt)
        {
            firstEncounterSequenceStarted = true;
            StartCoroutine(FirstEncounterTauntAndKillPlayer());
        }
    }

    private IEnumerator FirstEncounterTauntAndKillPlayer()
    {
        // Stop movement / charge
        isCharging = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Optionally play some animation here, e.g. "Taunt"
        if (anim != null)
            anim.SetTrigger("Taunt");

        // Play George's taunt dialogue
        if (DialogueManager.Instance != null && firstEncounterDialogue != null)
        {
            bool done = false;
            DialogueManager.Instance.Play(firstEncounterDialogue, () => done = true);

            // Wait until the dialogue is finished (unscaled time, DialogueManager handles pause)
            while (!done)
                yield return null;
        }

        // Mark that the player has died to George, so Yoji can react
        if (GameManager.Instance != null)
        {
            GameManager.Instance.hasDiedToGeorge = true;
            GameManager.Instance.SaveProgress();
        }

        // Kill the player using normal health system
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(9999);  // triggers normal death + respawn flow
            }
        }

        isFirstEncounter = false;
    }


    protected override void BossAI()
    {
        // During the first-encounter cinematic George doesn't fight normally
        if (isDead)
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
    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{bossName} defeated!");

        // Stop all movement and attacks
        isCharging = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Death");

        // Tell wave manager if needed (so waves know boss is dead)
        if (waveManager != null)
        {
            waveManager.OnBossDied(this);
        }

        // Play death dialogue, then teleport
        if (DialogueManager.Instance != null && deathDialogue != null)
        {
            DialogueManager.Instance.Play(deathDialogue, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.act1Cleared = true;  // or another flag you prefer
                    GameManager.Instance.SaveProgress();
                }

                SceneManager.LoadScene("GreenForestToRedForestScene");
            });
        }
        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.act1Cleared = true;
                GameManager.Instance.SaveProgress();
            }

            SceneManager.LoadScene("GreenForestToRedForestScene");
        }
    }


    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        chargeCooldown *= 0.7f; // Charge more frequently
        chargeSpeed *= 1.2f;
    }
}
