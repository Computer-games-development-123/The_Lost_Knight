---
paths:
  - "Assets/Scripts/**/*.cs"
---

# Unity C# rules

- Never call `GetComponent`, `GetComponentInChildren`, `GetComponentInParent`, or `GameObject.Find`/`FindGameObjectWithTag` inside `Update()`, `FixedUpdate()`, or `LateUpdate()`. Cache the reference once in `Start()` (the existing convention in `PlayerController`, `BossBase`, and enemy scripts), not `Awake()`, unless the value is needed before `Start()` runs.
- Null-check every cached `[SerializeField]` reference before using it, or fail loudly with `Debug.LogWarning`/`LogError` naming the missing field — don't let a missing reference throw a silent `NullReferenceException` mid-frame.
- Prefer `[SerializeField] private` over bare `public` for fields that exist only for Inspector wiring and aren't part of another script's API. Public is acceptable for boss/enemy tunable stats that designers adjust per-prefab (existing pattern).
- Unsubscribe from every event you subscribe to, in `OnDisable()` or `OnDestroy()` (whichever mirrors the subscribe call). This includes `GameManager.ProgressLoaded` and any `UnityEvent`/C# `event` you hook.
- No magic numbers for gameplay-tuning values (damage, cooldowns, speeds, thresholds) — expose them as a named `[SerializeField]`/`public` field with a sensible default, even if only one caller uses it. Numbers that are structurally meaningful (array index 0, `Vector2.zero`) are fine as literals.
- Match the file name to the class name inside it.
