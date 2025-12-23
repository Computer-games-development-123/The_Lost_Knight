using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalActiveByFlag : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject portal;

    [Header("Condition")]
    [SerializeField] private GameFlag flag;

    [Header("Optional")]
    [SerializeField] private bool openOnlyOnce = true;

    private bool opened = false;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Apply();
    }

    private void Update()
    {
        if (openOnlyOnce && opened) return;
        Apply();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply();
    }

    private void Apply()
    {
        if (portal == null || GameManager.Instance == null || flag == GameFlag.None)
            return;

        if (GameManager.Instance.GetFlag(flag))
        {
            if (!portal.activeSelf)
                Debug.LogWarning($"portal {portal.name} is activated");

            portal.SetActive(true);
            opened = true;
        }
    }
}
