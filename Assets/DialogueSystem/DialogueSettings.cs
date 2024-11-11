using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace FloxyDev
{
    [CreateAssetMenu(menuName = "Dialogue/DialogueSettings")]
    public class DialogueSettings : ScriptableObject
    {
        public static DialogueSettings Instance;
        public string[] actorNames;

        public List<ExpressionData> expressionData;

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
}