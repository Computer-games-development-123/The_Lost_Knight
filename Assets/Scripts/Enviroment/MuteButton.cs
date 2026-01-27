using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple Mute/Unmute button - Mutes/unmutes ALL audio (music + SFX)
/// Attach to a UI Button and assign muted/unmuted sprites
/// </summary>
[RequireComponent(typeof(Button))]
public class MuteButton : MonoBehaviour
{
    [Header("Button Sprites")]
    [Tooltip("Icon to show when audio is ON (speaker with sound waves 🔊)")]
    public Sprite unmutedSprite;

    [Tooltip("Icon to show when audio is OFF (speaker with X 🔇)")]
    public Sprite mutedSprite;

    private Button button;
    private Image buttonImage;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        // Add click listener
        button.onClick.AddListener(OnMuteButtonClicked);
    }

    private void Start()
    {
        // Update button appearance based on current mute state
        UpdateButtonAppearance();
    }

    private void OnMuteButtonClicked()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found! Cannot toggle mute.");
            return;
        }

        // Toggle mute for everything
        AudioManager.Instance.ToggleAllMute();

        // Update button appearance
        UpdateButtonAppearance();
    }

    private void UpdateButtonAppearance()
    {
        if (AudioManager.Instance == null || buttonImage == null) return;

        bool isMuted = AudioManager.Instance.IsAllMuted();

        // Update sprite based on mute state
        if (isMuted && mutedSprite != null)
        {
            buttonImage.sprite = mutedSprite;
        }
        else if (!isMuted && unmutedSprite != null)
        {
            buttonImage.sprite = unmutedSprite;
        }
    }

    // Update appearance when button becomes visible (in case mute state changed)
    private void OnEnable()
    {
        UpdateButtonAppearance();
    }
}
