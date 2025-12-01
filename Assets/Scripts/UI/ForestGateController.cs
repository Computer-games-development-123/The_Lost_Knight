using UnityEngine;
using UnityEngine.SceneManagement;

public class ForestGateController : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "GameScene";
    [SerializeField] private GameObject promptText;

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
        {
            promptText.SetActive(true);
            Debug.Log("[Gate] show prompt");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        if (promptText != null)
        {
            promptText.SetActive(false);
            Debug.Log("[Gate] hide prompt");
        }
    }

    private void Update()
    {
        if (!isPlayerInside) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
