using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FloxyDev.DialogueSystem
{
    public enum DropdownType
    {
        Actor,
        Expression
    }

    public class DropdownAttribute : PropertyAttribute
    {
        public DropdownType Type;
        public DropdownAttribute(DropdownType type) => Type = type;
    }

    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public class OptimizedDropdownDrawer : PropertyDrawer
    {
        private static string[] _cachedActorNames;
        private static string[] _cachedExpressionNames;
        private static bool _cacheNeedsRefresh = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DropdownAttribute dropdownAttr = (DropdownAttribute)attribute;
            string[] options = dropdownAttr.Type == DropdownType.Actor ? GetActorNames() : GetExpressionNames();

            int currentIndex = System.Array.IndexOf(options, property.stringValue);
            currentIndex = EditorGUI.Popup(position, label.text, currentIndex, options);

            if (currentIndex >= 0)
            {
                property.stringValue = options[currentIndex];
            }
        }

        public static void RefreshCache()
        {
            _cacheNeedsRefresh = true;
            _cachedActorNames = FetchDataFromAssets("actorNames");
            _cachedExpressionNames = FetchDataFromAssets("expressionNames");
        }

        private static string[] GetActorNames()
        {
            if (_cacheNeedsRefresh || _cachedActorNames == null)
            {
                _cachedActorNames = FetchDataFromAssets("actorNames");
            }

            return _cachedActorNames;
        }

        private static string[] GetExpressionNames()
        {
            if (_cacheNeedsRefresh || _cachedExpressionNames == null)
            {
                _cachedExpressionNames = FetchDataFromAssets("expressionNames");
            }

            return _cachedExpressionNames;
        }

        private static string[] FetchDataFromAssets(string field)
        {
            _cacheNeedsRefresh = false;
            List<string> names = new List<string>();

            var guids = AssetDatabase.FindAssets("t:DialogueSettings");
            foreach (var guid in guids)
            {
                var dialogueSettings =
                    AssetDatabase.LoadAssetAtPath<DialogueSettings>(AssetDatabase.GUIDToAssetPath(guid));
                if (dialogueSettings != null)
                {
                    if (field == "actorNames" && dialogueSettings.actorNames != null)
                        names.AddRange(dialogueSettings.actorNames);

                    if (field == "expressionNames" && dialogueSettings.expressionData != null)
                    {
                        foreach (var expr in dialogueSettings.expressionData)
                        {
                            names.Add(expr.expressionNames);
                        }
                    }
                }
            }

            return names.ToArray();
        }
    }


    #region CameraAction

    public enum CameraAction
    {
        None,
        Shake,
        Flash,
        ZoomIn,
        ZoomOut,
        FadeIn,
        FadeOut
    }

    #endregion

    #region CharacterSide

    public enum CharacterSide
    {
        None,
        Right,
        Left,
        Both
    }

    #endregion
}