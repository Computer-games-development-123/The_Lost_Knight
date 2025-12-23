using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterContext : MonoBehaviour
{
    public Rigidbody2D RB { get; protected set; }
    public AnimatorDriver AD { get; protected set; }
    public SpriteRenderer SR { get; protected set; }
    public CharacterStats CS { get; set; }
    public PlayerStats PS { get; private set; }
    public PlayerMovement PM { get; private set; }
    public Knockback KB { get; protected set; }
    public Abilities AB { get; protected set; }
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackX = 0.2f;

    public bool FacingRight { get; protected set; } = true;

    protected void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        AD = GetComponent<AnimatorDriver>();
        SR = GetComponent<SpriteRenderer>();
        CS = GetComponent<CharacterStats>();
        PS = GetComponent<PlayerStats>();
        PM = GetComponent<PlayerMovement>();
        KB = GetComponent<Knockback>();
        AB = GetComponent<Abilities>();
        if (attackPoint == null)
            Debug.LogWarning("Missing attack point");
    }

    public void SetFacing(bool facingRight)
    {
        if (FacingRight == facingRight) return;

        FacingRight = facingRight;
        SR.flipX = !FacingRight;

        if (attackPoint != null)
        {
            Vector3 lp = attackPoint.localPosition;
            lp.x = Mathf.Abs(attackX) * (FacingRight ? 1f : -1f);
            attackPoint.localPosition = lp;
        }
    }

    public Vector2 FacingDir => FacingRight ? Vector2.right : Vector2.left;
}
