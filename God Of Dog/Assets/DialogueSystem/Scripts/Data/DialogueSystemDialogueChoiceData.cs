using DialogueSystem.ScriptableObjects;
using System;
using UnityEngine;

namespace DialogueSystem.Data
{
    [Serializable]
    public class DialogueSystemDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DialogueSystemDialogueSO NextDialogue { get; set; }
    }
}

