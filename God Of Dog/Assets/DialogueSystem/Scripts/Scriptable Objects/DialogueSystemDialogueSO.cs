using DialogueSystem.Data;
using DialogueSystem.Enumerations;
using System.Collections.Generic;
using UnityEngine;


namespace DialogueSystem.ScriptableObjects
{
    public class DialogueSystemDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField] [field: TextArea()]public string Text { get; set; }
        [field: SerializeField] public List<DialogueSystemDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public DialogueSystemDialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }

        public void Initialize(string dialogueName, string text, List<DialogueSystemDialogueChoiceData> choices, DialogueSystemDialogueType dialogueType, bool isStartingDialogue)
        {
            DialogueName= dialogueName;
            Text= text;
            Choices= choices;
            DialogueType= dialogueType;
            IsStartingDialogue= isStartingDialogue;

        }
    }
}

