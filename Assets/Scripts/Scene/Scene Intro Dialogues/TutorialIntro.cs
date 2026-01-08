using System.Collections;
using UnityEngine;

public class TutorialIntro : MonoBehaviour
{
    public DialogueData tutorialDialogue;

    private void Start()
    {
        // CHECK: If tutorial already completed, don't play intro dialogue
        if (GameManager.Instance != null && GameManager.Instance.GetFlag(GameFlag.TutorialCompleted))
        {
            // Tutorial already done - skip the intro dialogue
            return;
        }

        StartCoroutine(PlayTutorialHint());
    }

    private IEnumerator PlayTutorialHint()
    {
        // wait 1 frame so DialogueManager is ready
        yield return null;

        if (DialogueManager.Instance != null && tutorialDialogue != null)
        {
            DialogueManager.Instance.Play(tutorialDialogue);
        }
    }
}