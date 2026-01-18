using System.Collections;
using UnityEngine;

public class ForestHubIntro : MonoBehaviour
{
    [Header("Dialogues in order")]
    public DialogueData[] dialogues;

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        yield return new WaitUntil(() =>
            GameManager.Instance != null && GameManager.Instance.IsProgressLoaded
        );

        if (GameManager.Instance.GetFlag(GameFlag.OpeningDialogueSeen))
            yield break;

        GameManager.Instance.SetFlag(GameFlag.OpeningDialogueSeen, true);
        GameManager.Instance.SaveProgress();

        if (DialogueManager.Instance == null || dialogues == null || dialogues.Length == 0)
            yield break;

        // MANUALLY disable input at the start
        if (UserInputManager.Instance != null)
        {
            UserInputManager.Instance.DisableInput();
            Debug.Log("🔒 ForestHub: Manually disabling input for entire sequence");
        }

        // Play all dialogues with keepInputDisabled = true so DialogueManager doesn't re-enable
        yield return PlaySequenceManual(0);

        // MANUALLY re-enable input at the end
        if (UserInputManager.Instance != null)
        {
            UserInputManager.Instance.EnableInput();
            Debug.Log("🔓 ForestHub: Manually enabling input - sequence complete");
        }
    }

    private IEnumerator PlaySequenceManual(int index)
    {
        // Skip null dialogues
        while (index < dialogues.Length && dialogues[index] == null)
            index++;

        if (index >= dialogues.Length)
        {
            Debug.Log("🎭 ForestHub: All dialogues played");
            yield break;
        }

        Debug.Log($"🎭 ForestHub: Playing dialogue {index}");

        bool dialogueDone = false;

        // ALWAYS keep input disabled - we manage it manually
        DialogueManager.Instance.Play(dialogues[index], () => dialogueDone = true, keepInputDisabled: true);

        yield return new WaitUntil(() => dialogueDone);

        Debug.Log($"🎭 ForestHub: Dialogue {index} finished");

        // Continue to next dialogue
        yield return PlaySequenceManual(index + 1);
    }
}