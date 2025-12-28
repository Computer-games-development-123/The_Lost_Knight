using UnityEngine;

public class TutorialDummyEnemy : EnemyBase
{
    [Header("Tutorial Settings")]
    public int hitsToTriggerDialogue = 3;          // when to show the tutorial dialogue
    public int hitsNeededToUnlockPortal = 3;       // how many hits to unlock the portal
    public DialogueData afterHitsDialogue;         // "Nice! You hit the dummy" etc.

    [Tooltip("Portal back to ForestHub that should appear only after the dummy is hit enough times.")]
    public GameObject portalToEnable;

    private int currentHits = 0;
    private bool dialoguePlayed = false;
    private bool portalUnlocked = false;

    protected override void Start()
    {
        base.Start();

        // Dummy never really dies - set very high HP
        currentHP = 99999;

        // Make sure portal starts hidden
        if (portalToEnable != null)
        {
            portalToEnable.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ TutorialDummyEnemy: portalToEnable is not assigned! Please drag the Forest_Hub_Portal GameObject to this field in Inspector!");
        }
    }

    protected override void Update()
    {
        // No AI / movement for the dummy
        // (do NOT call base.Update())
    }

    // Override BOTH damage methods to handle hits properly
    public override void TakeDamage(int damage)
    {
        HandleHit();
    }

    public override void TakeDamage(int damage, Vector2 hitDirection)
    {
        HandleHit();
    }

    private void HandleHit()
    {
        if (isDead) return;

        currentHits++;
        Debug.Log($"Tutorial dummy hit count: {currentHits}");

        // Play hurt animation
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        // Flash red briefly
        StartCoroutine(FlashRed());

        // 1) Dialogue after N hits - WITH CALLBACK TO UNLOCK PORTAL
        if (!dialoguePlayed && currentHits >= hitsToTriggerDialogue)
        {
            dialoguePlayed = true;    // Mark as played

            if (DialogueManager.Instance != null && afterHitsDialogue != null)
            {
                // ✅ FIX: Pass callback to unlock portal AFTER dialogue finishes
                DialogueManager.Instance.Play(afterHitsDialogue, OnDialogueComplete);
            }
            else
            {
                // No dialogue? Just unlock portal immediately
                UnlockPortal();
            }
        }

        // No HP reduction, no death for the dummy - just reset to keep it alive
        currentHP = 99999;
    }

    /// <summary>
    /// Called when the tutorial completion dialogue finishes
    /// </summary>
    private void OnDialogueComplete()
    {
        UnlockPortal();
    }

    /// <summary>
    /// Unlocks the portal back to Forest Hub
    /// </summary>
    private void UnlockPortal()
    {
        if (portalUnlocked) return; // Already unlocked
        
        portalUnlocked = true;

        // Set the flag
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(GameFlag.TutorialPortalUnlocked, true);
            GameManager.Instance.SaveProgress();
        }

        // Activate the portal GameObject
        if (portalToEnable != null)
        {
            portalToEnable.SetActive(true);
            Debug.Log("✅ Tutorial: portal back to ForestHub spawned!");
        }
        else
        {
            Debug.LogError("❌ portalToEnable is NOT ASSIGNED! Drag the Forest_Hub_Portal to TutorialDummyEnemy in Inspector!");
        }
    }

    protected override void Die()
    {
        // Tutorial dummy never dies - override to prevent death
        // Do nothing here
    }

    protected override void OnDrawGizmosSelected()
    {
        // No yellow debug circle for the dummy
    }
}