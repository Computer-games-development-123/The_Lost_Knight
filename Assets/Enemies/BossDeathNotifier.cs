using UnityEngine;

public class BossDeathNotifier : MonoBehaviour
{
    public WaveManager waveManager;

    private void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnBossDied();
        }
    }
}
