using System;
using Unity.VisualScripting;
using UnityEngine;

namespace FloxyDev
{
    public class DialogueTrigger : MonoBehaviour
    {
        public DialogueActivator dialogueActivator;

        public void StartDialogue()
        {
            DialogueSystem.Instance.StartDialogue(dialogueActivator);
        }
    }
}