public enum GameFlag
{
    None = 0,

    // --- Dialogues ---
    OpeningDialogueSeen,
    YojiFirstDialogueCompleted,
    YojiAfterGeorge,
    GreenBattleIntroSeen,

    // --- Portals / Gates ---
    ForestHubGateOpened,

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