using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreController : MonoBehaviour
{
    [Header("Store UI")]
    public GameObject storePanel;
    public TextMeshProUGUI coinsText;

    [Header("Store Items")]
    public ShopItem[] shopItems;

    [Header("Item UI Elements")]
    public Button[] purchaseButtons;
    public TextMeshProUGUI[] itemNameTexts;
    public TextMeshProUGUI[] itemCostTexts;
    public Image[] itemIcons;

    private bool storeUnlocked = false;
    private bool isStoreOpen = false;

    void Start()
    {
        if (storePanel != null)
            storePanel.SetActive(false);

        // Initialize store items
        SetupStoreUI();
    }

    void Update()
    {
        if (isStoreOpen)
        {
            UpdateCoinsDisplay();

            // Close with ESC
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseStore();
            }
        }
    }

    void SetupStoreUI()
    {
        // Setup each item button
        for (int i = 0; i < purchaseButtons.Length && i < shopItems.Length; i++)
        {
            int index = i; // Capture for lambda
            
            if (purchaseButtons[i] != null)
            {
                purchaseButtons[i].onClick.AddListener(() => PurchaseItem(index));
            }

            // Display item info
            if (itemNameTexts[i] != null && shopItems[i] != null)
                itemNameTexts[i].text = shopItems[i].itemName;

            if (itemCostTexts[i] != null && shopItems[i] != null)
                itemCostTexts[i].text = $"{shopItems[i].cost} Coins";

            if (itemIcons[i] != null && shopItems[i] != null && shopItems[i].itemIcon != null)
                itemIcons[i].sprite = shopItems[i].itemIcon;
        }
    }

    public void UnlockStore()
    {
        storeUnlocked = true;
        Debug.Log("Store has been unlocked!");
    }

    public void OpenStore()
    {
        if (!storeUnlocked)
        {
            Debug.Log("Store is locked! Complete first dialogue with Yoji.");
            return;
        }

        isStoreOpen = true;
        if (storePanel != null)
        {
            storePanel.SetActive(true);
        }

        Time.timeScale = 0f; // Pause game
        UpdateCoinsDisplay();
        Debug.Log("Store opened!");
    }

    public void CloseStore()
    {
        isStoreOpen = false;
        if (storePanel != null)
        {
            storePanel.SetActive(false);
        }

        Time.timeScale = 1f; // Resume game
        Debug.Log("Store closed!");
    }

    void UpdateCoinsDisplay()
    {
        if (coinsText != null && GameManager.Instance != null)
        {
            coinsText.text = $"Coins: {GameManager.Instance.coins}";
        }
    }

public void PurchaseItem(int itemIndex)
{
    if (itemIndex < 0 || itemIndex >= shopItems.Length) return;
    if (GameManager.Instance == null) return;
    if (shopItems[itemIndex] == null) return;

    ShopItem item = shopItems[itemIndex];

    // Check if player has enough coins
    if (GameManager.Instance.coins < item.cost)
    {
        Debug.Log($"Not enough coins! Need {item.cost}, have {GameManager.Instance.coins}");
        return;
    }

    // Deduct coins
    GameManager.Instance.SpendCoins(item.cost);

    // Find the player's health component
    PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();

    // Apply item effect
    switch (item.itemType)
    {
        case ShopItem.ShopItemType.HPUpgrade:
            if (playerHealth != null)
            {
                playerHealth.SetMaxHealth(playerHealth.MaxHealth + 15);
                playerHealth.Heal(15);
            }
            Debug.Log("Purchased HP Upgrade! +15 Max HP");
            break;

        case ShopItem.ShopItemType.DamageUpgrade:
            GameManager.Instance.swordDamage += 2;
            Debug.Log("Purchased Damage Upgrade! +2 Sword Damage");
            break;

        case ShopItem.ShopItemType.MagicArmor:
            if (playerHealth != null)
            {
                float currentMaxHP = playerHealth.MaxHealth;
                playerHealth.SetMaxHealth(currentMaxHP * 2);
                playerHealth.Heal(currentMaxHP); // Heal by the amount we just added
            }
            Debug.Log("Purchased Magic Armor! HP Doubled!");
            break;

        case ShopItem.ShopItemType.FlashHelmet:
            GameManager.Instance.hasTeleport = true;
            Debug.Log("Purchased Flash Helmet! Teleport ability unlocked!");
            break;

        case ShopItem.ShopItemType.SwordOfLight:
            GameManager.Instance.hasWaveOfLight = true;
            GameManager.Instance.swordDamage *= 2;
            Debug.Log("Purchased Sword of Light! Damage doubled + Wave of Light unlocked!");
            break;

        case ShopItem.ShopItemType.Potion:
            GameManager.Instance.potions++;
            Debug.Log("Purchased Potion!");
            break;
    }

    UpdateCoinsDisplay();
}
}