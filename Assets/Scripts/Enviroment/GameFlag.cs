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

    // -- Upgrades --
    hasTeleport,
    hasUpgradedSword,
    hasWaveOfFire,

    // --- Boss / Events ---
    TutorialCompleted,
    GeorgeFirstEncounter,
    GeorgeDefeated,
    FikaDefeated,
    PhillipDefeated,
    DitorDefeated,
    YojiDead
}