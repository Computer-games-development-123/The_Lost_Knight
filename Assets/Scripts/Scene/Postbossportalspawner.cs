using UnityEngine;

/// <summary>
/// Spawns a portal after boss is defeated.
/// UPDATED: Supports custom prompt text for spawned portals.
/// </summary>
public class PostBossPortalSpawner : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Vector3 portalSpawnPosition = new Vector3(10f, 0f, 0f);
    [SerializeField] private string targetSceneName = "GreenToRed";
    
    [Header("Prompt Customization")]
    [Tooltip("Custom text for this portal, e.g. 'Press F to Continue' - leave empty for default")]
    [SerializeField] private string customPromptText = "";
    
    [Header("Visual Effects (Optional)")]
    [SerializeField] private GameObject spawnEffect;
    [SerializeField] private float spawnEffectDuration = 1f;
    
    private GameObject spawnedPortal;
    private bool portalSpawned = false;

    /// <summary>
    /// Call this from GeorgeBoss after death dialogue completes
    /// </summary>
    public void SpawnPortal()
    {
        if (portalSpawned) return;

        Debug.Log("Spawning portal to next area...");

        // Spawn visual effect first
        if (spawnEffect != null)
        {
            GameObject effect = Instantiate(spawnEffect, portalSpawnPosition, Quaternion.identity);
            Destroy(effect, spawnEffectDuration);
        }

        // Spawn the portal
        if (portalPrefab != null)
        {
            spawnedPortal = Instantiate(portalPrefab, portalSpawnPosition, Quaternion.identity);
            
            // Configure the portal
            ScenePortal portalScript = spawnedPortal.GetComponent<ScenePortal>();
            if (portalScript != null)
            {
                portalScript.targetSceneName = targetSceneName;
                
                // Set custom prompt text if provided
                if (!string.IsNullOrEmpty(customPromptText))
                {
                    portalScript.SetPromptText(customPromptText);
                    Debug.Log($"Portal prompt set to: {customPromptText}");
                }
            }
            
            portalSpawned = true;
            Debug.Log($"Portal spawned at {portalSpawnPosition} → Target: {targetSceneName}");
        }
        else
        {
            Debug.LogError("Portal prefab is not assigned!");
        }
    }
}