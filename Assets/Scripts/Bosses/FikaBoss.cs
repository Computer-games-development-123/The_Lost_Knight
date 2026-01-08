using UnityEngine;

public class FikaBoss : BossBase
{
    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Shooting")]
    [SerializeField] private float timeBetweenShots = 3f;
    [SerializeField] private float attackSoonAfterSpawn = 0.6f;
    [SerializeField] private float attackSoonAfterTeleport = 0.35f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileLifetime = 4f;

    [Header("Teleport")]
    [SerializeField] private float teleportEverySeconds = 6f;
    [SerializeField] private float teleportChargeTime = 0.25f;
    [SerializeField] private float teleportRecoverTime = 0.35f;
    [SerializeField] private float teleportOffsetFromEdge = 1.2f;

    [Header("Teleport FX")]
    [SerializeField] private GameObject teleportSmokePrefab;
    [SerializeField] private float smokeYOffset = 0.2f;

    private float shootTimer;
    private float teleportTimer;

    private bool isTeleporting = false;
    private Camera mainCam;

    protected override void Start()
    {
        base.Start();

        mainCam = Camera.main;

        shootTimer = Mathf.Max(0.05f, attackSoonAfterSpawn);
        teleportTimer = teleportEverySeconds;
    }

    protected override void BossAI()
    {
        if (isDead) return;

        shootTimer -= Time.deltaTime;
        teleportTimer -= Time.deltaTime;

        if (isTeleporting)
            return;

        if (player != null)
        {
            float dx = player.position.x - transform.position.x;

            if (dx > 0f && !facingRight) Flip();
            else if (dx < 0f && facingRight) Flip();
        }

        // === Attack trigger לפי טיימר ===
        if (shootTimer <= 0f)
        {
            anim.SetTrigger("Attack");
            shootTimer = timeBetweenShots;
        }

        // === Teleport לפי טיימר ===
        if (teleportTimer <= 0f)
        {
            StartTeleport();
            teleportTimer = teleportEverySeconds;
        }
    }

    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount);
        if (isDead) return;

        StartTeleport();

        teleportTimer = teleportEverySeconds;
    }

    private void StartTeleport()
    {
        if (isTeleporting) return;

        isTeleporting = true;
        isInvulnerable = true;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        SpawnTeleportFX(transform.position);

        Invoke(nameof(DoTeleportToOtherSideEdge), teleportChargeTime);
    }

    private void DoTeleportToOtherSideEdge()
    {
        TeleportToOtherSideEdge();

        SpawnTeleportFX(transform.position);

        Invoke(nameof(EndTeleport), teleportRecoverTime);
    }

    private void EndTeleport()
    {
        isTeleporting = false;
        isInvulnerable = false;

        shootTimer = Mathf.Min(shootTimer, attackSoonAfterTeleport);
    }

    private void TeleportToOtherSideEdge()
    {
        if (mainCam == null)
        {
            transform.position = new Vector3(transform.position.x * -1f, transform.position.y, transform.position.z);
            return;
        }

        Vector3 pos = transform.position;

        float camHalfWidth = mainCam.orthographicSize * mainCam.aspect;
        float camX = mainCam.transform.position.x;

        bool bossOnLeftSide = pos.x < camX;

        if (bossOnLeftSide)
            pos.x = camX + camHalfWidth - teleportOffsetFromEdge;
        else
            pos.x = camX - camHalfWidth + teleportOffsetFromEdge;

        transform.position = pos;

        if (player != null)
        {
            float dx = player.position.x - transform.position.x;
            if (dx > 0f && !facingRight) Flip();
            else if (dx < 0f && facingRight) Flip();
        }
    }

    private void SpawnTeleportFX(Vector3 position)
    {
        if (teleportSmokePrefab == null)
            return;

        Vector3 fxPos = position;
        fxPos.y += smokeYOffset;

        GameObject fx = Instantiate(teleportSmokePrefab, fxPos, Quaternion.identity);
        Destroy(fx, 2f);
    }

    public void Shoot()
    {
        if (isTeleporting) return;

        if (projectilePrefab == null || spawnPoint == null)
            return;

        bool shootRight = facingRight;
        Vector2 dir = shootRight ? Vector2.right : Vector2.left;

        GameObject p = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);

        Vector3 s = p.transform.localScale;
        s.x = Mathf.Abs(s.x) * (shootRight ? 1f : -1f);
        p.transform.localScale = s;

        Rigidbody2D rbP = p.GetComponent<Rigidbody2D>();
        if (rbP != null)
            rbP.linearVelocity = dir * projectileSpeed;

        Destroy(p, projectileLifetime);
    }

    protected override void OnDeathDialogueComplete()
    {
        // Call base to handle coins and slain dialogue
        base.OnDeathDialogueComplete();

        // Fika-specific death logic
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFikaDefeated();
            GameManager.Instance.SaveProgress();
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
