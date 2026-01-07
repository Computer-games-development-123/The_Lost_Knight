using UnityEngine;

/// <summary>
/// Scene Music Trigger - Automatically plays music when scene loads
/// Attach to an empty GameObject in each scene
/// </summary>
public class SceneMusicTrigger : MonoBehaviour
{
    [Header("Scene Music")]
    [Tooltip("Which music should play in this scene?")]
    public SceneMusic sceneMusic = SceneMusic.ForestHub;

    [Header("Custom Music (Optional)")]
    [Tooltip("Use a custom AudioClip instead of predefined music")]
    public AudioClip customMusic;

    [Header("Settings")]
    [Tooltip("Play music immediately on scene load?")]
    public bool playOnStart = true;

    public enum SceneMusic
    {
        ForestHub,      // Forest Hub + Tutorial
        GreenForest,    // Green Forest battle
        RedForest,      // Red Forest battle
        DarkForest,     // Dark Forest
        Epilogue,       // Epilogue
        None            // No music
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlaySceneMusic();
        }
    }

    public void PlaySceneMusic()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found! Make sure AudioManager exists in the scene.");
            return;
        }

        // Use custom music if assigned
        if (customMusic != null)
        {
            AudioManager.Instance.PlayMusic(customMusic);
            return;
        }

        // Otherwise use predefined music
        AudioClip musicToPlay = null;

        switch (sceneMusic)
        {
            case SceneMusic.ForestHub:
                musicToPlay = AudioManager.Instance.forestHubMusic;
                break;
            case SceneMusic.GreenForest:
                musicToPlay = AudioManager.Instance.greenForestMusic;
                break;
            case SceneMusic.RedForest:
                musicToPlay = AudioManager.Instance.redForestMusic;
                break;
            case SceneMusic.DarkForest:
                musicToPlay = AudioManager.Instance.darkForestMusic;
                break;
            case SceneMusic.Epilogue:
                musicToPlay = AudioManager.Instance.epilogueMusic;
                break;
            case SceneMusic.None:
                AudioManager.Instance.StopMusic();
                return;
        }

        if (musicToPlay != null)
        {
            AudioManager.Instance.PlayMusic(musicToPlay);
        }
        else
        {
            Debug.LogWarning($"Music clip for {sceneMusic} not assigned in AudioManager!");
        }
    }
}