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
        yield return new WaitUntil(() =>
            GameManager.Instance != null && GameManager.Instance.IsProgressLoaded
        );

        if (GameManager.Instance.GetFlag(GameFlag.OpeningDialogueSeen))
            yield break;

        GameManager.Instance.SetFlag(GameFlag.OpeningDialogueSeen, true);
        GameManager.Instance.SaveProgress();

        if (DialogueManager.Instance != null && openingDialogue != null)
            DialogueManager.Instance.Play(openingDialogue);
    }

}
