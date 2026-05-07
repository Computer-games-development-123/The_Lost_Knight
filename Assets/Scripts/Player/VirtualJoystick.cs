using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// On-screen virtual joystick for mobile movement.
/// Reports horizontal axis to MobileInputBridge.MoveInput.
/// HORIZONTAL ONLY - the handle only moves left/right since the player
/// can only walk horizontally in this 2D platformer.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    [Tooltip("The background circle (this object's RectTransform)")]
    [SerializeField] private RectTransform background;

    [Tooltip("The handle/knob that moves with the finger")]
    [SerializeField] private RectTransform handle;

    [Header("Settings")]
    [Tooltip("Movement values below this magnitude register as 0 (deadzone)")]
    [Range(0f, 0.5f)]
    [SerializeField] private float deadzone = 0.15f;

    private Vector2 inputVector = Vector2.zero;

    private void Awake()
    {
        if (background == null)
            background = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Reset handle to center
        if (handle != null)
            handle.anchoredPosition = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out position))
            return;

        Vector2 size = background.sizeDelta;

        // Adjust for pivot so (0,0) is the visual center of the background,
        // regardless of how the RectTransform's pivot is set
        position.x -= (0.5f - background.pivot.x) * size.x;

        // Normalize to [-1, 1]
        position.x = (position.x / size.x) * 2f;

        // HORIZONTAL ONLY - ignore Y axis since player can only walk left/right
        inputVector = new Vector2(Mathf.Clamp(position.x, -1f, 1f), 0);

        // Move the handle visually - X only, stay centered vertically
        if (handle != null)
        {
            handle.anchoredPosition = new Vector2(
                inputVector.x * (size.x / 2f),
                0  // Always centered vertically
            );
        }

        // Send horizontal value to bridge (with deadzone)
        SendInputToBridge();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Reset handle and input
        inputVector = Vector2.zero;

        if (handle != null)
            handle.anchoredPosition = Vector2.zero;

        if (MobileInputBridge.Instance != null)
            MobileInputBridge.Instance.SetMoveInput(0f);
    }

    private void SendInputToBridge()
    {
        if (MobileInputBridge.Instance == null) return;

        float horizontal = inputVector.x;

        // Apply deadzone
        if (Mathf.Abs(horizontal) < deadzone)
            horizontal = 0f;

        MobileInputBridge.Instance.SetMoveInput(horizontal);
    }
}