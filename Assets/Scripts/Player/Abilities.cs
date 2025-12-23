using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(CharacterContext))]
[RequireComponent(typeof(Invulnerability))]
[RequireComponent(typeof(FormSwitcher))]
public class Abilities : MonoBehaviour
{
    [Header("Upgrades/Flags")]
    public bool hasDoubleJump;
    public bool hasTeleport = false;
    public bool hasUpgradedSword = false;
    public bool hasFireSword = false;
    public float teleportDistance = 4.5f;
    public LayerMask groundLayer;
    private CharacterContext ctx;
    private Invulnerability invuln;
    public bool isInvulnerable = false;


    private void Awake()
    {
        ctx = GetComponent<CharacterContext>();
        invuln = GetComponent<Invulnerability>();
        isInvulnerable = (invuln == null) ? false : true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) && hasTeleport) Teleport();

        if (Input.GetKeyDown(KeyCode.T) && hasFireSword)
        {
            ctx.AD.Transform();
            ctx.PS.fireSwordActivated = true;
        }
    }

    private void Teleport()
    {
        Vector2 dir = ctx.FacingDir;
        Vector2 newPos = (Vector2)transform.position + dir * teleportDistance;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, teleportDistance, groundLayer);

        if (hit.collider == null)
        {
            transform.position = newPos;
            invuln?.Trigger();
        }
    }
}
