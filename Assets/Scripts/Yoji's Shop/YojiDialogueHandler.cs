using UnityEngine;

public class YojiDialogueHandler : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueData openingDialogue;
    public DialogueData postGeorgeDialogue;
    public DialogueData unlockStoreDialogue;
    public DialogueData postFikaDialogue;

    [Header("Interaction UI")]
    public GameObject interactionPrompt;

    [Header("Portal Control")]
    [Tooltip("The Green Forest portal GameObject - starts INACTIVE, becomes active after dialogue")]
    public GameObject greenForestPortal;

    [Header("Yoji Visual")]
    [Tooltip("Yoji's sprite renderer - will be made invisible when dead")]
    public SpriteRenderer yojiSprite;

    private bool playerInRange = false;
    private GameManager GM => GameManager.Instance;
    private DialogueManager DM => DialogueManager.Instance;

    private void Awake()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Auto-find sprite renderer if not assigned
        if (yojiSprite == null)
            yojiSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameManagerReadyHelper.RunWhenReady(this, ApplySavedState);
    }

    private void Update()
    {
        // Don't allow dialogue interactions if Yoji is dead
        if (GM != null && GM.GetFlag(GameFlag.YojiDead))
            return;

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

        // Don't show dialogue if Yoji is dead
        if (GM.GetFlag(GameFlag.YojiDead)) return false;

        // First time talking to Yoji
        if (!GM.GetFlag(GameFlag.YojiFirstDialogueCompleted))
            return true;

        // After dying to George, before seeing first dialogue
        if (GM.GetFlag(GameFlag.GeorgeFirstEncounter) && !GM.GetFlag(GameFlag.YojiUnlocksStore))
            return true;

        // After first post-George dialogue, before upgrade dialogue
        if (GM.GetFlag(GameFlag.YojiUnlocksStore) && !GM.GetFlag(GameFlag.hasUpgradedSword))
            return true;

        // After defeating Fika, before this dialogue is seen
        if (GM.GetFlag(GameFlag.FikaDefeated) && !GM.GetFlag(GameFlag.YojiPostFikaDialogueSeen))
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
                OnOpeningDialogueComplete();
            }
            return;
        }

        // CASE 2: After dying to George - First dialogue
        if (GM.GetFlag(GameFlag.GeorgeFirstEncounter) && !GM.GetFlag(GameFlag.YojiUnlocksStore))
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

        // CASE 3: RIGHT AFTER post-George dialogue - Give upgrade and unlock store
        if (GM.GetFlag(GameFlag.YojiUnlocksStore) && !GM.GetFlag(GameFlag.hasUpgradedSword))
        {
            if (unlockStoreDialogue != null)
            {
                DM.Play(unlockStoreDialogue, OnPostGeorgeUpgradeDialogueComplete);
            }
            else
            {
                Debug.LogWarning("YojiDialogueHandler: postGeorgeUpgradeDialogue is not assigned!");
                OnPostGeorgeUpgradeDialogueComplete();
            }
            return;
        }

        // CASE 4: After defeating Fika (Reveal Wave Of Fire)
        if (GM.GetFlag(GameFlag.FikaDefeated) && !GM.GetFlag(GameFlag.YojiPostFikaDialogueSeen))
        {
            if (postFikaDialogue != null)
            {
                DM.Play(postFikaDialogue, OnPostFikaDialogueComplete);
            }
            else
            {
                Debug.LogWarning("YojiDialogueHandler: postFikaDialogue is not assigned!");
                OnPostFikaDialogueComplete();
            }
            return;
        }
    }

    /// <summary>
    /// Called after the opening dialogue finishes
    /// </summary>
    private void OnOpeningDialogueComplete()
    {
        if (GM != null)
        {
            GM.SetFlag(GameFlag.YojiFirstDialogueCompleted, true);
            GM.SetFlag(GameFlag.OpeningDialogueSeen, true);
        }

        if (greenForestPortal != null)
        {
            greenForestPortal.SetActive(true);
            Debug.Log("Green Forest portal activated!");
        }
        else
        {
            Debug.LogError("greenForestPortal is not assigned! Please drag the portal GameObject to YojiDialogueHandler in Inspector!");
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
    /// Called after the first post-George dialogue finishes
    /// This just sets a flag - the upgrade happens in the NEXT dialogue
    /// </summary>
    private void OnPostGeorgeDialogueComplete()
    {
        if (GM != null)
        {
            GM.SetFlag(GameFlag.YojiAfterGeorge, true);
            GM.SetFlag(GameFlag.YojiUnlocksStore, true);
        }

        Debug.Log("Post-George dialogue complete - player needs to talk again for upgrade");

        // Save progress
        if (GM != null)
        {
            GM.SaveProgress();
        }

        // Immediately show prompt again so player can get the upgrade dialogue
        if (playerInRange && ShouldShowDialoguePrompt())
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    /// <summary>
    /// Called after the post-George UPGRADE dialogue finishes
    /// THIS is where the player gets the sword upgrade and store unlocks
    /// </summary>
    private void OnPostGeorgeUpgradeDialogueComplete()
    {
        // Give sword upgrade to player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Abilities abilities = player.GetComponent<Abilities>();
            if (abilities != null)
            {
                abilities.UpgradeSword();
                Debug.Log("Player given sword upgrade - Can now damage George!");
            }
        }

        // Unlock the store
        if (StoreStateManager.Instance != null)
        {
            StoreStateManager.Instance.SetStoreState(StoreStateManager.StoreState.PostGeorge);
        }

        Debug.Log("Post-George UPGRADE dialogue complete - sword upgraded, store unlocked");

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

        // Re-show prompt if player is still in range and has more dialogue
        if (playerInRange && ShouldShowDialoguePrompt())
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    /// <summary>
    /// Called after the post-Fika dialogue finishes
    /// </summary>
    private void OnPostFikaDialogueComplete()
    {
        if (GM != null)
        {
            GM.SetFlag(GameFlag.YojiPostFikaDialogueSeen, true);
        }

        // Update store state to PostFika (unlocks new items after defeating Fika)
        if (StoreStateManager.Instance != null)
        {
            StoreStateManager.Instance.SetStoreState(StoreStateManager.StoreState.PostFika);
            Debug.Log("Store state changed to PostFika - New items unlocked!");
        }

        Debug.Log("Post-Fika dialogue complete - Congratulations given, store upgraded!");

        // Notify the store interaction handler that new items are available
        YojiStoreHandler storeHandler = GetComponent<YojiStoreHandler>();
        if (storeHandler != null)
        {
            storeHandler.OnStoreUnlocked(); // Refresh store availability
        }

        // Save progress
        if (GM != null)
        {
            GM.SaveProgress();
        }

        // Re-show prompt if player is still in range and has more dialogue
        if (playerInRange && ShouldShowDialoguePrompt())
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // Show prompt if there's dialogue available (and Yoji is not dead)
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

    private void ApplySavedState()
    {
        if (GM == null || !GM.IsProgressLoaded) return;

        // Check if Yoji is dead - make him invisible
        if (GM.GetFlag(GameFlag.YojiDead))
        {
            HideYoji();
            return;
        }

        if (GM.GetFlag(GameFlag.YojiFirstDialogueCompleted))
        {
            if (greenForestPortal != null)
                greenForestPortal.SetActive(true);

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            Debug.Log("Saved state applied: Yoji talked -> portal active");
        }
    }

    /// <summary>
    /// Makes Yoji invisible (called when YojiDead flag is set)
    /// </summary>
    private void HideYoji()
    {
        if (yojiSprite != null)
        {
            yojiSprite.enabled = false;
            Debug.Log("Yoji is dead - sprite hidden");
        }

        // Hide dialogue prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        Debug.Log("Yoji's spirit has moved on... but his store remains.");
    }
}