using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameManager - Manages game progression and flags
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Debug")]
    public bool showDebugLogs = true;

    [Header("Progression Flags (Legacy - for backwards compatibility)")]
    public bool hasDiedToGeorge = false;
    public bool yojiDead = false;

    [Header("Unified Flag System")]
    private Dictionary<GameFlag, bool> flags = new Dictionary<GameFlag, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (showDebugLogs) Debug.Log("✅ GameManager initialized");
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

        // Sync legacy public bools
        SyncPublicFlags();

        if (showDebugLogs) Debug.Log($"🚩 Flag set: {flag} = {value}");
    }

    public bool GetFlag(GameFlag flag)
    {
        if (flags.TryGetValue(flag, out bool value))
            return value;
        return false;
    }

    private void SyncPublicFlags()
    {
        // Sync only the legacy flags we still use
        hasDiedToGeorge = GetFlag(GameFlag.GeorgeFirstEncounter);
        yojiDead = GetFlag(GameFlag.YojiDead);
    }

    #endregion

    #region Backwards Compatibility - hasSpecialSwordUpgrade

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
                    return abilities.hasUpgradedSword;
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
        SetFlag(GameFlag.GeorgeDefeated, true);
        if (showDebugLogs) Debug.Log("🎉 George defeated!");
        UpdateStoreState();
        SaveProgress();
    }

    public void OnFikaDefeated()
    {
        SetFlag(GameFlag.FikaDefeated, true);
        if (showDebugLogs) Debug.Log("🎉 Fika defeated!");
        UpdateStoreState();
        SaveProgress();
    }

    public void OnPhilipDefeated()
    {
        SetFlag(GameFlag.PhillipDefeated, true);
        if (showDebugLogs) Debug.Log("🎉 Philip defeated!");
        UpdateStoreState();
        SaveProgress();
    }

    public void OnYojiDeath()
    {
        SetFlag(GameFlag.YojiDead, true);
        if (showDebugLogs) Debug.Log("💀 Yoji has died - Store is now free");
        UpdateStoreState();
        SaveProgress();
    }

    public void OnPlayerDiedToGeorge()
    {
        SetFlag(GameFlag.GeorgeFirstEncounter, true);
        if (showDebugLogs) Debug.Log("⚔️ Player died to George - Special upgrade available");
        SaveProgress();
    }

    public void OnPlayerDied()
    {
        if (showDebugLogs) Debug.Log("💀 Player died - respawning in ForestHub");
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
        if (showDebugLogs) Debug.Log("💾 Saving game progress...");

        SaveFlags();
        PlayerPrefs.Save();

        if (showDebugLogs) Debug.Log("✅ Game progress saved!");
    }

    private void SaveFlags()
    {
        foreach (var kvp in flags)
        {
            PlayerPrefs.SetInt("FLAG_" + kvp.Key.ToString(), kvp.Value ? 1 : 0);
        }
    }

    public void LoadProgress()
    {
        if (showDebugLogs) Debug.Log("📂 Loading game progress...");

        LoadFlags();

        if (showDebugLogs) Debug.Log("✅ Game progress loaded!");
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
        if (showDebugLogs) Debug.Log("🔄 Resetting all progress...");

        PlayerPrefs.DeleteAll();
        flags.Clear();

        if (showDebugLogs) Debug.Log("✅ Game progress reset!");
    }

    #endregion
}