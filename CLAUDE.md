# The Lost Knight — Project Conventions

Unity 6000.2.8f1, 2D, targeting Android (portrait+landscape autorotate, Play Store icons in `Assets/AppIcons/`). Single default assembly, no `.asmdef` files.

## Folder layout

Scripts live under `Assets/Scripts/<Category>/`:
`Managers`, `Bosses`, `Player`, `Enemies`, `Dialogues`, `Enviroment` (keep this spelling — matches the existing folder), `UI`, `Scene` (with nested `Scene/Scene Intro Dialogues`), `Authentication`, `Core`, `Utility`, `Yoji's Shop`.

Put a new script in the category folder matching what it *is* (a boss → `Bosses`, a manager singleton → `Managers`), not where it's used from.

## Naming

- Classes, methods, public fields: PascalCase.
- Private fields: plain camelCase, no underscore prefix (`currentHP`, not `_currentHP`).
- `[SerializeField] private` for inspector-editable fields that shouldn't be public API; plain `public` is fine for boss/enemy tunable stats (existing pattern).
- File name must match the class name inside it.

## Flags (`GameFlag`)

- Single global enum: `Assets/Scripts/Enviroment/GameFlag.cs`.
- **Always add new flags at the bottom of the enum, never insert in the middle or reorder.**
- Read/write flags only through `GameManager.Instance.GetFlag(flag)` / `SetFlag(flag, value)`. Never store gameplay progress any other way.
- Always null-check `GameManager.Instance` before use — it may not exist yet in edit-mode or before scene bootstrap.

## Adding a persisted value

- Persistence is Cloud Save only (Unity Gaming Services `DatabaseManager.SaveData`/`LoadData`). **Never use `PlayerPrefs`** for anything that must survive across devices/sessions.
- Flags are saved automatically as `FLAG_{EnumName}` (int 0/1) by `GameManager.SaveProgress()` — adding a flag to the enum is enough, no extra save code needed.
- A new non-flag persisted value needs its own explicit key, saved/loaded via `DatabaseManager.SaveData`/`LoadData` directly (see `PlayerHealth.SaveMaxHealthToCloud`, `ListStoreController.SaveStoreStock`).
- Save keys must never contain spaces — strip them (`itemName.Replace(" ", "_")`) before building a key.
- Call `GameManager.Instance.SaveProgress()` after any state change that must survive an app restart.

## Adding a new scene

- Register any new persistent singleton the scene depends on using the existing `Instance`/`DontDestroyOnLoad` pattern (see `GameManager`) — don't assume a manager from a previous scene is still there without checking `Instance != null`.
- Code that reads flags on scene start must wait for the async cloud load via `GameManagerReadyHelper.RunWhenReady(this, callback)` instead of reading `GameManager.Instance.GetFlag(...)` directly in `Start()`.
- Runtime-only state (current HP, current position, etc.) is never saved — it resets on scene load by design. Don't add cloud-save calls for it.

## Bosses

See `.claude/rules/bosses.md` for the full contract — always extend `BossBase`, never reimplement the damage/death loop from scratch.

## Dialogue

- All dialogue is a `DialogueData` ScriptableObject asset, played via `DialogueManager.Instance.Play(data, onComplete)` — never set dialogue UI text directly.
- Always null-check `DialogueManager.Instance` and `!DialogueManager.Instance.IsDialogueActive` before triggering.
- A dialogue that should only ever play once needs a dedicated `GameFlag` checked before `Play()` and set immediately after (see `StartDialogue.onceFlag`, `PhilipBoss.PhilipSpawnDialogueSeen`).

## Always / Never

- Always guard singleton `Awake()` with the duplicate-destroy pattern before assigning `Instance`.
- Always cache `GetComponent<T>()` results once in `Start()` (matches existing Player/Boss/Enemy scripts) — never call `GetComponent` or `GameObject.Find` inside `Update()`.
- Never touch `GameFlag` values directly as ints — always go through `GameManager.GetFlag`/`SetFlag`.
- Never add a new save system (no `PlayerPrefs`, no local JSON files) — Cloud Save via `DatabaseManager` is the only persistence path.
- Don't modify existing `GameFlag` enum ordering when adding flags — it breaks nothing technically today, but the file's own header comment reserves the bottom for new entries, so follow it.
