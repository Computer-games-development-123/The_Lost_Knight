using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData dialogue;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.F;
    public float interactionDistance = 2f;

    [Header("Portal to lock/unlock (Optional)")]
    [SerializeField] private GameObject portal;
    [SerializeField]
    [Tooltip("Mark the check box to open portal, unmark to close portal")]
    private bool lockOrUnlock;

    [Header("Anti loop")]
    [SerializeField] private float reTriggerCooldown = 0.2f;

    private Transform player;
    private float nextAllowedTime = 0f;

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (player == null) return;
        if (Time.unscaledTime < nextAllowedTime) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= interactionDistance &&
            Input.GetKeyDown(interactKey) &&
            DialogueManager.Instance != null &&
            !DialogueManager.Instance.IsDialogueActive)
        {
            nextAllowedTime = Time.unscaledTime + reTriggerCooldown;

            if (portal != null)
                DialogueManager.Instance.Play(dialogue, OnDialogueFinished);
            else
                DialogueManager.Instance.Play(dialogue);
        }
    }

    private void OnDialogueFinished()
    {
        nextAllowedTime = Time.unscaledTime + reTriggerCooldown;

        if (portal == null) return;

        if (lockOrUnlock)
        {
            portal.SetActive(true);

            if (GameManager.Instance != null && dialogue != null && dialogue.flag != GameFlag.None)
                GameManager.Instance.SetFlag(dialogue.flag, true);
        }
        else
        {
            portal.GetComponent<PortalSpawnEffect>()?.Close();
        }
    }
}
