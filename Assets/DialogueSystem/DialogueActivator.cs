using System.Collections.Generic;
using UnityEngine;

namespace FloxyDev
{
    [CreateAssetMenu(menuName = "Dialogue/DialogueActivator")]
    public class DialogueActivator : ScriptableObject
    {
        public List<DialogueLine> dialogueLines;
    }
}