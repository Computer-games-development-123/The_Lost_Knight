using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    [Tooltip("Name of the scene to load when the player uses this portal")]
    public string targetSceneName = "ForestHub";

    [Header("UI Prompt (optional)")]
    [SerializeField] private GameObject promptText;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    private bool isPlayerInside = false;

    private void Start()
    {
        if (promptText != null)
            promptText.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        if (promptText != null)
            promptText.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        if (promptText != null)
            promptText.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInside) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning("[ScenePortal] targetSceneName is empty!");
                return;
            }

            SceneManager.LoadScene(targetSceneName);
        }
    }
}
