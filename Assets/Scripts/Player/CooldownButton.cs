using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a cooldown overlay on a UI button.
/// Reads cooldown state from PlayerAttack (Fireball, BreathOfFire) or Abilities (Teleport).
/// Hides button visuals when ability is locked WITHOUT disabling the GameObject,
/// so the script can re-show the button when the ability is unlocked mid-game.
/// </summary>
public class CooldownButton : MonoBehaviour
{
    public enum AbilityType { Fireball, BreathOfFire, Teleport }

    [Header("Settings")]
    [SerializeField] private AbilityType ability;

    [Header("UI References")]
    [Tooltip("The dark overlay image that shrinks as cooldown progresses")]
    [SerializeField] private Image cooldownOverlay;

    [Tooltip("Optional text showing cooldown seconds remaining")]
    [SerializeField] private TMPro.TextMeshProUGUI cooldownText;

    [Header("Lock When Unowned")]
    [Tooltip("Hide button visuals when ability isn't unlocked yet")]
    [SerializeField] private bool hideWhenLocked = true;

    private Button button;
    private CanvasGroup canvasGroup;
    private PlayerAttack playerAttack;
    private Abilities abilities;

    private void Awake()
    {
        button = GetComponent<Button>();

        // Auto-add CanvasGroup so we can hide/show without disabling the GameObject.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (cooldownOverlay != null)
        {
            cooldownOverlay.type = Image.Type.Filled;
            cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.raycastTarget = false;
        }
    }

    private void Start()
    {
        FindPlayerReferences();
    }

    private void FindPlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            playerAttack = FindFirstObjectByType<PlayerAttack>();
            abilities = FindFirstObjectByType<Abilities>();
        }
        else
        {
            playerAttack = player.GetComponent<PlayerAttack>();
            abilities = player.GetComponent<Abilities>();
        }
    }

    private void Update()
    {
        // Re-find player if reference was lost (e.g. after a scene transition).
        if (playerAttack == null || abilities == null)
        {
            FindPlayerReferences();
            return;
        }

        bool unlocked = IsAbilityUnlocked();

        if (hideWhenLocked && !unlocked)
        {
            // Hide via CanvasGroup so this Update() keeps running.
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            return;
        }
        else
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        UpdateCooldownDisplay();
    }

    private bool IsAbilityUnlocked()
    {
        if (abilities == null) return false;

        return ability switch
        {
            AbilityType.Fireball => abilities.hasFireballSpell,
            AbilityType.BreathOfFire => abilities.hasBreathOfFire,
            AbilityType.Teleport => abilities.hasTeleport,
            _ => false,
        };
    }

    private void UpdateCooldownDisplay()
    {
        float lastUseTime = 0f;
        float cooldownDuration = 1f;

        switch (ability)
        {
            case AbilityType.Fireball:
                lastUseTime = playerAttack.LastFireballTime;
                cooldownDuration = playerAttack.fireballCooldown;
                break;
            case AbilityType.BreathOfFire:
                lastUseTime = playerAttack.LastBreathOfFireTime;
                cooldownDuration = playerAttack.breathOfFireCooldown;
                break;
            case AbilityType.Teleport:
                lastUseTime = abilities.LastTeleportTime;
                cooldownDuration = abilities.teleportCooldown;
                break;
        }

        float timeSinceUse = Time.time - lastUseTime;
        float remaining = cooldownDuration - timeSinceUse;

        if (remaining > 0f && lastUseTime > 0f)
        {
            // On cooldown
            float fillAmount = remaining / cooldownDuration;

            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = fillAmount;

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = remaining.ToString("F1");
            }

            canvasGroup.interactable = false;
        }
        else
        {
            // Ready
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 0f;

            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);

            canvasGroup.interactable = true;
        }
    }
}