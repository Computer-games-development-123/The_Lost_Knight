using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Only handles button selection and loading the appropriate ending scene
/// All cutscene logic is in the ending scenes themselves via EndingSceneManager
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
            finishDitorButton.onClick.AddListener(OnFinishDitorChosen);
        }

        if (escapeForestButton != null)
        {
            escapeForestImage = escapeForestButton.GetComponent<Image>();
            escapeForestButton.onClick.AddListener(OnEscapeForestChosen);
        }
    }

    private void Update()
    {
        if (!isChoosingEnding) return;

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

        // Confirm selection with F key
        if (Input.GetKeyDown(KeyCode.F))
        {
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

        if (choicePanel != null)
            choicePanel.SetActive(true);

        isChoosingEnding = true;
        currentButtonIndex = 0;
        UpdateButtonSelection();

        // Disable player movement/actions (should already be disabled, but just in case)
        if (UserInputManager.Instance != null)
        {
            UserInputManager.Instance.DisableInput();
        }
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
        if (currentButtonIndex == 0)
        {
            OnFinishDitorChosen();
        }
        else
        {
            OnEscapeForestChosen();
        }
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
        // Fade to black
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOut();
            yield return new WaitForSeconds(SceneFadeManager.Instance.fadeDuration);
        }
        else
        {
            // Fallback if no fade manager
            yield return new WaitForSeconds(1f);
        }

        if (showDebugLogs) Debug.Log($"Loading ending scene: {sceneName}");

        // Load the ending scene
        SceneManager.LoadScene(sceneName);
    }
}