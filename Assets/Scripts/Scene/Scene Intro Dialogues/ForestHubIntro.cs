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

        PlaySequence(0);
    }

    private void PlaySequence(int index)
    {
        while (index < dialogues.Length && dialogues[index] == null)
            index++;

        if (index >= dialogues.Length)
            return;

        DialogueManager.Instance.Play(dialogues[index], () =>
        {
            PlaySequence(index + 1);
        });
    }
}
