using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public int coins = 0;
    public int swordDamage = 8; // BASE DAMAGE SET TO 8
    public float maxHealth = 100f; // BASE MAX HP
    public int potions = 5;
    public bool hasTeleport = false;
    public bool hasWaveOfLight = false;

    [Header("Progression Flags")]
    public bool hasSpecialSwordUpgrade = false;
    public bool hasDiedToGeorge = false;
    public bool yojiDead = false;

    [Header("Act Completion")]
    public bool act1Cleared = false;
    public bool act2Cleared = false;
    public bool act3Cleared = false;

    [Header("NPC Interactions")]
    private Dictionary<string, bool> npcTalkFlags = new Dictionary<string, bool>();

    [Header("Dialogue Flags")]
    private Dictionary<string, bool> dialogueFlags = new Dictionary<string, bool>();

    private Dictionary<string, bool> generalFlags = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager initialized - Base sword damage: 8");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load saved progress when game starts
        if (PlayerPrefs.HasKey("SwordDamage"))
        {
            LoadProgress();
            Debug.Log($"📂 Loaded progress - Sword Damage: {swordDamage}");
        }
    }

    #region Currency
    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"Coins added: {amount}. Total: {coins}");
    }

    public void SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            Debug.Log($"Coins spent: {amount}. Remaining: {coins}");
        }
        else
        {
            Debug.LogWarning($"Not enough coins! Have {coins}, need {amount}");
        }
    }
    #endregion

    #region Potions
    public void UsePotion()
    {
        UsePotion(1);
    }

    public void UsePotion(int amount)
    {
        if (potions > 0)
        {
            potions -= amount;
            Debug.Log($"Potion used. Remaining: {potions}");
        }
        else
        {
            Debug.LogWarning("No potions available!");
        }
    }

    public bool HasPotions()
    {
        return potions > 0;
    }
    #endregion

    #region Flags System
    public void SetFlag(string flagName, bool value)
    {
        if (generalFlags.ContainsKey(flagName))
            generalFlags[flagName] = value;
        else
            generalFlags.Add(flagName, value);

        Debug.Log($"Flag set: {flagName} = {value}");
        UpdateStoreState();
    }

    public bool GetFlag(string flagName)
    {
        if (generalFlags.ContainsKey(flagName))
            return generalFlags[flagName];
        return false;
    }

    public void SetYojiTalked()
    {
        npcTalkFlags["Yoji"] = true;
        Debug.Log("Yoji talk flag set");
    }

    public bool HasTalkedTo(string npcName)
    {
        return npcTalkFlags.ContainsKey(npcName) && npcTalkFlags[npcName];
    }

    public bool HasSeenDialogue(string dialogueName)
    {
        return dialogueFlags.ContainsKey(dialogueName) && dialogueFlags[dialogueName];
    }

    public void SetDialogueSeen(string dialogueName)
    {
        if (dialogueFlags.ContainsKey(dialogueName))
            dialogueFlags[dialogueName] = true;
        else
            dialogueFlags.Add(dialogueName, true);
        
        Debug.Log($"Dialogue seen: {dialogueName}");
    }

    public bool hasSeenOpeningDialogue
    {
        get { return HasSeenDialogue("OpeningDialogue"); }
        set { if (value) SetDialogueSeen("OpeningDialogue"); }
    }
    #endregion

    #region Progression Events
    public void OnGeorgeDefeated()
    {
        act1Cleared = true;
        SetFlag("Act1Cleared", true);
        Debug.Log("Act 1 completed - George defeated!");
    }

    public void OnFikaDefeated()
    {
        act2Cleared = true;
        SetFlag("Act2Cleared", true);
        Debug.Log("Act 2 completed - Fika defeated!");
        UpdateStoreState();
    }

    public void OnPhilipDefeated()
    {
        act3Cleared = true;
        SetFlag("Act3Cleared", true);
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
        Debug.Log("Player died - respawning in ForestHub");
        SaveProgress();
        UnityEngine.SceneManagement.SceneManager.LoadScene("ForestHub");
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
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("SwordDamage", swordDamage);
        PlayerPrefs.SetFloat("MaxHealth", maxHealth); // SAVE MAX HP
        PlayerPrefs.SetInt("Potions", potions);
        PlayerPrefs.SetInt("HasTeleport", hasTeleport ? 1 : 0);
        PlayerPrefs.SetInt("HasWaveOfLight", hasWaveOfLight ? 1 : 0);
        PlayerPrefs.SetInt("HasSpecialSwordUpgrade", hasSpecialSwordUpgrade ? 1 : 0);
        PlayerPrefs.SetInt("HasDiedToGeorge", hasDiedToGeorge ? 1 : 0);
        PlayerPrefs.SetInt("Act1Cleared", act1Cleared ? 1 : 0);
        PlayerPrefs.SetInt("Act2Cleared", act2Cleared ? 1 : 0);
        PlayerPrefs.SetInt("Act3Cleared", act3Cleared ? 1 : 0);
        PlayerPrefs.SetInt("YojiDead", yojiDead ? 1 : 0);
        PlayerPrefs.SetInt("HasSeenOpeningDialogue", hasSeenOpeningDialogue ? 1 : 0);
        
        PlayerPrefs.Save();
        Debug.Log("Game progress saved!");
    }

    public void LoadProgress()
    {
        coins = PlayerPrefs.GetInt("Coins", 0);
        swordDamage = PlayerPrefs.GetInt("SwordDamage", 8); // LOAD WITH BASE 8
        maxHealth = PlayerPrefs.GetFloat("MaxHealth", 100f); // LOAD MAX HP
        potions = PlayerPrefs.GetInt("Potions", 5);
        hasTeleport = PlayerPrefs.GetInt("HasTeleport", 0) == 1;
        hasWaveOfLight = PlayerPrefs.GetInt("HasWaveOfLight", 0) == 1;
        hasSpecialSwordUpgrade = PlayerPrefs.GetInt("HasSpecialSwordUpgrade", 0) == 1;
        hasDiedToGeorge = PlayerPrefs.GetInt("HasDiedToGeorge", 0) == 1;
        act1Cleared = PlayerPrefs.GetInt("Act1Cleared", 0) == 1;
        act2Cleared = PlayerPrefs.GetInt("Act2Cleared", 0) == 1;
        act3Cleared = PlayerPrefs.GetInt("Act3Cleared", 0) == 1;
        yojiDead = PlayerPrefs.GetInt("YojiDead", 0) == 1;
        hasSeenOpeningDialogue = PlayerPrefs.GetInt("HasSeenOpeningDialogue", 0) == 1;
        
        Debug.Log($"Game progress loaded! MaxHP: {maxHealth}, Damage: {swordDamage}");
        UpdateStoreState();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        
        // RESET TO BASE VALUES
        coins = 0;
        swordDamage = 8;
        maxHealth = 100f; // RESET MAX HP
        potions = 5;
        hasTeleport = false;
        hasWaveOfLight = false;
        hasSpecialSwordUpgrade = false;
        hasDiedToGeorge = false;
        act1Cleared = false;
        act2Cleared = false;
        act3Cleared = false;
        yojiDead = false;
        
        npcTalkFlags.Clear();
        dialogueFlags.Clear();
        generalFlags.Clear();
        
        Debug.Log("Game progress reset to defaults (Base damage: 8, Max HP: 100)");
    }
    #endregion

    #region Utility Methods
    public void AddPotion(int amount = 1)
    {
        potions += amount;
        Debug.Log($"Potions added: {amount}. Total: {potions}");
    }

    public void IncreaseDamage(int amount)
    {
        swordDamage += amount;
        Debug.Log($"Sword damage increased by {amount}. New damage: {swordDamage}");
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        Debug.Log($"Max health increased by {amount}. New max health: {maxHealth}");
        
        // Also update player's current max HP
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(maxHealth);
        }
    }

    public void MultiplyMaxHealth(float multiplier)
    {
        maxHealth *= multiplier;
        Debug.Log($"Max health multiplied by {multiplier}. New max health: {maxHealth}");
        
        // Also update player's current max HP
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(maxHealth);
        }
    }
    #endregion
}