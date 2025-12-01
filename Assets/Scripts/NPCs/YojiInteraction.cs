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

    private void Update()
    {
        if (!playerInRange) return;
        if (DialogueManager.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.F) && !DialogueManager.Instance.IsDialogueActive)
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        if (!GM.HasTalkedTo("Yoji"))
        {
            GM.SetYojiTalked();
            interactionPrompt?.SetActive(false);

            DialogueManager.Instance.Play(openingDialogue, () =>
            {
                gateBarrier.OnYojiDialogueComplete();
                if (playerInRange)
                    interactionPrompt?.SetActive(true);
            });
            return;
        }

        if (GM.hasDiedToGeorge && !GM.hasSpecialSwordUpgrade)
        {
            interactionPrompt?.SetActive(false);

            DialogueManager.Instance.Play(postGeorgeDialogue, () =>
            {
                GM.hasSpecialSwordUpgrade = true;
                if (playerInRange)
                    interactionPrompt?.SetActive(true);
            });
            return;
        }

        // default: open store, etc.
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (!DialogueManager.Instance.IsDialogueActive)
            interactionPrompt?.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;

        if (interactionPrompt != null)   // Unity null check
            interactionPrompt.SetActive(false);
    }

}
