using UnityEngine;

/// <summary>
/// Makes a Canvas persist across scene loads (like GameManager).
/// Attach this to your StoreCanvas to keep it alive when scenes reload.
/// </summary>
public class PersistentCanvas : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Unique ID for this canvas. Used to prevent duplicates.")]
    public string canvasID = "StoreCanvas";

    private static System.Collections.Generic.HashSet<string> existingCanvases = 
        new System.Collections.Generic.HashSet<string>();

    private void Awake()
    {
        // Check if a canvas with this ID already exists
        if (existingCanvases.Contains(canvasID))
        {
            Debug.Log($"PersistentCanvas: Duplicate {canvasID} found, destroying this instance");
            Destroy(gameObject);
            return;
        }

        // Register this canvas ID
        existingCanvases.Add(canvasID);

        // Make persistent
        DontDestroyOnLoad(gameObject);
        Debug.Log($"PersistentCanvas: {gameObject.name} is now persistent across scenes");
    }

    private void OnDestroy()
    {
        // Remove from registry when destroyed
        existingCanvases.Remove(canvasID);
    }
}