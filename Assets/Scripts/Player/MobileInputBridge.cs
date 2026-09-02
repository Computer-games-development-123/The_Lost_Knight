using UnityEngine;

/// <summary>
/// Acts as a virtual input layer for mobile touch controls.
/// On-screen buttons set these flags; player scripts read them.
/// 
/// Flags stay true for one full frame so any Update() that runs after the
/// flag is set will see it (regardless of script execution order).
/// </summary>
public class MobileInputBridge : MonoBehaviour
{
    public static MobileInputBridge Instance { get; private set; }

    // Movement (continuous)
    public float MoveInput { get; private set; } = 0f;

    // Action flags - public properties read by other scripts
    public bool JumpPressed => _jumpFrame == Time.frameCount;
    public bool AttackPressed => _attackFrame == Time.frameCount;
    public bool PotionPressed => _potionFrame == Time.frameCount;
    public bool TeleportPressed => _teleportFrame == Time.frameCount;
    public bool FireballPressed => _fireballFrame == Time.frameCount;
    public bool BreathOfFirePressed => _breathFrame == Time.frameCount;
    public bool InteractPressed => _interactFrame == Time.frameCount;
    public bool ShopPressed => _shopFrame == Time.frameCount;
    public bool PausePressed => _pauseFrame == Time.frameCount;

    // Internal - the frame each action was triggered on
    private int _jumpFrame = -1;
    private int _attackFrame = -1;
    private int _potionFrame = -1;
    private int _teleportFrame = -1;
    private int _fireballFrame = -1;
    private int _breathFrame = -1;
    private int _interactFrame = -1;
    private int _shopFrame = -1;
    private int _pauseFrame = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Called by on-screen buttons
    public void SetMoveInput(float value) => MoveInput = value;
    public void OnJumpPressed() => _jumpFrame = Time.frameCount;
    public void OnAttackPressed() => _attackFrame = Time.frameCount;
    public void OnPotionPressed() => _potionFrame = Time.frameCount;
    public void OnTeleportPressed() => _teleportFrame = Time.frameCount;
    public void OnFireballPressed() => _fireballFrame = Time.frameCount;
    public void OnBreathOfFirePressed() => _breathFrame = Time.frameCount;
    public void OnInteractPressed() => _interactFrame = Time.frameCount;
    public void OnShopPressed() => _shopFrame = Time.frameCount;
    public void OnPausePressed() => _pauseFrame = Time.frameCount;
}