using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the ending cutscene sequences in dedicated ending scenes
/// Handles text fade in/out, background display, and achievement dialogue
/// </summary>
public class EndingSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI cutsceneText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup textCanvasGroup;
    [SerializeField] private CanvasGroup backgroundCanvasGroup;

    [Header("Ending Content")]
    [SerializeField] private EndingType endingType;
    [SerializeField] private Sprite endingBackground;
    [SerializeField] private DialogueData achievementDialogue;

    [Header("Timing Settings")]
    [SerializeField] private float textFadeInDuration = 2f;
    [SerializeField] private float textDisplayDuration = 3f;
    [SerializeField] private float textFadeOutDuration = 2f;
    [SerializeField] private float backgroundFadeDuration = 3f;

    [Header("Text Sequences")]
    [TextArea(2, 5)]
    [SerializeField]
    private string[] heroEndingTexts = new string[]
    {
        "You have slain Ditor, King Of The Dark Forest.",
        "You have successfully saved the forest.",
        "But Ditor has managed to kill you too..",
        "You Died..."
    };

    [TextArea(2, 5)]
    [SerializeField]
    private string[] survivorEndingTexts = new string[]
    {
        "You have successfully escaped the forest.",
        "But The Forest Burned.."
    };

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        // Disable player input (safety check)
        if (UserInputManager.Instance != null)
        {
            UserInputManager.Instance.DisableInput();
        }

        // Start the epilogue music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.epilogueMusic);
            if (showDebugLogs) Debug.Log("Started epilogue music");
        }

        // Initialize UI
        if (cutsceneText != null) cutsceneText.text = "";
        if (textCanvasGroup != null) textCanvasGroup.alpha = 0f;
        if (backgroundCanvasGroup != null) backgroundCanvasGroup.alpha = 0f;

        // Start the cutscene sequence
        StartCoroutine(PlayEndingSequence());
    }

    private IEnumerator PlayEndingSequence()
    {
        if (showDebugLogs) Debug.Log($"Starting {endingType} ending sequence");

        // Get the appropriate text sequence based on ending type
        string[] textSequence = endingType == EndingType.HeroOfTheForest ? heroEndingTexts : survivorEndingTexts;

        // Play through all text sequences
        for (int i = 0; i < textSequence.Length; i++)
        {
            yield return StartCoroutine(ShowText(textSequence[i]));
        }

        // After last text, fade to background
        yield return StartCoroutine(FadeToBackground());

        // Show achievement dialogue
        yield return new WaitForSeconds(1f);
        ShowAchievementDialogue();
    }

    private IEnumerator ShowText(string text)
    {
        if (cutsceneText != null)
        {
            cutsceneText.text = text;
        }

        // Fade in text
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, textFadeInDuration));

        // Display text
        yield return new WaitForSeconds(textDisplayDuration);

        // Fade out text
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1f, 0f, textFadeOutDuration));

        // Clear text
        if (cutsceneText != null)
        {
            cutsceneText.text = "";
        }

        // Small pause between texts
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator FadeToBackground()
    {
        if (showDebugLogs) Debug.Log("Fading to background");

        // Set the background image
        if (backgroundImage != null && endingBackground != null)
        {
            backgroundImage.sprite = endingBackground;
        }

        // Fade in background
        yield return StartCoroutine(FadeCanvasGroup(backgroundCanvasGroup, 0f, 1f, backgroundFadeDuration));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    private void ShowAchievementDialogue()
    {
        if (DialogueManager.Instance != null && achievementDialogue != null)
        {
            if (showDebugLogs) Debug.Log($"Showing achievement: {achievementDialogue.id}");

            // Play achievement dialogue
            // Note: We don't want to re-enable input after this dialogue since the game is over
            DialogueManager.Instance.Play(achievementDialogue, OnAchievementComplete, keepInputDisabled: true);
        }
        else
        {
            Debug.LogError("EndingSceneManager: Achievement dialogue not assigned!");
        }
    }

    private void OnAchievementComplete()
    {
        if (showDebugLogs) Debug.Log("Achievement dialogue complete - ending sequence finished");

        // The ending is now complete
        // Player can press F to close the achievement dialogue and the epilogue music continues
        // Game stays on this ending scene
    }
}

public enum EndingType
{
    HeroOfTheForest,
    SurvivorOfTheForest
}
