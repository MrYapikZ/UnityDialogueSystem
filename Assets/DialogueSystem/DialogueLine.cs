using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FloxyDev.DialogueSystem
{
    [Serializable]
    public class DialogueLine
    {
        public DLNodeLink nodeLink;
        public DLTextEvent textEvent;
        public DLActionEvent actionEvent;
        public DLuiEvent ui;
    }

    [Serializable]
    public struct DLNodeLink
    {
        [Header("Node Links")] public int nodeID;
        public List<DialogueChoice> choices;
        public int nextNodeID;
    }

    [Serializable]
    public struct DLTextEvent
    {
        [Header("TextEvent")] public bool isTextEvent;
        [Dropdown(DropdownType.Actor)] public string characterSpeaker;
        [TextArea(3, 5)] public string dialogueText;
        [Tooltip("Optional")] public AudioClip voiceClip;
    }

    [Serializable]
    public struct DLuiEvent
    {
        [Header("UI")] public CharacterSide characterShow;

        [Header("RightCharacter")] [Dropdown(DropdownType.Actor)]
        public string selectedActorRight;

        [Dropdown(DropdownType.Expression)] public string selectedExpressionRight;

        [Header("LeftCharacter")] [Dropdown(DropdownType.Actor)]
        public string selectedActorLeft;

        [Dropdown(DropdownType.Expression)] public string selectedExpressionLeft;

        public bool animateWhileSepeak;
    }

    [Serializable]
    public struct DLActionEvent
    {
        [Header("ActionEvent")] public bool isActionEvent;
        public List<CameraEffect> cameraEffects;
        public List<ActorInScenePosition> actorInScenePosition;
        public UnityEngine.Events.UnityEvent onDialogueEvent;
    }


    [Serializable]
    public struct ActorInScenePosition
    {
        public bool useInScenePosition;
        public GameObject actorGameObject;
        public float moveSpeed;
        public Vector3 actorStartPosition;
        public Vector3 actorEndPosition;
    }

    [Serializable]
    public struct CameraEffect
    {
        public CameraAction cameraAction;
        public float effectDuration;
        public float effectAmount;
        public Color effectColor;
        public Transform target;
        public bool isLoop;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public int nextNodeID;
    }
}