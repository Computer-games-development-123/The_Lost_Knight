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

        // Dummy never really dies
        currentHP = MaxHP;

        // Make sure portal starts hidden
        if (portalToEnable != null)
        {
            portalToEnable.SetActive(false);
        }
    }

    protected override void Update()
    {
        // No AI / movement for the dummy
        // (do NOT call base.Update())
    }

    public override void TakeDamage(int damage)
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

        // 1) Dialogue after N hits
        if (!dialoguePlayed && currentHits >= hitsToTriggerDialogue)
        {
            if (DialogueManager.Instance != null && afterHitsDialogue != null)
            {
                DialogueManager.Instance.Play(afterHitsDialogue);
            }

            dialoguePlayed = true;    // only once
        }

        // 2) Unlock portal after enough hits
        if (!portalUnlocked && currentHits >= hitsNeededToUnlockPortal)
        {
            portalUnlocked = true;

            if (portalToEnable != null)
            {
                portalToEnable.SetActive(true);
                Debug.Log("Tutorial: portal back to ForestHub unlocked.");
            }
        }

        // No HP reduction, no death for the dummy
    }

    protected override void OnDrawGizmosSelected()
    {
        // No yellow debug circle for the dummy
    }
}
