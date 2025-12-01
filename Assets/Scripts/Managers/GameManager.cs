using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public float maxHP = 50f;
    public float currentHP = 50f;
    public int swordDamage = 6;
    public int potions = 5;
    public int coins = 0;

    [Header("Abilities")]
    public bool hasTeleport = false;
    public bool hasWaveOfLight = false;

    [Header("Progression Flags")]
    public bool hasSeenOpeningDialogue = false;
    public bool hasSpecialSwordUpgrade = false;
    public bool hasDiedToGeorge = false;
    public bool act1Cleared = false;
    public bool act2Cleared = false;
    public bool act3Cleared = false;
    public bool yojiDead = false;
    public bool hasSeenFikaCutscene = false;
    public bool hasSeenDitorIntro = false;

    [Header("Store State")]
    public StoreMode currentStoreMode = StoreMode.Normal;

    [Header("Ending")]
    public EndingType endingChosen = EndingType.None;

    public enum StoreMode { Normal, Enhanced, Free, Inactive }
    public enum EndingType { None, Escape, Save }

    void Awake()
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

    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"Coins added: {amount}. Total: {coins}");
    }

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            Debug.Log($"Coins spent: {amount}. Remaining: {coins}");
            return true;
        }
        Debug.Log("Not enough coins!");
        return false;
    }

    public void UsePotion()
    {
        if (potions > 0)
        {
            float healAmount = 20f;
            currentHP = Mathf.Min(currentHP + healAmount, maxHP);
            potions--;
            Debug.Log($"Potion used. HP: {currentHP}/{maxHP}. Potions left: {potions}");
        }
        else
        {
            Debug.Log("No potions left!");
        }
    }

    public bool HasTalkedTo(string npcName)
    {
        return PlayerPrefs.GetInt($"TalkedTo_{npcName}", 0) == 1;
    }

    public void SetYojiTalked()
    {
        PlayerPrefs.SetInt("TalkedTo_Yoji", 1);
        PlayerPrefs.Save();
        Debug.Log("Yoji dialogue flag set.");
    }

    public void OnPlayerDied()
    {
        Debug.Log("Player died!");
        // You can add death penalties here
        coins = Mathf.Max(0, coins - 10); // Lose 10 coins on death
        currentHP = maxHP; // Reset HP
        
        // Reload current scene or respawn point
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetFloat("MaxHP", maxHP);
        PlayerPrefs.SetFloat("CurrentHP", currentHP);
        PlayerPrefs.SetInt("SwordDamage", swordDamage);
        PlayerPrefs.SetInt("Potions", potions);
        PlayerPrefs.SetInt("HasTeleport", hasTeleport ? 1 : 0);
        PlayerPrefs.SetInt("HasWaveOfLight", hasWaveOfLight ? 1 : 0);
        PlayerPrefs.SetInt("Act1Cleared", act1Cleared ? 1 : 0);
        PlayerPrefs.SetInt("Act2Cleared", act2Cleared ? 1 : 0);
        PlayerPrefs.SetInt("Act3Cleared", act3Cleared ? 1 : 0);
        PlayerPrefs.SetInt("YojiDead", yojiDead ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Progress saved!");
    }

    public void LoadProgress()
    {
        coins = PlayerPrefs.GetInt("Coins", 0);
        maxHP = PlayerPrefs.GetFloat("MaxHP", 50f);
        currentHP = PlayerPrefs.GetFloat("CurrentHP", 50f);
        swordDamage = PlayerPrefs.GetInt("SwordDamage", 6);
        potions = PlayerPrefs.GetInt("Potions", 5);
        hasTeleport = PlayerPrefs.GetInt("HasTeleport", 0) == 1;
        hasWaveOfLight = PlayerPrefs.GetInt("HasWaveOfLight", 0) == 1;
        act1Cleared = PlayerPrefs.GetInt("Act1Cleared", 0) == 1;
        act2Cleared = PlayerPrefs.GetInt("Act2Cleared", 0) == 1;
        act3Cleared = PlayerPrefs.GetInt("Act3Cleared", 0) == 1;
        yojiDead = PlayerPrefs.GetInt("YojiDead", 0) == 1;
        Debug.Log("Progress loaded!");
    }
}