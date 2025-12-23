using UnityEngine;
using TMPro;
public class PromptTrigger : MonoBehaviour
{
    [SerializeField] protected GameObject interactionPrompt;
    public string promptText = "Press F";
    protected TextMeshProUGUI promptTextComponent;
    protected bool playerInRange = false;
    protected virtual void Awake()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);

            // Find the TextMeshPro component in the prompt
            promptTextComponent = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();
        }

        UpdatePromptText();
    }

    protected virtual void Update()
    {
        if (!playerInRange) return;

        if (interactionPrompt != null && !interactionPrompt.activeSelf)
        {
            interactionPrompt.SetActive(true);
            UpdatePromptText();
        }
    }
    protected virtual void UpdatePromptText()
    {
        if (promptTextComponent != null && !string.IsNullOrEmpty(promptText))
        {
            promptTextComponent.text = promptText;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            UpdatePromptText();
        }

    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

    }

    // Allow changing prompt text at runtime
    protected virtual void SetPromptText(string newText)
    {
        promptText = newText;
        UpdatePromptText();
    }
}
