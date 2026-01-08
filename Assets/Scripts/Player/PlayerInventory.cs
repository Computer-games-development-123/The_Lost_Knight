using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave.Models;

/// <summary>
/// Player Inventory - Manages coins and potions with save/load
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public int coins = 0;
    public int potions = 5;

    private PlayerHealth playerHealth;
    private static bool cloudReady = false;
    private async void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();

        await EnsureCloudReady();
        await LoadInventoryCloud();
    }


    #region Coins

    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"Coins added: {amount}. Total: {coins}");
        SaveInventory(); // Auto-save on change
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
        {
            Debug.LogWarning($"Not enough coins! Have {coins}, need {amount}");
            return false;
        }

        coins -= amount;
        Debug.Log($"Coins spent: {amount}. Remaining: {coins}");
        SaveInventory(); // Auto-save on change
        return true;
    }

    #endregion

    #region Potions

    public void AddPotion(int amount = 1)
    {
        potions += amount;
        Debug.Log($"Potions added: {amount}. Total: {potions}");
        SaveInventory(); // Auto-save on change
    }

    public bool UsePotion(float healAmount)
    {
        if (potions <= 0)
        {
            Debug.Log("No potions left!");
            return false;
        }

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth not found!");
            return false;
        }

        if (playerHealth.IsAtFullHealth)
        {
            Debug.Log("HP already full - can't use potion!");
            return false;
        }

        potions--;
        playerHealth.Heal(healAmount);
        // Sound is now handled by PlayerHealth.Heal()

        Debug.Log($"Used potion to heal. Remaining potions: {potions}");
        SaveInventory(); // Auto-save on change
        return true;
    }

    public bool HasPotions()
    {
        return potions > 0;
    }

    #endregion

    #region Save/Load
    private async Task EnsureCloudReady()
    {
        if (cloudReady) return;

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        cloudReady = true;
    }
    private async void SaveInventory()
    {
        try
        {
            await EnsureCloudReady();
            await DatabaseManager.SaveData(
                ("PlayerCoins", coins),
                ("PlayerPotions", potions)
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError("Cloud SaveInventory failed: " + e);

        }
    }
    private async Task LoadInventoryCloud()
    {
        try
        {
            var data = await DatabaseManager.LoadData("PlayerCoins", "PlayerPotions");

            if (data.TryGetValue("PlayerCoins", out Item coinsItem))
                coins = coinsItem.Value.GetAs<int>();
            else
                coins = 0;

            if (data.TryGetValue("PlayerPotions", out Item potionsItem))
                potions = potionsItem.Value.GetAs<int>();
            else
                potions = 5;

            Debug.Log($"Inventory loaded (Cloud): {coins} coins, {potions} potions");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Cloud LoadInventory failed: " + e);

            Debug.Log($"Inventory loaded (Local fallback): {coins} coins, {potions} potions");
        }
    }
    #endregion
}