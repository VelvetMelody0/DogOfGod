using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;


namespace DialogueSystem.Elements
{
    using DialogueSystem.Data.Save;
    using DialogueSystem.Windows;
    using Enumerations;
    using Utilities;

    public class DialogueSystemSingleChoiceNode : DialogueSystemNode
    {

        public override void Initialize(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialize(nodeName, dialogueSystemGraphView, position);

            DialogueType = DialogueSystemDialogueType.SingleChoice;

            DialogueSystemChoiceSaveData choiceData = new DialogueSystemChoiceSaveData()
            {
                Text = "Next Dialogue"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();


            //OUTPUT CONTAINER
            foreach (DialogueSystemChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;

                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }

    }
}

