using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloxyDev.DialogueSystem
{
    public class DialogueActivator : MonoBehaviour
    {
        public static DialogueActivator Instance;
        public List<DialogueLine> dialogueLines;

        private void Awake()
        {
            Instance = this;
        }
    }
}