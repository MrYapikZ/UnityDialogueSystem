using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace FloxyDev
{
    [Serializable]
    public class DialogueLine
    {
        public int nodeID;
        [Header("TextEvent")] public bool isTextEvent;
        [TextArea(3, 5)] public string dialogueText;

        [Dropdown(DropdownType.Actor)] public string selectedActor;

        [Dropdown(DropdownType.Expression)] public string selectedExpression;

        public AudioClip voiceClip;

        [Header("ActionEvent")] public bool isActionEvent;
        public CameraAction cameraAction;
        public List<ActorInScenePosition> actorInScenePosition;
        public UnityEngine.Events.UnityEvent onDialogueEvent;

        [Header("Node Links")] public List<DialogueChoice> choices = new List<DialogueChoice>();
        public int nextNodeID;

        public bool HasChoices => choices.Count > 0;
    }

    [Serializable]
    public struct ActorInScenePosition
    {
        [Dropdown(DropdownType.Actor)] public string selectedActor;
        public GameObject actorGameObject;
        public Vector3 actorStartPosition;
        public Vector3 actorEndPosition;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public int nextNodeID;
    }
}