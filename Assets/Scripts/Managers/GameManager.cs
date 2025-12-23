using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Config (defaults)")]
    [SerializeField] private PlayerData playerConfig;

    [Header("Player (runtime ref - scene dependent)")]
    [SerializeField] private CharacterContext ctx;

    [Header("Intro Dialogue")]
    [SerializeField] private DialogueData introDialogue;

    [Header("Progression Flags (legacy bools)")]
    public bool hasDiedToGeorge = false;
    public bool yojiDead = false;

    [Header("Act Completion (legacy bools)")]
    public bool act1Cleared = false;
    public bool act2Cleared = false;
    public bool act3Cleared = false;

    // ✅ Enum->bool flags cache (session)
    private readonly Dictionary<GameFlag, bool> flags = new Dictionary<GameFlag, bool>();

    // ---------- Spawn / Travel ----------
    private string pendingSpawnId;
    private bool hasPendingSpawn;

    // ---------- Session init guard ----------
    private bool initializedThisSession = false;

    // ---------- Helpers ----------
    private PlayerStats Stats => (ctx != null) ? ctx.PS : null;

    // ---------- PlayerPrefs Keys ----------
    private const string K_COINS = "Coins";
    private const string K_SWORD_DAMAGE = "SwordDamage";
    private const string K_POTIONS = "Potions";

    private const string K_HAS_TELEPORT = "HasTeleport";
    private const string K_HAS_WAVE = "HasWaveOfLight";
    private const string K_HAS_SPECIAL_SWORD = "HasSpecialSwordUpgrade";

    // legacy keys (you still use these bools in Save/Load)
    private const string K_HAS_DIED_TO_GEORGE = "HasDiedToGeorge";
    private const string K_ACT1 = "Act1Cleared";
    private const string K_ACT2 = "Act2Cleared";
    private const string K_ACT3 = "Act3Cleared";
    private const string K_YOJI_DEAD = "YojiDead";

    // legacy "opening dialogue" key (optional import/back-compat)
    private const string K_OPENING_DIALOGUE = "HasSeenOpeningDialogue";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log("GameManager initialized");
    }

    // ✅ Start as coroutine: wait 1 frame so DialogueManager exists
    private IEnumerator Start()
    {
        yield return null;

        // Ensure we are initialized (in case OnSceneLoaded didn't run yet on first scene)
        if (!initializedThisSession)
        {
            BindPlayerInScene();

            if (HasAnySave())
                LoadProgress();
            else
                ApplyDefaultsFromConfig();

            initializedThisSession = true;
        }

    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ----- Public API for portals -----
    public void TravelTo(string sceneName, string spawnIdInNextScene)
    {
        pendingSpawnId = spawnIdInNextScene;
        hasPendingSpawn = true;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindPlayerInScene();

        if (!initializedThisSession)
        {
            if (HasAnySave())
                LoadProgress();
            else
                ApplyDefaultsFromConfig();

            initializedThisSession = true;
        }

        TryApplyPendingSpawn();

        if (SceneFadeManager.Instance != null)
            SceneFadeManager.Instance.FadeIn();
    }

    private bool HasAnySave()
    {
        return PlayerPrefs.HasKey(K_SWORD_DAMAGE) || PlayerPrefs.HasKey(K_COINS) || PlayerPrefs.HasKey(K_POTIONS);
    }

    private void BindPlayerInScene()
    {
        if (ctx != null && ctx.gameObject != null)
            return;

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            ctx = null;
            Debug.LogWarning("GameManager: No Player found in this scene (tag=Player).");
            return;
        }

        ctx = playerGO.GetComponent<CharacterContext>();
        if (ctx == null)
            Debug.LogError("GameManager: Player found but missing CharacterContext!", playerGO);
    }

    private void TryApplyPendingSpawn()
    {
        if (!hasPendingSpawn || ctx == null) return;

        var spawns = FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        bool found = false;
        for (int i = 0; i < spawns.Length; i++)
        {
            if (spawns[i].id == pendingSpawnId)
            {
                ctx.transform.position = spawns[i].transform.position;
                found = true;
                break;
            }
        }

        if (!found)
            Debug.LogWarning($"GameManager: SpawnPoint id '{pendingSpawnId}' not found in scene '{SceneManager.GetActiveScene().name}'.");

        hasPendingSpawn = false;
        pendingSpawnId = null;
    }

    // ---------- Defaults ----------
    private void ApplyDefaultsFromConfig()
    {
        if (playerConfig == null)
        {
            Debug.LogError("GameManager: playerConfig is not assigned!", this);
            return;
        }

        if (Stats != null)
        {
            Stats.coins = playerConfig.baseStartingCoins;
            Stats.potions = playerConfig.baseStartingPotions;
            Stats.damage = playerConfig.baseDamage;
        }

        if (ctx != null && ctx.AB != null)
        {
            ctx.AB.hasTeleport = false;
            ctx.AB.hasFireSword = false;
            ctx.AB.hasUpgradedSword = false;
        }

        hasDiedToGeorge = false;
        yojiDead = false;
        act1Cleared = false;
        act2Cleared = false;
        act3Cleared = false;

        flags.Clear();

        Debug.Log($"Defaults applied from PlayerConfig: damage={Stats?.damage}, coins={Stats?.coins}, potions={Stats?.potions}");
    }

    // =========================================================
    // ✅ Flags System (Enum -> bool) persisted in PlayerPrefs
    // =========================================================

    private static string FlagKey(GameFlag flag) => "FLAG_" + flag.ToString();

    public void SetFlag(GameFlag flag, bool value)
    {
        flags[flag] = value;

        PlayerPrefs.SetInt(FlagKey(flag), value ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ✅ THIS was the big bug: now it loads from PlayerPrefs when not cached
    public bool GetFlag(GameFlag flag)
    {
        if (flags.TryGetValue(flag, out bool value))
            return value;

        bool saved = PlayerPrefs.GetInt(FlagKey(flag), 0) == 1;
        flags[flag] = saved; // cache
        return saved;
    }

    public void SetDialogueSeen(GameFlag flag)
    {
        SetFlag(flag, true);
    }

    // ---------- Progression Events ----------
    public void OnGeorgeDefeated()
    {
        act1Cleared = true;
        SetFlag(GameFlag.Act1Cleared, true);
        Debug.Log("Act 1 completed - George defeated!");
    }

    public void OnFikaDefeated()
    {
        act2Cleared = true;
        SetFlag(GameFlag.Act2Cleared, true);
        Debug.Log("Act 2 completed - Fika defeated!");
        UpdateStoreState();
    }

    public void OnPhilipDefeated()
    {
        act3Cleared = true;
        SetFlag(GameFlag.Act3Cleared, true);
        Debug.Log("Act 3 completed - Philip defeated!");
    }

    public void OnYojiDeath()
    {
        yojiDead = true;
        Debug.Log("Yoji has died - Store is now free");
        UpdateStoreState();
    }

    public void OnPlayerDiedToGeorge()
    {
        hasDiedToGeorge = true;
        Debug.Log("Player died to George - Special upgrade available");
    }

    public void OnPlayerDied()
    {
        Debug.Log("Player died - respawning in Forest_Hub");
        SaveProgress();
        SceneManager.LoadScene("Forest_Hub");
    }

    // ---------- Store State ----------
    private void UpdateStoreState()
    {
        // Hook for StoreStateManager if you add one later
    }

    // ---------- Save / Load ----------
    public void SaveProgress()
    {
        if (Stats == null)
        {
            Debug.LogWarning("SaveProgress skipped: PlayerStats not available.");
            return;
        }

        PlayerPrefs.SetInt(K_COINS, Stats.coins);
        PlayerPrefs.SetInt(K_SWORD_DAMAGE, Stats.damage);
        PlayerPrefs.SetInt(K_POTIONS, Stats.potions);

        if (ctx != null && ctx.AB != null)
        {
            PlayerPrefs.SetInt(K_HAS_TELEPORT, ctx.AB.hasTeleport ? 1 : 0);
            PlayerPrefs.SetInt(K_HAS_WAVE, ctx.AB.hasFireSword ? 1 : 0);
            PlayerPrefs.SetInt(K_HAS_SPECIAL_SWORD, ctx.AB.hasUpgradedSword ? 1 : 0);
        }

        PlayerPrefs.Save();
        Debug.Log("Game progress saved!");
    }

    public void LoadProgress()
    {
        if (Stats == null)
        {
            Debug.LogWarning("LoadProgress skipped: PlayerStats not available.");
            return;
        }

        int defaultCoins = playerConfig != null ? playerConfig.baseStartingCoins : 0;
        int defaultDamage = playerConfig != null ? playerConfig.baseDamage : 10;
        int defaultPotions = playerConfig != null ? playerConfig.baseStartingPotions : 5;

        Stats.coins = PlayerPrefs.GetInt(K_COINS, defaultCoins);
        Stats.damage = PlayerPrefs.GetInt(K_SWORD_DAMAGE, defaultDamage);
        Stats.potions = PlayerPrefs.GetInt(K_POTIONS, defaultPotions);

        if (ctx != null && ctx.AB != null)
        {
            ctx.AB.hasTeleport = PlayerPrefs.GetInt(K_HAS_TELEPORT, 0) == 1;
            ctx.AB.hasFireSword = PlayerPrefs.GetInt(K_HAS_WAVE, 0) == 1;
            ctx.AB.hasUpgradedSword = PlayerPrefs.GetInt(K_HAS_SPECIAL_SWORD, 0) == 1;
        }

        hasDiedToGeorge = PlayerPrefs.GetInt(K_HAS_DIED_TO_GEORGE, 0) == 1;
        act1Cleared = PlayerPrefs.GetInt(K_ACT1, 0) == 1;
        act2Cleared = PlayerPrefs.GetInt(K_ACT2, 0) == 1;
        act3Cleared = PlayerPrefs.GetInt(K_ACT3, 0) == 1;
        yojiDead = PlayerPrefs.GetInt(K_YOJI_DEAD, 0) == 1;

        SetFlag(GameFlag.Act1Cleared, act1Cleared);
        SetFlag(GameFlag.Act2Cleared, act2Cleared);
        SetFlag(GameFlag.Act3Cleared, act3Cleared);

        if (PlayerPrefs.GetInt(K_OPENING_DIALOGUE, 0) == 1)
            SetFlag(GameFlag.OpeningDialogueSeen, true);

        _ = GetFlag(GameFlag.OpeningDialogueSeen);

        Debug.Log($"Game progress loaded! Damage: {Stats.damage}, Coins: {Stats.coins}, Potions: {Stats.potions}");
        UpdateStoreState();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        ApplyDefaultsFromConfig();
        initializedThisSession = true;
        Debug.Log("Game progress reset to defaults (from PlayerConfig).");
    }
}
