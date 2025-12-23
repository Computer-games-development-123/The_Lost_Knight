using UnityEngine;
using System.Collections;

public class DialogueAfterHits : MonoBehaviour, IHitConfirmListener
{
    [SerializeField] private HitConfirmBroadcaster source;
    [SerializeField] private int hitsRequired = 3;
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private GameFlag onceFlag = GameFlag.None;

    private int hitCount = 0;
    private bool triggered = false;

    private void Start()
    {
        if (source != null) source.Register(this);
    }

    private void OnDestroy()
    {
        if (source != null) source.Unregister(this);
    }

    public void OnHitConfirmed(GameObject target)
    {
        Debug.Log($"{name} received hit event for {target.name}");

        if (triggered) return;

        if (target != this.gameObject) return;

        hitCount++;
        if (hitCount >= hitsRequired)
            TriggerDialogue();
    }

    private void TriggerDialogue()
    {
        if (triggered) return;

        if (onceFlag != GameFlag.None &&
            GameManager.Instance != null &&
            GameManager.Instance.GetFlag(onceFlag))
            return;

        triggered = true;

        if (onceFlag != GameFlag.None && GameManager.Instance != null)
        {
            Debug.LogWarning($"flag {onceFlag} is {GameManager.Instance.GetFlag(onceFlag)}.");
            GameManager.Instance.SetFlag(onceFlag, true);
            Debug.LogWarning($"Setting flag {onceFlag} to be true.");
        }
        StartCoroutine(PlayNextFrame());
    }

    private IEnumerator PlayNextFrame()
    {
        yield return null;

        if (DialogueManager.Instance != null &&
            dialogue != null &&
            !DialogueManager.Instance.IsDialogueActive)
        {
            DialogueManager.Instance.Play(dialogue);
        }
    }
}
