using System.Collections;
using UnityEngine;

public class RedBattleStartIntro : MonoBehaviour
{
    public DialogueData BattleStartDialogue;
    private void Start()
    {
        StartCoroutine(PlayBattleStartDialogue());
    }

    private IEnumerator PlayBattleStartDialogue()
    {
        yield return null;

        if (DialogueManager.Instance != null && BattleStartDialogue != null)
        {
            DialogueManager.Instance.Play(BattleStartDialogue);
        }
    }
}
