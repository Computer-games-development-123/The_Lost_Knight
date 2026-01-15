using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause Menu Manager - Handles pausing the game and showing controls
/// Press P to pause/unpause
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.P;

    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Make sure pause menu starts hidden
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Set Canvas sorting order to appear above all other UI
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 10000; // Very high value to ensure it's on top
        }
    }

    private void Update()
    {
        // Don't allow pausing during dialogue
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        // Don't allow pausing when store is open
        ListStoreController storeController = FindFirstObjectByType<ListStoreController>();
        if (storeController != null && storeController.IsStoreOpen)
        {
            return;
        }

        // Toggle pause with P key
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    /// <summary>
    /// Pauses the game
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f; // Stop game time

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Disable player input
        if (UserInputManager.Instance != null)
        {
            UserInputManager.Instance.DisableInput();
        }

        // Pause audio
        if (AudioManager.Instance != null)
        {
            AudioListener.pause = true;
        }
    }

    /// <summary>
    /// Resumes the game
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f; // Resume game time

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Re-enable player input
        if (UserInputManager.Instance != null)
        {
            UserInputManager.Instance.EnableInput();
        }

        // Resume audio
        if (AudioManager.Instance != null)
        {
            AudioListener.pause = false;
        }
    }

    /// <summary>
    /// Quit to main menu
    /// </summary>
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale
        AudioListener.pause = false; // Resume audio
        isPaused = false;

        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Quit game (only works in builds, not in editor)
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnDestroy()
    {
        // Make sure time scale and audio are reset when scene changes
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (Instance == this)
        {
            Instance = null;
        }
    }
}