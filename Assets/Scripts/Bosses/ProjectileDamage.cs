using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProjectileDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask playerMask; // שים כאן שכבת Player

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private float destroyFallbackDelay = 1.2f; // אם אין Animation Event

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;

    private bool hasHit = false;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<Collider2D>();
        if (animator == null) animator = GetComponent<Animator>();

        // מומלץ לפרוג’קטיילים
        if (col != null) col.isTrigger = true;
        if (rb != null) rb.gravityScale = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // פגיעה רק בשחקן לפי שכבה (מומלץ)
        if (((1 << other.gameObject.layer) & playerMask) == 0)
            return;

        hasHit = true;

        // 1) Damage לשחקן
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(damage);

        // 2) לעצור תנועה ולא לאפשר פגיעות חוזרות
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (col != null)
            col.enabled = false;

        // 3) להפעיל אנימציית פגיעה
        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
        {
            animator.SetTrigger(hitTrigger);

            // גיבוי למחיקה אם לא הוספת Animation Event בסוף הקליפ
            Invoke(nameof(DestroySelf), destroyFallbackDelay);
        }
        else
        {
            // אם אין אנימטור - מוחקים ישר
            DestroySelf();
        }
    }

    // לקרוא לזה בסוף אנימציית Hit באמצעות Animation Event
    public void OnHitAnimationFinished()
    {
        DestroySelf();
    }

    private void DestroySelf()
    {
        CancelInvoke();
        Destroy(gameObject);
    }
}
