using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloxyDev.DialogueSystem
{
    [CreateAssetMenu(menuName = "Dialogue/DialogueSettings")]
    public class DialogueSettings : ScriptableObject
    {
        public static DialogueSettings Instance;
        public string[] actorNames;

        public List<ExpressionData> expressionData;

        public CameraModifier cameraModifier;

        private void Awake()
        {
            Instance = this;
        }
    }

    [Serializable]
    public struct ExpressionData
    {
        public string expressionNames;
        public List<ExpressionPerActor> expressionPerActor;
    }

    [Serializable]
    public struct ExpressionPerActor
    {
        [Dropdown(DropdownType.Actor)] public string selectedActor;
        public float frameRate;
        public List<Sprite> expressionSprite;
    }

    [Serializable]
    public struct CameraModifier
    {
        [Header("Shake Settings")] public float shakeAmplitude;
        public float shakeFrequency;
    }
}