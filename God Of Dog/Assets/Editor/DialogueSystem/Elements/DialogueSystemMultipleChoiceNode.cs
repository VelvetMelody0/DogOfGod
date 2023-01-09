using DialogueSystem.Elements;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using DialogueSystem.Data.Save;
    using DialogueSystem.Utilities;
    using DialogueSystem.Windows;
    using Enumerations;

    public class DialogueSystemMultipleChoiceNode : DialogueSystemNode
    {


        public override void Initialize(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            base.Initialize(nodeName, dialogueSystemGraphView, position);

            DialogueType = DialogueSystemDialogueType.MultipleChoice;

            DialogueSystemChoiceSaveData choiceData = new DialogueSystemChoiceSaveData()
            {
                Text = "New Choice"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();


            //MAIN CONTAINER
            Button addChoiceButton = DialogueSystemElementUtility.CreateButton("Add Choice", () =>
            {
                DialogueSystemChoiceSaveData choiceData = new DialogueSystemChoiceSaveData()
                {
                    Text = "New Choice"
                };

                Choices.Add(choiceData);

                Port choicePort = CreateChoicePort(choiceData);

                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);

            //OUTPUT CONTAINER
            foreach (DialogueSystemChoiceSaveData choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);

                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }


        #region Elements Creation
        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData= userData;

            DialogueSystemChoiceSaveData choiceData = (DialogueSystemChoiceSaveData) userData;

            //HELP choicePort.portName = "";

            Button deleteChoiceButton = DialogueSystemElementUtility.CreateButton("X", () =>
            {
                if(Choices.Count == 1)
                {
                    return;
                }

                if(choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }

                Choices.Remove(choiceData);

                graphView.RemoveElement(choicePort);
            });

            deleteChoiceButton.AddToClassList("ds-node__button");

            TextField choiceTextField = DialogueSystemElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });

            choiceTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__choice-textfield",
                "ds-node__textfield__hidden"
                );

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
        #endregion
    }
}

