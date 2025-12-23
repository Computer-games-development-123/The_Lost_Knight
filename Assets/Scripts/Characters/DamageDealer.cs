using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class DamageDealer : MonoBehaviour
{
    [Header("info")]
    private CharacterStats Stats;
    [SerializeField] private LayerMask targetLayers;
    private int Damage => Stats != null ? Stats.damage : 0;

    [Header("Hit Area (Attack)")]
    [SerializeField] private CircleCollider2D hitPoint;
    private float HitRange => hitPoint != null ? hitPoint.radius : 0.5f;

    [Header("Contact Damage (Collision)")]
    [SerializeField] private bool dealDamageOnTouch = true;
    [SerializeField] private int touchDamage = 1;
    [SerializeField] private float touchCooldown = 0.5f;

    private float nextTouchTime;

    [Header("Hit Confirm (Optional)")]
    [SerializeField] private HitConfirmBroadcaster hitBroadcaster;

    private void Awake()
    {
        if (Stats == null) Stats = GetComponent<CharacterStats>();

        if (hitBroadcaster == null)
            hitBroadcaster = GetComponentInParent<HitConfirmBroadcaster>();
    }

    public void DealDamage()
    {
        if (hitPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            hitPoint.transform.position,
            HitRange,
            targetLayers
        );

        for (int i = 0; i < hits.Length; i++)
        {
            CharacterStats cs = hits[i].GetComponent<CharacterStats>();
            if (cs == null) continue;
            if (cs == Stats) continue;

            cs.TakeDamage(Damage, transform.position);

            if (hitBroadcaster != null)
                hitBroadcaster.NotifyHit(cs.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        DealTouchDamage(col.collider);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        DealTouchDamage(col.collider);
    }

    private void DealTouchDamage(Collider2D other)
    {
        if (!dealDamageOnTouch || other.CompareTag("Enemy")) return;

        if (Time.time < nextTouchTime) return;

        if (((1 << other.gameObject.layer) & targetLayers) == 0) return;

        CharacterStats cs = other.GetComponent<CharacterStats>();
        if (cs == null) return;
        if (cs == Stats) return;

        cs.TakeDamage(touchDamage, transform.position);
        nextTouchTime = Time.time + touchCooldown;

        // if (hitBroadcaster != null)
        //     hitBroadcaster.NotifyHit(cs.gameObject);
    }
}
