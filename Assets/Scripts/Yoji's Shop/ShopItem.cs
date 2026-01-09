using UnityEngine;

[System.Serializable]
public class ShopItem
{
    [Header("Item Information")]
    public string itemName;

    [TextArea(2, 4)]
    public string description;

    public int cost;
    public Sprite itemIcon;

    [Header("Item Type")]
    public ShopItemType itemType;

    public enum ShopItemType
    {
        HPUpgrade,
        DamageUpgrade,
        EssenceOfTheForest,
        FlashAbility,
        WaveOfFire,
        Potion
    }
}