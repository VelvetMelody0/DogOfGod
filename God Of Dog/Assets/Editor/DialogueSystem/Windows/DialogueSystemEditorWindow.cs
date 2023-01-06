using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace DialogueSystem.Windows
{
    using DialogueSystem.Utilties;
    using Utilities;

    public class DialogueSystemEditorWindow : EditorWindow
    {
        private DialogueSystemGraphView graphView;
        private readonly string defaultFileName = "DialoguesFileName";
        private TextField fileNameTextField;
        private Button saveButton;
        [MenuItem("Window/DialogueSystem/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DialogueSystemEditorWindow>("Dialogue Graph");
        }

        private void CreateGUI()
        {
            AddGraphView();

            AddToolbar();

            AddStyles();
        }

        

        #region Elements Addition
        private void AddGraphView()
        {
            graphView = new DialogueSystemGraphView(this);
            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = DialogueSystemElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            saveButton = DialogueSystemElementUtility.CreateButton("Save", () => Save());

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);

            toolbar.AddStyleSheets("DialogueSystem/DialogueSystemToolbarStyles.uss");

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DialogueSystemVariables.uss");
        }
        #endregion
        #region Toolbar Actions
        private void Save()
        {
            if(string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalid file name.",
                    "Please ensure the file name you've typed in is valid.",
                    "Roger!"
                    );
                return;
            }

            DialogueSystemIOUtility.Initialize(graphView, fileNameTextField.value);
            DialogueSystemIOUtility.Save();
        }

        #endregion

        #region Utility Methods
        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
        #endregion
    }
}

