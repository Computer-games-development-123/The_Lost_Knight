using UnityEngine;

/// <summary>
/// Handles Yoji's DIALOGUE interactions (opening dialogue, post-George dialogue).
/// This is SEPARATE from the store interaction.
/// Attach this to Yoji GameObject.
/// </summary>
public class YojiDialogueHandler : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueData openingDialogue;
    public DialogueData postGeorgeDialogue;

    [Header("Interaction UI")]
    public GameObject interactionPrompt; // "Press F to Talk"

    [Header("References")]
    public YojiGateBarrier gateBarrier;

    private bool playerInRange = false;
    private GameManager GM => GameManager.Instance;
    private DialogueManager DM => DialogueManager.Instance;

    private void Awake()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
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
        if (!GM.HasTalkedTo("Yoji"))
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
        if (!GM.HasTalkedTo("Yoji"))
        {
            if (openingDialogue != null)
            {
                DM.Play(openingDialogue, OnOpeningDialogueComplete);
            }
            else
            {
                Debug.LogWarning("YojiDialogueHandler: openingDialogue is not assigned!");
                // Still mark as talked and open barrier
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
        // Mark that player has talked to Yoji
        GM.SetYojiTalked();

        // Mark opening dialogue as seen
        GM.hasSeenOpeningDialogue = true;

        // Open the barrier to Green Forest
        if (gateBarrier != null)
        {
            gateBarrier.OnYojiDialogueComplete();
        }

        Debug.Log("Opening dialogue complete - barrier opened");

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
        // Give the special sword upgrade (makes blade blue)
        GM.hasSpecialSwordUpgrade = true;

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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // Show prompt if there's dialogue available
        if (ShouldShowDialoguePrompt() && !DM.IsDialogueActive)
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