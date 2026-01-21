using UnityEngine;

/// <summary>
/// Boss Music Controller - Handles boss-specific music
/// Attach this to each boss prefab
/// </summary>
public class BossMusicController : MonoBehaviour
{
    [Header("Boss Music")]
    [Tooltip("Which boss music to play?")]
    public BossType bossType = BossType.George;

    [Header("Custom Music (Optional)")]
    [Tooltip("Use a custom AudioClip instead")]
    public AudioClip customBossMusic;

    [Header("Settings")]
    [Tooltip("Play boss roar sound when spawning?")]
    public bool playRoarOnSpawn = true;

    public enum BossType
    {
        George,
        Fika,
        Philip,
        Ditor,
        YojiDeath
    }

    private void Start()
    {
        PlayBossMusic();

        if (playRoarOnSpawn && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossRoar();
        }
    }

    private void PlayBossMusic()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found!");
            return;
        }

        // Use custom music if assigned
        if (customBossMusic != null)
        {
            AudioManager.Instance.PlayBossMusic(customBossMusic);
            return;
        }

        // Otherwise use predefined boss music
        AudioClip musicToPlay = null;

        switch (bossType)
        {
            case BossType.George:
                musicToPlay = AudioManager.Instance.georgeMusic;
                break;
            case BossType.Fika:
                musicToPlay = AudioManager.Instance.fikaMusic;
                break;
            case BossType.Philip:
                musicToPlay = AudioManager.Instance.philipMusic;
                break;
            case BossType.Ditor:
                musicToPlay = AudioManager.Instance.ditorMusic;
                break;
            case BossType.YojiDeath:
                musicToPlay = AudioManager.Instance.yojiDeathMusic;
                break;
        }

        if (musicToPlay != null)
        {
            AudioManager.Instance.PlayBossMusic(musicToPlay);
            Debug.Log($"🎵 Playing boss music: {bossType}");
        }
        else
        {
            Debug.LogWarning($"Boss music for {bossType} not assigned in AudioManager!");
        }
    }

    private void OnDestroy()
    {
        // Only return to scene music if the boss was actually defeated in normal gameplay
        // DON'T return to scene music if we're transitioning to an ending scene

        if (AudioManager.Instance != null && GetComponent<BossBase>() != null)
        {
            BossBase boss = GetComponent<BossBase>();

            // Only return to scene music if:
            // 1. Boss is dead (HP <= 0)
            // 2. We're NOT transitioning to an ending (checked via a flag in AudioManager)
            if (boss.CurrentHP <= 0 && !AudioManager.Instance.isTransitioningToEnding)
            {
                AudioManager.Instance.ReturnToSceneMusic();
                Debug.Log("Boss defeated - returning to scene music");
            }
        }
    }
}