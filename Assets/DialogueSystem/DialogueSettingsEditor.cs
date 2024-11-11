using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FloxyDev
{
    public class DialogueSettingsEditor : EditorWindow
    {
        private DialogueSettings _dialogueSettings;
        private SerializedObject _serializedSettings;
        private SerializedProperty _actorNames;
        private SerializedProperty _expressionData;

        private Vector2 _scrollPos;
        private int _selectedTab = 0;

        // Dictionary to store foldout states for expressions and expression per actor
        private readonly Dictionary<int, bool> _expressionFoldouts = new Dictionary<int, bool>();

        private readonly Dictionary<int, Dictionary<int, bool>> _expressionPerActorFoldouts =
            new Dictionary<int, Dictionary<int, bool>>();

        [MenuItem("Tools/Dialogue Settings Editor")]
        public static void ShowWindow()
        {
            GetWindow<DialogueSettingsEditor>("Dialogue Settings");
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load the existing DialogueSettings ScriptableObject
            string[] guids = AssetDatabase.FindAssets("t:DialogueSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _dialogueSettings = AssetDatabase.LoadAssetAtPath<DialogueSettings>(path);

                if (_dialogueSettings != null)
                {
                    _serializedSettings = new SerializedObject(_dialogueSettings);
                    _actorNames = _serializedSettings.FindProperty("actorNames");
                    _expressionData = _serializedSettings.FindProperty("expressionData");
                }
            }
        }

        private void OnGUI()
        {
            if (_dialogueSettings == null)
            {
                EditorGUILayout.HelpBox("No DialogueSettings found. Create one in the project.", MessageType.Warning);
                if (GUILayout.Button("Create New DialogueSettings"))
                {
                    CreateNewDialogueSettings();
                }

                return;
            }

            _serializedSettings.Update();

            GUILayout.Space(10);

            // Create Tab buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_selectedTab == 0, "Actors", "Button"))
            {
                _selectedTab = 0;
            }

            if (GUILayout.Toggle(_selectedTab == 1, "Expressions", "Button"))
            {
                _selectedTab = 1;
            }

            if (GUILayout.Toggle(_selectedTab == 2, "Refresh", "Button"))
            {
                _selectedTab = 2;
            }

            GUILayout.EndHorizontal();

            // Scrollable content for each tab
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (_selectedTab == 0)
            {
                DrawActorTab();
            }
            else if (_selectedTab == 1)
            {
                DrawExpressionTab();
            }
            else if (_selectedTab == 2)
            {
                DrawRefreshTab();
            }

            EditorGUILayout.EndScrollView();

            _serializedSettings.ApplyModifiedProperties();
        }

        private void DrawActorTab()
        {
            EditorGUILayout.LabelField("Actor Names", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_actorNames, new GUIContent("Actors"), true);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove Actor"))
            {
                _actorNames.arraySize--;
            }

            if (GUILayout.Button("Add New Actor"))
            {
                _actorNames.arraySize++;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("FloxyDev", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawExpressionTab()
        {
            EditorGUILayout.LabelField("Expressions", EditorStyles.boldLabel);

            // Iterate through expression data
            for (int i = 0; i < _expressionData.arraySize; i++)
            {
                SerializedProperty expressionElement = _expressionData.GetArrayElementAtIndex(i);
                SerializedProperty expressionNames = expressionElement.FindPropertyRelative("expressionNames");
                //SerializedProperty iconType = expressionElement.FindPropertyRelative("iconType");
                SerializedProperty expressionPerActor = expressionElement.FindPropertyRelative("expressionPerActor");

                EditorGUILayout.BeginVertical("box");

                // Retrieve foldout state for each expression
                if (!_expressionFoldouts.ContainsKey(i))
                {
                    _expressionFoldouts[i] = true; // Default to expanded
                }

                // Display foldout for each expression
                _expressionFoldouts[i] = EditorGUILayout.Foldout(_expressionFoldouts[i], expressionNames.stringValue);

                if (_expressionFoldouts[i])
                {
                    EditorGUILayout.PropertyField(expressionNames, new GUIContent("Expression Name"));
                    //EditorGUILayout.PropertyField(iconType, new GUIContent("Icon Type"));

                    // Draw expression per actor list using the custom drawer
                    EditorGUILayout.LabelField("Expressions per Actor", EditorStyles.boldLabel);
                    for (int j = 0; j < expressionPerActor.arraySize; j++)
                    {
                        SerializedProperty actorElement = expressionPerActor.GetArrayElementAtIndex(j);

                        // Initialize foldout state for each actor if not present
                        if (!_expressionPerActorFoldouts.ContainsKey(i))
                        {
                            _expressionPerActorFoldouts[i] = new Dictionary<int, bool>();
                        }

                        if (!_expressionPerActorFoldouts[i].ContainsKey(j))
                        {
                            _expressionPerActorFoldouts[i][j] = true; // Default to expanded
                        }

                        // Display foldout for each ExpressionPerActor
                        //expressionPerActorFoldouts[i][j] = EditorGUILayout.Foldout(expressionPerActorFoldouts[i][j], "Actor " + j);

                        //if (expressionPerActorFoldouts[i][j])
                        //{
                        EditorGUILayout.PropertyField(actorElement, new GUIContent("Expression Per Actor"));

                        // Add a button to remove the ExpressionPerActor
                        if (GUILayout.Button("Remove Actor Expression"))
                        {
                            expressionPerActor.DeleteArrayElementAtIndex(j);
                        }
                        //}
                    }
                    
                    EditorGUILayout.Space(10);

                    // Button to add a new Actor Expression
                    if (GUILayout.Button("Add Actor Expression"))
                    {
                        expressionPerActor.arraySize++;
                    }
                }

                EditorGUILayout.Space(10);

                // Button to remove the expression
                if (GUILayout.Button("Remove Expression"))
                {
                    _expressionData.DeleteArrayElementAtIndex(i);
                }

                EditorGUILayout.EndVertical();
            }

            // Button to add new expression
            if (GUILayout.Button("Add New Expression"))
            {
                _expressionData.arraySize++;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("FloxyDev", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawRefreshTab()
        {
            EditorGUILayout.LabelField("Refresh Cache", EditorStyles.boldLabel);

            if (GUILayout.Button("Refresh Lists"))
            {
                // Call your cache refresh method
                OptimizedDropdownDrawer.RefreshCache();
                Debug.Log("Dropdown lists have been refreshed.");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("FloxyDev", EditorStyles.centeredGreyMiniLabel);
        }

        private void CreateNewDialogueSettings()
        {
            _dialogueSettings = ScriptableObject.CreateInstance<DialogueSettings>();
            AssetDatabase.CreateAsset(_dialogueSettings, "Assets/DialogueSettings.asset");
            AssetDatabase.SaveAssets();
            LoadSettings();
        }
    }

    [CustomEditor(typeof(DialogueSettings))]
    public class DialogueSettingsEditors : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DialogueSettings settings = (DialogueSettings)target;
            if (GUILayout.Button("Refresh Data"))
            {
                // Custom logic to refresh your data
                OptimizedDropdownDrawer.RefreshCache();
            }

            // Draw Actor Names
            EditorGUILayout.PropertyField(serializedObject.FindProperty("actorNames"), true);

            // Draw Expression Data with Refresh Button
            SerializedProperty expressionDataProp = serializedObject.FindProperty("expressionData");

            EditorGUILayout.PropertyField(expressionDataProp, new GUIContent("Expression Data"), true);

            EditorGUILayout.LabelField("FloxyDev", EditorStyles.centeredGreyMiniLabel);

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(DialogueLine))]
    public class DialogueLineEditor : Editor
    {
        private bool _showTextEvent;
        private bool _showActionEvent;

        // Serialized properties for better handling
        private SerializedProperty _nodeID;
        private SerializedProperty _isTextEvent;
        private SerializedProperty _dialogueText;
        private SerializedProperty _selectedActor;
        private SerializedProperty _selectedExpression;
        private SerializedProperty _voiceClip;

        private SerializedProperty _isActionEvent;
        private SerializedProperty _cameraAction;
        private SerializedProperty _actorInScenePosition;
        private SerializedProperty _onNodeEnter;

        private SerializedProperty _choices;
        private SerializedProperty _nextNodeID;

        private void OnEnable()
        {
            // Linking the serialized properties to the fields in DialogueLine
            _nodeID = serializedObject.FindProperty("nodeID");

            _isTextEvent = serializedObject.FindProperty("isTextEvent");
            _dialogueText = serializedObject.FindProperty("dialogueText");
            _selectedActor = serializedObject.FindProperty("selectedActor");
            _selectedExpression = serializedObject.FindProperty("selectedExpression");
            _voiceClip = serializedObject.FindProperty("voiceClip");

            _isActionEvent = serializedObject.FindProperty("isActionEvent");
            _cameraAction = serializedObject.FindProperty("cameraAction");
            _actorInScenePosition = serializedObject.FindProperty("actorInScenePosition");
            _onNodeEnter = serializedObject.FindProperty("onNodeEnter");

            _choices = serializedObject.FindProperty("choices");
            _nextNodeID = serializedObject.FindProperty("nextNodeID");
        }

        public override void OnInspectorGUI()
        {
            // Fetch updated data
            serializedObject.Update();

            // Node ID
            EditorGUILayout.PropertyField(_nodeID);

            // TextEvent Section
            EditorGUILayout.PropertyField(_isTextEvent, new GUIContent("Is Text Event"));
            _showTextEvent = EditorGUILayout.Foldout(_showTextEvent, "Text Event");
            if (_showTextEvent && _isTextEvent.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_dialogueText);
                EditorGUILayout.PropertyField(_selectedActor);
                EditorGUILayout.PropertyField(_selectedExpression);
                EditorGUILayout.PropertyField(_voiceClip);
                EditorGUI.indentLevel--;
            }

            // ActionEvent Section
            EditorGUILayout.PropertyField(_isActionEvent, new GUIContent("Is Action Event"));
            _showActionEvent = EditorGUILayout.Foldout(_showActionEvent, "Action Event");
            if (_showActionEvent && _isActionEvent.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_cameraAction);
                EditorGUILayout.PropertyField(_actorInScenePosition, true);
                EditorGUILayout.PropertyField(_onNodeEnter);
                EditorGUI.indentLevel--;
            }

            // Node Links Section
            EditorGUILayout.PropertyField(_choices, new GUIContent("Choices"), true);
            EditorGUILayout.PropertyField(_nextNodeID);

            // Apply the modified properties
            serializedObject.ApplyModifiedProperties();
        }
    }
}