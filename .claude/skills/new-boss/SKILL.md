---
name: new-boss
description: Scaffold a new boss script following this project's BossBase pattern. Use when the user asks to create/add a new boss enemy. Takes the boss name as $ARGUMENTS.
disable-model-invocation: true
---

Scaffold `Assets/Scripts/Bosses/{$ARGUMENTS}Boss.cs` (PascalCase the name, e.g. `Ditor` → `DitorBoss.cs`) as a subclass of `BossBase`. Read `Assets/Scripts/Bosses/BossBase.cs` and one existing boss (`GeorgeBoss.cs` for a melee/flying pattern, `PhilipBoss.cs` for a ranged/portal pattern, `DitorBoss.cs` for a multi-attack combo pattern) before writing, so field names and style match.

Follow [[bosses]] for the death/defeat-flag contract. Concretely, the new script must have:

1. **Class + fields**: `public class {Name}Boss : BossBase`. Boss-specific tunables as `[Header("...")] public` fields (cooldowns, ranges, attack points) — matches existing boss style, not `[SerializeField] private`.
2. **`protected override void Start()`**: call `base.Start()` first, then init any boss-specific timers.
3. **`protected override void OnBossStart()`**: set `bossName`, and if this boss needs a first-encounter-only cutscene, gate it behind a new `GameFlag` (add it to the bottom of the enum in `Assets/Scripts/Enviroment/GameFlag.cs`, name it `{Name}SpawnDialogueSeen` or similar) checked here — see `PhilipBoss.OnBossStart`.
4. **`protected override void BossAI()`**: state machine over distance-to-player (see `GeorgeBoss`/`PhilipBoss` for the close/mid/far range pattern) driving attack coroutines.
5. **Damage/hitboxes** — pick one, matching the attack's shape:
   - Single melee point: a `public Transform attackPoint` + `public float attackRange`, hit-tested with `Physics2D.OverlapCircleAll(attackPoint.position, attackRange)` filtering `CompareTag("Player")`, called from inside an attack coroutine (see `GeorgeBoss.CloseRangeAttack`, `PhilipBoss.DealMeleeDamage`).
   - Multiple/combo hitboxes: a separate `BossAttackCollider`-style trigger component on a child GameObject that calls back into a public method on this boss (see `DitorBoss` + `BossAttackCollider`), when a single circle isn't enough.
6. **`protected override void Die()`**: `if (isDead) return; isDead = true;` first, `StopAllCoroutines()`, zero out velocity, then `base.Die()`.
7. **`protected override void OnDeathDialogueComplete()`**: call `base.OnDeathDialogueComplete()`, then set a new `GameFlag.{Name}Defeated` (added to the bottom of the enum) via `GameManager.Instance.SetFlag(...)` and call `GameManager.Instance.SaveProgress()`. Also add a corresponding `On{Name}Defeated()` method to `GameManager.cs` following the existing `OnGeorgeDefeated`/`OnFikaDefeated` pattern, and call it here instead of setting the flag inline, to match the existing convention.
8. Add `EnterPhase2()` override only if the boss has a phase 2 (scale up speed/damage/cooldowns, matching `GeorgeBoss.EnterPhase2`).

After writing the script, list out — do not attempt to do these yourself — the Inspector wiring the user must do in the Unity Editor:

- Create the boss prefab/GameObject with `SpriteRenderer`, `Rigidbody2D`, `Animator`, a `Collider2D`, tag it appropriately.
- Assign `maxHP`, `damage`, `moveSpeed`, `coinsReward` on the new component.
- Assign `spawnDialogue` / `deathDialogue` / `slainDialogue` `DialogueData` assets (create them if they don't exist yet).
- Wire `waveManager` (drag the scene's `WaveManager`) and any `attackPoint`/`groundCheck`/hitbox child transforms.
- Add the Animator triggers/bools this script calls (`Hurt`, `Death`, plus any attack-specific ones) to the boss's Animator Controller.
- Add the new `WaveManager` boss-spawn entry / prefab reference for the scene this boss belongs to.
- If a new `GameFlag` was added, no Inspector action needed — but double check `StoreStateManager`/other flag-dependent Inspector fields if this boss should affect store state.
