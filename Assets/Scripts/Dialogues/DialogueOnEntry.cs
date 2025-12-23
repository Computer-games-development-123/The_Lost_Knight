using UnityEngine;

public class DialogueOnEntry : MonoBehaviour
{
    public DialogueData dialogue;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (DialogueManager.Instance.IsDialogueActive) return;

        DialogueManager.Instance.Play(dialogue);
    }
}
