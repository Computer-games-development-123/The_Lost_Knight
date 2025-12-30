using UnityEngine;
using TMPro;

/// <summary>
/// Yoji Store Handler - FIXED version
/// Uses GameFlag enum instead of HasTalkedTo() method
/// </summary>
public class YojiStoreHandler : MonoBehaviour
{
    [Header("Store Reference")]
    public ListStoreController storeController;

    [Header("Store UI")]
    public GameObject storePrompt; // "Press E to Shop"
    public TextMeshProUGUI storePromptText;

    [Header("Input")]
    public KeyCode shopKey = KeyCode.E;

    private bool playerInRange = false;
    private GameManager GM => GameManager.Instance;
    private DialogueManager DM => DialogueManager.Instance;
    private StoreStateManager SSM => StoreStateManager.Instance;

    private void Awake()
    {
        if (storePrompt != null)
            storePrompt.SetActive(false);

        // Auto-find store controller if missing
        FindStoreController();
    }

    private void Start()
    {
        GameManagerReadyHelper.RunWhenReady(this, RefreshStorePrompt);
    }

    private void RefreshStorePrompt()
    {
        if (!playerInRange) return;

        bool show = ShouldShowStorePrompt() && (DM == null || !DM.IsDialogueActive);
        if (storePrompt != null)
        {
            storePrompt.SetActive(show);
            if (show) UpdateStorePromptText();
        }
    }


    private void FindStoreController()
    {
        if (storeController != null) return; // Already found

        // Method 1: Try FindFirstObjectByType (searches all scenes including DontDestroyOnLoad)
        storeController = FindFirstObjectByType<ListStoreController>(FindObjectsInactive.Include);

        // Method 2: If that fails, search by GameObject name
        if (storeController == null)
        {
            GameObject storePanel = GameObject.Find("StorePanel");
            if (storePanel != null)
            {
                storeController = storePanel.GetComponent<ListStoreController>();
            }
        }

        if (storeController != null)
        {
            Debug.Log($"YojiStoreHandler: Found ListStoreController on {storeController.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("YojiStoreHandler: Could not find ListStoreController in scene!");
        }
    }

    private void Update()
    {
        // Auto-reconnect store if reference was lost (e.g., after scene reload)
        if (storeController == null)
        {
            FindStoreController();
        }

        if (!playerInRange) return;

        // Don't show store prompt during dialogue
        if (DM != null && DM.IsDialogueActive)
        {
            if (storePrompt != null && storePrompt.activeSelf)
                storePrompt.SetActive(false);
            return;
        }

        // Check if store should be available
        if (ShouldShowStorePrompt())
        {
            if (storePrompt != null && !storePrompt.activeSelf)
            {
                storePrompt.SetActive(true);
                UpdateStorePromptText();
            }

            // Open store on key press
            if (Input.GetKeyDown(shopKey))
            {
                OpenStore();
            }
        }
        else
        {
            if (storePrompt != null && storePrompt.activeSelf)
                storePrompt.SetActive(false);
        }
    }

    /// <summary>
    /// Determines if the store prompt should be shown
    /// </summary>
    private bool ShouldShowStorePrompt()
    {
        if (SSM == null) return false;

        // Store must be unlocked
        if (!SSM.IsStoreUnlocked()) return false;

        // Don't show store if Yoji still has dialogue to say
        YojiDialogueHandler dialogueHandler = GetComponent<YojiDialogueHandler>();
        if (dialogueHandler != null)
        {
            // Check if there's pending dialogue
            if (GM != null)
            {
                // First time talking
                if (!GM.GetFlag(GameFlag.YojiFirstDialogueCompleted)) return false;

                // Post-George dialogue pending
                if (GM.GetFlag(GameFlag.GeorgeFirstEncounter) && !GM.GetFlag(GameFlag.hasUpgradedSword)) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Opens the store
    /// </summary>
    private void OpenStore()
    {
        if (storeController == null)
        {
            Debug.LogError("YojiStoreHandler: storeController is not assigned!");
            return;
        }

        storeController.OpenStore();

        // Hide prompt while store is open
        if (storePrompt != null)
            storePrompt.SetActive(false);
    }

    /// <summary>
    /// Updates the store prompt text based on store state
    /// </summary>
    private void UpdateStorePromptText()
    {
        if (storePromptText == null || SSM == null) return;

        if (SSM.IsStoreFree())
        {
            storePromptText.text = "Press E - Take Items";
        }
        else
        {
            storePromptText.text = "Press E - Shop";
        }
    }

    /// <summary>
    /// Called by YojiDialogueHandler when store becomes available
    /// </summary>
    public void OnStoreUnlocked()
    {
        if (playerInRange && ShouldShowStorePrompt())
        {
            if (storePrompt != null)
            {
                storePrompt.SetActive(true);
                UpdateStorePromptText();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // Show store prompt if appropriate
        if (ShouldShowStorePrompt() && (DM == null || !DM.IsDialogueActive))
        {
            if (storePrompt != null)
            {
                storePrompt.SetActive(true);
                UpdateStorePromptText();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (storePrompt != null)
            storePrompt.SetActive(false);
    }
}