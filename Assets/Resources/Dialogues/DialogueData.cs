using UnityEngine;

[CreateAssetMenu(menuName = "Game/Dialogue", fileName = "NewDialogue")]
public class DialogueData : ScriptableObject
{
    [Header("ID (optional but recommended)")]
    public string id;                       // e.g. "YOJI_FIRST", "BOSS1_SPAWN"

    [Header("Speaker")]
    public string speakerName;              // e.g. "Yoji", "George", "Narrator"

    [Header("Lines")]
    [TextArea(2, 5)]
    public string[] lines;                  // Each element is one step in the dialogue

    [Header("Settings")]
    public bool pauseGameDuringDialogue = true;
}
