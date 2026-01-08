using System.Collections;
using UnityEngine;

/// <summary>
/// Dark Battle Start Intro - Plays dialogue on first visit
/// Uses flag system to ensure dialogue plays only once
/// </summary>
public class DarkBattleStartIntro : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData BattleStartDialogue;

    [Header("Flag to Check")]
    [Tooltip("Which flag determines if this dialogue has been seen?")]
    public GameFlag dialogueSeenFlag = GameFlag.DarkBattleIntroSeen;

    [Header("Settings")]
    [Tooltip("If true, plays every time. If false, plays only once.")]
    public bool playEveryTime = false;

    private void Start()
    {
        if (ShouldPlayDialogue())
        {
            StartCoroutine(PlayBattleStartDialogue());
        }
        else
        {
            Debug.Log("Dark Battle intro dialogue already seen - skipping");
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

    private IEnumerator PlayBattleStartDialogue()
    {
        yield return null;

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager not found!");
            yield break;
        }

        if (BattleStartDialogue == null)
        {
            Debug.LogError("BattleStartDialogue is not assigned!");
            yield break;
        }

        Debug.Log("Starting Dark Battle intro dialogue");

        bool dialogueDone = false;
        DialogueManager.Instance.Play(BattleStartDialogue, () => dialogueDone = true);

        // Wait for player to finish this dialogue
        yield return new WaitUntil(() => dialogueDone);

        // Mark dialogue as seen after it completes
        OnDialogueComplete();
    }

    private void OnDialogueComplete()
    {
        // Mark dialogue as seen
        if (GameManager.Instance != null && !playEveryTime)
        {
            GameManager.Instance.SetFlag(dialogueSeenFlag, true);
            Debug.Log($"Dark Battle intro completed and marked as seen (Flag: {dialogueSeenFlag})");
        }
    }
}
