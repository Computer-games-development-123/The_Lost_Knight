using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave.Models;

/// <summary>
/// Store Controller - Manages shop UI and purchases
/// FIXED: Now saves and loads stock data to CloudSave
/// </summary>
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
    public Color selectedRowColor = new Color(0.2f, 0.6f, 0.2f, 0.5f);
    public Color normalRowColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private List<StoreItemRow> itemRows = new List<StoreItemRow>();
    private bool isStoreOpen = false;
    private int selectedIndex = 0;
    private static bool cloudReady = false;

    // Public property so PauseMenuManager can check if store is open
    public bool IsStoreOpen => isStoreOpen;

    [System.Serializable]
    public class StoreItemData
    {
        public string itemName;
        [TextArea(1, 3)]
        public string description;
        public int costPerUnit;
        public int maxStock;
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

    private bool stockLoaded = false;

    async void Start()
    {
        if (storePanel != null)
            storePanel.SetActive(false);

        // Initialize items with default values
        if (storeItems != null)
        {
            foreach (var item in storeItems)
            {
                if (item != null)
                    item.Initialize();
            }
        }

        // Load saved stock data from cloud - WAIT for it to complete
        await LoadStoreStock();
        stockLoaded = true;

        // Only build UI after stock is loaded
        BuildStoreUI();
        
        Debug.Log("Store initialization complete - stock loaded: " + stockLoaded);
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
        List<StoreItemRow> visibleRows = new List<StoreItemRow>();
        foreach (var row in itemRows)
        {
            if (row.rowObject.activeSelf)
                visibleRows.Add(row);
        }

        if (visibleRows.Count == 0) return;

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

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (selectedIndex >= 0 && selectedIndex < visibleRows.Count)
            {
                StoreItemRow selectedRow = visibleRows[selectedIndex];
                if (selectedRow.purchaseButton != null && selectedRow.purchaseButton.interactable)
                {
                    PurchaseItem(selectedRow.itemData);
                }
            }
        }

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

        foreach (Transform child in itemListContainer)
        {
            Destroy(child.gameObject);
        }
        itemRows.Clear();

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

            row.backgroundImage = rowObj.GetComponent<Image>();
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

        //Disables player input while being is shop menu.
        if (UserInputManager.Instance != null)
            UserInputManager.Instance.DisableInput();

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
        //Re-enables input from the player when it closes.
        if (UserInputManager.Instance != null)
            UserInputManager.Instance.EnableInput();
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
        if (coinsText != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    bool isFree = StoreStateManager.Instance != null && StoreStateManager.Instance.IsStoreFree();

                    if (isFree)
                    {
                        coinsText.color = freeTextColor;
                        coinsText.text = "FREE";
                    }
                    else
                    {
                        coinsText.text = $"Coins: {inventory.coins}";
                    }
                }
            }
        }
    }

    void RefreshAllItems()
    {
        foreach (var row in itemRows)
        {
            UpdateRowDisplay(row);
        }
    }
    
    void UpdateRowDisplay(StoreItemRow row)
    {
        if (row == null || row.itemData == null) return;

        bool isFreeStore = StoreStateManager.Instance != null && StoreStateManager.Instance.IsStoreFree();
        bool WaveRevealed = StoreStateManager.Instance != null && StoreStateManager.Instance.IsWaveOfFireRevealed();

        bool isWaveOfFire = row.itemData.itemType == ShopItem.ShopItemType.WaveOfFire;
        bool isLocked = isWaveOfFire && !WaveRevealed;
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

        if (row.itemIcon != null)
        {
            if (isLocked && lockedItemIcon != null)
                row.itemIcon.sprite = lockedItemIcon;
            else if (row.itemData.itemIcon != null)
                row.itemIcon.sprite = row.itemData.itemIcon;

            row.itemIcon.color = Color.white;
        }

        if (row.itemNameText != null)
        {
            row.itemNameText.text = isLocked ? "???" : row.itemData.itemName;
            row.itemNameText.color = isFreeStore ? freeTextColor : normalTextColor;
        }

        if (row.descriptionText != null)
        {
            row.descriptionText.text = isLocked ? "A mysterious item..." : row.itemData.description;
        }

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
                int itemCost = isFreeStore ? 0 : row.itemData.costPerUnit;

                // Get player coins
                int playerCoins = 0;
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                    if (inventory != null)
                    {
                        playerCoins = inventory.coins;
                    }
                }

                bool canAfford = (playerCoins >= itemCost) || isFreeStore;

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
        if (StoreStateManager.Instance == null || itemData == null)
            return;

        // Find Player GameObject
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        // Get components
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogError("PlayerInventory not found!");
            return;
        }

        bool isWaveOfFire = itemData.itemType == ShopItem.ShopItemType.WaveOfFire;
        bool WaveRevealed = StoreStateManager.Instance.IsWaveOfFireRevealed();

        if (isWaveOfFire && !WaveRevealed)
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

        // Check coins
        if (!isFree && inventory.coins < cost)
        {
            Debug.Log($"Not enough coins! Need {cost}, have {inventory.coins}");
            return;
        }

        // Spend coins
        if (!isFree)
        {
            inventory.SpendCoins(cost);
        }

        AudioManager.Instance?.PlayItemPurchase();

        // Apply item effect
        ApplyItemEffect(itemData.itemType, player);

        // Update stock
        if (itemData.maxStock != -1)
        {
            itemData.currentStock--;
        }

        // IMPORTANT: Save store stock to cloud
        SaveStoreStock();

        // Save progress
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveProgress();
        }

        UpdateCoinsDisplay();
        RefreshAllItems();

        Debug.Log($"{(isFree ? "Taken" : "Purchased")}: {itemData.itemName}! Stock remaining: {itemData.currentStock}");
    }

    void ApplyItemEffect(ShopItem.ShopItemType itemType, GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        Abilities abilities = player.GetComponent<Abilities>();

        switch (itemType)
        {
            case ShopItem.ShopItemType.HPUpgrade:
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(15);
                    playerHealth.Heal(15); // Also heal
                }
                break;

            case ShopItem.ShopItemType.DamageUpgrade:
                if (playerAttack != null)
                {
                    playerAttack.IncreaseDamage(2);
                    Debug.Log($"Damage increased! New damage: {playerAttack.swordDamage}");
                }
                break;

            case ShopItem.ShopItemType.EssenceOfTheForest:
                if (playerHealth != null)
                {
                    playerHealth.MultiplyMaxHealth(125);
                    playerHealth.Heal(playerHealth.MaxHealth); // Heal to new max
                }
                break;

            case ShopItem.ShopItemType.FlashAbility:
                if (abilities != null)
                {
                    abilities.UnlockTeleport();
                }
                break;

            case ShopItem.ShopItemType.WaveOfFire:
                if (abilities != null && playerAttack != null)
                {
                    abilities.UnlockWaveOfFire();
                    playerAttack.MultiplyDamage(2);
                }
                break;

            case ShopItem.ShopItemType.Potion:
                if (playerInventory != null)
                {
                    playerInventory.AddPotion(1);
                }
                break;
        }
    }

    #region Cloud Save/Load

    private async Task EnsureCloudReady()
    {
        if (cloudReady) return;

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        cloudReady = true;
        Debug.Log("StoreController: Cloud ready");
    }

    /// <summary>
    /// Save current stock of all items to CloudSave
    /// </summary>
    private async void SaveStoreStock()
    {
        Debug.Log("=== SAVING STORE STOCK ===");
        
        try
        {
            await EnsureCloudReady();

            List<(string key, object value)> stockData = new List<(string, object)>();

            for (int i = 0; i < storeItems.Length; i++)
            {
                if (storeItems[i] != null)
                {
                    // FIX: Remove spaces from key names (CloudSave doesn't allow spaces)
                    string sanitizedName = storeItems[i].itemName.Replace(" ", "_");
                    string key = $"StoreStock_{i}_{sanitizedName}";
                    int stock = storeItems[i].currentStock;
                    stockData.Add((key, stock));
                    
                    Debug.Log($"  Saving [{i}] {storeItems[i].itemName}: {stock} (key: {key})");
                }
            }

            await DatabaseManager.SaveData(stockData.ToArray());

            Debug.Log($"Store stock saved to cloud ({stockData.Count} items)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save store stock: {e}");
        }
        
        Debug.Log("=== SAVE COMPLETE ===");
    }

    /// <summary>
    /// Load stock data from CloudSave
    /// </summary>
    private async Task LoadStoreStock()
    {
        Debug.Log("=== LOADING STORE STOCK ===");
        
        try
        {
            await EnsureCloudReady();

            List<string> keys = new List<string>();
            for (int i = 0; i < storeItems.Length; i++)
            {
                if (storeItems[i] != null)
                {
                    //Remove spaces from key names (CloudSave doesn't allow spaces)
                    string sanitizedName = storeItems[i].itemName.Replace(" ", "_");
                    keys.Add($"StoreStock_{i}_{sanitizedName}");
                }
            }

            if (keys.Count == 0)
            {
                Debug.Log("No store items to load");
                return;
            }

            Debug.Log($"Requesting {keys.Count} keys from cloud...");
            Dictionary<string, Item> cloudData = await DatabaseManager.LoadData(keys.ToArray());
            Debug.Log($"Received {cloudData.Count} items from cloud");

            int loadedCount = 0;
            for (int i = 0; i < storeItems.Length; i++)
            {
                if (storeItems[i] != null)
                {
                    //Remove spaces from key names (CloudSave doesn't allow spaces)
                    string sanitizedName = storeItems[i].itemName.Replace(" ", "_");
                    string key = $"StoreStock_{i}_{sanitizedName}";
                    
                    Debug.Log($"  Checking item [{i}] {storeItems[i].itemName} (key: {key}):");
                    Debug.Log($"    Default stock: {storeItems[i].currentStock}");
                    
                    if (cloudData.TryGetValue(key, out Item item))
                    {
                        int savedStock = item.Value.GetAs<int>();
                        storeItems[i].currentStock = savedStock;
                        loadedCount++;
                        Debug.Log($"Loaded from cloud: {savedStock}");
                    }
                    else
                    {
                        // No saved data - use default (already initialized)
                        Debug.Log($"No saved data, using default: {storeItems[i].currentStock}");
                    }
                }
            }

            Debug.Log($"Store stock loaded from cloud ({loadedCount}/{storeItems.Length} items)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load store stock: {e}");
            Debug.Log("Using default stock values");
        }
        
        Debug.Log("=== LOAD COMPLETE ===");
    }

    #endregion
}