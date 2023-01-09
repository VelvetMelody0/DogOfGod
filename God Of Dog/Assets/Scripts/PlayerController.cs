using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //public object Player;
    public float maxSpeed = 5f;
    public float currentSpeed;
    public float acceleration = .05f;
    public float deceleration = .05f;
    public Rigidbody2D rb;

    //maybe use dictionary to add an int for OnTriggerEnter to see which trigger was walked into first/most recent, and then assign a bool to say whether or not you can talk to trigger depending on blah blah
    //private Dictionary<int, bool> {int charNumber, 
    public bool inConversation = false;
    public bool canStartConversation = false;
    public GameObject lastCollision;

    [SerializeField]
    private DialogueManager dialogueManager;

    Vector2 movement;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //input goes here
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        if(Input.GetButtonDown("Interact"))
        {
            if (inConversation == true)
            {
                dialogueManager.DisplayNextSentence();
            }
            if(canStartConversation == true)
            {
                canStartConversation = false;
                lastCollision.GetComponent<DialogueTrigger>().TriggerDialogue();
                inConversation = true;
            }
        }
    }

    private void FixedUpdate()
    {
        //physics and movement here
        rb.MovePosition(rb.position + movement * maxSpeed *Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //add another box colider on player pointing straight out, use that box collider only for these triggers to choose the right person to talk to
        lastCollision = collision.gameObject;
        if(collision.gameObject.tag == "NPC")
        {

            if (!inConversation)
            {
                if (collision.gameObject.GetComponent<DialogueTrigger>().startDialogueOnEnter == true)
                {
                    Debug.Log("startDialogueEnter == true");
                    inConversation = true;
                    collision.gameObject.GetComponent<DialogueTrigger>().TriggerDialogue();
                }
                else if(canStartConversation == false)
                {
                    canStartConversation = true;
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "NPC")
        {
            //inConversation = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "NPC")
        {
            collision.gameObject.GetComponent<DialogueTrigger>().StopDialogue();
            inConversation= false;
            canStartConversation = false;
        }
    }

}
