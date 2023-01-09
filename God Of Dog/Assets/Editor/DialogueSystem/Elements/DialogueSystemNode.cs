using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using DialogueSystem.Data.Save;
    using DialogueSystem.Utilities;
    using DialogueSystem.Windows;
    using Enumerations;
    using System.Linq;


    public class DialogueSystemNode: Node
    {
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<DialogueSystemChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DialogueSystemDialogueType DialogueType { get; set; }
        public DialogueSystemGroup Group { get; set; }

        protected DialogueSystemGraphView graphView;

        private Color defaultBackgroundColor;

        public virtual void Initialize(string nodeName, DialogueSystemGraphView dialogueSystemGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            DialogueName = nodeName;
            Choices = new List<DialogueSystemChoiceSaveData>();
            Text = "Dialogue text.";

            graphView= dialogueSystemGraphView;
            defaultBackgroundColor = new Color(29f/255f, 29f/255f, 30/255f);

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public virtual void Draw()
        {
            //TITLE CONTAINER
            TextField dialogueNameTextField = DialogueSystemElementUtility.CreateTextField(DialogueName, null, callback =>
            {
                TextField target = (TextField) callback.target;

                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if(string.IsNullOrEmpty(target.value))
                {
                    if(!string.IsNullOrEmpty(DialogueName))
                    {
                        ++graphView.NameErrorsAmount;
                    }
                }
                else
                {
                    if(string.IsNullOrEmpty(DialogueName))
                    {
                        --graphView.NameErrorsAmount;
                    }
                }

                if(Group == null)
                {
                    graphView.RemoveUngroupedNode(this);

                    DialogueName = target.value;

                    graphView.AddUngroupedNode(this);
                    return;
                }

                DialogueSystemGroup currentGroup = Group;

                graphView.RemoveGroupedNode(this, Group);

                DialogueName = callback.newValue;

                graphView.AddGroupedNode(this, currentGroup);
            });

            dialogueNameTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__filename-textfield",
                "ds-node__textfield__hidden"
                );

            titleContainer.Insert(0, dialogueNameTextField);

            //IMPUT CONTAINER
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);

            //EXTENSIONS CONTAINER
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = DialogueSystemElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = DialogueSystemElementUtility.CreateTextArea(Text, null, callback =>
            {
                Text = callback.newValue;
            });

            textTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__quote-textfield"
                );

            textFoldout.Add(textTextField);
            
            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);

            
        }

        #region Overrided Methods
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }
        #endregion

        #region Utilitiy Methods

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach(Port port in container.Children())
            {
                if(!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port) inputContainer.Children().First();

            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor= color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
        #endregion
    }
}