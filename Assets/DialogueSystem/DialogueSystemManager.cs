using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace FloxyDev.DialogueSystem
{
    public class DialogueSystemManager : MonoBehaviour
    {
        public static DialogueSystemManager Instance;

        [Header("UI Elements")] [SerializeField]
        private GameObject dialogueBox;

        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button nextDialogueButton;
        [SerializeField] private Image characterPortraitRight;
        [SerializeField] private Image characterPortraitLeft;

        [Header("Camera Settings")] [SerializeField]
        private CinemachineCamera dialogueCam;

        [SerializeField] private CanvasGroup camEffect;

        [Header("Settings")] [SerializeField] private DialogueSettings dialogueSettings;
        [SerializeField] private float typingSpeed = 0.05f;
        [SerializeField] private AudioClip typingSound;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private bool typingPerCharacter;
        [SerializeField] private bool enableSkip;

        [Header("Optional")] [SerializeField, Tooltip("Optional")]
        private TextMeshProUGUI dialogueHistoryText;

        [SerializeField, Tooltip("If there is an option")]
        private GameObject choicesPanel;

        [SerializeField, Tooltip("If there is an option")]
        private Button choiceButtonPrefab;

        private DialogueActivator _dialogueActivator;
        private Dictionary<string, bool> _dialogueConditions = new Dictionary<string, bool>();
        private Button _choiceButton;
        private int _dialogueIndex;
        private int _selectedChoice;
        private bool _isTyping;
        private bool _isSkipPressed;
        private readonly List<string> _dialogueHistory = new List<string>();
        private ExpressionPerActor _actorExpressionRight;
        private ExpressionPerActor _actorExpressionLeft;
        private int _currentFrameRight;
        private float _timerRight;
        private int _currentFrameLeft;
        private float _timerLeft;
        private CinemachineBasicMultiChannelPerlin _noiseProfile;
        private Coroutine _shakeCoroutine;
        private Transform _originalFollowTarget;
        private Vector3 _originalPosition;
        private float _originalOrtho;
        private bool _stopCamEffect;

        private void Awake()
        {
            Instance = this;
            if (dialogueCam != null)
            {
                _noiseProfile = dialogueCam.gameObject.GetComponent<CinemachineBasicMultiChannelPerlin>();
                _originalFollowTarget = dialogueCam.Follow;
                _originalPosition = dialogueCam.transform.position;
                _originalOrtho = dialogueCam.Lens.OrthographicSize;
            }
        }

        private void Start()
        {
            nextDialogueButton.onClick.AddListener(NextPress);
            dialogueBox.SetActive(false);
            camEffect.gameObject.SetActive(false);
        }

        private void Update()
        {
            UpdateSpriteAnimation(ref _actorExpressionRight, ref _timerRight, ref _currentFrameRight,
                characterPortraitRight);
            UpdateSpriteAnimation(ref _actorExpressionLeft, ref _timerLeft, ref _currentFrameLeft,
                characterPortraitLeft);
        }

        public void StartDialogue(DialogueActivator dialogue)
        {
            _dialogueActivator = dialogue;
            _dialogueIndex = 0;
            dialogueBox.SetActive(true);
            dialogueText.text = "";
            DisplayNextLine();
        }

        private void NextPress()
        {
            _isSkipPressed = true;
            DisplayNextLine();
            if (!_isTyping) _stopCamEffect = true;
        }

        private void DisplayNextLine()
        {
            if (_isTyping) return;
            _isSkipPressed = false;
            _noiseProfile.AmplitudeGain = 0f;
            _noiseProfile.FrequencyGain = 0f;
            if (_dialogueIndex < _dialogueActivator.dialogueLines.Count)
            {
                DialogueLine line = _dialogueActivator.dialogueLines[_dialogueIndex];
                if (line.actionEvent.isActionEvent)
                {
                    line.actionEvent.onDialogueEvent?.Invoke();

                    for (int i = 0; i < line.actionEvent.cameraEffects.Count; i++)
                    {
                        switch (line.actionEvent.cameraEffects[i].cameraAction)
                        {
                            case (CameraAction.None):
                                break;
                            case (CameraAction.Shake):
                                ShakeCamera(line.actionEvent.cameraEffects[i].effectDuration,
                                    line.actionEvent.cameraEffects[i].isLoop);
                                break;
                            case (CameraAction.Flash):
                                Flash(line.actionEvent.cameraEffects[i].effectDuration,
                                    line.actionEvent.cameraEffects[i].effectAmount,
                                    line.actionEvent.cameraEffects[i].effectColor,
                                    line.actionEvent.cameraEffects[i].isLoop);
                                break;
                            case (CameraAction.ZoomIn):
                                ZoomToTarget(line.actionEvent.cameraEffects[i].target,
                                    line.actionEvent.cameraEffects[i].effectAmount,
                                    line.actionEvent.cameraEffects[i].effectDuration);
                                break;
                            case (CameraAction.ZoomOut):
                                ZoomToDefault(line.actionEvent.cameraEffects[i].effectDuration);
                                break;
                            case (CameraAction.FadeIn):
                                DipToBlack(line.actionEvent.cameraEffects[i].effectDuration,
                                    line.actionEvent.cameraEffects[i].effectColor);
                                break;
                            case (CameraAction.FadeOut):
                                DipToDefault(line.actionEvent.cameraEffects[i].effectDuration,
                                    line.actionEvent.cameraEffects[i].effectColor);
                                break;
                        }
                    }

                    for (int i = 0; i < line.actionEvent.actorInScenePosition.Count; i++)
                    {
                        MoveActor(line.actionEvent.actorInScenePosition[i].actorGameObject,
                            line.actionEvent.actorInScenePosition[i].actorStartPosition,
                            line.actionEvent.actorInScenePosition[i].actorEndPosition,
                            line.actionEvent.actorInScenePosition[i].moveSpeed,
                            line.actionEvent.actorInScenePosition[i].useInScenePosition);
                    }
                }

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
            if (line.textEvent.voiceClip != null)
            {
                audioSource.PlayOneShot(line.textEvent.voiceClip);
            }

            _dialogueHistory.Add($"{line.textEvent.characterSpeaker}: {line.textEvent.dialogueText}");
            UpdateDialogueHistory();

            if (choiceButtonPrefab != null && choicesPanel != null)
            {
                if (line.nodeLink.choices != null && line.nodeLink.choices.Count > 0)
                {
                    nextDialogueButton.gameObject.SetActive(false);
                    ShowChoices(line.nodeLink.choices);
                }
                else
                {
                    nextDialogueButton.gameObject.SetActive(true);
                }
            }
        }

        private ExpressionPerActor GetActorExpression(string expressionName, string actorName)
        {
            var expressionData = dialogueSettings.expressionData
                .FirstOrDefault(ed => ed.expressionNames == expressionName);

            return expressionData.expressionPerActor
                .FirstOrDefault(exp => exp.selectedActor == actorName);
        }

        private IEnumerator TypeDialogue(DialogueLine line)
        {
            Debug.Log(line.textEvent.dialogueText);
            _isTyping = true;
            speakerNameText.text = line.textEvent.characterSpeaker;
            switch (line.ui.characterShow)
            {
                case CharacterSide.None:
                    characterPortraitRight.sprite = null;
                    characterPortraitLeft.sprite = null;
                    characterPortraitRight.gameObject.SetActive(false);
                    characterPortraitLeft.gameObject.SetActive(false);
                    break;

                case CharacterSide.Right:
                    _actorExpressionRight =
                        GetActorExpression(line.ui.selectedExpressionRight, line.ui.selectedActorRight);
                    characterPortraitRight.sprite = _actorExpressionRight.expressionSprite[0];
                    characterPortraitLeft.sprite = null;
                    characterPortraitRight.gameObject.SetActive(true);
                    characterPortraitLeft.gameObject.SetActive(false);
                    break;

                case CharacterSide.Left:
                    _actorExpressionLeft =
                        GetActorExpression(line.ui.selectedExpressionLeft, line.ui.selectedActorLeft);
                    characterPortraitLeft.sprite = _actorExpressionLeft.expressionSprite[0];
                    characterPortraitRight.sprite = null;
                    characterPortraitRight.gameObject.SetActive(false);
                    characterPortraitLeft.gameObject.SetActive(true);
                    break;

                case CharacterSide.Both:
                    _actorExpressionRight =
                        GetActorExpression(line.ui.selectedExpressionRight, line.ui.selectedActorRight);
                    _actorExpressionLeft =
                        GetActorExpression(line.ui.selectedExpressionLeft, line.ui.selectedActorLeft);
                    characterPortraitRight.sprite = _actorExpressionRight.expressionSprite[0];
                    characterPortraitLeft.sprite = _actorExpressionLeft.expressionSprite[0];
                    characterPortraitRight.gameObject.SetActive(true);
                    characterPortraitLeft.gameObject.SetActive(true);
                    break;
            }

            dialogueText.text = "";
            _isSkipPressed = false;
            foreach (char letter in line.textEvent.dialogueText)
            {
                dialogueText.text += letter;
                PlayTypingSound();

                if (enableSkip && _isSkipPressed)
                {
                    dialogueText.text = line.textEvent.dialogueText;
                    break;
                }

                yield return new WaitForSeconds(typingSpeed);
            }

            _isTyping = false;
            _dialogueIndex = line.nodeLink.nextNodeID;
        }

        private void PlayTypingSound()
        {
            if (typingSound != null && !audioSource.isPlaying || typingPerCharacter)
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

        void UpdateSpriteAnimation(ref ExpressionPerActor actorExpression, ref float timer, ref int currentFrame,
            Image characterPortrait)
        {
            if (actorExpression.expressionSprite == null || actorExpression.expressionSprite == null ||
                actorExpression.expressionSprite.Count == 0) return;

            //if (actorExpression.expressionSprite.Count == 1)

            timer += Time.deltaTime;
            if (timer >= actorExpression.frameRate)
            {
                timer = 0;
                currentFrame = (currentFrame + 1) % actorExpression.expressionSprite.Count;
                characterPortrait.sprite = actorExpression.expressionSprite[currentFrame];
            }
        }

        #region CameraEffect

        public void ZoomToDefault(float duration)
        {
            StartCoroutine(ZoomToTargetCoroutine(_originalFollowTarget, _originalOrtho, duration, false));
        }

        public void ZoomToTarget(Transform target, float zoomAmount, float zoomDuration)
        {
            if (dialogueCam != null && target != null)
            {
                StartCoroutine(ZoomToTargetCoroutine(target, zoomAmount, zoomDuration, true));
            }
        }

        private IEnumerator ZoomToTargetCoroutine(Transform target, float zoomAmount, float duration, bool toTarget)
        {
            if (toTarget)
            {
                _originalOrtho = dialogueCam.Lens.OrthographicSize;
                _originalFollowTarget = dialogueCam.Follow;
                _originalPosition = dialogueCam.transform.position;
            }

            dialogueCam.Follow = target;
            float timer = 0f;

            while (timer < duration)
            {
                dialogueCam.Lens.OrthographicSize = Mathf.Lerp(_originalOrtho, zoomAmount, timer / duration);
                dialogueCam.transform.position = Vector3.Lerp(_originalPosition, target.position, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            dialogueCam.Lens.FieldOfView = zoomAmount;
        }

        public void ShakeCamera(float duration, bool loop)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }

            _shakeCoroutine = StartCoroutine(CameraShakeCoroutine(duration, loop));
        }

        private IEnumerator CameraShakeCoroutine(float duration, bool loop)
        {
            if (_noiseProfile == null) yield break;

            // Set initial shake values
            _noiseProfile.AmplitudeGain = dialogueSettings.cameraModifier.shakeAmplitude;
            _noiseProfile.FrequencyGain = dialogueSettings.cameraModifier.shakeFrequency;

            float elapsedTime = 0f;

            // Shake the camera for the specified duration
            while (true)
            {
                elapsedTime += Time.deltaTime;

                // If loop effect is disabled and time is up, stop the shake
                if ((elapsedTime >= duration && !loop) || _stopCamEffect)
                {
                    _stopCamEffect = false;
                    camEffect.gameObject.SetActive(false);
                    break;
                }

                // Wait for the next frame
                yield return null;
            }

            // Reset the shake values
            _noiseProfile.AmplitudeGain = 0;
            _noiseProfile.FrequencyGain = 0;
        }

        public void DipToDefault(float duration, Color color)
        {
            StartCoroutine(DipToDefaultCoroutine(duration, color));
        }

        public void DipToBlack(float duration, Color color)
        {
            StartCoroutine(DipToBlackCoroutine(duration, color));
        }

        private IEnumerator DipToBlackCoroutine(float duration, Color color)
        {
            camEffect.gameObject.GetComponent<Image>().color = color;
            camEffect.gameObject.SetActive(true);
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                camEffect.alpha = Mathf.Lerp(0, 1, timer / duration);
                yield return null;
            }
        }

        private IEnumerator DipToDefaultCoroutine(float duration, Color color)
        {
            camEffect.gameObject.GetComponent<Image>().color = color;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                camEffect.alpha = Mathf.Lerp(1, 0, timer / duration);
                yield return null;
            }

            camEffect.gameObject.SetActive(false);
        }

        public void Flash(float duration, float amount, Color color, bool loop)
        {
            StartCoroutine(FlashCoroutine(duration, amount, color, loop));
        }

        private IEnumerator FlashCoroutine(float duration, float amount, Color color, bool loop)
        {
            camEffect.gameObject.GetComponent<Image>().color = color;
            camEffect.gameObject.SetActive(true);
            int flashAmount = (int)amount;
            while (true)
            {
                flashAmount--;
                // Fade In
                float elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    camEffect.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                camEffect.alpha = 1;

                // Fade Out
                elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    camEffect.alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                camEffect.alpha = 0;

                // If not looping, break out of the loop
                if ((flashAmount <= 0 && !loop) || _stopCamEffect)
                {
                    Debug.Log("stopcameffect" + _stopCamEffect);
                    _stopCamEffect = false;
                    camEffect.gameObject.SetActive(false);
                    break;
                }
            }
        }

        public void MoveActor(GameObject actor, Vector3 startPos, Vector3 endPos, float speed, bool useScenePos)
        {
            if (!useScenePos)
            {
                actor.transform.position = startPos;
            }

            StartCoroutine(MoveActorCoroutine(actor, endPos, speed));
        }

        private IEnumerator MoveActorCoroutine(GameObject actor, Vector3 endPos, float speed)
        {
            while (Vector3.Distance(actor.transform.position, endPos) > 0.01f)
            {
                float step = speed * Time.deltaTime;
                actor.transform.position = Vector3.MoveTowards(actor.transform.position, endPos, step);
                yield return null;
            }

            actor.transform.position = endPos;
        }

        #endregion
    }
}