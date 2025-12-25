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
    public string nextSceneName = "ForestHub";

    [Header("Portal Spawner")]
    public PostBossPortalSpawner portalSpawner;

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
    
    // Track spawned enemies to prevent counter corruption
    private HashSet<EnemyBase> spawnedEnemies = new HashSet<EnemyBase>();

    void Start()
    {
        // CRITICAL: Reset all state on scene load
        ResetWaveManager();
        
        if (waveCompleteUI != null)
            waveCompleteUI.SetActive(false);

        StartCoroutine(StartNextWave());
    }

    void OnDestroy()
    {
        // Clean up enemy references when WaveManager is destroyed
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
        
        if (showDebugLogs) Debug.Log("🔄 WaveManager reset for new scene load");
    }

    IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            // All waves complete
            allWavesComplete = true;
            if (showDebugLogs) Debug.Log("✅ All waves complete!");
            
            yield return new WaitForSeconds(2f);
            
            // Check if boss should spawn or if already defeated
            SpawnBoss();
            yield break;
        }

        waveInProgress = true;
        Wave currentWave = waves[currentWaveIndex];

        if (waveText != null)
            waveText.text = $"Wave {currentWaveIndex + 1}: {currentWave.waveName}";

        if (showDebugLogs) Debug.Log($"🌊 Starting Wave {currentWaveIndex + 1}: {currentWave.waveName}");

        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            if (currentWave.spawnPoints.Length == 0)
            {
                Debug.LogError("❌ No spawn points assigned for wave!");
                yield break;
            }

            Transform spawnPoint = currentWave.spawnPoints[Random.Range(0, currentWave.spawnPoints.Length)];
            GameObject enemyObj = Instantiate(currentWave.enemyPrefab, spawnPoint.position, Quaternion.identity);

            EnemyBase enemyScript = enemyObj.GetComponent<EnemyBase>();
            if (enemyScript != null)
            {
                enemyScript.waveManager = this;
                spawnedEnemies.Add(enemyScript); // Track this enemy
            }

            enemiesAlive++;
            yield return new WaitForSeconds(currentWave.spawnDelay);
        }

        waveInProgress = false;
        
        // ✅ FIX: Check if all enemies died during spawn
        if (enemiesAlive <= 0)
        {
            if (showDebugLogs) Debug.Log($"⚠️ Wave {currentWaveIndex + 1} completed during spawn - moving to next wave");
            currentWaveIndex++;
            
            if (waveCompleteUI != null)
            {
                waveCompleteUI.SetActive(true);
                StartCoroutine(HideWaveCompleteUI());
            }
            
            StartCoroutine(StartNextWave());
            yield break;
        }
        
        if (showDebugLogs) Debug.Log($"Wave {currentWaveIndex + 1} spawned. Enemies alive: {enemiesAlive}");
    }

    public void OnEnemyDied(EnemyBase enemy)
    {
        // CRITICAL: Only count enemies we actually spawned
        if (!spawnedEnemies.Contains(enemy))
        {
            if (showDebugLogs) Debug.LogWarning("⚠️ Unknown enemy tried to report death - ignoring");
            return;
        }

        spawnedEnemies.Remove(enemy);
        enemiesAlive--;
        
        if (showDebugLogs) Debug.Log($"💀 Enemy died. Remaining: {enemiesAlive}");

        // Sanity check: enemiesAlive should never go negative
        if (enemiesAlive < 0)
        {
            Debug.LogError($"❌ Enemy counter went negative! Resetting to 0. This is a bug!");
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
        // Only check if boss defeated AFTER all waves complete
        if (!allWavesComplete)
        {
            Debug.LogWarning("⚠️ SpawnBoss called but waves not complete yet!");
            return;
        }

        // Check if boss was already defeated
        if (IsBossAlreadyDefeated())
        {
            if (showDebugLogs) Debug.Log("Boss already defeated - spawning portal");
            HandleBossAlreadyDefeated();
            return;
        }
        
        // Boss not defeated yet - spawn normally
        if (showDebugLogs) Debug.Log("Boss not defeated yet - spawning boss for fight");
        
        // Check if this scene has a cutscene manager (Fika fight)
        FikaBossCutsceneManager cutsceneManager = FindFirstObjectByType<FikaBossCutsceneManager>();
        
        if (cutsceneManager != null)
        {
            cutsceneManager.TriggerBossCutscene();
            bossSpawned = true;
            if (showDebugLogs) Debug.Log("Boss cutscene triggered!");
        }
        else
        {
            // Normal boss spawn (George or Philip)
            if (bossPrefab != null && bossSpawnPoint != null)
            {
                bossSpawned = true;
                GameObject bossObj = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

                BossBase bossScript = bossObj.GetComponent<BossBase>();
                if (bossScript != null)
                {
                    bossScript.waveManager = this;
                }

                // All bosses find their own portal spawner
                if (showDebugLogs) Debug.Log($"✅ Boss spawned: {bossPrefab.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Boss prefab or spawn point not assigned!");
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
            if (GameManager.Instance.act1Cleared)
            {
                if (showDebugLogs) Debug.Log("⚠️ George already defeated (Act 1 cleared)");
                return true;
            }
        }

        if (bossName.Contains("fika"))
        {
            if (GameManager.Instance.act2Cleared)
            {
                if (showDebugLogs) Debug.Log("⚠️ Fika already defeated (Act 2 cleared)");
                return true;
            }
        }

        if (bossName.Contains("philip"))
        {
            if (GameManager.Instance.act3Cleared)
            {
                if (showDebugLogs) Debug.Log("⚠️ Philip already defeated (Act 3 cleared)");
                return true;
            }
        }

        return false;
    }

    private void HandleBossAlreadyDefeated()
    {
        if (showDebugLogs) Debug.Log("Boss already defeated - handling appropriately");

        if (DialogueManager.Instance != null && bossAlreadyDefeatedDialogue != null)
        {
            DialogueManager.Instance.Play(bossAlreadyDefeatedDialogue, () =>
            {
                SpawnPortalForClearedArea();
            });
        }
        else
        {
            SpawnPortalForClearedArea();
        }
    }

    private void SpawnPortalForClearedArea()
    {
        if (portalSpawner != null)
        {
            portalSpawner.SpawnPortal();
            if (showDebugLogs) Debug.Log("✅ Portal spawned for already-cleared area");
        }
        else
        {
            Debug.LogWarning("⚠️ No portal spawner - player might be stuck!");
            StartCoroutine(LoadNextSceneAfterDelay(3f));
        }
    }

    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            if (showDebugLogs) Debug.Log($"Loading {nextSceneName} as fallback...");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void OnBossDied(BossBase boss)
    {
        if (showDebugLogs) Debug.Log($"{boss.bossName} defeated!");
    }

    // Debug method to manually check state
    [ContextMenu("Debug Wave State")]
    private void DebugWaveState()
    {
        Debug.Log($"=== WAVE MANAGER DEBUG ===");
        Debug.Log($"Current Wave: {currentWaveIndex + 1}/{waves.Count}");
        Debug.Log($"Enemies Alive: {enemiesAlive}");
        Debug.Log($"Tracked Enemies: {spawnedEnemies.Count}");
        Debug.Log($"Wave In Progress: {waveInProgress}");
        Debug.Log($"Boss Spawned: {bossSpawned}");
        Debug.Log($"All Waves Complete: {allWavesComplete}");
        Debug.Log($"========================");
    }
}