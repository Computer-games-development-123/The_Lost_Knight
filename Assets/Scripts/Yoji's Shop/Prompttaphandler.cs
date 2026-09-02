using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Makes a world-space interaction prompt tappable on mobile.
/// Uses a direct UnityEvent so you can wire the action straight to the target script
/// in the Inspector — no dependency on input frame timing.
/// 
/// Usage:
/// 1. Add this script to the prompt GameObject (a BoxCollider2D will be auto-added)
/// 2. In the Inspector "On Tap" event, drag the target script (e.g. YojiDialogueHandler)
/// 3. Select the public method to call (e.g. HandleDialogueInteraction)
/// </summary>
public class PromptTapHandler : MonoBehaviour
{
    [Header("Tap Action")]
    [Tooltip("What happens when this prompt is tapped. Wire to a public method on the target script.")]
    public UnityEvent OnTap;

    [Header("Tap Area Padding")]
    [Tooltip("Adds extra invisible tap area around the prompt for easier finger tapping (in world units)")]
    [SerializeField] private float tapAreaPadding = 0.5f;

    private Camera mainCam;
    private Collider2D col;

    private void Awake()
    {
        mainCam = Camera.main;
        col = GetComponent<Collider2D>();

        if (col == null)
        {
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(2f + tapAreaPadding * 2, 1f + tapAreaPadding * 2);
            col = box;
        }
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        // Touch input
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began && IsTouchOnPrompt(touch.position))
            {
                FireTapAction();
                return;
            }
        }

        // Mouse input (editor testing)
        if (Input.GetMouseButtonDown(0) && IsTouchOnPrompt(Input.mousePosition))
        {
            FireTapAction();
        }
    }

    private bool IsTouchOnPrompt(Vector2 screenPosition)
    {
        Vector3 worldPoint = mainCam.ScreenToWorldPoint(screenPosition);
        worldPoint.z = 0;
        return col != null && col.OverlapPoint(worldPoint);
    }

    private void FireTapAction()
    {
        Debug.Log($"Prompt tapped: {gameObject.name}");
        OnTap?.Invoke();
    }
}