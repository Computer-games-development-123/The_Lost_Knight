using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the ending choice system for the game
/// Shows two buttons after Ditor's death dialogue, plays cutscenes, and loads ending scenes
/// </summary>
public class EndingChoiceManager : MonoBehaviour
{
    public static EndingChoiceManager Instance { get; private set; }

    [Header("Choice UI References")]
    [Tooltip("The canvas/panel that contains the choice buttons")]
    public GameObject choicePanel;
    [Tooltip("Button for 'Finish off Ditor' choice")]
    public Button finishDitorButton;
    [Tooltip("Button for 'Escape the forest' choice")]
    public Button escapeForestButton;

    [Header("Cutscene Text UI")]
    [Tooltip("TextMeshProUGUI for displaying cutscene text")]
    public TextMeshProUGUI cutsceneText;
    [Tooltip("Canvas that contains the cutscene text")]
    public GameObject cutsceneTextCanvas;

    [Header("Dialogue References")]
    [Tooltip("Achievement dialogue for Hero ending")]
    public DialogueData heroAchievementDialogue;
    [Tooltip("Achievement dialogue for Survivor ending")]
    public DialogueData survivorAchievementDialogue;

    [Header("Settings")]
    [Tooltip("How long each text message stays on screen")]
    public float textDisplayDuration = 5f;
    [Tooltip("Fade duration for text fade in/out")]
    public float textFadeDuration = 1f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private bool choiceMade = false;
    private bool isShowingChoice = false; // NEW: Track if we're currently showing the choice

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure EventSystem exists for UI navigation
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("Created EventSystem for button navigation");
        }

        // Make sure the choice panel is hidden but keep the canvas active
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);

            // Make sure parent canvas is active
            Transform canvasTransform = choicePanel.transform.parent;
            if (canvasTransform != null)
            {
                canvasTransform.gameObject.SetActive(true);
            }
        }

        // Hide cutscene text initially
        if (cutsceneTextCanvas != null)
        {
            cutsceneTextCanvas.SetActive(true); // Keep canvas active
            if (cutsceneText != null)
                cutsceneText.text = ""; // Just clear the text
        }

        // Set up button listeners
        if (finishDitorButton != null)
        {
            finishDitorButton.onClick.AddListener(OnFinishDitorChoice);
            // Disable mouse interaction - keyboard only
            finishDitorButton.interactable = true;
        }

        if (escapeForestButton != null)
        {
            escapeForestButton.onClick.AddListener(OnEscapeForestChoice);
            // Disable mouse interaction - keyboard only  
            escapeForestButton.interactable = true;
        }
    }

    private void Update()
    {
        // ONLY process input when we're actively showing the choice
        if (!isShowingChoice || choiceMade)
            return;

        // Use GetKeyDown which works even when Time.timeScale = 0
        // Arrow keys ONLY (removed WASD to avoid conflicts)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (finishDitorButton != null)
            {
                finishDitorButton.Select();
                if (showDebugLogs) Debug.Log("Selected: Finish Ditor button");
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (escapeForestButton != null)
            {
                escapeForestButton.Select();
                if (showDebugLogs) Debug.Log("Selected: Escape Forest button");
            }
        }

        // Enter to confirm selection
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (showDebugLogs) Debug.Log("Enter pressed - invoking selected button");

            // Check which button is currently selected
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
            {
                Button selectedButton = eventSystem.currentSelectedGameObject.GetComponent<Button>();
                if (selectedButton != null)
                {
                    if (showDebugLogs) Debug.Log($"Invoking button: {selectedButton.name}");
                    selectedButton.onClick.Invoke();
                }
                else
                {
                    if (showDebugLogs) Debug.LogWarning("No button component on selected object");
                }
            }
            else
            {
                if (showDebugLogs) Debug.LogWarning("No EventSystem or nothing selected");
            }
        }
    }

    /// <summary>
    /// Called by DitorBoss after the "Ending" dialogue completes
    /// Shows the two choice buttons to the player
    /// </summary>
    public void ShowEndingChoice()
    {
        if (showDebugLogs) Debug.Log("EndingChoiceManager: Showing ending choice");

        isShowingChoice = true; // Mark that we're showing the choice

        // PAUSE THE GAME
        Time.timeScale = 0f;
        if (showDebugLogs) Debug.Log("Game paused for ending choice");

        // Make sure the entire canvas is active first
        if (choicePanel != null)
        {
            // Enable the parent canvas if it exists
            Transform canvasTransform = choicePanel.transform.parent;
            if (canvasTransform != null)
            {
                GameObject canvas = canvasTransform.gameObject;
                if (!canvas.activeSelf)
                {
                    if (showDebugLogs) Debug.Log("Enabling parent canvas");
                    canvas.SetActive(true);
                }
            }

            // Then enable the panel
            choicePanel.SetActive(true);
            if (showDebugLogs) Debug.Log("Choice panel activated");

            // Select the first button so keyboard navigation works
            if (finishDitorButton != null)
            {
                finishDitorButton.Select();
                if (showDebugLogs) Debug.Log("First button selected for keyboard navigation");
            }
        }
        else
        {
            Debug.LogError("EndingChoiceManager: Choice panel is not assigned!");
        }

        // Make sure player can't move during choice
        if (UserInputManager.Instance != null)
        {
            UserInputManager.Instance.DisableInput();
        }
    }

    /// <summary>
    /// Player chose to finish off Ditor
    /// </summary>
    private void OnFinishDitorChoice()
    {
        if (choiceMade) return;
        choiceMade = true;
        isShowingChoice = false; // No longer showing choice

        if (showDebugLogs) Debug.Log("Player chose: Finish off Ditor");

        // Unpause the game so coroutines can run
        Time.timeScale = 1f;

        // Hide choice buttons
        if (choicePanel != null) choicePanel.SetActive(false);

        // Start the Hero ending cutscene
        StartCoroutine(HeroEndingCutscene());
    }

    /// <summary>
    /// Player chose to escape the forest
    /// </summary>
    private void OnEscapeForestChoice()
    {
        if (choiceMade) return;
        choiceMade = true;
        isShowingChoice = false; // No longer showing choice

        if (showDebugLogs) Debug.Log("Player chose: Escape the forest");

        // Unpause the game so coroutines can run
        Time.timeScale = 1f;

        // Hide choice buttons
        if (choicePanel != null) choicePanel.SetActive(false);

        // Start the Survivor ending cutscene
        StartCoroutine(SurvivorEndingCutscene());
    }

    /// <summary>
    /// Hero Ending Cutscene:
    /// - You have slain Ditor
    /// - But Ditor managed to kill you too
    /// - You have successfully saved the forest
    /// - Load Forest_Hub (normal)
    /// - Achievement: Hero of the Forest
    /// </summary>
    private IEnumerator HeroEndingCutscene()
    {
        // Play epilogue music and force it to stay
        if (AudioManager.Instance != null && AudioManager.Instance.epilogueMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.epilogueMusic, true); // Force restart
            if (showDebugLogs) Debug.Log("Playing epilogue music");
        }

        // Small delay to let music start
        yield return new WaitForSeconds(0.5f);

        // Fade to black
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOut();
            yield return new WaitForSeconds(SceneFadeManager.Instance.fadeDuration);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // Show cutscene text (canvas is already active, just need to show text)
        // The panel is already hidden, so text will be visible

        // Text 1: "You have slain Ditor, Dark King Of The Forest"
        yield return StartCoroutine(ShowCutsceneText("You have slain Ditor, Dark King Of The Forest"));

        // Text 2: "But Ditor has managed to kill you too"
        yield return StartCoroutine(ShowCutsceneText("But Ditor has managed to kill you too"));

        // Text 3: "You have successfully saved the forest"
        yield return StartCoroutine(ShowCutsceneText("You have successfully saved the forest"));

        // Text is now hidden (alpha = 0), ready for scene load

        // Load Forest_Hub scene
        if (showDebugLogs) Debug.Log("Loading Forest_Hub scene...");

        // Load scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Forest_Hub");

        // Wait for scene to finish loading
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (showDebugLogs) Debug.Log("Forest_Hub scene loaded");

        // Wait a moment for scene to initialize
        yield return new WaitForSeconds(0.5f);

        // Fade in to reveal the new scene
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeIn();
            yield return new WaitForSeconds(SceneFadeManager.Instance.fadeDuration);
        }

        // Show achievement dialogue
        yield return new WaitForSeconds(1f);

        if (DialogueManager.Instance != null && heroAchievementDialogue != null)
        {
            DialogueManager.Instance.Play(heroAchievementDialogue);
        }
        else
        {
            Debug.LogWarning("EndingChoiceManager: Hero achievement dialogue not assigned!");
        }

        // Epilogue music continues playing in background
        if (showDebugLogs) Debug.Log("Hero ending complete!");
    }

    /// <summary>
    /// Survivor Ending Cutscene:
    /// - You have successfully escaped the forest
    /// - But the forest got burned
    /// - Load Forest_Hub (burned version / placeholder)
    /// - Achievement: Survivor of the Forest
    /// </summary>
    private IEnumerator SurvivorEndingCutscene()
    {
        // Play epilogue music and force it to stay
        if (AudioManager.Instance != null && AudioManager.Instance.epilogueMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.epilogueMusic, true); // Force restart
            if (showDebugLogs) Debug.Log("Playing epilogue music");
        }

        // Small delay to let music start
        yield return new WaitForSeconds(0.5f);

        // Fade to black
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOut();
            yield return new WaitForSeconds(SceneFadeManager.Instance.fadeDuration);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // Show cutscene text (canvas is already active)

        // Text 1: "You have successfully escaped the forest"
        yield return StartCoroutine(ShowCutsceneText("You have successfully escaped the forest"));

        // Text 2: "But the forest got burned"
        yield return StartCoroutine(ShowCutsceneText("But the forest got burned"));

        // Text is now hidden (alpha = 0), ready for scene load

        // Load Forest_Hub scene (TODO: Replace with burned version when ready)
        if (showDebugLogs) Debug.Log("Loading Forest_Hub scene (burned version placeholder)...");

        // Load scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Forest_Hub");

        // Wait for scene to finish loading
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (showDebugLogs) Debug.Log("Forest_Hub scene loaded");

        // Wait a moment for scene to initialize
        yield return new WaitForSeconds(0.5f);

        // Fade in to reveal the new scene
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeIn();
            yield return new WaitForSeconds(SceneFadeManager.Instance.fadeDuration);
        }

        // Show achievement dialogue
        yield return new WaitForSeconds(1f);

        if (DialogueManager.Instance != null && survivorAchievementDialogue != null)
        {
            DialogueManager.Instance.Play(survivorAchievementDialogue);
        }
        else
        {
            Debug.LogWarning("EndingChoiceManager: Survivor achievement dialogue not assigned!");
        }

        // Epilogue music continues playing in background
        if (showDebugLogs) Debug.Log("Survivor ending complete!");
    }

    /// <summary>
    /// Shows a single line of cutscene text with fade in/out
    /// </summary>
    private IEnumerator ShowCutsceneText(string text)
    {
        if (cutsceneText == null)
        {
            Debug.LogError("EndingChoiceManager: Cutscene text component not assigned!");
            yield break;
        }

        if (showDebugLogs) Debug.Log($"Showing cutscene text: {text}");

        // Set text (initially transparent)
        cutsceneText.text = text;
        cutsceneText.color = new Color(cutsceneText.color.r, cutsceneText.color.g, cutsceneText.color.b, 0f);

        // Fade in
        yield return StartCoroutine(FadeText(cutsceneText, 0f, 1f, textFadeDuration));

        // Display for duration
        yield return new WaitForSeconds(textDisplayDuration);

        // Fade out
        yield return StartCoroutine(FadeText(cutsceneText, 1f, 0f, textFadeDuration));

        // Clear text
        cutsceneText.text = "";
    }

    /// <summary>
    /// Fades text from one alpha to another
    /// </summary>
    private IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }

        text.color = new Color(text.color.r, text.color.g, text.color.b, endAlpha);
    }
}