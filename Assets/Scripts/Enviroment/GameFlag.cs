public enum GameFlag
{
    None = 0,

    // --- Dialogues ---
    OpeningDialogueSeen,
    YojiFirstDialogueCompleted,
    YojiAfterGeorge,

    // --- Portals / Gates ---
    TutorialPortalUnlocked,
    ForestHubGateOpened,

    // --- Boss / Events ---
    EnterTutorial,
    TutorialCompleted,
    GeorgeFirstEncounter,
    GeorgeSecondEncounter,
    GeorgeDefeated,
    FikaDefeated,
    PhillipDefeated,
    DitorDefeated,
    YojiDead,
    
    // --- Act Progression ---
    Act1Cleared,
    Act2Cleared,
    Act3Cleared
}