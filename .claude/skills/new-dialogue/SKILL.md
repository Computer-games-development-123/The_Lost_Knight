---
name: new-dialogue
description: Add a new dialogue stage to the existing dialogue system, with its gating flag, trigger, and Inspector wiring. Use when the user asks to add a new conversation/dialogue beat. Takes a short description of the dialogue as $ARGUMENTS.
disable-model-invocation: true
---

Read `Assets/Scripts/Managers/DialogueManager.cs` and `Assets/ScriptableObject/Dialogues/DialogueData.cs` first, then follow [[bosses]]'s dialogue-once pattern and the project's general dialogue rules in `CLAUDE.md`.

A new dialogue stage ($ARGUMENTS) needs these pieces — write the code, but the `DialogueData` asset and scene wiring are Inspector work you list out, not perform:

1. **Gating flag**: if this dialogue should only ever play once, add a new entry to the bottom of `Assets/Scripts/Enviroment/GameFlag.cs` (never insert mid-enum), named `{Thing}DialogueSeen` or `{Thing}Seen` matching existing naming (`YojiPostFikaDialogueSeen`, `GreenBattleIntroSeen`).
2. **DialogueData asset**: tell the user to create one via `Assets > Create > Scriptable Objects > DialogueData`, and what to set: `id` (unique string), `flag` (only if `DialogueTrigger` should auto-set it on completion — otherwise leave `None` and set the flag from code), `speakerName`, `lines[]`, optional `portrait`, `pauseGameDuringDialogue`.
3. **Trigger condition** — pick the mechanism matching how this dialogue should start, matching existing usage:
   - Fires once when a scene loads: `StartDialogue` component, with `onceFlag` set to the new gating flag.
   - Fires when the player walks into a trigger volume: `DialogueOnEntry` on a `Collider2D` (`isTrigger` on) tagged for `Player` detection.
   - Fires on proximity + keypress/tap, optionally locking/unlocking a portal: `DialogueTrigger`, with `lockOrUnlock` and `portal` set if this dialogue should gate a portal.
   - Part of a multi-stage NPC conversation tree (like Yoji): add a new `if (GM.GetFlag(prereqFlag) && !GM.GetFlag(newFlag))` branch to the existing handler's `ShouldShowDialoguePrompt()` and `HandleDialogueInteraction()`, in flag-check order matching the existing branches, plus an `On{Stage}DialogueComplete()` callback that sets the new flag and calls `GameManager.Instance.SaveProgress()`.
   - Triggered by a game event (boss spawn/death, item pickup): call `DialogueManager.Instance.Play(dialogueDataField, onComplete)` directly from that event's handler, null-checking `DialogueManager.Instance` first (see any `BossBase`/boss subclass call site).
4. Every completion callback that should persist must call `GameManager.Instance.SetFlag(newFlag, true)` then `GameManager.Instance.SaveProgress()` — matches `YojiDialogueHandler`'s `On*DialogueComplete` methods.

After the code, list the Inspector steps the user still has to do:
- Create and fill in the `DialogueData` asset (or assets, if there's a branch).
- Drag the asset onto the new/edited script's `DialogueData` field(s) in the Inspector.
- If using `DialogueTrigger`/`DialogueOnEntry`/`StartDialogue`, add that component to the right GameObject in the scene and wire `portal`/`interactionPrompt`/collider references.
- If this extends an existing handler (e.g. Yoji), no new GameObject is needed — just confirm the existing handler component's fields include the new `DialogueData` reference.
