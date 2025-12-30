using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Authentication;   // AuthenticationService
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;             // UnityServices
using UnityEngine;
using System.Linq;

/*
 * This class manages loading and saving key-value pairs in the CloudSaveService.
 */
public class DatabaseManager {
    // Sample code from https://docs.unity.com/ugs/manual/cloud-save/manual/tutorials/unity-sdk

    public static async Task<Dictionary<string, string>> SaveData(params (string key, object value)[] kwargs) {
        // Idea from  here: https://stackoverflow.com/a/77002085/827927
        Dictionary<string, object> playerData = kwargs.ToDictionary(x => x.key, x => x.value);
        var result = await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
        Debug.Log($"Saved data {string.Join(',', playerData)}. result={string.Join(',', result)}");
        return result;
    }


    public static async Task<Dictionary<string, Item>> LoadData(params string[] args) {
        Debug.Log($"LoadData {string.Join(',',args)}");
        HashSet<string> keys = new HashSet<string>(args);
        Dictionary<string, Item> playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
        Debug.Log($"loaded player data: {string.Join(',', playerData)}");
        return playerData;
    }
}
