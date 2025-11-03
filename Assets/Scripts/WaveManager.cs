using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int enemyCount = 3;
        public float spawnInterval = 1f;
    }

    public Wave[] waves;                  // define in Inspector
    public Transform[] spawnPoints;       // where to spawn
    public GameObject enemyPrefab;        // what to spawn

    private int currentWave = 0;
    private int enemiesAlive = 0;

    void Start()
    {
        if (waves.Length > 0)
        {
            StartCoroutine(RunWave(currentWave));
        }
        else
        {
            Debug.LogWarning("No waves defined on WaveManager!");
        }
    }

    IEnumerator RunWave(int index)
    {
        Wave w = waves[index];

        for (int i = 0; i < w.enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(w.spawnInterval);
        }

        // wait until all enemies from this wave are dead
        while (enemiesAlive > 0)
        {
            yield return null;
        }

        // go to next wave
        currentWave++;
        if (currentWave < waves.Length)
        {
            StartCoroutine(RunWave(currentWave));
        }
        else
        {
            Debug.Log("✅ All waves cleared!");
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("No enemy prefab set on WaveManager!");
            return;
        }

        // pick random spawn point
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject e = Instantiate(enemyPrefab, sp.position, Quaternion.identity);
        enemiesAlive++;

        // make sure enemy reports back when it dies
        EnemyDeathNotifier notifier = e.AddComponent<EnemyDeathNotifier>();
        notifier.manager = this;
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;
    }
}
