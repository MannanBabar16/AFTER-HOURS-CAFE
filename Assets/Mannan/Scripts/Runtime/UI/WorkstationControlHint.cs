using DG.Tweening;
using Mannan.Core.Validation;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Mannan.UI
{
    /// <summary>
    /// Minimalist screen-space workstation control hint for After Hours Café.
    /// Anchored in an unobtrusive screen safe area (e.g. bottom-left) during workstation close-up mode.
    /// Provides discoverable, non-intrusive "[Q] Back" presentation with dynamic action binding display.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/UI/Workstation Control Hint")]
    public sealed class WorkstationControlHint : MonoBehaviour, IValidatable
    {
        [Header("References")]
        [Tooltip("CanvasGroup controlling hint alpha visibility.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Visual root transform used for slide animation.")]
        [SerializeField] private RectTransform rectTransform;

        [Tooltip("TMP label showing the cancel action key badge (e.g. '[ Q ]').")]
        [SerializeField] private TMP_Text keyBadgeLabel;

        [Tooltip("TMP label showing the cancel action description (e.g. 'Back').")]
        [SerializeField] private TMP_Text actionLabel;

        [Header("Tuning")]
        [SerializeField] private float slideDuration = 0.25f;
        [SerializeField] private float idleRestingAlpha = 0.75f;
        [SerializeField] private float delayBeforeDimming = 2.5f;

        private Vector2 _targetAnchoredPos;
        private Vector2 _hiddenAnchoredPos;
        private Sequence _animSequence;
        private Tween _dimTween;
        private bool _isShowing;

        private void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            _targetAnchoredPos = rectTransform != null ? rectTransform.anchoredPosition : new Vector2(36f, 36f);
            _hiddenAnchoredPos = _targetAnchoredPos - new Vector2(0f, 20f);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = _hiddenAnchoredPos;
            }
        }

        private void OnDisable()
        {
            _animSequence?.Kill();
            _dimTween?.Kill();
            _isShowing = false;
        }

        public void Show(string cancelBindingKey = "Q", string actionDescription = "Back")
        {
            if (keyBadgeLabel != null)
            {
                string key = string.IsNullOrEmpty(cancelBindingKey) ? "Q" : cancelBindingKey;
                keyBadgeLabel.text = $"[ {key} ]";
            }

            if (actionLabel != null)
            {
                actionLabel.text = actionDescription;
            }

            _animSequence?.Kill();
            _dimTween?.Kill();

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = _hiddenAnchoredPos;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            _animSequence = DOTween.Sequence();
            if (rectTransform != null)
            {
                _animSequence.Join(DOTween.To(() => rectTransform.anchoredPosition, p => rectTransform.anchoredPosition = p, _targetAnchoredPos, slideDuration).SetEase(Ease.OutCubic));
            }

            if (canvasGroup != null)
            {
                _animSequence.Join(DOTween.To(() => canvasGroup.alpha, a => canvasGroup.alpha = a, 1f, slideDuration).SetEase(Ease.OutQuad));
            }

            // After a short delay, gently relax alpha to idleRestingAlpha for unobtrusive discoverability
            _animSequence.AppendInterval(delayBeforeDimming);
            _animSequence.Append(DOTween.To(() => canvasGroup != null ? canvasGroup.alpha : 1f, a =>
            {
                if (canvasGroup != null) canvasGroup.alpha = a;
            }, idleRestingAlpha, 0.6f).SetEase(Ease.InOutQuad));

            _isShowing = true;
        }

        public void Hide()
        {
            if (!_isShowing && (canvasGroup == null || canvasGroup.alpha <= 0.01f))
            {
                return;
            }

            _isShowing = false;
            _animSequence?.Kill();
            _dimTween?.Kill();

            _animSequence = DOTween.Sequence();
            if (rectTransform != null)
            {
                _animSequence.Join(DOTween.To(() => rectTransform.anchoredPosition, p => rectTransform.anchoredPosition = p, _hiddenAnchoredPos, slideDuration * 0.8f).SetEase(Ease.InCubic));
            }

            if (canvasGroup != null)
            {
                _animSequence.Join(DOTween.To(() => canvasGroup.alpha, a => canvasGroup.alpha = a, 0f, slideDuration * 0.8f).SetEase(Ease.InQuad));
            }
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (canvasGroup == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UI",
                    "WorkstationControlHint is missing a CanvasGroup reference.",
                    this));
            }

            if (keyBadgeLabel == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UI",
                    "WorkstationControlHint keyBadgeLabel is unassigned.",
                    this));
            }
        }
    }
}
