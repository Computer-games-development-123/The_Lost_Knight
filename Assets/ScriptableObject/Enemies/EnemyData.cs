using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Lost Knight/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy Info")]
    public string enemyName;
    public EnemyBehaviorType behaviorType;

    [Header("Stats")]
    public int maxHP = 18;
    public int damage = 7;
    public float moveSpeed = 2f;
    public int coinsDropped = 1;

    [Header("Visuals")]
    public Sprite enemySprite;
    public RuntimeAnimatorController animatorController;

    public enum EnemyBehaviorType
    {
        Walker,
        FastWalker,
        Jumper,
        Ranged,
        Elite
    }
}