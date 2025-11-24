using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Progress Flags")]
    public bool hasTalkedToYoji = false;   // הפלג החדש

    public bool gameWon = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnBossDefeated()
    {
        if (gameWon) return;

        gameWon = true;
        Debug.Log("Boss defeated! YOU WIN!");
        // בהמשך: מעבר לסצנת מעבר ליער הבא
    }

    // פונקציה נוחה ליוג'י לקרוא
    public void SetYojiTalked()
    {
        if (!hasTalkedToYoji)
        {
            hasTalkedToYoji = true;
            Debug.Log("Yoji first dialogue completed. Barrier can now open.");
        }
    }
}
