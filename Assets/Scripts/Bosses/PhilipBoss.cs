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

    protected override void OnBossStart()
    {
        base.OnBossStart();
        
        bossName = "Philip";

        // Set Yoji as dead when Philip appears
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(GameFlag.YojiDead, true);
            GameManager.Instance.SaveProgress();
            Debug.Log("Philip has killed Yoji - YojiDead flag set");
        }

        // Update store to free (PostPhilip state)
        if (StoreStateManager.Instance != null)
        {
            StoreStateManager.Instance.SetStoreState(StoreStateManager.StoreState.PostPhilip);
            Debug.Log("Store is now free - Yoji's Legacy");
        }
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

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{bossName} defeated!");

        //isSlaming = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Death");

        if (waveManager != null)
        {
            waveManager.OnBossDied(this);
        }

        if (DialogueManager.Instance != null && deathDialogue != null)
        {
            DialogueManager.Instance.Play(deathDialogue, OnDeathDialogueComplete);
        }
        else
        {
            OnDeathDialogueComplete();
        }
    }

    protected override void OnDeathDialogueComplete()
    {
        // Call base to handle coins and slain dialogue
        base.OnDeathDialogueComplete();

        Debug.Log($"Philip defeated - spawning portals...");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhilipDefeated();
        }
    }
}