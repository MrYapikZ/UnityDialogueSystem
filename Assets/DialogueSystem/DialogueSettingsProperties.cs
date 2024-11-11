using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FloxyDev
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
        private static string[] cachedActorNames;
        private static string[] cachedExpressionNames;
        private static bool cacheNeedsRefresh = true;

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
            cacheNeedsRefresh = true;
            cachedActorNames = FetchDataFromAssets("actorNames");
            cachedExpressionNames = FetchDataFromAssets("expressionNames");
        }

        private static string[] GetActorNames()
        {
            if (cacheNeedsRefresh || cachedActorNames == null)
            {
                cachedActorNames = FetchDataFromAssets("actorNames");
            }

            return cachedActorNames;
        }

        private static string[] GetExpressionNames()
        {
            if (cacheNeedsRefresh || cachedExpressionNames == null)
            {
                cachedExpressionNames = FetchDataFromAssets("expressionNames");
            }

            return cachedExpressionNames;
        }

        private static string[] FetchDataFromAssets(string field)
        {
            cacheNeedsRefresh = false;
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
        DipInBlack,
        DipInWhite,
        DipOutBlack,
        DipOutWhite,
        OpenEye,
        CloseEye
    }

    #endregion
}