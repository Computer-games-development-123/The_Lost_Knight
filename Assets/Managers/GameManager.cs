using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Progress Flags")]
    public bool hasTalkedToYoji = false;

    [Header("Game State")]
    public bool gameWon = false;

    [Header("Scene Names")]
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string deathSceneName = "DeathScene";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // נקרא מ-WaveManager כשבוס מת
    public void OnBossDefeated()
    {
        if (gameWon) return;

        gameWon = true;
        Debug.Log("Boss defeated! YOU WIN!");

        SceneManager.LoadScene(winSceneName);
    }

    // נקרא כששחקן מת
    public void OnPlayerDied()
    {
        Debug.Log("Loading death scene...");
        SceneManager.LoadScene(deathSceneName);
    }

    // ליוג'י
    public void SetYojiTalked()
    {
        if (!hasTalkedToYoji)
        {
            hasTalkedToYoji = true;
            Debug.Log("Yoji first dialogue completed. Barrier can now open.");
        }
    }
}
