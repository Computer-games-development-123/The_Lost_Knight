using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// On-screen virtual joystick for mobile movement.
/// Reports horizontal axis to MobileInputBridge.MoveInput.
/// Drag the handle within the background; release to snap back to center.
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

    [Tooltip("If true, joystick snaps to where the finger first touches inside the background")]
    [SerializeField] private bool snapToTouch = false;

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
        Debug.Log("Joystick clicked!"); // ADD THIS LINE
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out position))
            return;

        // Normalize position to [-1, 1] based on background size
        Vector2 size = background.sizeDelta;
        position.x = (position.x / size.x) * 2f;
        position.y = (position.y / size.y) * 2f;

        inputVector = (position.magnitude > 1f) ? position.normalized : position;

        // Move the handle visually
        if (handle != null)
        {
            handle.anchoredPosition = new Vector2(
                inputVector.x * (size.x / 2f),
                inputVector.y * (size.y / 2f)
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