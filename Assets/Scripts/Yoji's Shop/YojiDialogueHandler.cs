using UnityEngine;

/// <summary>
/// Yoji Dialogue Handler - INDEPENDENT version
/// Sets its own flags directly (doesn't call GameManager.SetYojiTalked())
/// Only depends on GameManager for flag checking
/// </summary>
public class YojiDialogueHandler : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueData openingDialogue;
    public DialogueData postGeorgeDialogue;

    [Header("Interaction UI")]
    public GameObject interactionPrompt; // "Press F to Talk"

    [Header("Portal Control")]
    [Tooltip("The Green Forest portal GameObject - starts INACTIVE, becomes active after dialogue")]
    public GameObject greenForestPortal;

    private bool playerInRange = false;
    private GameManager GM => GameManager.Instance;
    private DialogueManager DM => DialogueManager.Instance;

    private void Awake()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Start()
    {
        // Check if player already talked to Yoji (from save file)
        if (GM != null && GM.GetFlag(GameFlag.YojiFirstDialogueCompleted))
        {
            // Portal should already be open
            if (greenForestPortal != null)
            {
                greenForestPortal.SetActive(true);
                Debug.Log("✅ Player already talked to Yoji - portal active");
            }
        }
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (DM == null) return;
        if (DM.IsDialogueActive) return; // Don't interrupt active dialogue

        // Check if we should show prompt
        if (ShouldShowDialoguePrompt())
        {
            if (interactionPrompt != null && !interactionPrompt.activeSelf)
                interactionPrompt.SetActive(true);

            // Handle input
            if (Input.GetKeyDown(KeyCode.F))
            {
                HandleDialogueInteraction();
            }
        }
        else
        {
            if (interactionPrompt != null && interactionPrompt.activeSelf)
                interactionPrompt.SetActive(false);
        }
    }

    /// <summary>
    /// Determines if Yoji has dialogue available for the player
    /// </summary>
    private bool ShouldShowDialoguePrompt()
    {
        if (GM == null) return false;

        // First time talking to Yoji
        if (!GM.GetFlag(GameFlag.YojiFirstDialogueCompleted))
            return true;

        // After dying to George, before getting upgrade
        if (GM.hasDiedToGeorge && !GM.hasSpecialSwordUpgrade)
            return true;

        return false;
    }

    /// <summary>
    /// Handles the dialogue interaction based on game state
    /// </summary>
    private void HandleDialogueInteraction()
    {
        if (GM == null || DM == null) return;

        // Hide prompt during dialogue
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // CASE 1: First time talking to Yoji (Opening Dialogue)
        if (!GM.GetFlag(GameFlag.YojiFirstDialogueCompleted))
        {
            if (openingDialogue != null)
            {
                DM.Play(openingDialogue, OnOpeningDialogueComplete);
            }
            else
            {
                Debug.LogWarning("YojiDialogueHandler: openingDialogue is not assigned!");
                // Still execute completion logic
                OnOpeningDialogueComplete();
            }
            return;
        }

        // CASE 2: After dying to George, give special sword upgrade
        if (GM.hasDiedToGeorge && !GM.hasSpecialSwordUpgrade)
        {
            if (postGeorgeDialogue != null)
            {
                DM.Play(postGeorgeDialogue, OnPostGeorgeDialogueComplete);
            }
            else
            {
                Debug.LogWarning("YojiDialogueHandler: postGeorgeDialogue is not assigned!");
                OnPostGeorgeDialogueComplete();
            }
            return;
        }
    }

    /// <summary>
    /// Called after the opening dialogue finishes
    /// </summary>
    private void OnOpeningDialogueComplete()
    {
        // ✅ FIXED: Set flag directly (NOT through GameManager.SetYojiTalked()!)
        if (GM != null)
        {
            GM.SetFlag(GameFlag.YojiFirstDialogueCompleted, true);
            GM.SetFlag(GameFlag.OpeningDialogueSeen, true);
        }

        // ✅ ACTIVATE THE GREEN FOREST PORTAL DIRECTLY
        if (greenForestPortal != null)
        {
            greenForestPortal.SetActive(true);
            Debug.Log("✅ Green Forest portal activated!");
        }
        else
        {
            Debug.LogError("❌ greenForestPortal is not assigned! Please drag the portal GameObject to YojiDialogueHandler in Inspector!");
        }

        // Save progress
        if (GM != null)
        {
            GM.SaveProgress();
        }

        Debug.Log("Opening dialogue complete - portal opened");

        // Re-show prompt if player is still in range and has more dialogue
        if (playerInRange && ShouldShowDialoguePrompt())
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    /// <summary>
    /// Called after the post-George dialogue finishes (gives upgrade)
    /// </summary>
    private void OnPostGeorgeDialogueComplete()
    {
        // Give sword upgrade to player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Abilities abilities = player.GetComponent<Abilities>();
            if (abilities != null)
            {
                abilities.UpgradeSword(); // ✅ CORRECT: This is the sword upgrade!
                Debug.Log("✅ Player given sword upgrade - Can now damage George!");
            }
        }

        // Unlock the store
        if (StoreStateManager.Instance != null)
        {
            StoreStateManager.Instance.SetStoreState(StoreStateManager.StoreState.PostGeorge);
        }

        Debug.Log("Post-George dialogue complete - sword upgraded, store unlocked");

        // Notify the store interaction handler that store is now available
        YojiStoreHandler storeHandler = GetComponent<YojiStoreHandler>();
        if (storeHandler != null)
        {
            storeHandler.OnStoreUnlocked();
        }

        // Save progress
        if (GM != null)
        {
            GM.SaveProgress();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // Show prompt if there's dialogue available
        if (ShouldShowDialoguePrompt() && (DM == null || !DM.IsDialogueActive))
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
}