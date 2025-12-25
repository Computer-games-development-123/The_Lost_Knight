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

        // 🔥 CRITICAL: Reset hit counter when George spawns
        // This ensures player always needs 5 hits in ONE attempt
        ResetInvulnerableHitCount();

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    private void ResetInvulnerableHitCount()
    {
        invulnerableHitCount = 0;
        firstEncounterSequenceStarted = false;
        Debug.Log("🔄 George's hit counter reset - player needs 5 fresh hits");
    }

    protected override void OnInvulnerableHit()
    {
        base.OnInvulnerableHit();

        if (!isInvulnerable) return;

        invulnerableHitCount++;
        Debug.Log($"⚔️ George was hit while invulnerable! {invulnerableHitCount}/{hitsToTriggerTaunt}");

        if (!firstEncounterSequenceStarted && invulnerableHitCount >= hitsToTriggerTaunt)
        {
            firstEncounterSequenceStarted = true;
            StartCoroutine(FirstEncounterTauntAndKillPlayer());
        }
    }

    private IEnumerator FirstEncounterTauntAndKillPlayer()
    {
        Debug.Log("🎭 George taunt sequence triggered!");
        
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
                Debug.Log("💀 George kills the player!");
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

        Debug.Log($"💀 {bossName} defeated!");

        isCharging = false;
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

        // Update GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGeorgeDefeated();
            GameManager.Instance.SaveProgress();
            Debug.Log("✅ Act 1 marked as cleared!");
        }
        else
        {
            Debug.LogError("❌ GameManager not found! Cannot mark Act 1 as cleared!");
        }

        // Find and spawn portal (same as Fika)
        PostBossPortalSpawner portalSpawner = FindFirstObjectByType<PostBossPortalSpawner>();
        if (portalSpawner != null)
        {
            Debug.Log("Found PostBossPortalSpawner, spawning portal...");
            portalSpawner.SpawnPortal();
        }
        else
        {
            Debug.LogError("❌ PostBossPortalSpawner not found in scene! Loading scene as fallback...");
            SceneManager.LoadScene("GreenToRed");
        }

        // Destroy George after delay
        Destroy(gameObject, 2f);
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        chargeCooldown *= 0.7f;
        chargeSpeed *= 1.2f;
        
        Debug.Log($"{bossName} entered Phase 2!");
    }
}