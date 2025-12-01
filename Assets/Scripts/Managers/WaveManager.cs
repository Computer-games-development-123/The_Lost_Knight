using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int enemyCount;
        public float spawnDelay = 1f;
        public Transform[] spawnPoints;
    }

    [Header("Wave Settings")]
    public List<Wave> waves = new List<Wave>();
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public string nextSceneName = "ForestHub"; // Scene to load after boss

    [Header("UI References")]
    public GameObject waveCompleteUI;
    public TMPro.TextMeshProUGUI waveText;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;
    private bool bossSpawned = false;

    void Start()
    {
        if (waveCompleteUI != null)
            waveCompleteUI.SetActive(false);

        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            // All waves complete, spawn boss
            yield return new WaitForSeconds(2f);
            SpawnBoss();
            yield break;
        }

        waveInProgress = true;
        Wave currentWave = waves[currentWaveIndex];

        if (waveText != null)
            waveText.text = $"Wave {currentWaveIndex + 1}: {currentWave.waveName}";

        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            if (currentWave.spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned for wave!");
                yield break;
            }

            Transform spawnPoint = currentWave.spawnPoints[Random.Range(0, currentWave.spawnPoints.Length)];
            GameObject enemy = Instantiate(currentWave.enemyPrefab, spawnPoint.position, Quaternion.identity);

            // Register this WaveManager with the enemy
            EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
            if (enemyScript != null)
            {
                enemyScript.waveManager = this;
            }

            enemiesAlive++;
            yield return new WaitForSeconds(currentWave.spawnDelay);
        }

        waveInProgress = false;
    }

    // FIXED METHOD - Called by EnemyBase when enemy dies
    public void OnEnemyDied(EnemyBase enemy)
    {
        enemiesAlive--;
        Debug.Log($"Enemy died. Remaining: {enemiesAlive}");

        if (enemiesAlive <= 0 && !waveInProgress && !bossSpawned)
        {
            currentWaveIndex++;

            if (waveCompleteUI != null)
            {
                waveCompleteUI.SetActive(true);
                StartCoroutine(HideWaveCompleteUI());
            }

            StartCoroutine(StartNextWave());
        }
    }

    IEnumerator HideWaveCompleteUI()
    {
        yield return new WaitForSeconds(2f);
        if (waveCompleteUI != null)
            waveCompleteUI.SetActive(false);
    }

    void SpawnBoss()
    {
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            bossSpawned = true;
            GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

            // Register this WaveManager with the boss
            BossBase bossScript = boss.GetComponent<BossBase>();
            if (bossScript != null)
            {
                bossScript.waveManager = this;
            }

            Debug.Log("Boss spawned!");
        }
        else
        {
            Debug.LogWarning("Boss prefab or spawn point not assigned!");
        }
    }

    // FIXED METHOD - Called by BossBase when boss dies
    public void OnBossDied(BossBase boss)
    {
        Debug.Log("Boss defeated! Loading next scene...");
        StartCoroutine(LoadNextSceneAfterDelay(3f));
    }

    IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name not set!");
        }
    }
}