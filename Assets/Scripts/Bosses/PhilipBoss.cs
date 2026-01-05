using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class PhilipBoss : BossBase
{
    [Header("Philip Specific")]
    public GameObject specialAttackPortal;
    protected override void Start()
    {
        base.Start();
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
    }

    void SpecialAttack()
    {
        anim.SetTrigger("attack2");
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