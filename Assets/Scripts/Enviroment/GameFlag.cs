public enum GameFlag
{
    None = 0,

    // --- Dialogues ---
    OpeningDialogueSeen,
    YojiFirstDialogueCompleted,
    YojiAfterGeorge,
    YojiUnlocksStore,
    YojiPostFikaDialogueSeen,
    GreenBattleIntroSeen,
    RedBattleIntroSeen,
    DarkBattleIntroSeen,
    FinalBattleIntroSeen,

    // -- Upgrades --
    hasTeleport,
    hasUpgradedSword,
    hasBreathOfFire,
    hasFireballSpell,

    // --- Boss / Events ---
    TutorialCompleted,
    GeorgeFirstEncounter,
    GeorgeDefeated,
    FikaCutsceneSeen,
    FikaDefeated,
    PhilipDefeated,
    DitorDefeated,
    YojiDead
}