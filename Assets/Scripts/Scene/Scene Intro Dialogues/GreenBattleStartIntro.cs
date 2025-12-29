using System.Collections;
using UnityEngine;

/// <summary>
/// Green Battle Start Intro - Plays dialogue only on first visit
/// </summary>
public class GreenBattleStartIntro : MonoBehaviour
{
    public DialogueData BattleStartDialogue;

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
            StartCoroutine(PlayBattleStartDialogue());
        }
        else
        {
            Debug.Log("🚫 Green Battle intro dialogue already seen - skipping");
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

        if (DialogueManager.Instance != null && BattleStartDialogue != null)
        {
            Debug.Log("🎬 Playing Green Battle intro dialogue");

            // Play dialogue and mark as seen when complete
            DialogueManager.Instance.Play(BattleStartDialogue, OnDialogueComplete);
        }
    }

    private void OnDialogueComplete()
    {
        // Mark dialogue as seen
        if (GameManager.Instance != null && !playEveryTime)
        {
            GameManager.Instance.SetFlag(dialogueSeenFlag, true);
            GameManager.Instance.SaveProgress();
            Debug.Log($"✅ Green Battle intro marked as seen (Flag: {dialogueSeenFlag})");
        }
    }
}