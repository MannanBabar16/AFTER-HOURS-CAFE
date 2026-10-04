using System.Collections.Generic;
using DG.Tweening;
using Mannan.Core.Validation;
using Mannan.Interaction;
using Mannan.Player;
using TMPro;
using UnityEngine;

namespace Mannan.UI
{
    /// <summary>
    /// Reusable single-instance world-space interaction prompt for After Hours Café.
    /// Attaches visually to the currently focused interactable's InteractionAnchor.
    /// Billboards to the 3/4 diorama camera with cozy, soft, non-intrusive presentation.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/UI/World Interaction Prompt")]
    public sealed class WorldInteractionPrompt : MonoBehaviour, IValidatable
    {
        [Header("References")]
        [Tooltip("The player InteractionSensor to observe for focus changes.")]
        [SerializeField] private InteractionSensor sensor;

        [Tooltip("The player input reader used to display the active binding key.")]
        [SerializeField] private PlayerInputReader inputReader;

        [Tooltip("Camera to billboard towards. If null, resolves to Camera.main.")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("CanvasGroup controlling prompt alpha fade.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Visual root transform used for scale animation.")]
        [SerializeField] private Transform visualRoot;

        [Tooltip("TextMeshPro label showing the action key badge (e.g. '[E]').")]
        [SerializeField] private TMP_Text keyBadgeLabel;

        [Tooltip("TextMeshPro label showing the action text (e.g. 'Use Espresso Machine').")]
        [SerializeField] private TMP_Text actionTextLabel;

        [Header("Animation Tuning")]
        [SerializeField] private float fadeInDuration = 0.22f;
        [SerializeField] private float fadeOutDuration = 0.16f;
        [SerializeField] private float glideDuration = 0.2f;
        [SerializeField] private float verticalFloatOffset = 0.08f;

        private IInteractable _currentFocus;
        private bool _isWorkstationActive;
        private Sequence _animSequence;
        private Tween _glideTween;
        private Vector3 _baseScale = Vector3.one;

        private void Reset()
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            visualRoot = transform.Find("VisualRoot") ?? transform;
        }

        private void Awake()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            _baseScale = visualRoot.localScale;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            if (sensor != null)
            {
                sensor.OnFocusChanged += HandleFocusChanged;
            }

            WorkstationInteractable.OnWorkstationEnteredGlobal += HandleWorkstationEntered;
            WorkstationInteractable.OnWorkstationExitedGlobal += HandleWorkstationExited;
        }

        private void OnDisable()
        {
            if (sensor != null)
            {
                sensor.OnFocusChanged -= HandleFocusChanged;
            }

            WorkstationInteractable.OnWorkstationEnteredGlobal -= HandleWorkstationEntered;
            WorkstationInteractable.OnWorkstationExitedGlobal -= HandleWorkstationExited;

            _animSequence?.Kill();
            _glideTween?.Kill();
        }

        private void LateUpdate()
        {
            // Billboard towards gameplay camera: matching camera rotation gives stable, distortion-free orientation
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                transform.rotation = targetCamera.transform.rotation;
            }

            // Keep tracking focus object position if it moves
            if (_currentFocus != null && !_isWorkstationActive && (_glideTween == null || !_glideTween.IsActive()))
            {
                transform.position = _currentFocus.GetPromptWorldPosition();
            }
        }

        public void BindSensor(InteractionSensor targetSensor, PlayerInputReader targetInputReader = null)
        {
            if (sensor != null)
            {
                sensor.OnFocusChanged -= HandleFocusChanged;
            }

            sensor = targetSensor;
            if (targetInputReader != null)
            {
                inputReader = targetInputReader;
            }

            if (sensor != null && enabled)
            {
                sensor.OnFocusChanged += HandleFocusChanged;
            }
        }

        private void HandleFocusChanged(IInteractable newFocus)
        {
            if (_isWorkstationActive)
            {
                return;
            }

            if (newFocus == _currentFocus)
            {
                return;
            }

            IInteractable previousFocus = _currentFocus;
            _currentFocus = newFocus;

            if (_currentFocus != null)
            {
                UpdateLabels();

                Vector3 targetPos = _currentFocus.GetPromptWorldPosition();

                if (previousFocus == null)
                {
                    // Fresh focus: snap position, then scale up and fade in with small vertical ease
                    transform.position = targetPos;
                    AnimateEnter(targetPos);
                }
                else
                {
                    // Transferring focus from another object: glide cleanly to new position
                    AnimateTransfer(targetPos);
                }
            }
            else
            {
                AnimateExit();
            }
        }

        private void HandleWorkstationEntered(WorkstationInteractable ws)
        {
            _isWorkstationActive = true;
            _currentFocus = null;
            AnimateExit();
        }

        private void HandleWorkstationExited(WorkstationInteractable ws)
        {
            _isWorkstationActive = false;

            // Restore focus from sensor if player is currently facing an interactable
            if (sensor != null && sensor.CurrentFocus != null)
            {
                HandleFocusChanged(sensor.CurrentFocus);
            }
        }

        private void UpdateLabels()
        {
            if (_currentFocus == null) return;

            string key = inputReader != null ? inputReader.GetInteractBindingDisplayString() : "E";
            if (string.IsNullOrEmpty(key)) key = "E";

            if (keyBadgeLabel != null)
            {
                keyBadgeLabel.text = $"[ {key} ]";
            }

            if (actionTextLabel != null)
            {
                actionTextLabel.text = $"{_currentFocus.ActionName} {_currentFocus.PromptText}";
            }
        }

        private void AnimateEnter(Vector3 targetPos)
        {
            _animSequence?.Kill();
            _glideTween?.Kill();

            Vector3 startPos = targetPos - Vector3.up * verticalFloatOffset;
            transform.position = startPos;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (visualRoot != null)
            {
                visualRoot.localScale = _baseScale * 0.85f;
            }

            _animSequence = DOTween.Sequence();
            _animSequence.Join(DOTween.To(() => transform.position, p => transform.position = p, targetPos, fadeInDuration).SetEase(Ease.OutQuad));

            if (canvasGroup != null)
            {
                _animSequence.Join(DOTween.To(() => canvasGroup.alpha, a => canvasGroup.alpha = a, 1f, fadeInDuration).SetEase(Ease.OutQuad));
            }

            if (visualRoot != null)
            {
                // Quick soft pop: 0.85 -> 1.05 -> 1.0
                _animSequence.Join(DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, _baseScale * 1.05f, fadeInDuration * 0.65f).SetEase(Ease.OutQuad).SetTarget(visualRoot));
                _animSequence.Append(DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, _baseScale, fadeInDuration * 0.35f).SetEase(Ease.InOutQuad).SetTarget(visualRoot));
            }
        }

        private void AnimateTransfer(Vector3 newTargetPos)
        {
            _glideTween?.Kill();
            _glideTween = DOTween.To(() => transform.position, p => transform.position = p, newTargetPos, glideDuration).SetEase(Ease.OutCubic);

            // Subtle gentle pulse on transfer
            if (visualRoot != null)
            {
                DOTween.Kill(visualRoot);
                visualRoot.localScale = _baseScale * 0.95f;
                DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, _baseScale, glideDuration).SetEase(Ease.OutBack).SetTarget(visualRoot);
            }

            if (canvasGroup != null && canvasGroup.alpha < 0.9f)
            {
                DOTween.To(() => canvasGroup.alpha, a => canvasGroup.alpha = a, 1f, 0.15f);
            }
        }

        private void AnimateExit()
        {
            _animSequence?.Kill();
            _glideTween?.Kill();

            _animSequence = DOTween.Sequence();

            if (canvasGroup != null)
            {
                _animSequence.Join(DOTween.To(() => canvasGroup.alpha, a => canvasGroup.alpha = a, 0f, fadeOutDuration).SetEase(Ease.InQuad));
            }

            if (visualRoot != null)
            {
                _animSequence.Join(DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, _baseScale * 0.85f, fadeOutDuration).SetEase(Ease.InQuad).SetTarget(visualRoot));
            }
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (canvasGroup == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UI",
                    "WorldInteractionPrompt is missing a CanvasGroup reference.",
                    this));
            }

            if (actionTextLabel == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UI",
                    "WorldInteractionPrompt actionTextLabel is unassigned.",
                    this));
            }
        }
    }
}
