using System.Collections;
using UnityEngine;

/// <summary>
/// Green Battle Start Intro - Plays multiple dialogues in sequence on first visit
/// Player presses F to advance through all 3 dialogues
/// </summary>
public class GreenBattleStartIntro : MonoBehaviour
{
    [Header("Dialogue Sequence")]
    public DialogueData hpAndPotionsDialogue;
    public DialogueData coinsDialogue;
    public DialogueData prepareForBattleDialogue;

    [Header("Flag to Check")]
    [Tooltip("Which flag determines if this dialogue has been seen?")]
    public GameFlag dialogueSeenFlag = GameFlag.GeorgeFirstEncounter;

    [Header("Settings")]
    [Tooltip("If true, plays every time. If false, plays only once.")]
    public bool playEveryTime = false;

    private void Start()
    {
        if (ShouldPlayDialogue())
        {
            StartCoroutine(PlayDialogueSequence());
        }
        else
        {
            Debug.Log("Green Battle intro dialogue already seen - skipping");
        }
    }

    private bool ShouldPlayDialogue()
    {
        // If set to play every time, always return true
        if (playEveryTime)
            return true;

        // Check if dialogue has been seen
        if (GameManager.Instance != null)
        {
            bool alreadySeen = GameManager.Instance.GetFlag(dialogueSeenFlag);
            return !alreadySeen; // Play if NOT seen
        }

        // If no GameManager, play dialogue as fallback
        return true;
    }

    private IEnumerator PlayDialogueSequence()
    {
        yield return null;

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager not found!");
            yield break;
        }

        Debug.Log("Starting Green Battle intro dialogue sequence");

        // Dialogue 1: HP and Potions
        if (hpAndPotionsDialogue != null)
        {
            bool dialogue1Done = false;
            // Keep input disabled (not the last dialogue)
            DialogueManager.Instance.Play(hpAndPotionsDialogue, () => dialogue1Done = true, keepInputDisabled: true);
            yield return new WaitUntil(() => dialogue1Done);
        }

        // Dialogue 2: Coins
        if (coinsDialogue != null)
        {
            bool dialogue2Done = false;
            // Keep input disabled (not the last dialogue)
            DialogueManager.Instance.Play(coinsDialogue, () => dialogue2Done = true, keepInputDisabled: true);
            yield return new WaitUntil(() => dialogue2Done);
        }

        // Dialogue 3: Prepare for Battle
        if (prepareForBattleDialogue != null)
        {
            bool dialogue3Done = false;
            // This is the last dialogue, so input will be re-enabled automatically
            DialogueManager.Instance.Play(prepareForBattleDialogue, () => dialogue3Done = true, keepInputDisabled: false);
            yield return new WaitUntil(() => dialogue3Done);
        }

        // Mark all dialogues as seen after the sequence completes
        OnDialogueSequenceComplete();
    }

    private void OnDialogueSequenceComplete()
    {
        // Mark dialogue as seen
        if (GameManager.Instance != null && !playEveryTime)
        {
            GameManager.Instance.SetFlag(dialogueSeenFlag, true);
            Debug.Log($"Green Battle intro sequence completed and marked as seen (Flag: {dialogueSeenFlag})");
        }
    }
}