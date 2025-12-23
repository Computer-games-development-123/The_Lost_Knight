using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterContext))]
public class CharacterStats : MonoBehaviour
{
    [Header("Config data")]
    public CharacterData data;
    public CharacterContext ctx;

    [Header("Runtime")]
    public int currentHP;
    public int MaxHP => data.baseMaxHP;
    public int damage;
    public bool isDead = false;
    public float movingSpeed = 2f;
    public event Action<CharacterStats> OnDied;

    protected virtual void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"{name}: CharacterStats missing CharacterData", this);
            enabled = false;
            return;
        }

        currentHP = MaxHP;
        damage = data.baseDamage;
        if (ctx == null) ctx = GetComponent<CharacterContext>();
    }

    public void TakeDamage(int amount, Vector2 fromPosition)
    {
        if (isDead) return;

        currentHP = Mathf.Max(currentHP - amount, 0);

        if (ctx.AD != null)
            ctx.AD.Hurt();

        StartCoroutine(FlashRed());

        if (ctx.KB != null)
            ctx.KB.ApplyKnockback(fromPosition);

        if (currentHP <= 0)
            Dead();
    }

    protected IEnumerator FlashRed()
    {
        if (ctx.SR != null)
        {
            ctx.SR.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            ctx.SR.color = Color.white;
        }
    }

    public void Dead()
    {
        if (isDead) return;

        isDead = true;

        if (ctx.AD != null)
            ctx.AD.Death();

        if (CompareTag("Enemy"))
        {
            // Enemy specific death handling can go here
        }

        OnDied?.Invoke(this);
    }
}
