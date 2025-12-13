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

    [Header("Portal Spawning")]
    public PostBossPortalSpawner portalSpawner; // Assign in Inspector

    protected override void OnBossStart()
    {
        base.OnBossStart();
        bossName = "George";

        if (GameManager.Instance != null)
        {
            bool hasUpgrade = GameManager.Instance.hasSpecialSwordUpgrade;
            isInvulnerable = !hasUpgrade;

            if (!hasUpgrade)
            {
                spawnDialogue = null;
            }
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null in GeorgeBoss! Assuming first encounter (invulnerable).");
            isInvulnerable = true;
            spawnDialogue = null;
        }

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
        isCharging = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Taunt");

        if (DialogueManager.Instance != null && firstEncounterDialogue != null)
        {
            bool done = false;
            DialogueManager.Instance.Play(firstEncounterDialogue, () => done = true);

            while (!done)
                yield return null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.hasDiedToGeorge = true;
            GameManager.Instance.SaveProgress();
        }

        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(9999);
            }
        }

        isFirstEncounter = false;
    }

    protected override void BossAI()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (Time.time >= lastChargeTime + chargeCooldown && distanceToPlayer > 3f && !isCharging)
        {
            StartCoroutine(ChargeAttack());
        }
        else if (!isCharging)
        {
            base.BossAI();
        }
    }

    IEnumerator ChargeAttack()
    {
        isCharging = true;
        lastChargeTime = Time.time;

        if (anim != null)
            anim.SetTrigger("ChargeWindup");

        yield return new WaitForSeconds(0.5f);

        if (player != null)
        {
            Vector2 chargeDirection = (player.position - transform.position).normalized;
            rb.linearVelocity = chargeDirection * chargeSpeed;
        }

        if (anim != null)
            anim.SetTrigger("Charging");

        yield return new WaitForSeconds(1f);

        rb.linearVelocity = Vector2.zero;
        isCharging = false;
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{bossName} defeated!");

        isCharging = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Death");

        if (waveManager != null)
        {
            waveManager.OnBossDied(this);
        }

        // Play death dialogue, then SPAWN PORTAL (not load scene!)
        if (DialogueManager.Instance != null && deathDialogue != null)
        {
            DialogueManager.Instance.Play(deathDialogue, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnGeorgeDefeated();
                    GameManager.Instance.SaveProgress();
                }

                // SPAWN THE PORTAL instead of loading scene
                if (portalSpawner != null)
                {
                    portalSpawner.SpawnPortal();
                }
                else
                {
                    Debug.LogError("Portal spawner not assigned! Falling back to direct scene load.");
                    SceneManager.LoadScene("GreenToRed");
                }
            });
        }
        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGeorgeDefeated();
                GameManager.Instance.SaveProgress();
            }

            // No dialogue, spawn portal immediately
            if (portalSpawner != null)
            {
                portalSpawner.SpawnPortal();
            }
            else
            {
                SceneManager.LoadScene("GreenToRed");
            }
        }
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        chargeCooldown *= 0.7f;
        chargeSpeed *= 1.2f;
    }
}