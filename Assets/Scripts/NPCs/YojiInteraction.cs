using UnityEngine;
using TMPro;

public class YojiInteraction : MonoBehaviour
{
    public DialogueData openingDialogue;
    public DialogueData postGeorgeDialogue;
    public GameObject interactionPrompt;
    public YojiGateBarrier gateBarrier;

    private bool playerInRange;
    private GameManager GM => GameManager.Instance;

    private void Awake()
    {
        // Try to auto-find the prompt if not assigned in the Inspector
        if (interactionPrompt == null)
        {
            Transform child = transform.Find("InteractionPrompt");
            if (child != null)
            {
                interactionPrompt = child.gameObject;
            }
        }

        // Make sure it's hidden at start
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private bool CanInteractWithYoji()
    {
        if (GM == null) return false;

        // First-time dialogue
        if (!GM.HasTalkedTo("Yoji"))
            return true;

        // After death to George, before special sword upgrade
        if (GM.hasDiedToGeorge && !GM.hasSpecialSwordUpgrade)
            return true;

        // Otherwise: no need to talk now (no prompt, no interaction)
        return false;
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (!CanInteractWithYoji()) return;
        if (DialogueManager.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.F) && !DialogueManager.Instance.IsDialogueActive)
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        // First time talking to Yoji
        if (!GM.HasTalkedTo("Yoji"))
        {
            GM.SetYojiTalked();
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            DialogueManager.Instance.Play(openingDialogue, () =>
            {
                gateBarrier.OnYojiDialogueComplete();

                // Only re-show prompt if player still needs Yoji and is in range
                if (playerInRange && interactionPrompt != null && CanInteractWithYoji())
                    interactionPrompt.SetActive(true);
            });
            return;
        }

        // After dying to George, before upgrade
        if (GM.hasDiedToGeorge && !GM.hasSpecialSwordUpgrade)
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            DialogueManager.Instance.Play(postGeorgeDialogue, () =>
            {
                GM.hasSpecialSwordUpgrade = true;

                if (playerInRange && interactionPrompt != null && CanInteractWithYoji())
                    interactionPrompt.SetActive(true);
            });
            return;
        }

        // Default: no interaction (store/etc can be added later if you want)
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;

        if (interactionPrompt == null)
        {
            Debug.LogWarning("YojiInteraction: interactionPrompt is missing or was destroyed.");
            // Don't return; the player can still press F if CanInteractWithYoji() is true.
        }

        if (DialogueManager.Instance == null) return;
        if (!CanInteractWithYoji()) return;

        if (interactionPrompt != null && !DialogueManager.Instance.IsDialogueActive)
        {
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
