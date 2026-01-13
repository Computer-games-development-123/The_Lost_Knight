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

    [Header("Portals")]
    [Tooltip("Portal back")]
    public GameObject portalBack;
    [Tooltip("Portal to next area")]
    public GameObject portalToNextArea;

    [Header("UI References")]
    public GameObject waveUI;
    public TMPro.TextMeshProUGUI waveText;
    public GameObject BossHealthBar;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;
    private bool bossSpawned = false;
    private bool allWavesComplete = false;

    private HashSet<EnemyBase> spawnedEnemies = new HashSet<EnemyBase>();

    void Start()
    {
        if (IsBossAlreadyDefeated())
        {
            SpawnPortals();
            return;
        }
        closeBackPortal();
        ResetWaveManager();

        if (waveUI != null)
            waveUI.SetActive(false);

        StartCoroutine(StartNextWave());
    }

    void OnDestroy()
    {
        spawnedEnemies.Clear();
    }

    void closeBackPortal()
    {
        portalBack.GetComponent<PortalSpawnEffect>().Close();
    }

    private void ResetWaveManager()
    {
        currentWaveIndex = 0;
        enemiesAlive = 0;
        waveInProgress = false;
        bossSpawned = false;
        allWavesComplete = false;
        spawnedEnemies.Clear();

        if (showDebugLogs) Debug.Log("WaveManager reset");
    }

    IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            allWavesComplete = true;
            if (showDebugLogs) Debug.Log("All waves complete!");

            yield return new WaitForSeconds(2f);

            SpawnBoss();
            yield break;
        }

        waveInProgress = true;
        Wave currentWave = waves[currentWaveIndex];

        if (waveText != null)
            waveText.text = $"Wave {currentWaveIndex + 1}: {currentWave.waveName}";

        if (waveUI != null)
        {
            waveUI.SetActive(true);
            StartCoroutine(HideWaveCompleteUI());
        }

        if (showDebugLogs) Debug.Log($"Wave {currentWaveIndex + 1}: {currentWave.waveName}");

        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            if (currentWave.spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points!");
                yield break;
            }

            Transform spawnPoint = currentWave.spawnPoints[Random.Range(0, currentWave.spawnPoints.Length)];
            GameObject enemyObj = Instantiate(currentWave.enemyPrefab, spawnPoint.position, Quaternion.identity);

            EnemyBase enemyScript = enemyObj.GetComponent<EnemyBase>();
            if (enemyScript != null)
            {
                enemyScript.waveManager = this;
                spawnedEnemies.Add(enemyScript);
            }

            enemiesAlive++;
            yield return new WaitForSeconds(currentWave.spawnDelay);
        }

        waveInProgress = false;

        if (enemiesAlive <= 0)
        {
            if (showDebugLogs) Debug.Log($"Wave {currentWaveIndex + 1} completed during spawn");
            currentWaveIndex++;

            StartCoroutine(StartNextWave());
            yield break;
        }

        if (showDebugLogs) Debug.Log($"Wave {currentWaveIndex + 1} spawned. Enemies: {enemiesAlive}");
    }

    public void OnEnemyDied(EnemyBase enemy)
    {
        if (!spawnedEnemies.Contains(enemy))
        {
            if (showDebugLogs) Debug.LogWarning("Unknown enemy - ignoring");
            return;
        }

        spawnedEnemies.Remove(enemy);
        enemiesAlive--;

        if (showDebugLogs) Debug.Log($"Enemy died. Remaining: {enemiesAlive}");

        if (enemiesAlive < 0)
        {
            Debug.LogError($"Enemy counter negative!");
            enemiesAlive = 0;
        }

        if (enemiesAlive <= 0 && !waveInProgress && !bossSpawned)
        {
            currentWaveIndex++;
            StartCoroutine(StartNextWave());
        }
    }

    IEnumerator HideWaveCompleteUI()
    {
        yield return new WaitForSeconds(2f);
        if (waveUI != null)
            waveUI.SetActive(false);
    }

    void SpawnBoss()
    {
        if (!allWavesComplete)
        {
            Debug.LogWarning("SpawnBoss called but waves not complete!");
            return;
        }

        // Check if boss already defeated
        if (IsBossAlreadyDefeated())
        {
            // Boss already defeated - just spawn portals, no UI needed
            if (showDebugLogs) Debug.Log("Boss already defeated - spawning portals");
            SpawnPortals();
            return;
        }

        // Boss not defeated - spawn boss
        if (showDebugLogs) Debug.Log("Boss not defeated - spawning boss");

        // Check for cutscene (Fika)
        FikaBossCutsceneManager cutsceneManager = FindFirstObjectByType<FikaBossCutsceneManager>();

        if (cutsceneManager != null)
        {
            cutsceneManager.TriggerBossCutscene();
            bossSpawned = true;
        }
        else
        {
            // Normal boss spawn
            if (bossPrefab != null && bossSpawnPoint != null)
            {
                bossSpawned = true;
                GameObject bossObj = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

                BossBase bossScript = bossObj.GetComponent<BossBase>();
                if (bossScript != null)
                {
                    bossScript.waveManager = this;
                }

                if (showDebugLogs) Debug.Log($"Boss spawned: {bossPrefab.name}");
            }
        }
        if (cutsceneManager == null) BossHealthBar.SetActive(true);
    }

    private bool IsBossAlreadyDefeated()
    {
        if (GameManager.Instance == null || bossPrefab == null)
            return false;

        string bossName = bossPrefab.name.ToLower();

        if (bossName.Contains("george"))
        {
            return GameManager.Instance.GetFlag(GameFlag.GeorgeDefeated);
        }

        if (bossName.Contains("fika"))
        {
            return GameManager.Instance.GetFlag(GameFlag.FikaDefeated);
        }

        if (bossName.Contains("philip"))
        {
            return GameManager.Instance.GetFlag(GameFlag.PhilipDefeated);
        }

        return false;
    }

    /// <summary>
    /// Spawns portals directly - called when boss already defeated
    /// </summary>
    private void SpawnPortals()
    {
        if (showDebugLogs) Debug.Log("Spawning portals for cleared area...");

        if (portalBack != null)
        {
            portalBack.SetActive(true);
            if (showDebugLogs) Debug.Log($"Portal activated: {portalBack}");
        }
        else if (portalBack == null)
        {
            Debug.LogWarning($"Portal not found: {portalBack}");
        }

        if (portalToNextArea != null)
        {
            portalToNextArea.SetActive(true);
            if (showDebugLogs) Debug.Log($"Portal activated: {portalToNextArea}");
        }
        else
        {
            Debug.LogWarning($"Portal not found: {portalToNextArea}");
        }
    }

    public void OnBossDied(BossBase boss)
    {
        if (showDebugLogs) Debug.Log($"{boss.bossName} defeated!");
        BossHealthBar.SetActive(false);
        SpawnPortals();
    }

    [ContextMenu("Debug Wave State")]
    private void DebugWaveState()
    {
        Debug.Log($"=== WAVE MANAGER DEBUG ===");
        Debug.Log($"Current Wave: {currentWaveIndex + 1}/{waves.Count}");
        Debug.Log($"Enemies Alive: {enemiesAlive}");
        Debug.Log($"Boss Spawned: {bossSpawned}");
        Debug.Log($"========================");
    }
}