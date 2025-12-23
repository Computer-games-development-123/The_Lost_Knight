using UnityEngine;

public class PlayerStats : CharacterStats
{
    [Header("Settings for player")]
    [Header("Config data")]
    public PlayerData playerData;

    [Header("Runtime")]
    public int coins;
    public int potions;

    public int MaxPotions => playerData.MaxPotions;
    public int UpgradeSwordAddition => playerData.upgradeSwordAddition;
    public int FireSwordAddition => playerData.fireSwordAddition;
    public bool fireSwordActivated;

    protected override void Awake()
    {
        base.Awake();
        fireSwordActivated = false;
        if (playerData == null)
        {
            Debug.LogError($"{name}: PlayerData is NULL on PlayerStats!", this);
            enabled = false;
            return;
        }

        coins = playerData.baseStartingCoins;
        potions = playerData.baseStartingPotions;
    }

    void Update()
    {
        if (ctx.AB.hasFireSword && fireSwordActivated)
        {
            Debug.LogWarning("Upgrading sword from " + damage);
            UpgradeToFireSword();
            Debug.LogWarning("Upgrading sword to " + damage);
            fireSwordActivated = false;
        }

        if (Input.GetKeyDown(KeyCode.H) && potions > 0) Heal();

    }

    public void EarnCoins(int amount) => coins += amount;

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        return true;
    }

    public void AddPotion()
    {
        potions = Mathf.Min(potions + 1, MaxPotions);
    }

    public void Heal()
    {
        if (potions <= 0) return;

        currentHP = Mathf.Min(currentHP + playerData.healPerPotion, MaxHP);
        potions = Mathf.Max(potions - 1, 0);
    }

    public void UpgradeSword() => damage += UpgradeSwordAddition;
    public void UpgradeToFireSword() => damage += FireSwordAddition;
}
