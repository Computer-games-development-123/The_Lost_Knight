using UnityEngine;

public class UserInputManager : MonoBehaviour
{
    public static UserInputManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool inputEnabled = true;
    public bool IsInputEnabled => inputEnabled;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void DisableInput()
    {
        inputEnabled = false;
        Debug.Log("🔒 Player input disabled");
    }

    public void EnableInput()
    {
        inputEnabled = true;
        Debug.Log("🔓 Player input enabled");
    }
    public void SetInput(bool enabled)
    {
        inputEnabled = enabled;
        Debug.Log(enabled ? "🔓 Player input enabled" : "🔒 Player input disabled");
    }
}

// UserInputManager.Instance.DisableInput(); הפעלת נעילת מקשים
// UserInputManager.Instance.EnableInput();  בטילת נעילת מקשים
