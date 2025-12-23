using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(CharacterContext))]
public class SwordAttack : MonoBehaviour, IHitConfirmListener
{
    [Header("Input")]
    [SerializeField] private KeyCode attackKey = KeyCode.X;

    [Header("Combo")]
    [SerializeField] private float comboWindow = 0.35f;
    [SerializeField] private int maxComboSteps = 3;

    [Header("Movement During Attack")]
    [SerializeField] private bool lockMovementDuringAttack = false;
    [SerializeField, Range(0f, 1f)] private float attackMoveMultiplier = 0.2f;

    [Header("Hit Confirm (Broadcaster)")]
    [SerializeField] private HitConfirmBroadcaster hitBroadcaster;

    private CharacterContext ctx;
    private PlayerMovement movement;
    private AnimatorDriver pad;

    private int comboStep = 0;
    private float comboTimer = 0f;

    private bool hitConfirmedThisStep = false;

    private bool isAttacking = false;
    bool grounded => movement.IsGrounded;

    private void Awake()
    {
        ctx = GetComponent<CharacterContext>();
        movement = GetComponent<PlayerMovement>();
        pad = GetComponent<AnimatorDriver>();

        if (pad == null)
            Debug.LogError($"{name}: Missing AnimatorDriver on the same GameObject!", this);
    }

    private void Start()
    {
        if (hitBroadcaster == null)
            hitBroadcaster = GetComponentInChildren<HitConfirmBroadcaster>();

        if (hitBroadcaster == null)
            hitBroadcaster = GetComponentInParent<HitConfirmBroadcaster>();

        if (hitBroadcaster != null)
            hitBroadcaster.Register(this);
        else
            Debug.LogWarning($"{name}: No HitConfirmBroadcaster found, OnHitConfirmed won't fire.");
    }

    private void OnDestroy()
    {
        if (hitBroadcaster != null)
            hitBroadcaster.Unregister(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(attackKey))
            RegisterAttackInput();

        if (comboStep > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                ResetCombo();
        }
    }

    private void RegisterAttackInput()
    {
        if (pad == null) return;

        if (comboStep == 0)
        {
            comboStep = 1;
            comboTimer = comboWindow;
            hitConfirmedThisStep = false;

            StartAttackAnim(comboStep);
            return;
        }

        if (comboTimer > 0f)
        {
            if (!hitConfirmedThisStep)
                return;

            hitConfirmedThisStep = false;

            comboStep = Mathf.Clamp(comboStep + 1, 1, maxComboSteps);
            comboTimer = comboWindow;

            StartAttackAnim(comboStep);
        }
    }

    private void StartAttackAnim(int step)
    {
        isAttacking = true;

        if (lockMovementDuringAttack)
        {
            movement.MovementLocked = true;
        }
        else
        {
            movement.MovementMultiplier = attackMoveMultiplier;
        }

        pad.Combo(step);
        pad.Attacking(true);

        if (!grounded)
        {
            pad.JumpAttack();
            return;
        }
        else
        {
            pad.Attack();
        }
    }

    private void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;
        hitConfirmedThisStep = false;
        isAttacking = false;

        if (pad != null)
        {
            pad.Combo(0);
            pad.Attacking(false);
        }

        if (movement != null)
        {
            movement.MovementLocked = false;
            movement.MovementMultiplier = 1f;
        }
    }

    public void OnHitConfirmed(GameObject target)
    {
        if (!isAttacking) return;
        hitConfirmedThisStep = true;
    }

    public void OnAttackEnd()
    {
        if (pad != null) pad.Attacking(false);

        if (movement != null)
        {
            movement.MovementLocked = false;
            movement.MovementMultiplier = 1f;
        }

        isAttacking = false;

        if (comboTimer <= 0f)
            ResetCombo();
    }
}
