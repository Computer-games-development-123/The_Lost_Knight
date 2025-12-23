using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : CharacterData
{
    [Header("Enemy Info")]
    public string enemyName;
    public EnemyBehaviorType behaviorType;

    [Header("Stats")]
    public int coinsDropped = 1;

    // [Header("Knockback Settings")]
    // public float knockbackForce = 8f;
    // public float knockbackDuration = 0.12f;

    // [Header("Visuals")]
    // public Sprite enemySprite;
    // public RuntimeAnimatorController animatorController;

    public enum EnemyBehaviorType
    {
        Walker,
        FastWalker,
        Jumper,
        Ranged,
        Elite
    }
}
