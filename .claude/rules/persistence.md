---
paths:
  - "Assets/Scripts/Managers/DatabaseManager.cs"
  - "Assets/Scripts/Managers/GameManager.cs"
  - "Assets/Scripts/Managers/StoreStateManager.cs"
  - "Assets/Scripts/Enviroment/GameFlag.cs"
  - "Assets/Scripts/Authentication/**/*.cs"
  - "Assets/Scripts/Player/PlayerHealth.cs"
  - "Assets/Scripts/Yoji's Shop/*.cs"
---

# Persistence rules

- Persistence is Cloud Save only, via `DatabaseManager.SaveData(params (string key, object value)[])` / `DatabaseManager.LoadData(params string[])`. Never introduce `PlayerPrefs`, local files, or any other save path.
- **Save keys must never contain spaces or other characters CloudSave rejects.** Sanitize any user-facing/inspector string before using it in a key (`name.Replace(" ", "_")`), exactly like `ListStoreController.SaveStoreStock` does for item names.
- Key naming: flags use `FLAG_{GameFlag enum name}` (handled automatically by `GameManager`, don't hand-roll this). A new standalone value gets its own descriptive PascalCase key (`PlayerMaxHealth`); a per-item/per-index value gets `Category_{index}_{sanitizedName}` (`StoreStock_0_HP_Upgrade`).
- What must survive a **scene change**: anything read through `GameManager.GetFlag`/`SetFlag` — it lives in the in-memory dictionary on the `DontDestroyOnLoad` `GameManager` singleton, no extra work needed as long as you go through that API.
- What must survive an **app restart**: only what you explicitly push to Cloud Save. Setting a flag with `SetFlag` alone does NOT persist it — you must also call `GameManager.Instance.SaveProgress()` (or, for non-flag values, your own `SaveData` call) after the change.
- Runtime-only values (current HP, current position, anything reset each scene load by design) must NOT be wrapped in cloud save calls — see `PlayerHealth.currentHealth`, which is explicitly commented as not saved and is recomputed from `maxHealth` in `Start()`.
- To verify a value actually round-trips: change it, call the relevant save method, then call the matching `DatabaseManager.LoadData` for that exact key (or restart play mode) and confirm the loaded value matches — don't assume a save call succeeded just because it didn't throw; `SaveProgress`/`SaveData` calls are `async void`/fire-and-forget and swallow failures into `Debug.LogError`.
