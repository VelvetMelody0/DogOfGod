using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public bool startDialogueOnEnter;

    private void Start()
    {
        startDialogueOnEnter = dialogue.startDialogueOnTriggerEnter;
    }

    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
    }

    public void StopDialogue()
    {
        FindObjectOfType<DialogueManager>().EndDialogue();
    }
}
