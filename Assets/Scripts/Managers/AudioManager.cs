using UnityEngine;
using System.Collections;

/// <summary>
/// Audio Manager - Handles all music and sound effects
/// Persists across scenes with DontDestroyOnLoad
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip forestHubMusic;      // Forest Hub + Tutorial
    public AudioClip greenForestMusic;    // Green Forest battle
    public AudioClip redForestMusic;      // Red Forest battle
    public AudioClip darkForestMusic;     // Dark Forest
    public AudioClip epilogueMusic;       // Epilogue

    [Header("Boss Music")]
    public AudioClip georgeMusic;
    public AudioClip fikaMusic;
    public AudioClip philipMusic;
    public AudioClip ditorMusic;
    public AudioClip yojiDeathMusic;

    [Header("Player Sound Effects")]
    public AudioClip playerJump;
    public AudioClip playerAttack;
    public AudioClip playerTakeDamage;
    public AudioClip playerDeath;
    public AudioClip playerHeal;

    [Header("Enemy Sound Effects")]
    public AudioClip enemyHit;
    public AudioClip enemyDeath;

    [Header("Boss Sound Effects")]
    public AudioClip bossHit;
    public AudioClip bossDeath;
    public AudioClip bossRoar;

    [Header("UI Sound Effects")]
    public AudioClip coinPickup;
    public AudioClip itemPurchase;
    public AudioClip portalOpen;
    public AudioClip dialogueAdvance;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public float fadeDuration = 1.5f;

    private AudioClip currentMusic;
    private AudioClip musicBeforeBoss;
    private bool isFading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.parent = transform;
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.parent = transform;
                sfxSource = sfxObj.AddComponent<AudioSource>();
            }

            musicSource.volume = musicVolume;
            sfxSource.volume = sfxVolume;

            Debug.Log("✅ AudioManager initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip clip, bool forceRestart = false)
    {
        if (clip == null) return;

        if (currentMusic == clip && musicSource.isPlaying && !forceRestart)
        {
            Debug.Log($"🎵 {clip.name} already playing");
            return;
        }

        StartCoroutine(CrossfadeMusic(clip));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        if (isFading) yield break;
        isFading = true;

        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            for (float t = 0; t < fadeDuration / 2; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / (fadeDuration / 2));
                yield return null;
            }
            musicSource.volume = 0;
            musicSource.Stop();
        }

        currentMusic = newClip;
        musicSource.clip = newClip;
        musicSource.Play();

        for (float t = 0; t < fadeDuration / 2; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, musicVolume, t / (fadeDuration / 2));
            yield return null;
        }
        musicSource.volume = musicVolume;

        isFading = false;
        Debug.Log($"🎵 Now playing: {newClip.name}");
    }

    public void PlayBossMusic(AudioClip bossClip)
    {
        if (currentMusic != bossClip)
        {
            musicBeforeBoss = currentMusic;
        }
        PlayMusic(bossClip, true);
    }

    public void ReturnToSceneMusic()
    {
        if (musicBeforeBoss != null)
        {
            PlayMusic(musicBeforeBoss);
            musicBeforeBoss = null;
        }
    }

    /// <summary>
    /// Force stop boss music and clear stored music
    /// Use when player dies or scene needs to fully reset music
    /// </summary>
    public void ForceStopBossMusic()
    {
        musicBeforeBoss = null;
        currentMusic = null;
    }

    public void StopMusic()
    {
        StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = musicVolume;
        currentMusic = null;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
        }
    }

    // Easy access methods
    public void PlayPlayerJump() => PlaySFX(playerJump);
    public void PlayPlayerAttack() => PlaySFX(playerAttack);
    public void PlayPlayerTakeDamage() => PlaySFX(playerTakeDamage);
    public void PlayPlayerDeath() => PlaySFX(playerDeath);
    public void PlayPlayerHeal() => PlaySFX(playerHeal);
    public void PlayEnemyHit() => PlaySFX(enemyHit);
    public void PlayEnemyDeath() => PlaySFX(enemyDeath);
    public void PlayBossHit() => PlaySFX(bossHit);
    public void PlayBossDeath() => PlaySFX(bossDeath);
    public void PlayBossRoar() => PlaySFX(bossRoar);
    public void PlayCoinPickup() => PlaySFX(coinPickup);
    public void PlayItemPurchase() => PlaySFX(itemPurchase);
    public void PlayPortalOpen() => PlaySFX(portalOpen);
    public void PlayDialogueAdvance() => PlaySFX(dialogueAdvance);

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
}