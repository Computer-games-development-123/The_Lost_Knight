using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterContext : MonoBehaviour
{
    // Core Components
    public Rigidbody2D RB { get; private set; }
    public Animator Anim { get; private set; }
    public SpriteRenderer SR { get; private set; }
    
    // BUNCH 2 Components
    public PlayerController PC { get; private set; }
    public PlayerHealth PH { get; private set; }
    public PlayerAttack PA { get; private set; }
    
    // BUNCH 1 Components (optional)
    public Abilities AB { get; private set; }
    public Invulnerability Inv { get; private set; }
    public Knockback KB { get; private set; }
    
    [SerializeField] private Transform attackPoint;
    public bool FacingRight { get; private set; } = true;

    private void Awake()
    {
        // Core
        RB = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        SR = GetComponent<SpriteRenderer>();
        
        // BUNCH 2
        PC = GetComponent<PlayerController>();
        PH = GetComponent<PlayerHealth>();
        PA = GetComponent<PlayerAttack>();
        
        // BUNCH 1 (optional)
        AB = GetComponent<Abilities>();
        Inv = GetComponent<Invulnerability>();
        KB = GetComponent<Knockback>();
    }

    public void SetFacing(bool facingRight)
    {
        if (FacingRight == facingRight) return;
        FacingRight = facingRight;
        
        // BUNCH 2 uses scale flipping
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public Vector2 FacingDir => FacingRight ? Vector2.right : Vector2.left;
}