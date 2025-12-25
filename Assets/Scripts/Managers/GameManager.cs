using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameManager - ONLY manages game progression and flags
/// Does NOT manage player stats (that's PlayerState's job!)
/// Does NOT have UsePotion/SpendCoins/etc (call PlayerState directly!)
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Progression Flags (Public for backwards compatibility)")]
    public bool hasDiedToGeorge = false;
    public bool yojiDead = false;
    public bool act1Cleared = false;
    public bool act2Cleared = false;
    public bool act3Cleared = false;

    [Header("Unified Flag System")]
    private Dictionary<GameFlag, bool> flags = new Dictionary<GameFlag, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ GameManager initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadProgress();
    }

    #region Flag System (UNIFIED)
    
    public void SetFlag(GameFlag flag, bool value)
    {
        flags[flag] = value;
        
        // Sync public bools for backwards compatibility
        SyncPublicFlags();
        
        Debug.Log($"🚩 Flag set: {flag} = {value}");
    }

    public bool GetFlag(GameFlag flag)
    {
        if (flags.TryGetValue(flag, out bool value))
            return value;
        return false;
    }

    private void SyncPublicFlags()
    {
        // Sync enum flags to public bools
        act1Cleared = GetFlag(GameFlag.Act1Cleared);
        act2Cleared = GetFlag(GameFlag.Act2Cleared);
        act3Cleared = GetFlag(GameFlag.Act3Cleared);
        hasDiedToGeorge = GetFlag(GameFlag.GeorgeFirstEncounter);
        yojiDead = GetFlag(GameFlag.YojiDead);
    }
    
    #endregion

    #region Backwards Compatibility - hasSpecialSwordUpgrade
    
    // For GeorgeBoss to check if player has upgrade
    public bool hasSpecialSwordUpgrade
    {
        get
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Abilities abilities = player.GetComponent<Abilities>();
                if (abilities != null)
                {
                    return abilities.hasUpgradedSword; // ✅ CORRECT: Check sword upgrade flag!
                }
            }
            return false;
        }
    }
    
    public bool hasSeenOpeningDialogue
    {
        get => GetFlag(GameFlag.OpeningDialogueSeen);
        set => SetFlag(GameFlag.OpeningDialogueSeen, value);
    }
    
    #endregion

    #region Progression Events
    
    public void OnGeorgeDefeated()
    {
        SetFlag(GameFlag.Act1Cleared, true);
        SetFlag(GameFlag.GeorgeDefeated, true);
        Debug.Log("🎉 Act 1 completed - George defeated!");
        SaveProgress();
    }

    public void OnFikaDefeated()
    {
        SetFlag(GameFlag.Act2Cleared, true);
        SetFlag(GameFlag.FikaDefeated, true);
        Debug.Log("🎉 Act 2 completed - Fika defeated!");
        UpdateStoreState();
        SaveProgress();
    }

    public void OnPhilipDefeated()
    {
        SetFlag(GameFlag.Act3Cleared, true);
        SetFlag(GameFlag.PhillipDefeated, true);
        Debug.Log("🎉 Act 3 completed - Philip defeated!");
        SaveProgress();
    }

    public void OnYojiDeath()
    {
        SetFlag(GameFlag.YojiDead, true);
        Debug.Log("💀 Yoji has died - Store is now free");
        UpdateStoreState();
        SaveProgress();
    }

    public void OnPlayerDiedToGeorge()
    {
        SetFlag(GameFlag.GeorgeFirstEncounter, true);
        Debug.Log("⚔️ Player died to George - Special upgrade available");
        SaveProgress();
    }

    public void OnPlayerDied()
    {
        Debug.Log("💀 Player died - respawning in ForestHub");
        SaveProgress();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Forest_Hub");
    }
    
    #endregion

    #region Store State Management
    
    private void UpdateStoreState()
    {
        if (StoreStateManager.Instance != null)
        {
            StoreStateManager.Instance.UpdateStoreStateFromGameManager();
        }
    }
    
    #endregion

    #region Save/Load System
    
    public void SaveProgress()
    {
        Debug.Log("💾 Saving game progress...");
        
        // Save progression flags
        SaveFlags();
        
        // TODO: Save player stats directly from components when needed
        
        PlayerPrefs.Save();
        Debug.Log("✅ Game progress saved!");
    }

    private void SaveFlags()
    {
        // Save all progression flags
        foreach (var kvp in flags)
        {
            PlayerPrefs.SetInt("FLAG_" + kvp.Key.ToString(), kvp.Value ? 1 : 0);
        }
    }

    public void LoadProgress()
    {
        Debug.Log("📂 Loading game progress...");
        
        // Load progression flags
        LoadFlags();
        
        // TODO: Load player stats directly to components when needed
        
        Debug.Log("✅ Game progress loaded!");
    }

    private void LoadFlags()
    {
        foreach (GameFlag flag in System.Enum.GetValues(typeof(GameFlag)))
        {
            if (flag == GameFlag.None) continue;
            
            int saved = PlayerPrefs.GetInt("FLAG_" + flag.ToString(), 0);
            flags[flag] = (saved == 1);
        }
        
        SyncPublicFlags();
    }

    public void ResetProgress()
    {
        Debug.Log("🔄 Resetting all progress...");
        
        PlayerPrefs.DeleteAll();
        flags.Clear();
        
        // TODO: Reset player stats on components when needed
        
        Debug.Log("✅ Game progress reset to defaults");
    }
    
    #endregion
}