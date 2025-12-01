using UnityEngine;

public class YojiGateBarrier : MonoBehaviour
{
    [Header("Barrier Settings")]
    [SerializeField] private GameObject barrierVisual;   // What you see
    [SerializeField] private Collider2D barrierCollider; // What blocks the player
    public bool isOpen { get; private set; }

    void Awake()
    {
        // If you forget to assign in Inspector, fall back to this object
        if (barrierVisual == null)
            barrierVisual = gameObject;

        if (barrierCollider == null)
            barrierCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        // If player has already talked to Yoji in a previous run, start open
        if (GameManager.Instance != null && GameManager.Instance.HasTalkedTo("Yoji"))
        {
            OpenBarrier();
        }
        else
        {
            CloseBarrier();
        }
    }

    public void OpenBarrier()
    {
        isOpen = true;

        if (barrierVisual != null)
            barrierVisual.SetActive(false);

        if (barrierCollider != null)
            barrierCollider.enabled = false;

        Debug.Log("YojiGateBarrier: Barrier opened");
    }

    public void CloseBarrier()
    {
        isOpen = false;

        if (barrierVisual != null)
            barrierVisual.SetActive(true);

        if (barrierCollider != null)
            barrierCollider.enabled = true;
    }

    // Called once when Yoji's opening dialogue is finished
    public void OnYojiDialogueComplete()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetYojiTalked();
        }
        OpenBarrier();
    }
}
