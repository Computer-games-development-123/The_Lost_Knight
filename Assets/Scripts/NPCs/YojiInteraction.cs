using UnityEngine;
using TMPro;

public class YojiInteraction : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueUI;              // Panel with background
    public TextMeshProUGUI dialogueText;       // TMP text inside the panel
    public GameObject interactionPrompt;       // Optional "Press F" prompt

    [Header("Script References")]
    public YojiGateBarrier gateBarrier;        // Barrier that opens after first talk
    public StoreController storeController;    // Store logic (optional)

    [Header("Dialogue Texts")]
    [TextArea(3, 6)]
    public string openingDialogue =
        "You're Lost?\n" +
        "Well, If you're looking for a way out of the forest, you should head right\n" +
        "But you will find the way out quite challenging\n" +
        "Stay Safe.";

    [TextArea(3, 6)]
    public string postGeorgeDialogue =
        "So you found George ehh?\n" +
        "Yea, I don't like him either\n" +
        "Take this and beat him now.";

    private bool playerInRange = false;
    private bool hasGivenUpgrade = false;      // Local, only for this run
    private bool isDialogueActive = false;
    private bool pendingOpenGate = false;      // True when first dialogue is showing

    // Convenience accessor
    private GameManager GM => GameManager.Instance;

    void Start()
    {
        // Hide UI at start
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (GM == null) return; // Failsafe

        // Open dialogue / interact
        if (playerInRange && Input.GetKeyDown(KeyCode.F) && !isDialogueActive)
        {
            HandleInteraction();
            return;
        }

        // Close dialogue
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Escape)))
        {
            CloseDialogue();
        }
    }

    private void HandleInteraction()
    {
        if (GM == null)
        {
            Debug.LogError("GameManager not found! Make sure there is exactly one GameManager in the scene.");
            return;
        }

        // opening dialogue + open barrier on close
        if (ShouldShowOpeningDialogue())
        {
            pendingOpenGate = true; // Open gate when dialogue closes
            ShowDialogue(openingDialogue);
            return;
        }

        // after first death to George (give upgrade once)
        if (ShouldShowPostGeorgeDialogue())
        {
            ShowDialogue(postGeorgeDialogue);

            GM.hasSpecialSwordUpgrade = true;
            hasGivenUpgrade = true;

            if (storeController != null)
                storeController.UnlockStore();

            return;
        }

        //  DEFAULT – open store / fallback
        OpenStoreOrWarn();
    }


    // First time ever talking to Yoji in this run
    private bool ShouldShowOpeningDialogue()
    {
        return !GM.HasTalkedTo("Yoji");
    }

    // After the player has died to George, but before getting the sword upgrade
    private bool ShouldShowPostGeorgeDialogue()
    {
        return GM.hasDiedToGeorge           // Player actually met & died to George
               && !GM.hasSpecialSwordUpgrade
               && !hasGivenUpgrade;
    }

    private void OpenStoreOrWarn()
    {
        if (storeController != null)
        {
            storeController.OpenStore();
        }
        else
        {
            Debug.LogWarning("Store Controller not assigned on YojiInteraction!");
        }
    }


    private void ShowDialogue(string text)
    {
        isDialogueActive = true;

        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
        }
        else
        {
            Debug.LogError("DialogueUI is not assigned in Inspector on YojiInteraction!");
        }

        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
        else
        {
            Debug.LogError("DialogueText is not assigned in Inspector on YojiInteraction!");
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        Time.timeScale = 0f; // Pause game while reading
    }

    private void CloseDialogue()
    {
        isDialogueActive = false;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (interactionPrompt != null && playerInRange)
            interactionPrompt.SetActive(true);

        Time.timeScale = 1f;

        // If this was the *first* dialogue, now we actually open the gate
        if (pendingOpenGate)
        {
            pendingOpenGate = false;

            if (gateBarrier != null)
            {
                gateBarrier.OnYojiDialogueComplete();
            }
            else
            {
                Debug.LogError("GateBarrier reference missing on YojiInteraction!");
            }
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        Debug.Log("Player entered Yoji's trigger zone");

        if (interactionPrompt != null && !isDialogueActive)
            interactionPrompt.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        Debug.Log("Player left Yoji's trigger zone");

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    // Just to see Yoji's interaction zone in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.size);
        }
    }
}
