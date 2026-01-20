using System.Collections;
using UnityEngine;

/// <summary>
/// Final Battle Start Intro - Plays two dialogues in sequence before Ditor fight
/// Uses flag system to ensure dialogue plays only once
/// </summary>
public class FinalBattleStartIntro : MonoBehaviour
{
    [Header("Dialogue Sequence")]
    [Tooltip("First dialogue")]
    public DialogueData firstDialogue;

    [Tooltip("Second dialogue")]
    public DialogueData secondDialogue;

    [Tooltip("Third dialogue")]
    public DialogueData thirdDialogue;

    [Header("Flag to Check")]
    [Tooltip("Which flag determines if this dialogue has been seen?")]
    public GameFlag dialogueSeenFlag = GameFlag.FinalBattleIntroSeen;

    [Header("Settings")]
    [Tooltip("If true, plays every time. If false, plays only once.")]
    public bool playEveryTime = false;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private void Start()
    {
        if (ShouldPlayDialogue())
        {
            StartCoroutine(PlayDialogueSequence());
        }
        else
        {
            if (showDebugLogs) Debug.Log("Final Battle intro dialogue already seen - skipping");
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
        // Wait a frame for scene to initialize
        yield return null;

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("FinalBattleStartIntro: DialogueManager not found!");
            yield break;
        }

        // IMPORTANT: Wait for Ditor to spawn before playing intro dialogues
        // This ensures the boss is visible during the dramatic intro
        if (showDebugLogs) Debug.Log("Waiting for Ditor to spawn...");

        DitorBoss ditor = null;
        float maxWaitTime = 5f; // Don't wait forever
        float waitedTime = 0f;

        while (ditor == null && waitedTime < maxWaitTime)
        {
            ditor = FindFirstObjectByType<DitorBoss>();
            if (ditor == null)
            {
                yield return new WaitForSeconds(0.1f);
                waitedTime += 0.1f;
            }
        }

        if (ditor != null)
        {
            if (showDebugLogs) Debug.Log("Ditor found! Starting intro dialogues");
        }
        else
        {
            Debug.LogWarning("FinalBattleStartIntro: Ditor not found after waiting, playing dialogues anyway");
        }

        if (showDebugLogs) Debug.Log("Starting Final Battle intro dialogue sequence");

        // Dialogue 1: First dialogue
        if (firstDialogue != null)
        {
            bool dialogue1Done = false;
            // Keep input disabled (not the last dialogue)
            DialogueManager.Instance.Play(firstDialogue, () => dialogue1Done = true, keepInputDisabled: true);
            yield return new WaitUntil(() => dialogue1Done);

            if (showDebugLogs) Debug.Log("First dialogue complete");
        }
        else
        {
            Debug.LogWarning("FinalBattleStartIntro: First dialogue not assigned!");
        }

        // Dialogue 2: Second dialogue
        if (secondDialogue != null)
        {
            bool dialogue2Done = false;
            // Keep input disabled (not the last dialogue)
            DialogueManager.Instance.Play(secondDialogue, () => dialogue2Done = true, keepInputDisabled: true);
            yield return new WaitUntil(() => dialogue2Done);

            if (showDebugLogs) Debug.Log("Second dialogue complete");
        }
        else
        {
            Debug.LogWarning("FinalBattleStartIntro: Second dialogue not assigned!");
        }

        // Dialogue 3: Third dialogue
        if (thirdDialogue != null)
        {
            bool dialogue3Done = false;
            // This is the last dialogue, so input will be re-enabled automatically
            DialogueManager.Instance.Play(thirdDialogue, () => dialogue3Done = true, keepInputDisabled: false);
            yield return new WaitUntil(() => dialogue3Done);

            if (showDebugLogs) Debug.Log("Third dialogue complete");
        }
        else
        {
            Debug.LogWarning("FinalBattleStartIntro: Third dialogue not assigned!");
        }

        // Mark dialogues as seen after the sequence completes
        OnDialogueSequenceComplete();
    }

    private void OnDialogueSequenceComplete()
    {
        // Mark dialogue as seen
        if (GameManager.Instance != null && !playEveryTime)
        {
            GameManager.Instance.SetFlag(dialogueSeenFlag, true);
            if (showDebugLogs) Debug.Log($"Final Battle intro sequence completed and marked as seen (Flag: {dialogueSeenFlag})");
        }

        if (showDebugLogs) Debug.Log("Final Battle intro complete - player can now fight Ditor!");
    }
}