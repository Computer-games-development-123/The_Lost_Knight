# The Lost Knight  
2D Action / Wave Survival Platformer – Unity Project  
**ITCH.IO:** https://imrfatty.itch.io/the-lost-knight  
**Wiki:** [https://github.com/Computer-games-development-123/The-Lost_Knight.wiki.git](https://github.com/Computer-games-development-123/The_Lost_Knight/wiki/The_Lost_Knight_elements%E2%80%90formal)  
## ▶️ [Play The Lost Knight- Twinery version](https://computer-games-development-123.github.io/The_Lost_Knight/)


Unity Version: **Unity 6 (6000.2.8f1)**  
Target Platforms: **WebGL / PC**

---

# 🧭 Project Overview
**The Lost Knight** is a 2D combat-focused platformer prototype centered around enemy waves, boss fights, and a simple hub area (ForestHub) that connects narrative and gameplay progression.

This version implements the **Core Loop of Act I – Green Forest**:
- Hub with NPC (Yoji)
- Gate unlocking through dialogue
- Enemy wave arena
- Boss fight
- Respawn rules

The project is structured for gradual expansion into Act II & Act III (Red and Dark Forests) and includes a connected full Wiki.

---

# 🔁 Core Loop  
**The Lost Knight – Core Gameplay Loop (Prototype)**

1. Player starts in **ForestHub**.  
2. Player interacts with **Yoji (NPC)**.  
3. Yoji unlocks the **Green Forest Gate**.  
4. Player enters **GreenForest Combat Scene**.  
5. Fight enemy waves (Wave Manager).  
6. Boss appears after all enemies die.  
7. If the player dies → restart the combat scene.  
8. If the boss dies → level is cleared (future expansion: move to next forest).

---

# 🗺️ Scenes

## 🌳 1. ForestHub
- Player spawn
- Yoji (NPC) with interaction prompt
- Locked gate → unlocks after required dialogue
- Scoreboard placeholder  
- Leads to the combat arena

## ⚔️ 2. Combat_GreenForest
- Platform layout
- Enemy spawn points (left & right)
- Boss spawn point
- UI: Player Health Bar
- WaveManager + SpawnManager
- On clearing waves → boss appears

---

# 🎮 Controls

| Action | Key |
|-------|------|
| Move  | A / D or Left / Right Arrows |
| Jump  | Space |
| Attack | X |
| Interact | Up Arrow |
| Pause | Editor-only for now |

---

# ⚙️ Core Systems

## 🧍 Player
- **PlayerController** – movement, jumping, facing direction  
- **PlayerAttack** – melee hit detection via OverlapCircle  
- **PlayerHealth** – HP, damage handling, death events  

## 👾 Enemies & Boss
- **Enemy**, **EnemyMovement**, **EnemyDeathNotifier**  
- **GeorgeBossController** for boss logic  

## 🌊 Wave & Spawn Management
- **SpawnManager** – spawn points  
- **WaveManager** – waves, boss spawning, enemy tracking  

## 🧩 NPC & Hub Progression
- **YojiInteraction** – dialogue & unlocking gate  
- **ForestGateController** – loads combat scene  

## ❤️ UI
- **PlayerHealthUI** – slider updates & color changes  

---

# 📁 Project Structure (Unity Folders)

Assets/
- Scenes/
- Player/
- Enemies/
- Managers/
- Scripts/NPCs/
- UI/
- Prefabs/

---

# 📢 Credits  
Developer: Itzhak Bista, Adir Ofir
Course: Computer Games Development  
Engine: Unity 6  
Platform: WebGL & PC

---

# 🚀 Future Expansion
- Act II – Red Forest  
- Act III – Dark Forest  
- Full story & endings  
- Shop system  
- Scoreboard  
- Save system  
