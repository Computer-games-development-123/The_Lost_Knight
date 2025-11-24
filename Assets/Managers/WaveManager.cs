using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        [Tooltip("Prefab של האויב בגל הזה (Enemy1 / Enemy2 וכו')")]
        public GameObject enemyPrefab;

        [Tooltip("כמה אויבים בגל הזה")]
        public int enemyCount = 3;

        [Tooltip("השהייה בין אויב לאויב")]
        public float spawnInterval = 1f;
    }

    [Header("Waves (Option B)")]
    public Wave[] waves;                // 
                                        // Wave0: Enemy1 x7
                                        // Wave1: Enemy2 x2
                                        // Wave2: Enemy2 x4
                                        // Wave3: Enemy2 x7

    [Header("Boss")]
    [Tooltip("Prefab של הבוס (למשל GeorgeBoss)")]
    public GameObject bossPrefab;

    [Tooltip("השהייה קצרה לפני ספאון בוס אחרי שהגלים נגמרים")]
    public float delayBeforeBoss = 2f;

    [Header("Managers")]
    [Tooltip("רפרנס ל-SpawnManager בסצנה (מכיל נקודות ספאון וכו')")]
    public SpawnManager spawnManager;

    [Header("Spawn Settings")]
    [Tooltip("פיזור אקראי בציר X סביב נקודת הספאון כדי שלא כל האויבים יוולדו באותה נקודה")]
    public float spawnSpread = 0.5f;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool bossAlive = false;

    private void Start()
    {
        if (spawnManager == null)
        {
            spawnManager = FindFirstObjectByType<SpawnManager>();
        }

        if (waves != null && waves.Length > 0)
        {
            StartCoroutine(RunWave(currentWave));
        }
        else
        {
            Debug.LogWarning("WaveManager: no waves defined!");
        }
    }

    private IEnumerator RunWave(int index)
    {
        Wave w = waves[index];

        if (w.enemyPrefab == null)
        {
            Debug.LogError("Wave " + index + " has no enemyPrefab assigned!");
            yield break;
        }

        // יצירת האויבים בגל
        for (int i = 0; i < w.enemyCount; i++)
        {
            SpawnEnemy(w.enemyPrefab);
            yield return new WaitForSeconds(w.spawnInterval);
        }

        // לחכות שכל האויבים ימותו
        while (enemiesAlive > 0)
        {
            yield return null;
        }

        // לעבור לגל הבא או לבוס
        currentWave++;

        if (currentWave < waves.Length)
        {
            StartCoroutine(RunWave(currentWave));
        }
        else
        {
            // כל הגלים הסתיימו → עוברים לבוס
            Debug.Log("All waves cleared, spawning boss...");
            yield return new WaitForSeconds(delayBeforeBoss);
            StartBossPhase();
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnManager == null)
        {
            Debug.LogError("WaveManager: no SpawnManager assigned!");
            return;
        }

        // לוקחים נקודת ספאון רנדומלית מה-SpawnManager
        Transform sp = spawnManager.GetRandomSpawnPoint();
        if (sp == null)
        {
            Debug.LogError("WaveManager: SpawnManager has no spawn points!");
            return;
        }

        // אוף-סט אקראי קטן בציר X כדי שהם לא יהיו באותה נקודה בדיוק
        float offsetX = Random.Range(-spawnSpread, spawnSpread);
        Vector3 spawnPos = sp.position + new Vector3(offsetX, 0f, 0f);

        GameObject e = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        if (e == null) return;

        enemiesAlive++;

        // לוודא שיש EnemyDeathNotifier
        EnemyDeathNotifier notifier = e.GetComponent<EnemyDeathNotifier>();
        if (notifier == null)
        {
            notifier = e.AddComponent<EnemyDeathNotifier>();
        }
        notifier.manager = this;
    }

    private void StartBossPhase()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("WaveManager: bossPrefab not assigned!");
            return;
        }

        if (spawnManager == null)
        {
            Debug.LogError("WaveManager: no SpawnManager for boss spawn!");
            return;
        }

        GameObject boss = spawnManager.SpawnBoss(bossPrefab);
        bossAlive = true;

        // לוודא שיש BossDeathNotifier
        BossDeathNotifier notifier = boss.GetComponent<BossDeathNotifier>();
        if (notifier == null)
        {
            notifier = boss.AddComponent<BossDeathNotifier>();
        }
        notifier.waveManager = this;

        Debug.Log("Boss spawned!");
    }

    // נקרא מ-EnemyDeathNotifier
    public void OnEnemyDied()
    {
        enemiesAlive--;
        if (enemiesAlive < 0) enemiesAlive = 0;
    }

    // נקרא מ-BossDeathNotifier
    public void OnBossDied()
    {
        if (!bossAlive) return;

        bossAlive = false;
        Debug.Log("Boss died!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBossDefeated();
        }
    }
}
