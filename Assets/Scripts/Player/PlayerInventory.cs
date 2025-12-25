using UnityEngine;

/// <summary>
/// Player Inventory - Manages coins and potions ONLY
/// This is the ONLY place where coins and potions are stored!
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
    }

    #region Coins

    /// <summary>
    /// Add coins to inventory (called by EnemyBase when killed, or by store when sold)
    /// </summary>
    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"💰 Coins added: {amount}. Total: {coins}");
    }

    /// <summary>
    /// Spend coins (called by StoreController when purchasing)
    /// Returns true if successful, false if not enough coins
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (coins < amount)
        {
            Debug.LogWarning($"⚠️ Not enough coins! Have {coins}, need {amount}");
            return false;
        }

        coins -= amount;
        Debug.Log($"💰 Coins spent: {amount}. Remaining: {coins}");
        return true;
    }

    #endregion

    #region Potions

    /// <summary>
    /// Add potions to inventory (called by StoreController when purchasing)
    /// </summary>
    public void AddPotion(int amount = 1)
    {
        potions += amount;
        Debug.Log($"🧪 Potions added: {amount}. Total: {potions}");
    }

    /// <summary>
    /// Use a potion to heal
    /// Called by PlayerController when pressing H key
    /// Returns true if potion was used, false otherwise
    /// </summary>
    public bool UsePotion(float healAmount)
    {
        // Check if we have potions
        if (potions <= 0)
        {
            Debug.Log("⚠️ No potions left!");
            return false;
        }

        // Check if player health is available
        if (playerHealth == null)
        {
            Debug.LogWarning("⚠️ PlayerHealth not found!");
            return false;
        }

        // ✅ NEW: Check if player is at full health
        if (playerHealth.IsAtFullHealth)
        {
            Debug.Log("⚠️ HP already full - can't use potion!");
            return false;
        }

        // Use the potion
        potions--;
        playerHealth.Heal(healAmount);
        Debug.Log($"🧪 Used potion to heal. Remaining potions: {potions}");
        return true;
    }

    /// <summary>
    /// Check if player has potions
    /// </summary>
    public bool HasPotions()
    {
        return potions > 0;
    }

    #endregion
}