using DialogueSystem.Elements;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Windows
{
    using DialogueSystem.Utilties;
    using Elements;
    using Enumerations;
    //using Utilities;
    using Data.Error;
    using System.Xml.Linq;
    using UnityEngine.Analytics;
    using DialogueSystem.Data.Save;
    using static UnityEditor.Experimental.GraphView.GraphView;

    public class DialogueSystemGraphView : GraphView
    {
        private DialogueSystemEditorWindow editorWindow;
        private DialogueSystemSearchWindow searchWindow;

        private SerializableDictionary<string, DialogueSystemNodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, DialogueSystemGroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, DialogueSystemNodeErrorData>> groupedNodes;

        private int nameErrorsAmount;

        public int NameErrorsAmount
        {
            get
            {
                return nameErrorsAmount;
            }

            set
            {
                nameErrorsAmount = value;

                if(nameErrorsAmount ==0)
                {
                    editorWindow.EnableSaving();
                }
                if(nameErrorsAmount == 1)
                {
                    editorWindow.DisableSaving();
                }
            }

        }
        public DialogueSystemGraphView(DialogueSystemEditorWindow dialogueSystemEditorWindow)
        {
            editorWindow = dialogueSystemEditorWindow;

            ungroupedNodes = new SerializableDictionary<string, DialogueSystemNodeErrorData>();
            groups = new SerializableDictionary<string, DialogueSystemGroupErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DialogueSystemNodeErrorData>>();

            AddManipulators();

            AddSearchWindow();

            AddGridBackground();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
        }



        #region Overrided Methods
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }
                if (startPort.node == port.node)
                {
                    return;
                }
                if (startPort.direction == port.direction)
                {
                    return;
                }
                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        #endregion
        #region Manipulators
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContexualMenu("Add Node(Single Choice)", DialogueSystemDialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContexualMenu("Add Node(Multiple Choice)", DialogueSystemDialogueType.MultipleChoice));

            this.AddManipulator(CreateGroupContexualMenu());
        }

        private IManipulator CreateGroupContexualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
                );


            return contextualMenuManipulator;
        }



        private IManipulator CreateNodeContexualMenu(string actionTitle, DialogueSystemDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );


            return contextualMenuManipulator;
        }
        #endregion
        #region Elements Creation
        public DialogueSystemGroup CreateGroup(string title, Vector2 localMousePosition)
        {
            DialogueSystemGroup group = new DialogueSystemGroup(title, localMousePosition);

            AddGroup(group);

            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if(!(selectedElement is DialogueSystemNode))
                {
                    continue;
                }

                DialogueSystemNode node = (DialogueSystemNode)selectedElement;

                group.AddElement(node);
            }
            return group;
        }

        

        public DialogueSystemNode CreateNode(DialogueSystemDialogueType dialogueType, Vector2 position)
        {

            Type nodeType = Type.GetType($"DialogueSystem.Elements.DialogueSystem{dialogueType}Node");
            DialogueSystemNode node = (DialogueSystemNode)Activator.CreateInstance(nodeType);

            node.Initialize(this, position);
            node.Draw();

            AddUngroupedNode(node);

            return node;
        }
        #endregion

        #region Callbacks
        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(DialogueSystemGroup);
                Type edgeType = typeof(UnityEditor.Experimental.GraphView.Edge);


                List<DialogueSystemGroup> groupsToDelete = new List<DialogueSystemGroup>();
                List<UnityEditor.Experimental.GraphView.Edge> edgesToDelete = new List<UnityEditor.Experimental.GraphView.Edge>();
                List<DialogueSystemNode> nodesToDelete = new List<DialogueSystemNode>();
                foreach(GraphElement element in selection)
                {
                    if(element is DialogueSystemNode node)
                    {
                        nodesToDelete.Add(node);

                        continue;
                    }

                    if(element.GetType() == edgeType)
                    {
                        UnityEditor.Experimental.GraphView.Edge edge = (UnityEditor.Experimental.GraphView.Edge)element;

                        edgesToDelete.Add(edge);

                        continue;
                    }    

                    if(element.GetType() != groupType)
                    {
                        continue;
                    }
                    DialogueSystemGroup group = (DialogueSystemGroup) element;
                    

                    groupsToDelete.Add(group);

                }

                foreach (DialogueSystemGroup group in groupsToDelete)
                {
                    List<DialogueSystemNode> groupNodes = new List<DialogueSystemNode>();

                    foreach (GraphElement groupElement in group.containedElements)
                    {
                        if(!(groupElement is DialogueSystemNode))
                        {
                            continue;
                        }

                        DialogueSystemNode groupNode = (DialogueSystemNode) groupElement;

                        groupNodes.Add((DialogueSystemNode)groupElement);
                    }

                    group.RemoveElements(groupNodes);

                    RemoveGroup(group);

                    RemoveElement(group);
                }

                DeleteElements(edgesToDelete);

                foreach(DialogueSystemNode node in nodesToDelete)
                {
                    if(node.Group!= null)
                    {
                        node.Group.RemoveElement(node);
                    }

                    RemoveUngroupedNode(node);

                    node.DisconnectAllPorts();

                    RemoveElement(node);
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if(!(element is DialogueSystemNode))
                    {
                        continue;
                    }
                    DialogueSystemGroup nodeGroup = (DialogueSystemGroup) group;

                    DialogueSystemNode node = (DialogueSystemNode) element;

                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, nodeGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DialogueSystemNode))
                    {
                        continue;
                    }
                    DialogueSystemNode node = (DialogueSystemNode)element;

                    RemoveGroupedNode(node, group);
                    AddUngroupedNode(node);
                }

            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DialogueSystemGroup dialogueSystemGroup = (DialogueSystemGroup)group;

                dialogueSystemGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(dialogueSystemGroup.title))
                {
                    if (!string.IsNullOrEmpty(dialogueSystemGroup.OldTitle))
                    {
                        ++NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dialogueSystemGroup.OldTitle))
                    {
                        --NameErrorsAmount;
                    }
                }

                RemoveGroup(dialogueSystemGroup);

                dialogueSystemGroup.OldTitle = dialogueSystemGroup.title;

                AddGroup(dialogueSystemGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if(changes.edgesToCreate != null)
                {
                    foreach(UnityEditor.Experimental.GraphView.Edge edge in changes.edgesToCreate)
                    {
                        DialogueSystemNode nextNode = (DialogueSystemNode) edge.input.node;

                        DialogueSystemChoiceSaveData choiceData = (DialogueSystemChoiceSaveData) edge.output.userData;

                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if(changes.elementsToRemove!= null)
                {
                    Type edgeType = typeof(UnityEditor.Experimental.GraphView.Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if(element.GetType() != edgeType)
                        {
                            UnityEditor.Experimental.GraphView.Edge edge = (UnityEditor.Experimental.GraphView.Edge) element;

                            DialogueSystemChoiceSaveData choiceData = (DialogueSystemChoiceSaveData) edge.output.userData;

                            choiceData.NodeID = "";
                        }
                    }
                }

                return changes;
            };
        }
        #endregion

        #region Repeated Elements
        public void AddUngroupedNode(DialogueSystemNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                DialogueSystemNodeErrorData nodeErrorData = new DialogueSystemNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                ungroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }

            List<DialogueSystemNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Add(node);

            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (ungroupedNodesList.Count == 2)
            {
                ++NameErrorsAmount;
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(DialogueSystemNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            List<DialogueSystemNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Remove(node);

            node.ResetStyle();

            if (ungroupedNodesList.Count == 1)
            {
                --NameErrorsAmount;
                ungroupedNodesList[0].ResetStyle();
                return;
            }

            if (ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        private void AddGroup(DialogueSystemGroup group)
        {
            string groupName = group.title.ToLower();
            if(!groups.ContainsKey(groupName))
            {
                DialogueSystemGroupErrorData groupErrorData = new DialogueSystemGroupErrorData();

                groupErrorData.Groups.Add(group);

                groups.Add(groupName, groupErrorData);

                return;
            }

            List<DialogueSystemGroup> groupsList = groups[groupName].Groups;

            groupsList.Add(group);

            Color errorColor = groups[groupName].ErrorData.Color;

            group.SetErrorStyle(errorColor);

            if(groupsList.Count == 2)
            {
                ++NameErrorsAmount;
                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        private void RemoveGroup(DialogueSystemGroup group)
        {
            string oldGroupName = group.OldTitle.ToLower();

            List<DialogueSystemGroup> groupsList = groups[oldGroupName].Groups;

            groupsList.Remove(group);

            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --NameErrorsAmount;
                groupsList[0].ResetStyle();

                return;
            }

            if (groupsList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }
        }

        public void AddGroupedNode(DialogueSystemNode node, DialogueSystemGroup group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = group;

            if(!groupedNodes.ContainsKey(group))
            {
                groupedNodes.Add(group, new SerializableDictionary<string, DialogueSystemNodeErrorData>());
            }

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                DialogueSystemNodeErrorData nodeErrorData = new DialogueSystemNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            List<DialogueSystemNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Add(node);

            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                ++NameErrorsAmount;
                groupedNodesList[0].SetErrorStyle(errorColor);
            }

        }

        public void RemoveGroupedNode(DialogueSystemNode node, Group group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = null;

            List<DialogueSystemNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);

            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {   
                --NameErrorsAmount;

                groupedNodesList[0].ResetStyle();

                return;
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }
        #endregion
        #region Elements Addition
        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DialogueSystemSearchWindow>();

                searchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void AddGridBackground()
        {
            GridBackground gridbackground = new GridBackground();

            gridbackground.StretchToParentSize();

            Insert(0, gridbackground);
        }



        private void AddStyles()
        {
            this.AddStyleSheets(
                "DialogueSystem/DialogueSystemGraphViewStyles.uss",
                "DialogueSystem/DialogueSystemNodeStyles.uss"
                );
        }
        #endregion

        #region Utilities
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSeachWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if(isSeachWindow)
            {
                worldMousePosition -= editorWindow.position.position;
            };

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }
        #endregion
    }
}

