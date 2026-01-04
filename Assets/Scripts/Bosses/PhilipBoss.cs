using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class PhilipBoss : BossBase
{
    [Header("Philip Specific")]
    public GameObject specialAttackPortal;
    // public float slamCooldown = 4f;
    // public int slamDamage = 15;
    // private float lastSlamTime;
    // private bool isSlaming = false;

    // protected override void OnBossStart()
    // {
    //     base.OnBossStart();
    //     bossName = "Philip";
    //     maxHP = 150;
    //     currentHP = maxHP;
    //     moveSpeed = 2f;
    //     damage = 12;
    // }
    protected override void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) SpecialAttack();
    }
    protected override void BossAI()
    {
        //base.BossAI();
        SpecialAttack();
        // if (Time.time >= lastSlamTime + slamCooldown && distanceToPlayer < 5f)
        // {
        //     StartCoroutine(SpecialAttack());
        // }
        // else
        // {
        //     base.BossAI();
        // }
    }

    void SpecialAttack()
    {
        // isSlaming = true;
        // lastSlamTime = Time.time;

        // // Jump up
        // rb.linearVelocity = new Vector2(0, 10f);

        // if (anim != null)
        //     anim.SetTrigger("Jump");
        anim.SetTrigger("attack2");

        // // Slam down
        // rb.linearVelocity = new Vector2(0, -15f);

        // yield return new WaitUntil(() => rb.linearVelocity.y == 0);

        // // Create shockwave
        // if (shockwavePrefab != null)
        // {
        //     Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        // }

        // // Damage player if nearby
        // if (player != null && Vector2.Distance(transform.position, player.position) < 4f)
        // {
        //     PlayerHealth ph = player.GetComponent<PlayerHealth>();
        //     if (ph != null)
        //     {
        //         ph.TakeDamage(slamDamage);
        //     }
        // }

        // if (anim != null)
        //     anim.SetTrigger("SlamImpact");

        // yield return new WaitForSeconds(1f);

        // isSlaming = false;
    }

    IEnumerator OnSpecialAttackStart()
    {
        yield return new WaitForSeconds(0.5f);
        specialAttackPortal.SetActive(true);
    }

    public IEnumerator OnSpecialAttackEnd()
    {
        anim.SetTrigger("ClosePortal");
        yield return new WaitForSeconds(0.5f);
        specialAttackPortal.SetActive(false);
    }

    protected override void EnterPhase2()
    {
        // base.EnterPhase2();
        // slamCooldown *= 0.7f;
        // slamDamage = Mathf.RoundToInt(slamDamage * 1.5f);
    }

    // protected override void Die()
    // {
    //     if (isDead) return;
    //     isDead = true;

    //     Debug.Log($"💀 {bossName} defeated!");

    //     isSlaming = false;

    //     if (rb != null)
    //         rb.linearVelocity = Vector2.zero;

    //     if (anim != null)
    //         anim.SetTrigger("Death");

    //     if (waveManager != null)
    //     {
    //         waveManager.OnBossDied(this);
    //     }

    //     if (DialogueManager.Instance != null && deathDialogue != null)
    //     {
    //         DialogueManager.Instance.Play(deathDialogue, OnDeathDialogueComplete);
    //     }
    //     else
    //     {
    //         OnDeathDialogueComplete();
    //     }
    // }

    // private void OnDeathDialogueComplete()
    // {
    //     Debug.Log($"✅ Philip defeated - spawning portals...");

    //     if (GameManager.Instance != null)
    //     {
    //         GameManager.Instance.OnPhilipDefeated();
    //     }

    //     Destroy(gameObject, 2f);
    // }
}