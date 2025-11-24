using UnityEngine;

public class EnemyDeathNotifier : MonoBehaviour
{
    public WaveManager manager;

    void OnDestroy()
    {
        if (manager != null)
        {
            manager.OnEnemyDied();
        }
    }
}
