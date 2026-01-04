using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Universal portal system - works for ALL portals in the game.
/// Just change the Target Scene Name and Prompt Text for each instance.
/// </summary>
public class ScenePortal : MonoBehaviour
{
    [Header("Portal Settings")]
    public string targetSceneName = " ";

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Prompt Customization")]
    [Tooltip("Text shown to player, e.g. 'Press F to Enter Tutorial' or 'Continue'")]
    public string promptText = "Press F to Enter";
    private TextMeshProUGUI promptTextComponent;

    [Header("Timing")]
    [SerializeField] private float fadeWaitTime = 1.2f;

    // [Header("Visuals (Optional)")]
    // [SerializeField] private Animator portalAnimator;
    // [SerializeField] private ParticleSystem portalParticles;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private bool playerInRange = false;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);

            // Find the TextMeshPro component in the prompt
            promptTextComponent = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();
        }

        UpdatePromptText();
    }

    private void Update()
    {
        if (!playerInRange || isTransitioning) return;

        // Show prompt when player is near
        if (interactionPrompt != null && !interactionPrompt.activeSelf)
        {
            interactionPrompt.SetActive(true);
            UpdatePromptText(); // Update text when showing
        }

        // Handle interaction
        if (Input.GetKeyDown(interactKey))
        {
            EnterPortal();
        }
    }

    private void UpdatePromptText()
    {
        if (promptTextComponent != null && !string.IsNullOrEmpty(promptText))
        {
            promptTextComponent.text = promptText;
        }
    }

    private void EnterPortal()
    {
        if (isTransitioning) return;

        if (showDebugLogs)
            Debug.Log($"🌀 Entering portal to {targetSceneName}");

        isTransitioning = true;

        // Hide prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Save progress before transitioning
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveProgress();
        }

        // Start fade and load scene
        StartCoroutine(TransitionToScene());
    }

    private IEnumerator TransitionToScene()
    {
        // Trigger fade out
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOut();

            if (showDebugLogs)
                Debug.Log($"⏳ Waiting {fadeWaitTime} seconds for fade...");

            yield return new WaitForSecondsRealtime(fadeWaitTime);
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning("⚠️ SceneFadeManager not found! Loading scene without fade.");

            yield return new WaitForSecondsRealtime(0.5f);
        }

        // Load the target scene
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            if (showDebugLogs)
                Debug.Log($"📂 Loading scene: {targetSceneName}");

            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("❌ Target scene name is not set!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (interactionPrompt != null && !isTransitioning)
        {
            interactionPrompt.SetActive(true);
            UpdatePromptText();
        }

        if (showDebugLogs)
            Debug.Log($"👤 Player entered portal range: {gameObject.name}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (showDebugLogs)
            Debug.Log($"👤 Player left portal range: {gameObject.name}");
    }

    // Allow changing prompt text at runtime
    public void SetPromptText(string newText)
    {
        promptText = newText;
        UpdatePromptText();
    }
}