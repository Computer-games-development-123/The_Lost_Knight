using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    [Header("ID")]
    public string id; // ✅ For BUNCH 2's DialogueManager
    public GameFlag flag = GameFlag.None; // ✅ For BUNCH 1's dialogue triggers
    
    [Header("Speaker")]
    public string speakerName;
    
    [Header("Lines")]
    [TextArea(2, 5)]
    public string[] lines;
    
    [Header("Character image (optional)")]
    public Sprite portrait;
    
    [Header("Settings")]
    public bool pauseGameDuringDialogue = true;
}