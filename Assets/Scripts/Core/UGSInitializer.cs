using System;
using Unity.Services.Core;
using UnityEngine;

public class UGSInitializer : MonoBehaviour
{
    public static bool IsReady { get; private set; }
    private static UGSInitializer instance;

    private async void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
            await UnityServices.InitializeAsync();
            IsReady = true;
            Debug.Log("UGS Initialized");
        }
        catch (Exception e)
        {
            Debug.LogError("UGS init failed: " + e.Message);
        }
    }
}
