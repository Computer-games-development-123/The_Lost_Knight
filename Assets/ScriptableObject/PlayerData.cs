using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : CharacterData
{
    [Header("Stats")]
    public int MaxPotions = 5;

    [Header("Combat")]
    public int upgradeSwordAddition = 5;
    public int fireSwordAddition = 15;

    [Header("Economy")]
    public int baseStartingCoins = 0;
    public int baseStartingPotions = 5;
    public int baseMaxPotions = 5;
    public int healPerPotion = 25;
}
