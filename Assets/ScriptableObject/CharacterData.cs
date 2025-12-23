using UnityEngine;

public abstract class CharacterData : ScriptableObject
{
    [Header("Stats")]
    public int baseMaxHP = 100;
    public float moveSpeed = 2f;

    [Header("Combat")]
    public int baseDamage = 10;
    public float attackRange = 0.5f;
    public float attackCooldown = 0.5f;

}
