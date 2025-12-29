using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages screen fade effects for scene transitions.
/// IMPROVED: Uses SceneManager callback to ensure fade-in happens after scene loads
/// </summary>
public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance { get; private set; }

    [Header("Fade Settings")]
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private Canvas fadeCanvas;
    private Image fadeImage;
    private bool isFading = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (showDebugLogs)
                Debug.Log("SceneFadeManager: Initialized");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupFadeUI();

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe when destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SetupFadeUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // Always on top

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create full-screen black image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(fadeCanvas.transform);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f); // Start BLACK

        // Stretch to fill screen
        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        if (showDebugLogs)
            Debug.Log("Fade UI created - starting BLACK");
    }

    private void Start()
    {
        // Fade in after a brief delay
        Invoke(nameof(InitialFadeIn), 0.2f);
    }

    private void InitialFadeIn()
    {
        if (showDebugLogs)
            Debug.Log("Starting initial fade in...");

        FadeIn();
    }

    // Called automatically when a new scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (showDebugLogs)
            Debug.Log($"Scene loaded: {scene.name} - Starting fade in");

        // Fade in whenever a new scene loads
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f, 0f)); // Always fade from black to clear
    }

    /// <summary>
    /// Fade screen to black (before loading new scene)
    /// </summary>
    public void FadeOut()
    {
        if (showDebugLogs)
            Debug.Log("Fading OUT (to black)");

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(fadeImage.color.a, 1f));
    }

    /// <summary>
    /// Fade screen to transparent (after loading new scene)
    /// </summary>
    public void FadeIn()
    {
        if (showDebugLogs)
            Debug.Log("Fading IN (to clear)");

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(fadeImage.color.a, 0f));
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha)
    {
        isFading = true;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
        isFading = false;

        if (showDebugLogs)
            Debug.Log($"Fade complete - Alpha: {targetAlpha}");
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = Mathf.Clamp01(alpha);
            fadeImage.color = color;
        }
    }

    public void SetFadeOutImmediate()
    {
        StopAllCoroutines();
        SetAlpha(1f);
        isFading = false;
    }

    public void SetFadeInImmediate()
    {
        StopAllCoroutines();
        SetAlpha(0f);
        isFading = false;
    }
}