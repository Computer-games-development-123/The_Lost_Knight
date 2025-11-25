# The Lost Knight – Core Loop Prototype

Unity 2D prototype for the core gameplay loop of **The Lost Knight**.
ITCH.IO LINK : https://imrfatty.itch.io/the-lost-knight
This version focuses on:
- ForestHub (hub) scene with NPC interaction (Yoji) and a locked gate.
- Green combat arena with enemy waves and a first boss.
- Basic combat, health and enemy wave system.

Unity version: **Unity 6 (6000.2.8f1)**  
Target platform: WebGL / PC

---

## 1. Game Overview

**Genre:** 2D combat platformer / wave survival  
**Core Loop (current prototype):**

1. Player starts in **ForestHub**.
2. Player talks with **Yoji** to unlock the gate.
3. Player enters the **Green Forest combat scene** through the gate.
4. Player fights several waves of enemies.
5. After all waves are cleared, the **boss** spawns.
6. If the player dies – he will restart the combat scene from the beginning.
7. When the boss dies – the level is considered cleared (future versions will move to the next forest).

This assignment focuses only on **Act I – Green Forest** and on the **core gameplay systems**:
movement, combat, health, waves, boss, and hub progression.

---

## 2. Scenes

### 2.1 ForestHub
- Simple flat ground with:
  - **Player**
  - **Yoji** (NPC + shop object)
  - **GreenForestGate** (portal to combat scene)
  - Scoreboard placeholder
- When the player gets close to Yoji, a message appears.
- Press **Up Arrow** to start dialogue with Yoji.
- After the required dialogue, the **gate is unlocked** and the player can enter the combat scene.

### 2.2 Combat_GreenForest
- Platforms layout with:
  - Player spawn point
  - Left / Right enemy spawn points
  - Boss spawn point
  - WaveManager + SpawnManager
  - PlayerHealthBar (UI)
- Enemies spawn in **waves**.
- After all waves are cleared, the **boss** spawns.
- Player and enemies can kill each other using the combat system.

---

## 3. Controls

- **Move** – `A`/`D` or Left / Right Arrows  
- **Jump** – `Space` (Jump input)  
- **Attack** – `X` (melee attack in front of the player)  
- **Interact** – `Up Arrow` (talk to Yoji, use gates, etc.)  
- **Pause / Editor** – default Unity play/stop (not implemented in UI yet).

---

## 4. Core Systems

### 4.1 Player

**Scripts:**
- `PlayerController`
  - Handles movement, jumping, facing direction.
  - Uses Unity Input (`Horizontal`, `Jump`).
  - Clamps the player to the camera view.
- `PlayerAttack`
  - Reads `facingDir` from `PlayerController`.
  - On `X` key:
    - Uses an `attackPoint` transform and `OverlapCircle` to detect enemies on `enemyLayer`.
    - Calls `Enemy.TakeDamage(damage)` on all hit enemies.
- `PlayerHealth`
  - Stores `maxHealth` and current HP.
  - Public method `TakeDamage(int amount)`.
  - When HP ≤ 0:
    - Logs “Player died!” (future versions will handle respawn / game over).

### 4.2 Enemies & Boss

**Scripts:**
- `Enemy`
  - Stores max HP, current HP.
  - `TakeDamage(int amount)` reduces HP and destroys the GameObject on death.
- `EnemyMovement` (planned / simple version)
  - Moves enemy back and forth between two X positions.
  - Flips direction when reaching edges.
- `EnemyDeathNotifier`
  - Added to enemies by `WaveManager`.
  - Holds reference to `WaveManager` and calls `OnEnemyDied()` on death.

**Boss:**
- Uses `Enemy` for HP.
- Uses dedicated **boss controller** (e.g. `GeorgeBossController`) for movement/AI.
- Spawns only after all normal waves are cleared.

### 4.3 Wave & Spawn Management

**Scripts:**
- `SpawnManager`
  - Holds references to spawn points (e.g. left / right).
  - Method `GetRandomSpawnPoint()` returns a random `Transform`.
  - Method `SpawnEnemy(GameObject prefab)` instantiates an enemy at a random spawn point.
- `WaveManager`
  - Defines nested `Wave` struct/class:
    - `GameObject enemyPrefab`
    - `int enemyCount`
    - `float spawnInterval`
  - Inspector fields:
    - `Wave[] waves`
    - `GameObject bossPrefab`
    - `SpawnManager spawnManager`
    - `Transform bossSpawnPoint`
  - Responsibilities:
    - Spawns enemies wave by wave (Option B: “waves then boss”).
    - Tracks `enemiesAlive` using `EnemyDeathNotifier`.
    - When all waves are cleared → spawn boss.
    - When boss dies → marks level as completed (future scenes will be triggered here).

### 4.4 NPC & Progression (ForestHub)

**Scripts:**
- `ShopObject`
  - Existing script attached to Yoji – handles shop logic / interaction placeholder.
- `YojiInteraction`
  - Detects when player is in range (collider trigger).
  - Shows “press UpArrow to talk” message.
  - Displays dialogue lines using a simple UI panel and `TextMeshPro` text.
  - After the first important dialogue:
    - Sets a **progress flag** (for example, in `GameManager` or via serialized boolean).
    - Notifies the gate that the player is allowed to pass.
- `ForestGateController`
  - Has reference to:
    - Scene name to load (e.g. `"Combat_GreenForest"`).
    - Prompt text object.
    - Some progression flag / reference to Yoji.
  - Only allows entering the gate (UpArrow) if the player has talked with Yoji.

### 4.5 UI – Health Bar

**Scripts:**
- `PlayerHealthUI`
  - References:
    - `PlayerHealth playerHealth`
    - `Slider healthSlider`
    - `Image fillImage` (for color)
  - Every frame:
    - Updates slider value to `currentHP / maxHP`.
    - Changes bar color between **green → yellow → red** depending on HP%.

---

## 5. Project Structure (Folders)

Under `Assets/`:

- `Scenes/`
  - `ForestHub.unity`
  - `Combat_GreenForest.unity`
- `Player/`
  - `PlayerController.cs`
  - `PlayerAttack.cs`
  - `PlayerHealth.cs`
- `Enemies/`
  - `Enemy.cs`
  - `EnemyMovement.cs`
  - `EnemyDeathNotifier.cs`
  - `GeorgeBossController.cs` (or other boss controller)
- `Managers/`
  - `WaveManager.cs`
  - `SpawnManager.cs`
  - `GameManager.cs` (if used for global flags)
- `Scripts/NPCs/`
  - `YojiInteraction.cs`
  - `ForestGateController.cs`
  - `ShopObject.cs`
- `UI/`
  - `PlayerHealthUI.cs`
- `Prefabs/`
  - `Player.prefab`
  - `EnemyType1.prefab`, `EnemyType2.prefab`, `GeorgeBoss.prefab`
  - `Ground.prefab`, `Platform.prefab`
- `Settings/`, `TextMesh Pro/`, `Utils/`  
  (Unity and helper assets)

---

## 6. UML Diagram (text)

The UML diagram below is a **high-level class diagram** of the main scripts.

```text
+------------------+          uses            +-------------------+
|  GameManager     |------------------------->|  WaveManager      |
+------------------+                         +-------------------+
| - flags          |                         | - waves[]         |
|                  |                         | - spawnManager    |
+------------------+                         | - bossPrefab      |
                                             | - enemiesAlive    |
                                             +---------+---------+
                                                       |
                                                       | uses
                                                       v
                                             +-------------------+
                                             |  SpawnManager     |
                                             +-------------------+
                                             | - spawnPoints[]   |
                                             +---------+---------+
                                                       |
                                                       | instantiates
                                                       v
                     +-------------------+    has      +-------------------+
                     |   Enemy           |<----------- | EnemyDeathNotifier|
                     +-------------------+             +-------------------+
                     | - maxHealth       |             | - manager:WaveMgr |
                     | - currentHealth   |             +-------------------+
                     +---------^---------+
                               |
                               | composition
                               v
                     +-------------------+
                     | BossController    |
                     +-------------------+

+-------------------+      has           +-------------------+
| PlayerController  |------------------->| PlayerAttack      |
+-------------------+                    +-------------------+
| - moveSpeed       |                    | - attackRange     |
| - jumpForce       |                    | - damage          |
| - facingDir       |                    | - enemyLayer      |
+---------+---------+                    +-------------------+
          |
          | has
          v
+-------------------+      observed by    +-------------------+
|  PlayerHealth     |<------------------- | PlayerHealthUI    |
+-------------------+                     +-------------------+
| - maxHealth       |                     | - healthSlider    |
| - currentHP       |                     | - fillImage       |
+-------------------+                     +-------------------+

+-------------------+   interacts via UpArrow   +-------------------+
|     Player        |-------------------------->| YojiInteraction   |
| (GameObject       |                           +-------------------+
|  + Controller     |                           | - dialogueLines[] |
|  + Attack         |                           | - UI references   |
|  + Health)        |                           | - unlockGateFlag  |
+-------------------+                           +---------+---------+
                                                           |
                                                           | notifies
                                                           v
                                                  +-------------------+
                                                  | ForestGateControl |
                                                  +-------------------+
                                                  | - sceneToLoad     |
                                                  | - isUnlocked      |
                                                  +-------------------+
