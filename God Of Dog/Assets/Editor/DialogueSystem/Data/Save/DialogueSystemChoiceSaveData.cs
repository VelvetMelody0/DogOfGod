using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Data.Save
{
    [Serializable]
    public class DialogueSystemChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}

