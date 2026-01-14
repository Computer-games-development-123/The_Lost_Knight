using UnityEngine;
using System.Collections;

public class DitorBoss : BossBase
{
    public enum AttackType
    {
        ForwardAttack,
        Uppercut,
        AirCombo
    }

    [Header("Combat - General")]
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float attackCooldown = 1.25f;
    [SerializeField] private float recoverTime = 0.35f;

    [Header("Hitbox")]
    [SerializeField] private Transform hitPoint;
    [SerializeField] private float hitRadius = 0.6f;
    [SerializeField] private LayerMask playerMask;

    [Header("Forward Attack")]
    [SerializeField] private int forwardDamageBonus = 0;
    [SerializeField] private float forwardLungeForce = 7f;

    [Header("Uppercut")]
    [SerializeField] private int uppercutDamageBonus = 3;
    [SerializeField] private float uppercutUpForce = 10f;
    [SerializeField] private float uppercutForwardForce = 4f;

    [Header("Air Combo (Special)")]
    [SerializeField] private int airHitCount = 3;
    [SerializeField] private float airJumpForce = 9f;
    [SerializeField] private int airDamageBonusPerHit = -2;

    [Header("AI Weights")]
    [SerializeField] private int weightForward = 55;
    [SerializeField] private int weightUppercut = 25;
    [SerializeField] private int weightAirCombo = 20;

    private float attackTimer = 0f;
    private bool isAttacking = false;

    private int airHitsDone = 0;

    protected override void Start()
    {
        base.Start();
        if (hitPoint == null) hitPoint = transform;
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;
    }

    protected override void BossAI()
    {
        if (player == null) return;

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        base.BossAI();

        float dist = Vector2.Distance(player.position, transform.position);

        if (dist <= attackRange && attackTimer <= 0f)
        {
            AttackType chosen = ChooseAttack();
            StartCoroutine(AttackRoutine(chosen));
        }
    }

    private AttackType ChooseAttack()
    {
        int wF = weightForward;
        int wU = weightUppercut;
        int wA = weightAirCombo;

        if (isPhase2)
        {
            wF = Mathf.Max(10, wF - 15);
            wU += 10;
            wA += 15;
        }

        int total = wF + wU + wA;
        int roll = Random.Range(0, total);

        if (roll < wF) return AttackType.ForwardAttack;
        roll -= wF;
        if (roll < wU) return AttackType.Uppercut;
        return AttackType.AirCombo;
    }

    private IEnumerator AttackRoutine(AttackType type)
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        FacePlayer();

        airHitsDone = 0;

        if (anim != null)
        {
            switch (type)
            {
                case AttackType.ForwardAttack:
                    anim.SetTrigger("Attack1");
                    break;

                case AttackType.Uppercut:
                    anim.SetTrigger("Attack2");
                    break;

                case AttackType.AirCombo:
                    anim.SetTrigger("Attack3");
                    break;
            }
        }
        yield return new WaitForSeconds(recoverTime);

        isAttacking = false;
    }

    private void FacePlayer()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        if (dx > 0f && !facingRight) Flip();
        else if (dx < 0f && facingRight) Flip();
    }

    public void AE_HitForward()
    {
        if (isDead) return;

        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * forwardLungeForce, rb.linearVelocity.y);

        DealHit(damage + forwardDamageBonus);
    }

    public void AE_HitUppercut()
    {
        if (isDead) return;

        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * uppercutForwardForce, uppercutUpForce);

        DealHit(damage + uppercutDamageBonus);
    }

    public void AE_AirStartJump()
    {
        if (isDead) return;

        rb.linearVelocity = new Vector2(0f, airJumpForce);
    }

    public void AE_AirHit()
    {
        if (isDead) return;

        if (airHitsDone >= airHitCount) return;

        int hitDamage = damage + airDamageBonusPerHit;
        if (isPhase2) hitDamage += 1;

        DealHit(hitDamage);
        airHitsDone++;
    }

    public void AE_AttackEnd()
    {
        isAttacking = false;
    }

    private void DealHit(int dmg)
    {
        if (hitPoint == null) return;

        Collider2D col = Physics2D.OverlapCircle(hitPoint.position, hitRadius, playerMask);
        if (col == null) return;

        PlayerHealth ph = col.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(dmg);
    }

    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null) return;
        Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // בכוונה ריק
    }
}
