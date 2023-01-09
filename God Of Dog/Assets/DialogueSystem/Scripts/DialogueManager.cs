using DialogueSystem.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//namespace DialogueSystem
//{
    public class DialogueManager : MonoBehaviour
    {
    #region Switch To Dialogue Trigger?
    /* Dialogue Scriptable Objects */

        [SerializeField] private DialogueSystemDialogueContainerSO dialogueContainer;
        [SerializeField] private DialogueSystemDialogueGroupSO dialogueGroup;
        [SerializeField] private DialogueSystemDialogueSO dialogue;

        /* Filters */
        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        /* Indexes */
        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;
    #endregion
    //textmeshprougui is just text but fancy
    public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogueText;

        public Image charPortraitHolder;

        public Animator animator;

        [Header("Audio")]
        [SerializeField] private AudioClip dialogueTypingSoundClip;
        [SerializeField] private bool stopAudioSource;
        public AudioSource audioSource;

        private int audioDelay;

        private float waitTime = .02f;
        private float shortWaitTime = .4f;
        private float longWaitTime = .8f;

        private Queue<string> sentences;
        private string currentSentence;
        private char[] punctuation = { ',', '.', '?', ':', ';', ' ', '!' };


        private bool isTyping = false;

        private int charactersTyped = 0;

        // Start is called before the first frame update
        void Start()
        {
            sentences = new Queue<string>();
        }

        public void StartDialogue(Dialogue dialogue)
        {
            //gets the sprite and audio from the dialogue/dialoguetrigger script to be used by the dialogue manager
            charPortraitHolder.sprite = dialogue.charPortrait;
            dialogueTypingSoundClip = dialogue.audioClip;
            audioDelay = dialogue.audioDelay;


            animator.SetBool("IsOpen", true);

            nameText.text = dialogue.name;

            sentences.Clear();

            foreach (string sentence in dialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }


            DisplayNextSentence();
        }

        public void DisplayNextSentence()
        {
            if (sentences.Count == 0)
            {
                if (isTyping == false)
                {
                    EndDialogue();
                    return;
                }
            }

            //string sentence = sentences.Dequeue();
            StopAllCoroutines();
            if (isTyping == false)
            {
                currentSentence = sentences.Dequeue();
                charactersTyped = 0;
                StartCoroutine(TypeSentence(currentSentence));
            }
            else if (isTyping == true)
            {
                dialogueText.text = currentSentence;
                //plays audio one time so it doesn't end on a pause in audio;
                if (audioSource.isPlaying == false)
                {
                    audioSource.PlayOneShot(dialogueTypingSoundClip);
                }
                isTyping = false;
            }

        }

        IEnumerator TypeSentence(string sentence)
        {
            isTyping = true;

            dialogueText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                //finds last char in sentence, might be important later
                //bool isLastChar = letter > sentence.Length - 1;

                //adds text to dialogue box one letter at a time
                dialogueText.text += letter;

                if (!punctuation.Contains(letter))
                {
                    if (isTyping == true)
                    {
                        PlayDialogueSound(charactersTyped);
                        charactersTyped++;
                    }

                }
                //determines how long to pause for depending on the char
                if (letter == ',' || letter == ';' || letter == ':')
                {
                    yield return new WaitForSeconds(shortWaitTime);
                }
                else if (letter == '.' || letter == '!' || letter == '?')
                {
                    yield return new WaitForSeconds(longWaitTime);
                }
                else yield return new WaitForSeconds(waitTime);


            }
            isTyping = false;
        }


        public void EndDialogue()
        {
            isTyping = false;
            animator.SetBool("IsOpen", false);
            StopAllCoroutines();
        }

        private void PlayDialogueSound(int currentDisplayedCharacterCount)
        {

            if (currentDisplayedCharacterCount % audioDelay == 0)
            {
                if (stopAudioSource == true)
                {
                    audioSource.Stop();
                }

                audioSource.PlayOneShot(dialogueTypingSoundClip);
            }
        }
    }
//}

