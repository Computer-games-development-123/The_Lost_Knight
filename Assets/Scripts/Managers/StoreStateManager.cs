using UnityEngine;

/// <summary>
/// Store State Manager - Tracks store progression states
/// </summary>
public class StoreStateManager : MonoBehaviour
{
    public static StoreStateManager Instance { get; private set; }

    public enum StoreState
    {
        Locked,           // Before first George encounter
        UnlockedBasic,    // After first George encounter (sword upgrade) - store opens but special items hidden
        PostGeorge,       // After George defeated - Fireball spell revealed
        PostFika,         // After Fika defeated - Breath of Fire revealed
        PostPhilip        // After Philip (Yoji dead), everything free
    }

    [Header("Current Store State")]
    public StoreState currentState = StoreState.Locked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("StoreStateManager initialized");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        UpdateStoreStateFromGameManager();
    }

    private void Start()
    {
        GameManagerReadyHelper.RunWhenReady(this, UpdateStoreStateFromGameManager);
    }

    public void UpdateStoreStateFromGameManager()
    {
        if (GameManager.Instance == null) return;

        // Determine state based on boss defeats and flags
        if (GameManager.Instance.GetFlag(GameFlag.YojiDead))
        {
            currentState = StoreState.PostPhilip;
        }
        else if (GameManager.Instance.GetFlag(GameFlag.FikaDefeated))
        {
            currentState = StoreState.PostFika;
        }
        else if (GameManager.Instance.GetFlag(GameFlag.GeorgeDefeated))
        {
            currentState = StoreState.PostGeorge;
        }
        else if (GameManager.Instance.GetFlag(GameFlag.YojiUnlocksStore))
        {
            // Store is unlocked but George not yet defeated - basic items only
            currentState = StoreState.UnlockedBasic;
        }
        else
        {
            currentState = StoreState.Locked;
        }

        Debug.Log($"Store state: {currentState}");
    }

    public void SetStoreState(StoreState newState)
    {
        currentState = newState;
        Debug.Log($"Store state set to: {currentState}");
    }

    public bool IsStoreUnlocked()
    {
        return currentState != StoreState.Locked;
    }

    public bool IsFireballRevealed()
    {
        // Fireball spell unlocks ONLY after George is actually defeated
        bool georgeDefeated = GameManager.Instance != null && GameManager.Instance.GetFlag(GameFlag.GeorgeDefeated);
        bool revealed = georgeDefeated;
        Debug.Log($"[StoreStateManager] IsFireballRevealed check: GeorgeDefeated={georgeDefeated}, revealed={revealed}");
        return revealed;
    }

    public bool IsBreathOfFireRevealed()
    {
        // Breath of Fire unlocks after Fika is defeated
        bool revealed = currentState >= StoreState.PostFika;
        Debug.Log($"[StoreStateManager] IsBreathOfFireRevealed check: currentState={currentState}, PostFika={StoreState.PostFika}, revealed={revealed}");
        return revealed;
    }

    // Legacy method for backwards compatibility
    public bool IsWaveOfFireRevealed()
    {
        // Keep for any other code that might use it
        return IsFireballRevealed();
    }

    public bool IsStoreFree()
    {
        return currentState == StoreState.PostPhilip;
    }

    public string GetStateDisplayName()
    {
        switch (currentState)
        {
            case StoreState.Locked:
                return "Locked";
            case StoreState.UnlockedBasic:
                return "Yoji's Shop";
            case StoreState.PostGeorge:
                return "Yoji's Shop";
            case StoreState.PostFika:
                return "Yoji's Shop";
            case StoreState.PostPhilip:
                return "Yoji's Legacy";
            default:
                return "Unknown";
        }
    }
}