using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// SIMPLIFIED EndingChoiceManager
/// Only handles button selection and loading the appropriate ending scene.
/// Now supports both keyboard navigation AND direct button tap (mobile).
/// </summary>
public class EndingChoiceManager : MonoBehaviour
{
    public static EndingChoiceManager Instance { get; private set; }

    [Header("Choice UI References")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Button finishDitorButton;
    [SerializeField] private Button escapeForestButton;

    [Header("Button Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Scene Names")]
    [SerializeField] private string heroEndingSceneName = "Ending_HeroOfTheForest";
    [SerializeField] private string survivorEndingSceneName = "Ending_SurvivorOfTheForest";

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private int currentButtonIndex = 0; // 0 = Finish Ditor, 1 = Escape Forest
    private bool isChoosingEnding = false;
    private bool canAcceptInput = false;
    private Image finishDitorImage;
    private Image escapeForestImage;

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

        // Hide everything at start
        if (choicePanel != null)
            choicePanel.SetActive(false);

        // Get button images for color changes
        if (finishDitorButton != null)
        {
            finishDitorImage = finishDitorButton.GetComponent<Image>();

            // Wire up the OnClick for direct tap support
            finishDitorButton.onClick.AddListener(OnFinishDitorTapped);
        }

        if (escapeForestButton != null)
        {
            escapeForestImage = escapeForestButton.GetComponent<Image>();

            // Wire up the OnClick for direct tap support
            escapeForestButton.onClick.AddListener(OnEscapeForestTapped);
        }
    }

    private void Update()
    {
        if (!isChoosingEnding || !canAcceptInput) return;

        // Handle navigation between buttons (use arrow keys only to avoid conflicts)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentButtonIndex = 0;
            UpdateButtonSelection();
            if (showDebugLogs) Debug.Log("Selected: Finish Ditor button");
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentButtonIndex = 1;
            UpdateButtonSelection();
            if (showDebugLogs) Debug.Log("Selected: Escape Forest button");
        }

        // Confirm selection with Enter key
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (showDebugLogs) Debug.Log("Enter key pressed - confirming selection");
            ConfirmSelection();
        }
    }

    /// <summary>
    /// Called by DitorBoss after the ending dialogue completes
    /// Shows the two choice buttons
    /// </summary>
    public void ShowEndingChoice()
    {
        if (showDebugLogs) Debug.Log("EndingChoiceManager: Showing ending choice");

        StartCoroutine(ShowChoiceWithDelay());
    }

    private IEnumerator ShowChoiceWithDelay()
    {
        // Small delay to ensure dialogue system has fully released input
        yield return new WaitForSeconds(0.3f);

        if (choicePanel != null)
            choicePanel.SetActive(true);

        isChoosingEnding = true;
        canAcceptInput = false;
        currentButtonIndex = 0;
        UpdateButtonSelection();

        // Disable player movement/actions
        if (UserInputManager.Instance != null)
        {
            UserInputManager.Instance.DisableInput();
        }

        if (showDebugLogs) Debug.Log("Choice panel shown - waiting for input cooldown");

        // Wait a bit more before accepting input (prevents accidental F key from dialogue)
        yield return new WaitForSeconds(0.5f);

        canAcceptInput = true;
        if (showDebugLogs) Debug.Log("Now accepting input! Use UP/DOWN to select, ENTER to confirm, or TAP a button");
    }

    private void UpdateButtonSelection()
    {
        // Update button colors to show which is selected
        if (currentButtonIndex == 0)
        {
            // Finish Ditor selected
            if (finishDitorImage != null)
                finishDitorImage.color = selectedColor;
            if (escapeForestImage != null)
                escapeForestImage.color = normalColor;
        }
        else
        {
            // Escape Forest selected
            if (finishDitorImage != null)
                finishDitorImage.color = normalColor;
            if (escapeForestImage != null)
                escapeForestImage.color = selectedColor;
        }
    }

    private void ConfirmSelection()
    {
        if (!canAcceptInput)
        {
            if (showDebugLogs) Debug.Log("Input not ready yet - ignoring confirmation");
            return;
        }

        canAcceptInput = false;
        isChoosingEnding = false;

        if (currentButtonIndex == 0)
        {
            OnFinishDitorChosen();
        }
        else
        {
            OnEscapeForestChosen();
        }
    }

    /// <summary>
    /// Called when the player TAPS the Finish Ditor button (mobile)
    /// </summary>
    private void OnFinishDitorTapped()
    {
        if (!canAcceptInput || !isChoosingEnding)
        {
            if (showDebugLogs) Debug.Log("Tap ignored - input not ready");
            return;
        }

        if (showDebugLogs) Debug.Log("Finish Ditor button TAPPED");

        canAcceptInput = false;
        isChoosingEnding = false;
        currentButtonIndex = 0;
        OnFinishDitorChosen();
    }

    /// <summary>
    /// Called when the player TAPS the Escape Forest button (mobile)
    /// </summary>
    private void OnEscapeForestTapped()
    {
        if (!canAcceptInput || !isChoosingEnding)
        {
            if (showDebugLogs) Debug.Log("Tap ignored - input not ready");
            return;
        }

        if (showDebugLogs) Debug.Log("Escape Forest button TAPPED");

        canAcceptInput = false;
        isChoosingEnding = false;
        currentButtonIndex = 1;
        OnEscapeForestChosen();
    }

    private void OnFinishDitorChosen()
    {
        if (showDebugLogs) Debug.Log("Player chose: Finish Ditor (Hero ending)");

        isChoosingEnding = false;

        if (choicePanel != null)
            choicePanel.SetActive(false);

        StartCoroutine(LoadEndingScene(heroEndingSceneName));
    }

    private void OnEscapeForestChosen()
    {
        if (showDebugLogs) Debug.Log("Player chose: Escape Forest (Survivor ending)");

        isChoosingEnding = false;

        if (choicePanel != null)
            choicePanel.SetActive(false);

        StartCoroutine(LoadEndingScene(survivorEndingSceneName));
    }

    private IEnumerator LoadEndingScene(string sceneName)
    {
        // Set flag to prevent boss music from being restored
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.isTransitioningToEnding = true;
            if (showDebugLogs) Debug.Log("Set isTransitioningToEnding = true");
        }

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

        if (showDebugLogs) Debug.Log($"Loading ending scene: {sceneName}");

        SceneManager.LoadScene(sceneName);
    }
}