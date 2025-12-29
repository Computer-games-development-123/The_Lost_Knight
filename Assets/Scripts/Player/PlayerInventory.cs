using UnityEngine;

/// <summary>
/// Player Inventory - Manages coins and potions with save/load
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public int coins = 0;
    public int potions = 5;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();

        // Load inventory when player spawns
        LoadInventory();
    }

    #region Coins

    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"💰 Coins added: {amount}. Total: {coins}");
        SaveInventory(); // Auto-save on change
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
        {
            Debug.LogWarning($"⚠️ Not enough coins! Have {coins}, need {amount}");
            return false;
        }

        coins -= amount;
        Debug.Log($"💰 Coins spent: {amount}. Remaining: {coins}");
        SaveInventory(); // Auto-save on change
        return true;
    }

    #endregion

    #region Potions

    public void AddPotion(int amount = 1)
    {
        potions += amount;
        Debug.Log($"🧪 Potions added: {amount}. Total: {potions}");
        SaveInventory(); // Auto-save on change
    }

    public bool UsePotion(float healAmount)
    {
        if (potions <= 0)
        {
            Debug.Log("⚠️ No potions left!");
            return false;
        }

        if (playerHealth == null)
        {
            Debug.LogWarning("⚠️ PlayerHealth not found!");
            return false;
        }

        if (playerHealth.IsAtFullHealth)
        {
            Debug.Log("⚠️ HP already full - can't use potion!");
            return false;
        }

        potions--;
        playerHealth.Heal(healAmount);
        Debug.Log($"🧪 Used potion to heal. Remaining potions: {potions}");
        SaveInventory(); // Auto-save on change
        return true;
    }

    public bool HasPotions()
    {
        return potions > 0;
    }

    #endregion

    #region Save/Load

    private void SaveInventory()
    {
        PlayerPrefs.SetInt("PlayerCoins", coins);
        PlayerPrefs.SetInt("PlayerPotions", potions);
        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        coins = PlayerPrefs.GetInt("PlayerCoins", 0); // Default: 0 coins
        potions = PlayerPrefs.GetInt("PlayerPotions", 5); // Default: 5 potions

        Debug.Log($"📂 Inventory loaded: {coins} coins, {potions} potions");
    }

    /// <summary>
    /// Call this to manually save inventory
    /// </summary>
    public void Save()
    {
        SaveInventory();
    }

    #endregion
}