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
    public GameFlag flagAfterWin;

    [Header("Dialogues")]
    public DialogueData beforeWaveDialogue;
    public DialogueData afterWaveDialogue;
    public DialogueData bossAlreadyDefeatedDialogue;

    [Header("UI References")]
    public GameObject waveCompleteUI;
    public TMPro.TextMeshProUGUI waveText;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;
    private bool bossSpawned = false;
    private bool allWavesComplete = false;

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
            // All waves complete
            allWavesComplete = true;
            Debug.Log("✅ All waves complete!");

            yield return new WaitForSeconds(2f);

            // NOW check if boss should spawn or skip
            SpawnBoss();
            yield break;
        }

        waveInProgress = true;
        Wave currentWave = waves[currentWaveIndex];

        if (waveText != null)
            waveText.text = $"Wave {currentWaveIndex + 1}: {currentWave.waveName}";

        Debug.Log($"Starting Wave {currentWaveIndex + 1}: {currentWave.waveName}");

        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            if (currentWave.spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned for wave!");
                yield break;
            }

            Transform spawnPoint = currentWave.spawnPoints[Random.Range(0, currentWave.spawnPoints.Length)];
            GameObject enemy = Instantiate(currentWave.enemyPrefab, spawnPoint.position, Quaternion.identity);

            EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
            if (enemyScript != null)
            {
                enemyScript.waveManager = this;
            }

            enemiesAlive++;
            yield return new WaitForSeconds(currentWave.spawnDelay);
        }

        waveInProgress = false;
        Debug.Log($"Wave {currentWaveIndex + 1} spawned. Enemies alive: {enemiesAlive}");
    }

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
        // IMPORTANT: Only check if boss defeated AFTER all waves complete
        if (!allWavesComplete)
        {
            Debug.LogWarning("⚠️ SpawnBoss called but waves not complete yet!");
            return;
        }

        // Check if boss was already defeated
        if (IsBossAlreadyDefeated())
        {
            Debug.Log("Boss already defeated - spawning portal");
            HandleBossAlreadyDefeated();
            return;
        }

        // Boss not defeated yet - spawn normally
        Debug.Log("Boss not defeated yet - spawning boss for fight");

        // Check if this scene has a cutscene manager (Fika/Mona fight)
        // FikaBossCutsceneManager cutsceneManager = FindFirstObjectByType<FikaBossCutsceneManager>();

        // if (cutsceneManager != null)
        // {
        //     // Trigger cutscene (Fika fight)
        //     cutsceneManager.TriggerBossCutscene();
        //     bossSpawned = true;
        //     Debug.Log("Boss cutscene triggered!");
        // }
        // else
        // {
        //     // Normal boss spawn (George or Philip)
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            bossSpawned = true;
            GameObject bossObj = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

            BossBase bossScript = bossObj.GetComponent<BossBase>();
            if (bossScript != null)
            {
                bossScript.waveManager = this;
            }

            // NO BOSS-SPECIFIC CODE!
            // All bosses find their own portal spawner in Die() method

            Debug.Log($"Boss spawned: {bossPrefab.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ Boss prefab or spawn point not assigned!");
        }
        // }
    }

    private bool IsBossAlreadyDefeated()
    {
        if (GameManager.Instance == null || bossPrefab == null)
            return false;

        string bossName = bossPrefab.name.ToLower();

        // Check George
        if (bossName.Contains("george"))
        {
            if (GameManager.Instance.act1Cleared)
            {
                Debug.Log("⚠️ George already defeated (Act 1 cleared)");
                return true;
            }
        }

        // Check Fika
        if (bossName.Contains("fika"))
        {
            if (GameManager.Instance.act2Cleared)
            {
                Debug.Log("⚠️ Fika already defeated (Act 2 cleared)");
                return true;
            }
        }

        // Check Philip
        if (bossName.Contains("philip"))
        {
            if (GameManager.Instance.act3Cleared)
            {
                Debug.Log("⚠️ Philip already defeated (Act 3 cleared)");
                return true;
            }
        }

        return false;
    }

    private void HandleBossAlreadyDefeated()
    {
        Debug.Log("Boss already defeated - handling appropriately");

        // Option 1: Show dialogue (if assigned)
        if (DialogueManager.Instance != null && bossAlreadyDefeatedDialogue != null)
        {
            DialogueManager.Instance.Play(bossAlreadyDefeatedDialogue, () =>
            {
                SpawnPortalForClearedArea();
            });
        }
        else
        {
            // Option 2: Just spawn portal immediately
            SpawnPortalForClearedArea();
        }
    }

    private void SpawnPortalForClearedArea()
    {
        // Spawn the portal so player can continue forward
        // if (portalSpawner != null)
        // {
        //     portalSpawner.SpawnPortal();
        //     Debug.Log("✅ Portal spawned for already-cleared area");
        // }
        // else
        // {
        //     Debug.LogWarning("⚠️ No portal spawner - player might be stuck!");

        //     // Fallback: Load next scene directly after delay
        //     StartCoroutine(LoadNextSceneAfterDelay(3f));
        // }
    }

    public void OnBossDied(BossBase boss)
    {
        GameManager.Instance.SetFlag(flagAfterWin, true);
        Debug.Log($"{boss.bossName} defeated!");
        // Boss handles its own death, portal spawning, etc.
    }
}