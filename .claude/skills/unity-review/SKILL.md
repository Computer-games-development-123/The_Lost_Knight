---
name: unity-review
description: Review specific C# files against this project's rules (unity-csharp, bosses, persistence) and common Unity pitfalls. Use when the user asks to review, audit, or check boss/gameplay/save scripts they point at.
---

Review only the files the user points at (or the current diff, if they don't name files). Do not review unrelated code.

For each file, check against whichever of these applies by path:

- Any `Assets/Scripts/**/*.cs` file: [[unity-csharp]] — `GetComponent`/`Find` outside `Update`, cached in `Start()`; null-checked serialized fields; `[SerializeField] private` over bare `public` for Inspector-only fields; events unsubscribed in `OnDisable`/`OnDestroy`; no unexplained magic numbers; filename matches class name.
- A boss script under `Assets/Scripts/Bosses/`: [[bosses]] — extends `BossBase` and calls `base.Start()`; `Die()` guards `isDead` at the top before doing anything else; death handled exactly once (no path where `Die()` or the defeat flag can be hit twice); defeat flag + `SaveProgress()` happen in `OnDeathDialogueComplete()`, not `Die()`; cutscene-only-once logic gated by a dedicated flag.
- A save/cloud-save-related script (`Managers/DatabaseManager.cs`, `Managers/GameManager.cs`, anything calling `DatabaseManager.SaveData`/`LoadData`): [[persistence]] — no `PlayerPrefs`; save keys have no spaces; flag values only touched through `GameManager.GetFlag`/`SetFlag`; a state change that should survive an app restart actually calls a save method, not just an in-memory `SetFlag`.

Also flag, independent of the above:
- Missing null-check before dereferencing a singleton `Instance` or a `[SerializeField]` reference.
- Coroutines started without ever being stopped on death/disable (leaks, or acting on a destroyed boss/enemy).
- `Time.timeScale` set without a matching reset path.
- Duplicate/dead code copy-pasted from `BossBase` instead of calling `base.X()`.

Output: a flat list of concrete issues, most severe first, each as `file:line — issue`. No praise, no summary of what the code does, no restating the rules that passed.
