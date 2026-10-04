using System.Collections.Generic;
using DG.Tweening;
using Mannan.Core.Validation;
using Mannan.Interaction;
using TMPro;
using UnityEngine;

namespace Mannan.UI
{
    /// <summary>
    /// Minimalist uGUI + TextMeshPro + DOTween prompt badge.
    /// Reacts to interaction sensor focus changes to display clean, cozy prompts.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/UI/Interaction Prompt View")]
    public sealed class InteractionPromptView : MonoBehaviour, IValidatable
    {
        [Header("References")]
        [Tooltip("The InteractionSensor to listen to.")]
        [SerializeField] private InteractionSensor sensor;

        [Tooltip("CanvasGroup controlling prompt alpha and visibility.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("TextMeshPro text showing the key badge (e.g. '[E]').")]
        [SerializeField] private TMP_Text keyLabel;

        [Tooltip("TextMeshPro text showing the prompt description.")]
        [SerializeField] private TMP_Text actionLabel;

        [Header("Animation Tuning")]
        [SerializeField] private float fadeDuration = 0.2f;

        private Tween _fadeTween;
        private bool _isWorkstationMode;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
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

            _fadeTween?.Kill();
        }

        private void HandleWorkstationEntered(WorkstationInteractable ws)
        {
            SetWorkstationPrompt(ws != null ? ws.PromptText : "Workstation");
        }

        private void HandleWorkstationExited(WorkstationInteractable ws)
        {
            ClearWorkstationPrompt();
        }

        public void BindSensor(InteractionSensor targetSensor)
        {
            if (sensor != null)
            {
                sensor.OnFocusChanged -= HandleFocusChanged;
            }

            sensor = targetSensor;

            if (sensor != null && enabled)
            {
                sensor.OnFocusChanged += HandleFocusChanged;
            }
        }

        private void HandleFocusChanged(IInteractable focus)
        {
            if (_isWorkstationMode)
            {
                return;
            }

            if (focus != null)
            {
                ShowPrompt("E", $"{focus.ActionName} {focus.PromptText}");
            }
            else
            {
                HidePrompt();
            }
        }

        public void SetWorkstationPrompt(string workstationName)
        {
            _isWorkstationMode = true;
            ShowPrompt("Q", $"Exit {workstationName}");
        }

        public void ClearWorkstationPrompt()
        {
            _isWorkstationMode = false;
            HidePrompt();
        }

        public void ShowPrompt(string key, string message)
        {
            if (keyLabel != null) keyLabel.text = $"[{key}]";
            if (actionLabel != null) actionLabel.text = message;

            if (canvasGroup != null)
            {
                _fadeTween?.Kill();
                _fadeTween = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, fadeDuration).SetEase(Ease.OutQuad);
            }
        }

        public void HidePrompt()
        {
            if (canvasGroup != null)
            {
                _fadeTween?.Kill();
                _fadeTween = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeDuration).SetEase(Ease.OutQuad);
            }
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (canvasGroup == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UI",
                    "InteractionPromptView is missing a CanvasGroup reference.",
                    this));
            }

            if (actionLabel == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UI",
                    "InteractionPromptView actionLabel is unassigned.",
                    this));
            }
        }
    }
}
