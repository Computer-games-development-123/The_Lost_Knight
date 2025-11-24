using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("=== Spawn Points ===")]
    [Tooltip("Regular Enemies Spawn Points")]
    public Transform[] enemySpawnPoints;

    [Tooltip("Boss Spawn Point")]
    public Transform bossSpawnPoint;

    // מחזיר נקודת ספאון רנדומלית לאויבים רגילים
    public Transform GetRandomSpawnPoint()
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogError("SpawnManager: No enemy spawn points assigned!");
            return null;
        }

        int index = Random.Range(0, enemySpawnPoints.Length);
        return enemySpawnPoints[index];
    }

    // Spawns a regular enemy at a random spawn point (פונקציה אופציונלית)
    public GameObject SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("SpawnManager: Tried to spawn NULL enemy prefab!");
            return null;
        }

        Transform sp = GetRandomSpawnPoint();
        if (sp == null)
        {
            // כבר הודענו בלוג ונחזיר null
            return null;
        }

        GameObject enemy = Instantiate(enemyPrefab, sp.position, Quaternion.identity);
        Debug.Log($"SpawnManager: Spawned enemy '{enemyPrefab.name}' at {sp.position}");

        return enemy;
    }

    // Spawns the boss at the boss spawn point
    public GameObject SpawnBoss(GameObject bossPrefab)
    {
        if (bossPrefab == null)
        {
            Debug.LogError("SpawnManager: Tried to spawn NULL boss prefab!");
            return null;
        }

        Transform sp = bossSpawnPoint;

        // fallback אם אין נקודת ספאון לבוס
        if (sp == null)
        {
            Debug.LogWarning("SpawnManager: No boss spawn point found! Using enemy spawn point as fallback.");

            sp = GetRandomSpawnPoint();
            if (sp == null)
            {
                Debug.LogError("SpawnManager: No spawn points available at all!");
                return null;
            }
        }

        GameObject boss = Instantiate(bossPrefab, sp.position, Quaternion.identity);
        Debug.Log($"SpawnManager: Spawned BOSS '{bossPrefab.name}' at {sp.position}");

        return boss;
    }
}
