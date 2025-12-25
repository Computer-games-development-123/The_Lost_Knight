using UnityEngine;

/// <summary>
/// Store State Manager - Tracks store progression states
/// FIXED: Uses GameFlag enum (NOT strings!)
/// </summary>
public class StoreStateManager : MonoBehaviour
{
    public static StoreStateManager Instance { get; private set; }

    public enum StoreState
    {
        Locked,           // Before first George death
        PostGeorge,       // After George death, basic items available
        PostFika,         // After Fika, Sword of Light visible
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
            Debug.Log("✅ StoreStateManager: Initialized and persisting across scenes");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize state based on GameManager flags
        UpdateStoreStateFromGameManager();
    }

    public void UpdateStoreStateFromGameManager()
    {
        if (GameManager.Instance == null) return;

        // Determine state based on progression
        if (GameManager.Instance.yojiDead)
        {
            currentState = StoreState.PostPhilip;
        }
        // ✅ FIXED: Use GameFlag enum instead of string!
        else if (GameManager.Instance.GetFlag(GameFlag.Act2Cleared))
        {
            currentState = StoreState.PostFika;
        }
        else if (GameManager.Instance.hasSpecialSwordUpgrade)
        {
            currentState = StoreState.PostGeorge;
        }
        else
        {
            currentState = StoreState.Locked;
        }

        Debug.Log($"🏪 Store state updated to: {currentState}");
    }

    public void SetStoreState(StoreState newState)
    {
        currentState = newState;
        Debug.Log($"🏪 Store state manually set to: {currentState}");
    }

    public bool IsStoreUnlocked()
    {
        return currentState != StoreState.Locked;
    }

    public bool IsSwordOfLightRevealed()
    {
        return currentState == StoreState.PostFika || currentState == StoreState.PostPhilip;
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
            case StoreState.PostGeorge:
                return "Yoji's Shop";
            case StoreState.PostFika:
                return "Yoji's Shop - Rare Item Available!";
            case StoreState.PostPhilip:
                return "Yoji's Legacy - Take What You Need";
            default:
                return "Unknown";
        }
    }
}