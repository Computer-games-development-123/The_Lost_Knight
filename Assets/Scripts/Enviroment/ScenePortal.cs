using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ScenePortal : PromptTrigger
{
    [Header("Portal Settings")]
    public string targetSceneName = "GreenToRed";

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("Timing")]
    [SerializeField] private float fadeWaitTime = 1.2f;

    [Header("Spawn ID in next scene")]
    [SerializeField] private string spawnIdInNextScene;

    [Header("Debug")]
    public bool showDebugLogs = false;
    private bool isTransitioning = false;


    protected override void Update()
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
        // Fade out
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOut();
            yield return new WaitForSecondsRealtime(fadeWaitTime);
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }

        if (!string.IsNullOrEmpty(targetSceneName))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.TravelTo(targetSceneName, spawnIdInNextScene);
            else
                Debug.LogError("❌ GameManager.Instance is null! Cannot travel.");
        }
        else
        {
            Debug.LogError("❌ Target scene name is not set!");
        }
    }


    protected override void OnTriggerEnter2D(Collider2D other)
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

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (showDebugLogs)
            Debug.Log($"👤 Player left portal range: {gameObject.name}");
    }
}