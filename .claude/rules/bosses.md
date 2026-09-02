---
paths:
  - "Assets/Scripts/Bosses/**/*.cs"
---

# Boss rules

- Every boss extends `BossBase`. Don't reimplement HP/damage/death from scratch — override the `protected virtual` hooks (`Start`, `OnBossStart`, `BossAI`, `TakeDamage`, `EnterPhase2`, `Die`, `OnDeathDialogueComplete`).
- If you override `Start()`, call `base.Start()` first — it caches `sr`/`rb`/`anim` and finds the player.
- If you override `TakeDamage`, preserve the base's ordering: `isDead` guard → `isInvulnerable` guard (calling `OnInvulnerableHit()` and returning without damage) → subtract HP → `Die()` when HP hits 0. Call `base.TakeDamage()` when you don't need to change this ordering; only inline the logic if you must interleave boss-specific behavior (e.g. suppressing the hurt animation mid-attack, as `PhilipBoss` does).
- **Death must be handled exactly once.** If you override `Die()`, guard it exactly like `GeorgeBoss`/`PhilipBoss` do:
  ```csharp
  if (isDead) return;
  isDead = true;
  ```
  at the very top, before touching coroutines, physics, or animation state.
- Stop all boss coroutines in `Die()` (`StopAllCoroutines()`) before playing the death animation, so no attack/movement coroutine can act on a dead boss.
- Call `waveManager.OnBossDied(this)` from `Die()` if a `waveManager` reference exists, so wave/portal progression advances.
- Play `deathDialogue` (if assigned) before `OnDeathDialogueComplete()`; if there is no `deathDialogue`, call `OnDeathDialogueComplete()` directly instead of skipping it.
- **The defeat flag is set in `OnDeathDialogueComplete()`, not in `Die()`.** Call `base.OnDeathDialogueComplete()` first (awards coins, plays `slainDialogue`), then set this boss's `GameFlag` and call `GameManager.Instance.SaveProgress()`.
- A cutscene/spawn dialogue that should only play on the first defeat/encounter must be gated by its own dedicated `GameFlag`, checked in `OnBossStart()` and set immediately (with `SaveProgress()`) right after — see `PhilipBoss.PhilipSpawnDialogueSeen` and `GeorgeBoss`'s `hasUpgradedSword` check. Never rely on `isDead`/scene state alone to decide whether a cutscene has been seen.
