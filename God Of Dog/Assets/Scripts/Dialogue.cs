using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Dialogue
{
    public Sprite charPortrait;
    public string name;
    public AudioClip audioClip;
    public int audioDelay = 4;
    public bool startDialogueOnTriggerEnter;

    [TextArea(3,10)]
    public string[] sentences;



}
