using System.Collections;
using UnityEngine;

public class TutorialIntro : MonoBehaviour
{
    public DialogueData tutorialDialogue;

    private void Start()
    {
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
