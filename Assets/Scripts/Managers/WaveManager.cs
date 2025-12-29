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

    [Header("Portals (Direct References)")]
    [Tooltip("Portal back to hub - leave empty to find by name")]
    public string portalBackToHubName = "Forest_Hub_Portal";
    [Tooltip("Portal to next area - leave empty to find by name")]
    public string portalToNextAreaName = "GreenToRed_Portal";

    [Header("Dialogues")]
    public DialogueData beforeWaveDialogue;
    public DialogueData afterWaveDialogue;
    public DialogueData bossAlreadyDefeatedDialogue;

    [Header("UI References")]
    public GameObject waveCompleteUI;
    public TMPro.TextMeshProUGUI waveText;

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
        ResetWaveManager();

        if (waveCompleteUI != null)
            waveCompleteUI.SetActive(false);

        StartCoroutine(StartNextWave());
    }

    void OnDestroy()
    {
        spawnedEnemies.Clear();
    }

    private void ResetWaveManager()
    {
        currentWaveIndex = 0;
        enemiesAlive = 0;
        waveInProgress = false;
        bossSpawned = false;
        allWavesComplete = false;
        spawnedEnemies.Clear();

        if (showDebugLogs) Debug.Log("🔄 WaveManager reset");
    }

    IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            allWavesComplete = true;
            if (showDebugLogs) Debug.Log("✅ All waves complete!");

            yield return new WaitForSeconds(2f);

            SpawnBoss();
            yield break;
        }

        waveInProgress = true;
        Wave currentWave = waves[currentWaveIndex];

        if (waveText != null)
            waveText.text = $"Wave {currentWaveIndex + 1}: {currentWave.waveName}";

        if (showDebugLogs) Debug.Log($"🌊 Wave {currentWaveIndex + 1}: {currentWave.waveName}");

        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            if (currentWave.spawnPoints.Length == 0)
            {
                Debug.LogError("❌ No spawn points!");
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
            if (showDebugLogs) Debug.Log($"⚠️ Wave {currentWaveIndex + 1} completed during spawn");
            currentWaveIndex++;

            if (waveCompleteUI != null)
            {
                waveCompleteUI.SetActive(true);
                StartCoroutine(HideWaveCompleteUI());
            }

            StartCoroutine(StartNextWave());
            yield break;
        }

        if (showDebugLogs) Debug.Log($"Wave {currentWaveIndex + 1} spawned. Enemies: {enemiesAlive}");
    }

    public void OnEnemyDied(EnemyBase enemy)
    {
        if (!spawnedEnemies.Contains(enemy))
        {
            if (showDebugLogs) Debug.LogWarning("⚠️ Unknown enemy - ignoring");
            return;
        }

        spawnedEnemies.Remove(enemy);
        enemiesAlive--;

        if (showDebugLogs) Debug.Log($"💀 Enemy died. Remaining: {enemiesAlive}");

        if (enemiesAlive < 0)
        {
            Debug.LogError($"❌ Enemy counter negative!");
            enemiesAlive = 0;
        }

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
        if (!allWavesComplete)
        {
            Debug.LogWarning("⚠️ SpawnBoss called but waves not complete!");
            return;
        }

        // Check if boss already defeated
        if (IsBossAlreadyDefeated())
        {
            if (showDebugLogs) Debug.Log("✅ Boss already defeated - spawning portals");
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

                if (showDebugLogs) Debug.Log($"✅ Boss spawned: {bossPrefab.name}");
            }
        }
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
            return GameManager.Instance.GetFlag(GameFlag.PhillipDefeated);
        }

        return false;
    }

    /// <summary>
    /// Spawns portals directly - called when boss already defeated
    /// </summary>
    private void SpawnPortals()
    {
        if (showDebugLogs) Debug.Log("🌀 Spawning portals for cleared area...");

        GameObject portalBack = FindObjectInHierarchy(portalBackToHubName);
        GameObject portalNext = FindObjectInHierarchy(portalToNextAreaName);

        if (portalBack != null)
        {
            portalBack.SetActive(true);
            if (showDebugLogs) Debug.Log($"✅ Portal activated: {portalBackToHubName}");
        }
        else if (!string.IsNullOrEmpty(portalBackToHubName))
        {
            Debug.LogWarning($"⚠️ Portal not found: {portalBackToHubName}");
        }

        if (portalNext != null)
        {
            portalNext.SetActive(true);
            if (showDebugLogs) Debug.Log($"✅ Portal activated: {portalToNextAreaName}");
        }
        else if (!string.IsNullOrEmpty(portalToNextAreaName))
        {
            Debug.LogWarning($"⚠️ Portal not found: {portalToNextAreaName}");
        }
    }

    /// <summary>
    /// Finds GameObject anywhere in hierarchy
    /// </summary>
    private GameObject FindObjectInHierarchy(string objectName)
    {
        if (string.IsNullOrEmpty(objectName)) return null;

        GameObject obj = GameObject.Find(objectName);
        if (obj != null) return obj;

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.hideFlags == HideFlags.None && go.scene.isLoaded)
            {
                if (go.name == objectName)
                {
                    return go;
                }
            }
        }

        return null;
    }

    public void OnBossDied(BossBase boss)
    {
        if (showDebugLogs) Debug.Log($"{boss.bossName} defeated!");
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