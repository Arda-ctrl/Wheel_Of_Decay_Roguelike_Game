using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string text;
        public AudioClip voiceClip; // Optional voice clip for this line
    }

    public string npcName;
    public DialogueLine[] dialogueLines;
} 