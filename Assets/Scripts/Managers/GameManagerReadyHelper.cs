using UnityEngine;
using System;

public static class GameManagerReadyHelper
{
    /// <summary>
    /// Runs the given action immediately if GameManager progress is already loaded,
    /// otherwise runs it once when progress finishes loading.
    /// </summary>
    public static void RunWhenReady(MonoBehaviour owner, Action action)
    {
        if (action == null) return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManagerReadyHelper: GameManager.Instance is null");
            return;
        }

        // Already loaded -> run now
        if (GameManager.Instance.IsProgressLoaded)
        {
            action.Invoke();
            return;
        }

        // Otherwise wait for ProgressLoaded event
        void Handler()
        {
            // Safety: owner might be destroyed
            if (owner == null) return;

            GameManager.Instance.ProgressLoaded -= Handler;
            action.Invoke();
        }

        GameManager.Instance.ProgressLoaded += Handler;

        // Ensure cleanup if owner is destroyed before load completes
        owner.StartCoroutine(RemoveOnDestroy(owner, Handler));
    }

    private static System.Collections.IEnumerator RemoveOnDestroy(
        MonoBehaviour owner,
        Action handler
    )
    {
        while (owner != null)
            yield return null;

        if (GameManager.Instance != null)
            GameManager.Instance.ProgressLoaded -= handler;
    }
}
