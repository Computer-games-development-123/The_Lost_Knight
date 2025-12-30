using UnityEngine;

/// <summary>
/// Handles player abilities: Teleport and Wave of Fire
/// Owns the ability unlock flags (hasTeleport, hasWaveOfFire)
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class Abilities : MonoBehaviour
{
    [Header("Ability Unlocks")]
    public bool hasTeleport = false;
    public bool hasWaveOfFire = false;
    public bool hasUpgradedSword = false;

    [Header("Teleport Settings")]
    public float teleportDistance = 4.5f;
    public LayerMask groundLayer;

    private PlayerController controller;
    private Invulnerability invulnerability;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        invulnerability = GetComponent<Invulnerability>();
    }

    private void Start()
    {
        // ✅ Load upgrades from save as backup (in case LoadProgress ran before Player spawned)
        hasUpgradedSword = GameManager.Instance.GetFlag(GameFlag.hasUpgradedSword);
        hasTeleport = GameManager.Instance.GetFlag(GameFlag.hasTeleport);
        hasWaveOfFire = GameManager.Instance.GetFlag(GameFlag.hasWaveOfFire);

        if (hasUpgradedSword)
            Debug.Log("✅ Abilities Start: Loaded hasUpgradedSword = true from save");
    }

    private void Update()
    {
        // Teleport ability (ALT key)
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if (hasTeleport)
            {
                TryTeleport();
            }
            else
            {
                Debug.Log("Teleport ability not unlocked yet!");
            }
        }
    }

    private void TryTeleport()
    {
        if (controller == null)
        {
            Debug.LogWarning("⚠️ PlayerController not found!");
            return;
        }

        Vector2 dir = controller.facingDir();
        Vector2 newPos = (Vector2)transform.position + dir * teleportDistance;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, teleportDistance, groundLayer);

        if (hit.collider == null)
        {
            transform.position = newPos;

            // ✅ FIXED: Use Invulnerability component instead of PlayerController
            if (invulnerability != null)
            {
                invulnerability.Trigger();
            }

            Debug.Log("✨ Teleported!");
        }
        else
        {
            Debug.Log("⚠️ Can't teleport - obstacle in the way!");
        }
    }

    #region Unlock Methods (Called by StoreController)

    /// <summary>
    /// Unlock teleport ability
    /// Called by StoreController when purchasing Flash Helmet
    /// </summary>
    public void UnlockTeleport()
    {
        hasTeleport = true;
        Debug.Log("✨ Teleport ability unlocked!");
    }

    /// <summary>
    /// Unlock Wave of Fire ability
    /// Called by StoreController when purchasing Sword of Fire
    /// </summary>
    public void UnlockWaveOfFire()
    {
        hasWaveOfFire = true;
        Debug.Log("✨ Wave of Fire ability unlocked!");
    }

    /// <summary>
    /// Upgrade sword (Yoji's special upgrade after George first encounter)
    /// This allows player to damage George
    /// </summary>
    public void UpgradeSword()
    {
        hasUpgradedSword = true;

        // ✅ Save immediately so it persists across scene loads!
        PlayerPrefs.SetInt("PlayerHasUpgradedSword", 1);
        PlayerPrefs.Save();

        Debug.Log("⚔️ Sword upgraded by Yoji! Can now damage George!");
    }

    #endregion
}