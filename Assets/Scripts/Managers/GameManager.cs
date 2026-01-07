using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave.Models;
using System.Collections;

/// <summary>
/// GameManager - Manages game progression and flags (Cloud Save ONLY)
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private static bool cloudReady = false;
    public bool IsProgressLoaded { get; private set; } = false;
    public event System.Action ProgressLoaded;

    [Header("Debug")]
    public bool showDebugLogs = true;

    [Header("Unified Flag System")]
    private Dictionary<GameFlag, bool> flags = new Dictionary<GameFlag, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitDefaultFlags();

            if (showDebugLogs) Debug.Log("GameManager initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        await LoadProgress();
    }

    #region Flag System

    public void SetFlag(GameFlag flag, bool value)
    {
        flags[flag] = value;

        if (showDebugLogs)
            Debug.Log($"Flag set: {flag} = {value}");
    }

    public bool GetFlag(GameFlag flag)
    {
        return flags.TryGetValue(flag, out bool value) && value;
    }

    private void InitDefaultFlags()
    {
        foreach (GameFlag flag in System.Enum.GetValues(typeof(GameFlag)))
        {
            if (flag == GameFlag.None) continue;

            if (!flags.ContainsKey(flag))
                flags[flag] = false;
        }
    }

    #endregion

    #region Progression Events

    public void OnGeorgeDefeated()
    {
        SetFlag(GameFlag.GeorgeDefeated, true);
        UpdateStoreState();
        SaveProgress();
    }

    public void OnFikaDefeated()
    {
        SetFlag(GameFlag.FikaDefeated, true);
        UpdateStoreState();
        SaveProgress();
    }

    public void OnPhilipDefeated()
    {
        SetFlag(GameFlag.PhillipDefeated, true);
        UpdateStoreState();
        SaveProgress();
    }

    public void OnYojiDeath()
    {
        SetFlag(GameFlag.YojiDead, true);
        UpdateStoreState();
        SaveProgress();
    }

    public void OnPlayerDiedToGeorge()
    {
        SetFlag(GameFlag.GeorgeFirstEncounter, true);
        UpdateStoreState();
        SaveProgress();
    }

    public void OnPlayerDied()
    {
        if (showDebugLogs) Debug.Log("Player died - respawning in ForestHub");

        if (AudioManager.Instance != null)
        {
            //Stops music immediately and clears all state
            AudioManager.Instance.StopMusicImmediately();
        }

        SaveProgress();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Forest_Hub");
    }

    #endregion

    #region Store

    private void UpdateStoreState()
    {
        if (StoreStateManager.Instance != null)
            StoreStateManager.Instance.UpdateStoreStateFromGameManager();
    }

    #endregion

    #region Cloud Save

    private async Task EnsureCloudReady()
    {
        if (cloudReady) return;

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        cloudReady = true;

        if (showDebugLogs)
        {
            Debug.Log("☁️ Cloud ready");
            Debug.Log("🆔 PlayerID = " + AuthenticationService.Instance.PlayerId);
        }
    }

    public async void SaveProgress()
    {
        try
        {
            await EnsureCloudReady();

            InitDefaultFlags();

            List<(string key, object value)> list = new List<(string, object)>();
            foreach (var kvp in flags)
            {
                list.Add(($"FLAG_{kvp.Key}", kvp.Value ? 1 : 0));
            }

            await DatabaseManager.SaveData(list.ToArray());

            if (showDebugLogs)
                Debug.Log("Cloud save (flags) completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Cloud save failed: " + e);
        }
    }

    public async Task LoadProgress()
    {
        try
        {
            await EnsureCloudReady();

            List<string> keys = new List<string>();
            foreach (GameFlag flag in System.Enum.GetValues(typeof(GameFlag)))
            {
                if (flag == GameFlag.None) continue;
                keys.Add($"FLAG_{flag}");
            }

            Dictionary<string, Item> cloudData =
                await DatabaseManager.LoadData(keys.ToArray());

            flags.Clear();
            InitDefaultFlags();

            foreach (GameFlag flag in System.Enum.GetValues(typeof(GameFlag)))
            {
                if (flag == GameFlag.None) continue;

                string key = $"FLAG_{flag}";
                if (cloudData.TryGetValue(key, out Item item))
                {
                    flags[flag] = item.Value.GetAs<int>() == 1;
                }
            }

            IsProgressLoaded = true;

            if (showDebugLogs)
                Debug.Log($"✅ Cloud load completed. Flags loaded: {cloudData.Count}");

            // ✅ apply things that depend on flags right now
            UpdateStoreState();

            // ✅ notify others
            ProgressLoaded?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Cloud load failed: " + e);

            IsProgressLoaded = true;

            UpdateStoreState();
            ProgressLoaded?.Invoke();
        }
    }


    #endregion
}
