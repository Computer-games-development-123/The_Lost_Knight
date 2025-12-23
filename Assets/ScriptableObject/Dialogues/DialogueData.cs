using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    [Header("ID (flag)")]
    public GameFlag flag;                       // e.g. "YOJI_FIRST", "BOSS1_SPAWN"

    [Header("Speaker")]
    public string speakerName;              // e.g. "Yoji", "George", "Narrator"

    [Header("Lines")]
    [TextArea(2, 5)]
    public string[] lines;                  // Each element is one step in the dialogue

    [Header("Character image")]
    public Sprite portrait;

    [Header("Settings")]
    public bool pauseGameDuringDialogue = true;
}
