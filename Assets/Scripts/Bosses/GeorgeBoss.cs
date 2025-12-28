using UnityEngine;
using System.Collections;

/// <summary>
/// George Boss - AUTO-FIND VERSION
/// Automatically finds portals by name - no Inspector assignment needed!
/// </summary>
public class GeorgeBoss : BossBase
{
    [Header("George Dialogues")]
    public DialogueData firstEncounterDialogue;

    [Header("Portal Names (will be found automatically)")]
    [Tooltip("Name of portal back to hub")]
    public string portalBackToHubName = "Forest_Hub_Portal";
    
    [Tooltip("Name of portal to next area")]
    public string portalToNextAreaName = "GreenToRed_Portal";

    private GameObject portalBackToHub;
    private GameObject portalToNextArea;

    [Header("First Encounter Settings")]
    public int hitsToTriggerTaunt = 5;
    private int invulnerableHitCount = 0;
    private bool firstEncounterSequenceStarted = false;

    [Header("Attack Settings")]
    public float attackCooldown = 4f;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    [Header("Range Detection")]
    public float closeRangeThreshold = 2f;
    public Transform attackPoint;
    public float attackRange = 1f;

    [Header("Flying Settings")]
    public float flySpeed = 6f;
    private bool isFlying = false;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;

    [Header("Boundaries")]
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded = false;

    protected override void Start()
    {
        base.Start();
        
        Debug.Log("========== GEORGE START ==========");
        Debug.Log($"Looking for portal 1: '{portalBackToHubName}'");
        Debug.Log($"Looking for portal 2: '{portalToNextAreaName}'");
        
        // Auto-find portals by name
        FindPortals();
        
        // Hide portals at start
        if (portalBackToHub != null)
        {
            bool wasActive = portalBackToHub.activeSelf;
            portalBackToHub.SetActive(false);
            Debug.Log($"✅ FOUND portal '{portalBackToHubName}' (was {(wasActive ? "active" : "inactive")}, now hidden)");
        }
        else
        {
            Debug.LogError($"❌ FAILED to find portal named '{portalBackToHubName}' in scene!");
            Debug.LogError("Please check: 1) Portal exists in scene, 2) Name matches exactly (case-sensitive), 3) Portal is not disabled");
        }
        
        if (portalToNextArea != null)
        {
            bool wasActive = portalToNextArea.activeSelf;
            portalToNextArea.SetActive(false);
            Debug.Log($"✅ FOUND portal '{portalToNextAreaName}' (was {(wasActive ? "active" : "inactive")}, now hidden)");
        }
        else
        {
            Debug.LogError($"❌ FAILED to find portal named '{portalToNextAreaName}' in scene!");
            Debug.LogError("Please check: 1) Portal exists in scene, 2) Name matches exactly (case-sensitive), 3) Portal is not disabled");
        }
        
        Debug.Log("========== GEORGE START COMPLETE ==========");
    }

    private void FindPortals()
    {
        Debug.Log($"🔍 Searching for portal: '{portalBackToHubName}'...");
        portalBackToHub = FindObjectInHierarchy(portalBackToHubName);
        Debug.Log($"   Result: {(portalBackToHub != null ? "FOUND" : "NOT FOUND")}");
        
        Debug.Log($"🔍 Searching for portal: '{portalToNextAreaName}'...");
        portalToNextArea = FindObjectInHierarchy(portalToNextAreaName);
        Debug.Log($"   Result: {(portalToNextArea != null ? "FOUND" : "NOT FOUND")}");
        
        // List all GameObjects in scene for debugging
        Debug.Log("📋 All GameObjects in scene:");
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Portal") || obj.name.Contains("portal"))
            {
                Debug.Log($"   - Found object with 'Portal' in name: '{obj.name}' (active: {obj.activeInHierarchy})");
            }
        }
    }

    /// <summary>
    /// Finds a GameObject by name anywhere in the hierarchy (including children)
    /// </summary>
    private GameObject FindObjectInHierarchy(string objectName)
    {
        // First try normal Find (for root objects)
        GameObject obj = GameObject.Find(objectName);
        if (obj != null) return obj;

        // If not found, search through ALL GameObjects (including inactive children)
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
        {
            // Make sure it's a scene object, not a prefab or asset
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

    protected override void Update()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;

        base.Update();
    }

    protected override void OnBossStart()
    {
        base.OnBossStart();
        bossName = "George";

        if (GameManager.Instance != null)
        {
            bool hasUpgrade = GameManager.Instance.hasSpecialSwordUpgrade;
            isInvulnerable = !hasUpgrade;

            if (!hasUpgrade)
            {
                spawnDialogue = null;
            }
        }
        else
        {
            isInvulnerable = true;
            spawnDialogue = null;
        }

        ResetInvulnerableHitCount();
    }

    private void ResetInvulnerableHitCount()
    {
        invulnerableHitCount = 0;
        firstEncounterSequenceStarted = false;
    }

    protected override void OnInvulnerableHit()
    {
        base.OnInvulnerableHit();

        if (!isInvulnerable) return;

        invulnerableHitCount++;
        Debug.Log($"⚔️ Hit {invulnerableHitCount}/{hitsToTriggerTaunt}");

        if (!firstEncounterSequenceStarted && invulnerableHitCount >= hitsToTriggerTaunt)
        {
            firstEncounterSequenceStarted = true;
            StartCoroutine(FirstEncounterTauntAndKillPlayer());
        }
    }

    private IEnumerator FirstEncounterTauntAndKillPlayer()
    {
        isAttacking = false;
        isFlying = false;
        
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Taunt");

        if (DialogueManager.Instance != null && firstEncounterDialogue != null)
        {
            bool done = false;
            DialogueManager.Instance.Play(firstEncounterDialogue, () => done = true);

            while (!done)
                yield return null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.hasDiedToGeorge = true;
            GameManager.Instance.SaveProgress();
        }

        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(9999);
            }
        }
    }

    protected override void BossAI()
    {
        if (isDead || player == null) return;
        if (isAttacking || isFlying) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (Time.time >= lastAttackTime + attackCooldown && isGrounded && distanceToPlayer <= closeRangeThreshold)
        {
            StartCoroutine(AttackSequence());
        }
        else if (isGrounded)
        {
            WalkTowardsPlayer();
        }
    }

    private void WalkTowardsPlayer()
    {
        if (player == null) return;

        float moveDirection = Mathf.Sign(player.position.x - transform.position.x);

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveDirection * walkSpeed, rb.linearVelocity.y);
        }

        if (anim != null)
        {
            anim.SetBool("IsMoving", true);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection < 0;
        }
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetBool("IsMoving", false);

        yield return StartCoroutine(CloseRangeAttack());
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FlyAway());

        isAttacking = false;
    }

    IEnumerator CloseRangeAttack()
    {
        if (anim != null)
            anim.SetTrigger("Punch");

        yield return new WaitForSeconds(0.3f);

        if (attackPoint != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.TakeDamage(damage);
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator FlyAway()
    {
        isFlying = true;

        if (anim != null)
        {
            anim.SetBool("IsGrounded", false);
            anim.SetBool("IsMoving", true);
        }

        float direction = Mathf.Sign(transform.position.x - player.position.x);
        if (direction == 0) direction = 1;

        float targetX = Mathf.Clamp(transform.position.x + (direction * 5f), minX, maxX);
        float startX = transform.position.x;
        float distance = Mathf.Abs(targetX - startX);

        float timer = 0f;
        while (timer < 0.5f)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(direction * flySpeed, flySpeed);
            timer += Time.deltaTime;
            yield return null;
        }

        while (Mathf.Abs(transform.position.x - startX) < distance &&
               transform.position.x > minX && transform.position.x < maxX)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(direction * flySpeed, 0);
            yield return null;
        }

        float descentTimer = 0f;
        while (!isGrounded && descentTimer < 3f)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.down * 8f;
            descentTimer += Time.deltaTime;
            yield return null;
        }

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("IsGrounded", true);
            anim.SetBool("IsMoving", false);
        }

        isFlying = false;
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        isAttacking = false;
        isFlying = false;

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
        Debug.Log("========== GEORGE DEATH DIALOGUE COMPLETE ==========");
        Debug.Log("✅ Starting portal spawn sequence...");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGeorgeDefeated();
            GameManager.Instance.SaveProgress();
            Debug.Log("✅ GameManager updated - Act 1 cleared");
        }
        else
        {
            Debug.LogError("❌ GameManager is null!");
        }

        // Spawn both portals
        Debug.Log($"🌀 Attempting to spawn portal 1: '{portalBackToHubName}'");
        if (portalBackToHub != null)
        {
            Debug.Log($"   Portal object exists: {portalBackToHub.name}");
            Debug.Log($"   Portal was active: {portalBackToHub.activeSelf}");
            portalBackToHub.SetActive(true);
            Debug.Log($"   Portal is now active: {portalBackToHub.activeSelf}");
            Debug.Log($"✅ SUCCESS: Portal '{portalBackToHubName}' spawned!");
        }
        else
        {
            Debug.LogError($"❌ FAILED: Portal '{portalBackToHubName}' is NULL - cannot spawn!");
            Debug.LogError("This means GameObject.Find() didn't find it at Start()");
        }

        Debug.Log($"🌀 Attempting to spawn portal 2: '{portalToNextAreaName}'");
        if (portalToNextArea != null)
        {
            Debug.Log($"   Portal object exists: {portalToNextArea.name}");
            Debug.Log($"   Portal was active: {portalToNextArea.activeSelf}");
            portalToNextArea.SetActive(true);
            Debug.Log($"   Portal is now active: {portalToNextArea.activeSelf}");
            Debug.Log($"✅ SUCCESS: Portal '{portalToNextAreaName}' spawned!");
        }
        else
        {
            Debug.LogError($"❌ FAILED: Portal '{portalToNextAreaName}' is NULL - cannot spawn!");
            Debug.LogError("This means GameObject.Find() didn't find it at Start()");
        }

        Debug.Log("========== PORTAL SPAWN SEQUENCE COMPLETE ==========");
        Debug.Log("🗑️ Destroying George in 2 seconds...");
        
        Destroy(gameObject, 2f);
    }

    protected override void EnterPhase2()
    {
        base.EnterPhase2();
        
        attackCooldown *= 0.75f;
        walkSpeed *= 1.3f;
        flySpeed *= 1.2f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeRangeThreshold);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Vector3 tl = new Vector3(minX, maxY, 0);
        Vector3 tr = new Vector3(maxX, maxY, 0);
        Vector3 bl = new Vector3(minX, minY, 0);
        Vector3 br = new Vector3(maxX, minY, 0);

        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
    }
}