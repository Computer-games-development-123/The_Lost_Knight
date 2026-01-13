using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    [Header("ID")]
    public string id;
    public GameFlag flag = GameFlag.None;
    
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