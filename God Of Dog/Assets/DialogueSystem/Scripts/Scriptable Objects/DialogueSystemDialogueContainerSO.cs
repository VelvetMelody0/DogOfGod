using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{
    public class DialogueSystemDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<DialogueSystemDialogueGroupSO, List<DialogueSystemDialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<DialogueSystemDialogueSO> UngroundedDialogues { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            DialogueGroups = new SerializableDictionary<DialogueSystemDialogueGroupSO, List<DialogueSystemDialogueSO>>();
            UngroundedDialogues = new List<DialogueSystemDialogueSO>();
        }    
    }

}
