using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ListStoreController : MonoBehaviour
{
    [Header("Store UI")]
    public GameObject storePanel;
    public TextMeshProUGUI storeTitleText;
    public TextMeshProUGUI coinsText;
    public Transform itemListContainer;
    public GameObject storeItemRowPrefab;

    [Header("Store Items Data")]
    public StoreItemData[] storeItems;

    [Header("Visual Settings")]
    public Sprite lockedItemIcon;
    public Color normalTextColor = Color.white;
    public Color freeTextColor = new Color(0.5f, 1f, 0.5f);

    private List<StoreItemRow> itemRows = new List<StoreItemRow>();
    private bool isStoreOpen = false;

    [System.Serializable]
    public class StoreItemData
    {
        public string itemName;
        [TextArea(1, 3)]
        public string description;
        public int costPerUnit;
        public int maxStock; // -1 for unlimited
        public Sprite itemIcon;
        public ShopItem.ShopItemType itemType;

        [HideInInspector] public int currentStock;

        public void Initialize()
        {
            currentStock = maxStock;
        }
    }

    private class StoreItemRow
    {
        public GameObject rowObject;
        public Image itemIcon;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI stockText;
        public Button purchaseButton;
        public TextMeshProUGUI buttonText;
        public StoreItemData itemData;
    }

    void Start()
    {
        if (storePanel != null)
            storePanel.SetActive(false);

        foreach (var item in storeItems)
        {
            item.Initialize();
        }

        BuildStoreUI();
    }

    void Update()
    {
        if (isStoreOpen)
        {
            UpdateCoinsDisplay();
            RefreshAllItems();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseStore();
            }
        }
    }

    void BuildStoreUI()
    {
        foreach (Transform child in itemListContainer)
        {
            Destroy(child.gameObject);
        }
        itemRows.Clear();

        foreach (var itemData in storeItems)
        {
            GameObject rowObj = Instantiate(storeItemRowPrefab, itemListContainer);
            StoreItemRow row = new StoreItemRow
            {
                rowObject = rowObj,
                itemData = itemData
            };

            // IMPROVED: Search recursively for components
            row.itemIcon = FindChildByName<Image>(rowObj.transform, "ItemIcon");
            
            // Find all TextMeshPro components and match by name
            TextMeshProUGUI[] allTexts = rowObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allTexts)
            {
                string textName = text.gameObject.name;
                if (textName == "ItemName") row.itemNameText = text;
                else if (textName == "Description") row.descriptionText = text;
                else if (textName == "Price") row.priceText = text;
                else if (textName == "Stock") row.stockText = text;
                else if (textName.Contains("ButtonText") || textName.Contains("Text") && text.GetComponentInParent<Button>())
                    row.buttonText = text;
            }

            row.purchaseButton = rowObj.GetComponentInChildren<Button>(true);
            if (row.buttonText == null && row.purchaseButton != null)
            {
                row.buttonText = row.purchaseButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            // Log what we found for debugging
            Debug.Log($"Row created for {itemData.itemName}:");
            Debug.Log($"  Icon: {(row.itemIcon != null ? "✓" : "✗")}");
            Debug.Log($"  Name: {(row.itemNameText != null ? "✓" : "✗")}");
            Debug.Log($"  Desc: {(row.descriptionText != null ? "✓" : "✗")}");
            Debug.Log($"  Price: {(row.priceText != null ? "✓" : "✗")}");
            Debug.Log($"  Stock: {(row.stockText != null ? "✓" : "✗")}");
            Debug.Log($"  Button: {(row.purchaseButton != null ? "✓" : "✗")}");

            if (row.purchaseButton != null)
            {
                row.purchaseButton.onClick.AddListener(() => PurchaseItem(row));
            }

            itemRows.Add(row);
        }

        RefreshAllItems();
    }

    // Helper to find child by name recursively
    private T FindChildByName<T>(Transform parent, string name) where T : Component
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                T component = child.GetComponent<T>();
                if (component != null) return component;
            }

            T result = FindChildByName<T>(child, name);
            if (result != null) return result;
        }
        return null;
    }

    public void OpenStore()
    {
        if (StoreStateManager.Instance == null)
        {
            Debug.LogError("StoreStateManager not found!");
            return;
        }

        if (!StoreStateManager.Instance.IsStoreUnlocked())
        {
            Debug.Log("Store is locked!");
            return;
        }

        isStoreOpen = true;
        if (storePanel != null)
        {
            storePanel.SetActive(true);
        }

        Time.timeScale = 0f;
        UpdateStoreTitle();
        UpdateCoinsDisplay();
        RefreshAllItems();

        Debug.Log("Store opened!");
    }

    public void CloseStore()
    {
        isStoreOpen = false;
        if (storePanel != null)
        {
            storePanel.SetActive(false);
        }

        Time.timeScale = 1f;
        Debug.Log("Store closed!");
    }

    void UpdateStoreTitle()
    {
        if (storeTitleText != null && StoreStateManager.Instance != null)
        {
            storeTitleText.text = StoreStateManager.Instance.GetStateDisplayName();
        }
    }

    void UpdateCoinsDisplay()
    {
        if (coinsText != null && GameManager.Instance != null)
        {
            if (StoreStateManager.Instance != null && StoreStateManager.Instance.IsStoreFree())
            {
                coinsText.text = "Everything is Free - Honor Yoji's Memory";
                coinsText.color = freeTextColor;
            }
            else
            {
                coinsText.text = $"Coins: {GameManager.Instance.coins}";
                coinsText.color = normalTextColor;
            }
        }
    }

    void RefreshAllItems()
    {
        if (StoreStateManager.Instance == null) return;

        bool isFreeStore = StoreStateManager.Instance.IsStoreFree();
        bool swordRevealed = StoreStateManager.Instance.IsSwordOfLightRevealed();

        foreach (var row in itemRows)
        {
            if (row.itemData == null) continue;

            bool isSwordOfLight = row.itemData.itemType == ShopItem.ShopItemType.SwordOfLight;
            bool isLocked = isSwordOfLight && !swordRevealed;
            bool outOfStock = row.itemData.currentStock == 0 && row.itemData.maxStock != -1;

            if (outOfStock)
            {
                row.rowObject.SetActive(false);
                continue;
            }
            else
            {
                row.rowObject.SetActive(true);
            }

            // Update icon
            if (row.itemIcon != null)
            {
                if (isLocked && lockedItemIcon != null)
                {
                    row.itemIcon.sprite = lockedItemIcon;
                }
                else if (row.itemData.itemIcon != null)
                {
                    row.itemIcon.sprite = row.itemData.itemIcon;
                }
                row.itemIcon.color = Color.white;
            }

            // Update item name
            if (row.itemNameText != null)
            {
                row.itemNameText.text = isLocked ? "???" : row.itemData.itemName;
                row.itemNameText.color = isFreeStore ? freeTextColor : normalTextColor;
            }

            // Update description
            if (row.descriptionText != null)
            {
                row.descriptionText.text = isLocked ? "A mysterious item..." : row.itemData.description;
            }

            // Update price
            if (row.priceText != null)
            {
                if (isLocked)
                {
                    row.priceText.text = "??? Coins";
                }
                else if (isFreeStore)
                {
                    row.priceText.text = "FREE";
                    row.priceText.color = freeTextColor;
                }
                else
                {
                    row.priceText.text = $"{row.itemData.costPerUnit} coins";
                    row.priceText.color = normalTextColor;
                }
            }

            // Update stock
            if (row.stockText != null)
            {
                if (isLocked)
                {
                    row.stockText.text = "Stock: ?";
                }
                else if (row.itemData.maxStock == -1)
                {
                    row.stockText.text = "Stock: ∞";
                }
                else
                {
                    row.stockText.text = $"New Stock: {row.itemData.currentStock}";
                }
            }

            // Update button
            if (row.purchaseButton != null)
            {
                if (isLocked)
                {
                    row.purchaseButton.interactable = false;
                    if (row.buttonText != null)
                        row.buttonText.text = "LOCKED";
                }
                else
                {
                    bool canAfford = isFreeStore || (GameManager.Instance != null &&
                                     GameManager.Instance.coins >= row.itemData.costPerUnit);

                    row.purchaseButton.interactable = canAfford;

                    if (row.buttonText != null)
                    {
                        row.buttonText.text = isFreeStore ? "TAKE" : "BUY";
                    }
                }
            }
        }
    }

    void PurchaseItem(StoreItemRow row)
    {
        if (GameManager.Instance == null || StoreStateManager.Instance == null) return;
        if (row.itemData == null) return;

        bool isSwordOfLight = row.itemData.itemType == ShopItem.ShopItemType.SwordOfLight;
        bool swordRevealed = StoreStateManager.Instance.IsSwordOfLightRevealed();

        if (isSwordOfLight && !swordRevealed)
        {
            Debug.Log("This item is locked!");
            return;
        }

        if (row.itemData.maxStock != -1 && row.itemData.currentStock <= 0)
        {
            Debug.Log("Out of stock!");
            return;
        }

        bool isFree = StoreStateManager.Instance.IsStoreFree();
        int cost = isFree ? 0 : row.itemData.costPerUnit;

        if (!isFree && GameManager.Instance.coins < cost)
        {
            Debug.Log($"Not enough coins! Need {cost}, have {GameManager.Instance.coins}");
            return;
        }

        if (!isFree)
        {
            GameManager.Instance.SpendCoins(cost);
        }

        ApplyItemEffect(row.itemData.itemType);

        if (row.itemData.maxStock != -1)
        {
            row.itemData.currentStock--;
            Debug.Log($"{row.itemData.itemName} stock remaining: {row.itemData.currentStock}");
        }

        UpdateCoinsDisplay();
        RefreshAllItems();

        string action = isFree ? "Taken" : "Purchased";
        Debug.Log($"{action}: {row.itemData.itemName}!");
    }

    void ApplyItemEffect(ShopItem.ShopItemType itemType)
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();

        switch (itemType)
        {
            case ShopItem.ShopItemType.HPUpgrade:
                if (playerHealth != null)
                {
                    playerHealth.SetMaxHealth(playerHealth.MaxHealth + 15);
                    playerHealth.Heal(15);
                }
                Debug.Log("HP Upgrade applied! +15 Max HP");
                break;

            case ShopItem.ShopItemType.DamageUpgrade:
                GameManager.Instance.swordDamage += 2;
                Debug.Log("Damage Upgrade applied! +2 Sword Damage");
                break;

            case ShopItem.ShopItemType.MagicArmor:
                if (playerHealth != null)
                {
                    float currentMaxHP = playerHealth.MaxHealth;
                    playerHealth.SetMaxHealth(currentMaxHP * 2);
                    playerHealth.Heal(currentMaxHP);
                }
                Debug.Log("Magic Armor applied! HP Doubled!");
                break;

            case ShopItem.ShopItemType.FlashHelmet:
                GameManager.Instance.hasTeleport = true;
                Debug.Log("Flash Helmet applied! Teleport unlocked!");
                break;

            case ShopItem.ShopItemType.SwordOfLight:
                GameManager.Instance.hasWaveOfLight = true;
                GameManager.Instance.swordDamage *= 2;
                Debug.Log("Sword of Light applied! Damage doubled + Wave of Light unlocked!");
                break;

            case ShopItem.ShopItemType.Potion:
                GameManager.Instance.AddPotion(1);
                Debug.Log("Potion added to inventory!");
                break;
        }
    }
}