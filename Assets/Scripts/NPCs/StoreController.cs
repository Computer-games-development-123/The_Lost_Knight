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
    public Color coinsTextColor = Color.yellow;
    public Color selectedRowColor = new Color(1f, 1f, 0.5f, 0.3f); // Highlight color
    public Color normalRowColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private List<StoreItemRow> itemRows = new List<StoreItemRow>();
    private bool isStoreOpen = false;
    private int selectedIndex = 0;

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
        public Image backgroundImage;
        public Image itemIcon;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI stockText;
        public Button purchaseButton;
        public TextMeshProUGUI buttonText;
        public StoreItemData itemData;
        public int rowIndex;
    }

    void Start()
    {
        if (storePanel != null)
            storePanel.SetActive(false);

        if (storeItems != null)
        {
            foreach (var item in storeItems)
            {
                if (item != null)
                    item.Initialize();
            }
        }

        BuildStoreUI();
    }

    void Update()
    {
        if (isStoreOpen)
        {
            HandleKeyboardInput();
            UpdateCoinsDisplay();
            RefreshAllItems();
        }
    }

    void HandleKeyboardInput()
    {
        // Get visible (non-out-of-stock) rows
        List<StoreItemRow> visibleRows = new List<StoreItemRow>();
        foreach (var row in itemRows)
        {
            if (row.rowObject.activeSelf)
                visibleRows.Add(row);
        }

        if (visibleRows.Count == 0) return;

        // Navigate with arrow keys
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex++;
            if (selectedIndex >= visibleRows.Count)
                selectedIndex = 0;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex--;
            if (selectedIndex < 0)
                selectedIndex = visibleRows.Count - 1;
            UpdateSelection();
        }

        // Purchase with Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (selectedIndex >= 0 && selectedIndex < visibleRows.Count)
            {
                StoreItemRow selectedRow = visibleRows[selectedIndex];
                if (selectedRow.purchaseButton != null && selectedRow.purchaseButton.interactable)
                {
                    PurchaseItem(selectedRow.itemData);
                }
                else
                {
                    Debug.Log("Cannot purchase this item!");
                }
            }
        }

        // Close with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseStore();
        }
    }

    void UpdateSelection()
    {
        List<StoreItemRow> visibleRows = new List<StoreItemRow>();
        foreach (var row in itemRows)
        {
            if (row.rowObject.activeSelf)
                visibleRows.Add(row);
        }

        for (int i = 0; i < visibleRows.Count; i++)
        {
            if (visibleRows[i].backgroundImage != null)
            {
                visibleRows[i].backgroundImage.color = (i == selectedIndex) ? selectedRowColor : normalRowColor;
            }
        }
    }

    void BuildStoreUI()
    {
        if (itemListContainer == null || storeItemRowPrefab == null || storeItems == null)
            return;

        // Clear existing rows
        foreach (Transform child in itemListContainer)
        {
            Destroy(child.gameObject);
        }
        itemRows.Clear();

        // Create a row for each item
        int rowIndex = 0;
        foreach (var itemData in storeItems)
        {
            if (itemData == null) continue;

            GameObject rowObj = Instantiate(storeItemRowPrefab, itemListContainer);
            StoreItemRow row = new StoreItemRow
            {
                rowObject = rowObj,
                itemData = itemData,
                rowIndex = rowIndex
            };

            // Find background image (for highlighting)
            row.backgroundImage = rowObj.GetComponent<Image>();

            // Find components
            row.itemIcon = FindChildByName<Image>(rowObj.transform, "ItemIcon");
            
            TextMeshProUGUI[] allTexts = rowObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allTexts)
            {
                string textName = text.gameObject.name;
                if (textName == "ItemName") row.itemNameText = text;
                else if (textName == "Description") row.descriptionText = text;
                else if (textName == "Price") row.priceText = text;
                else if (textName == "Stock") row.stockText = text;
                else if (textName.Contains("ButtonText") || (textName.Contains("Text") && text.GetComponentInParent<Button>()))
                    row.buttonText = text;
            }

            row.purchaseButton = rowObj.GetComponentInChildren<Button>(true);
            if (row.buttonText == null && row.purchaseButton != null)
            {
                row.buttonText = row.purchaseButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            itemRows.Add(row);
            rowIndex++;
            
            // Force immediate update
            UpdateRowDisplay(row);
        }

        selectedIndex = 0;
        UpdateSelection();
    }

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
        if (StoreStateManager.Instance == null || !StoreStateManager.Instance.IsStoreUnlocked())
            return;

        isStoreOpen = true;
        if (storePanel != null)
        {
            storePanel.SetActive(true);
        }

        Time.timeScale = 0f;
        selectedIndex = 0;
        UpdateStoreTitle();
        UpdateCoinsDisplay();
        RefreshAllItems();
        UpdateSelection();
    }

    public void CloseStore()
    {
        isStoreOpen = false;
        if (storePanel != null)
        {
            storePanel.SetActive(false);
        }

        Time.timeScale = 1f;
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
                coinsText.color = coinsTextColor;
            }
        }
    }

    void RefreshAllItems()
    {
        if (StoreStateManager.Instance == null) return;

        foreach (var row in itemRows)
        {
            if (row.itemData == null) continue;
            UpdateRowDisplay(row);
        }
    }

    void UpdateRowDisplay(StoreItemRow row)
    {
        if (row == null || row.itemData == null) return;

        bool isFreeStore = StoreStateManager.Instance != null && StoreStateManager.Instance.IsStoreFree();
        bool swordRevealed = StoreStateManager.Instance != null && StoreStateManager.Instance.IsSwordOfLightRevealed();

        bool isSwordOfLight = row.itemData.itemType == ShopItem.ShopItemType.SwordOfLight;
        bool isLocked = isSwordOfLight && !swordRevealed;
        bool outOfStock = row.itemData.currentStock == 0 && row.itemData.maxStock != -1;

        if (outOfStock)
        {
            row.rowObject.SetActive(false);
            return;
        }
        else
        {
            row.rowObject.SetActive(true);
        }

        // Update icon
        if (row.itemIcon != null)
        {
            if (isLocked && lockedItemIcon != null)
                row.itemIcon.sprite = lockedItemIcon;
            else if (row.itemData.itemIcon != null)
                row.itemIcon.sprite = row.itemData.itemIcon;
            
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
                row.priceText.text = "??? coins";
            }
            else if (isFreeStore)
            {
                row.priceText.text = "FREE";
                row.priceText.color = freeTextColor;
            }
            else
            {
                row.priceText.text = $"{row.itemData.costPerUnit} coins";
                row.priceText.color = Color.yellow;
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
                row.stockText.text = $"Stock: {row.itemData.currentStock}";
            }
        }

        // Update button - TRULY FIXED
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
                // Proper affordability check
                int itemCost = isFreeStore ? 0 : row.itemData.costPerUnit;
                int playerCoins = (GameManager.Instance != null) ? GameManager.Instance.coins : 0;
                
                bool canAfford = (playerCoins >= itemCost) || isFreeStore;

                // Button is interactable if player can afford
                row.purchaseButton.interactable = canAfford;

                if (row.buttonText != null)
                {
                    row.buttonText.text = isFreeStore ? "TAKE" : "BUY";
                }
            }
        }
    }

    void PurchaseItem(StoreItemData itemData)
    {
        if (GameManager.Instance == null || StoreStateManager.Instance == null || itemData == null)
            return;

        bool isSwordOfLight = itemData.itemType == ShopItem.ShopItemType.SwordOfLight;
        bool swordRevealed = StoreStateManager.Instance.IsSwordOfLightRevealed();

        if (isSwordOfLight && !swordRevealed)
        {
            Debug.Log("This item is locked!");
            return;
        }

        if (itemData.maxStock != -1 && itemData.currentStock <= 0)
        {
            Debug.Log("Out of stock!");
            return;
        }

        bool isFree = StoreStateManager.Instance.IsStoreFree();
        int cost = isFree ? 0 : itemData.costPerUnit;

        if (!isFree && GameManager.Instance.coins < cost)
        {
            Debug.Log($"Not enough coins! Need {cost}, have {GameManager.Instance.coins}");
            return;
        }

        if (!isFree)
        {
            GameManager.Instance.SpendCoins(cost);
        }

        ApplyItemEffect(itemData.itemType);

        if (itemData.maxStock != -1)
        {
            itemData.currentStock--;
        }

        UpdateCoinsDisplay();
        RefreshAllItems();

        Debug.Log($"✅ {(isFree ? "Taken" : "Purchased")}: {itemData.itemName}!");
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
                break;

            case ShopItem.ShopItemType.DamageUpgrade:
                GameManager.Instance.swordDamage += 2;
                break;

            case ShopItem.ShopItemType.MagicArmor:
                if (playerHealth != null)
                {
                    float currentMaxHP = playerHealth.MaxHealth;
                    playerHealth.SetMaxHealth(currentMaxHP * 2);
                    playerHealth.Heal(currentMaxHP);
                }
                break;

            case ShopItem.ShopItemType.FlashHelmet:
                GameManager.Instance.hasTeleport = true;
                break;

            case ShopItem.ShopItemType.SwordOfLight:
                GameManager.Instance.hasWaveOfLight = true;
                GameManager.Instance.swordDamage *= 2;
                break;

            case ShopItem.ShopItemType.Potion:
                GameManager.Instance.AddPotion(1);
                break;
        }
    }
}