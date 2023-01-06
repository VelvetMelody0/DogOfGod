using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DialogueSystem.Data.Error
{
    using Elements;
    public class DialogueSystemNodeErrorData
    {
        public DialogueSystemErrorData ErrorData { get; set; }

        public List<DialogueSystemNode> Nodes { get; set; }

        public DialogueSystemNodeErrorData()
        {
            ErrorData= new DialogueSystemErrorData();
            Nodes= new List<DialogueSystemNode>();
        }
    }

}
