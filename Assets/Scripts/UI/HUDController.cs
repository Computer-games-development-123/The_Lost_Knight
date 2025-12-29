using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// HUD Controller - Displays player stats in UI
/// NO PlayerState - reads directly from components
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI potionsText;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Health Bar Asset")]
    [SerializeField] private Image healthFill; // Image type = Filled

    private PlayerInventory playerInventory;
    private PlayerHealth playerHealth;

    private void Start()
    {
        TryBindPlayer();
        Refresh();
    }

    private void Update()
    {
        if (playerInventory == null || playerHealth == null)
        {
            TryBindPlayer();
            return;
        }

        Refresh();
    }

    private void TryBindPlayer()
    {
        // Find Player GameObject
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInventory = player.GetComponent<PlayerInventory>();
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("HUDController: PlayerInventory component not found on Player!");
        }

        if (playerHealth == null)
        {
            Debug.LogWarning("HUDController: PlayerHealth component not found on Player!");
        }
    }

    private void Refresh()
    {
        if (playerInventory == null || playerHealth == null) return;

        // Coins (from PlayerInventory)
        if (coinsText != null)
            coinsText.text = $"{playerInventory.coins}";

        // Potions (from PlayerInventory)
        if (potionsText != null)
            potionsText.text = $"{playerInventory.potions}";

        // Health bar (from PlayerHealth)
        if (healthFill != null)
        {
            float maxHp = Mathf.Max(1f, playerHealth.MaxHealth);
            healthFill.fillAmount = playerHealth.CurrentHealth / maxHp;
        }

        // HP text (from PlayerHealth)
        if (hpText != null)
            hpText.text = $"{playerHealth.CurrentHealth:F0}/{playerHealth.MaxHealth:F0}";
    }
}