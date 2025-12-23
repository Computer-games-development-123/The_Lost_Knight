using UnityEngine;
using System.Collections;

public class StartDialogue : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;

    [Header("Only once (optional)")]
    [Tooltip("If set, the dialogue will NOT play when this flag is already true.")]
    [SerializeField] private GameFlag onceFlag = GameFlag.None;
    [SerializeField]
    [Tooltip("Seconds before dialogue starts.")]
    private float startDialogue = 1.0f;

    [Header("Portal to close (optional)")]
    [Tooltip("If set, the portal will be closed after dialogue.")]
    [SerializeField] private GameObject portal = null;
    [Header("Only once (optional)")]
    [Tooltip("If this flag is true the portal will not close (saves progress).")]
    [SerializeField] private GameFlag flag;
    [SerializeField]
    [Tooltip("Seconds before dialogue starts.")]
    private float closePortal = 0.5f;

    private void Start()
    {
        if (dialogue == null) return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("StartDialogue: GameManager.Instance is null");
            return;
        }

        if (onceFlag != GameFlag.None && GameManager.Instance.GetFlag(onceFlag))
            return;

        if (onceFlag != GameFlag.None)
            GameManager.Instance.SetFlag(onceFlag, true);

        if (portal == null || GameManager.Instance.GetFlag(flag)) StartCoroutine(PlayNextFrame());
        else StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        yield return new WaitForSecondsRealtime(closePortal);
        portal.GetComponent<PortalSpawnEffect>().Close();

        yield return new WaitForSecondsRealtime(startDialogue);
        DialogueManager.Instance.Play(dialogue);
    }

    private IEnumerator PlayNextFrame()
    {
        yield return new WaitForSecondsRealtime(startDialogue);

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.Play(dialogue);
        else
            Debug.LogWarning("StartDialogue: DialogueManager.Instance is null");
    }
}
