using UnityEngine;
using System.Collections;

public class PhilipBoss : BossBase
{
    [Header("Philip Specific")]
    public GameObject shockwavePrefab;
    public float slamCooldown = 4f;
    public int slamDamage = 15;
    private float lastSlamTime;
    private bool isSlaming = false;

    [Header("Portals (Direct References)")]
    public string portalBackToHubName = "Forest_Hub_Portal";
    public string portalToNextAreaName = "FinalArea_Portal"; // Or end game portal

    protected override void OnBossStart()
    {
        base.OnBossStart();
        bossName = "Philip";
        maxHP = 150;
        currentHP = maxHP;
        moveSpeed = 2f;
        damage = 12;
    }

    protected override void BossAI()
    {
        if (player == null || isSlaming) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (Time.time >= lastSlamTime + slamCooldown && distanceToPlayer < 5f)
        {
            StartCoroutine(GroundSlam());
        }
        else
        {
            base.BossAI();
        }
    }

    IEnumerator GroundSlam()
    {
        isSlaming = true;
        lastSlamTime = Time.time;

        // Jump up
        rb.linearVelocity = new Vector2(0, 10f);

        if (anim != null)
            anim.SetTrigger("Jump");

        yield return new WaitForSeconds(0.5f);

        // Slam down
        rb.linearVelocity = new Vector2(0, -15f);

        yield return new WaitUntil(() => rb.linearVelocity.y == 0);

        // Create shockwave
        if (shockwavePrefab != null)
        {
            Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        }

        // Damage player if nearby
        if (player != null && Vector2.Distance(transform.position, player.position) < 4f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(slamDamage);
            }
        }

        if (anim != null)
            anim.SetTrigger("SlamImpact");

        yield return new WaitForSeconds(1f);

        isSlaming = false;
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        slamCooldown *= 0.7f;
        slamDamage = Mathf.RoundToInt(slamDamage * 1.5f);
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"💀 {bossName} defeated!");

        isSlaming = false;

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

    private void OnDeathDialogueComplete()
    {
        Debug.Log($"✅ Philip defeated - spawning portals...");

        // ✅ Update GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhilipDefeated();
        }

        // ✅ Spawn portals
        GameObject portalBack = FindObjectInHierarchy(portalBackToHubName);
        GameObject portalNext = FindObjectInHierarchy(portalToNextAreaName);

        if (portalBack != null)
        {
            portalBack.SetActive(true);
            Debug.Log($"🌀 Portal spawned: {portalBackToHubName}");
        }

        if (portalNext != null)
        {
            portalNext.SetActive(true);
            Debug.Log($"🌀 Portal spawned: {portalToNextAreaName}");
        }

        Destroy(gameObject, 2f);
    }

    private GameObject FindObjectInHierarchy(string objectName)
    {
        if (string.IsNullOrEmpty(objectName)) return null;

        GameObject obj = GameObject.Find(objectName);
        if (obj != null) return obj;

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.hideFlags == HideFlags.None && go.scene.isLoaded)
            {
                if (go.name == objectName)
                {
                    return go;
                }
            }
        }

        return null;
    }
}