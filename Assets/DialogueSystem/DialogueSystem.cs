using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FloxyDev
{
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance;
        [Header("UI Elements")] [SerializeField]
        private GameObject dialogueBox;

        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button nextDialogueButton;
        [SerializeField] private Image characterPortrait;

        [SerializeField, Tooltip("Optional")] private TextMeshProUGUI dialogueHistoryText;

        [SerializeField, Tooltip("If there is an option")]
        private GameObject choicesPanel;

        [SerializeField, Tooltip("If there is an option")]
        private Button choiceButtonPrefab;

        [Header("Settings")] 
        [SerializeField] private DialogueSettings dialogueSettings;
        [SerializeField] private float typingSpeed = 0.05f;
        [SerializeField] private AudioClip typingSound;
        [SerializeField] private bool enableSkip;
        [SerializeField] private AudioSource audioSource;

        private DialogueActivator _dialogueActivator;
        private Dictionary<string, bool> _dialogueConditions = new Dictionary<string, bool>();
        private Button _choiceButton;
        private int _dialogueIndex;
        private int _selectedChoice;
        private bool _isTyping;
        private readonly List<string> _dialogueHistory = new List<string>();
        private ExpressionPerActor _actorExpression;
        private int _currentFrame = 0;
        private float _timer = 0f;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            nextDialogueButton.onClick.AddListener(DisplayNextLine);
            dialogueBox.SetActive(false);
        }

        private void Update()
        {
            if (_isTyping) SpriteAnimator();
        }

        public void StartDialogue(DialogueActivator dialogue)
        {
            _dialogueActivator = dialogue;
            _dialogueIndex = 0;
            dialogueBox.SetActive(true);
            dialogueText.text = "";
            DisplayNextLine();
        }

        private void DisplayNextLine()
        {
            if (_isTyping) return;

            if (_dialogueIndex < _dialogueActivator.dialogueLines.Count)
            {
                DialogueLine line = _dialogueActivator.dialogueLines[_dialogueIndex];
                line.onDialogueEvent?.Invoke();

                ShowDialogueLine(line);
            }
            else
            {
                EndDialogue();
            }
        }

        private void ShowDialogueLine(DialogueLine line)
        {
            StartCoroutine(TypeDialogue(line));

            if (line.voiceClip != null)
            {
                audioSource.PlayOneShot(line.voiceClip);
            }

            _dialogueHistory.Add($"{line.selectedActor}: {line.dialogueText}");
            UpdateDialogueHistory();

            if (choiceButtonPrefab != null && choicesPanel != null)
            {
                if (line.choices != null && line.choices.Count > 0)
                {
                    nextDialogueButton.gameObject.SetActive(false);
                    ShowChoices(line.choices);
                }
                else
                {
                    nextDialogueButton.gameObject.SetActive(true);
                }
            }
        }

        private IEnumerator TypeDialogue(DialogueLine line)
        {
            Debug.Log(line.dialogueText);
            _isTyping = true;
            speakerNameText.text = line.selectedActor;
            ExpressionData expressionData =
                dialogueSettings.expressionData.FirstOrDefault(ed =>
                    ed.expressionNames == line.selectedExpression);
            _actorExpression = expressionData.expressionPerActor
                .FirstOrDefault(exp => exp.selectedActor == line.selectedActor);
            characterPortrait.sprite = _actorExpression.expressionSprite[0];

            dialogueText.text = "";
            foreach (char letter in line.dialogueText.ToCharArray())
            {
                dialogueText.text += letter;
                PlayTypingSound();
                yield return new WaitForSeconds(typingSpeed);
            }

            _isTyping = false;
            _dialogueIndex = line.nextNodeID;
        }

        private void PlayTypingSound()
        {
            if (typingSound != null)
            {
                audioSource.PlayOneShot(typingSound);
            }
        }

        private void ShowChoices(List<DialogueChoice> choices)
        {
            foreach (Transform child in choicesPanel.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (DialogueChoice choice in choices)
            {
                Button choiceButton = Instantiate(choiceButtonPrefab, choicesPanel.transform);
                choiceButton.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;
                choiceButton.onClick.AddListener(() => OnChoiceSelected(choice));
            }
        }

        private void OnChoiceSelected(DialogueChoice choice)
        {
            choicesPanel.SetActive(false);
            _dialogueIndex = choice.nextNodeID;
            DisplayNextLine();
        }

        private void EndDialogue()
        {
            dialogueBox.SetActive(false);
        }

        private void UpdateDialogueHistory()
        {
            if (dialogueHistoryText != null)
            {
                dialogueHistoryText.text = string.Join("\n", _dialogueHistory);
            }
        }

        private void SpriteAnimator()
        {
            if (_actorExpression.expressionSprite.Count == 0 && _actorExpression.expressionSprite == null) return;

            _timer += Time.deltaTime;
            if (_timer >= _actorExpression.frameRate)
            {
                _timer = 0;
                _currentFrame++;
                if (_currentFrame >= _actorExpression.expressionSprite.Count)
                {
                    _currentFrame = 0;
                }

                characterPortrait.sprite = _actorExpression.expressionSprite[_currentFrame];
            }
        }
    }
}