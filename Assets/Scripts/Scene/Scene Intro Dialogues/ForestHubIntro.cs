using System.Collections;
using UnityEngine;

public class ForestHubIntro : MonoBehaviour
{
    public DialogueData openingDialogue;

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        // wait 1 frame so DialogueManager & GameManager are initialized
        yield return null;

        if (GameManager.Instance == null)
            yield break;

        // Already seen? Do nothing.
        if (GameManager.Instance.hasSeenOpeningDialogue)
            yield break;

        // Mark as seen and save
        GameManager.Instance.hasSeenOpeningDialogue = true;
        GameManager.Instance.SaveProgress();

        if (DialogueManager.Instance != null && openingDialogue != null)
        {
            DialogueManager.Instance.Play(openingDialogue);
        }
    }
}
