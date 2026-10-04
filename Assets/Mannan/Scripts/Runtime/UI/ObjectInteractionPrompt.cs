using DG.Tweening;
using Mannan.Core.Validation;
using Mannan.Interaction;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Mannan.UI
{
    /// <summary>
    /// Screen-space object-tracking interaction prompt for After Hours Café.
    /// Projects the world position of the focused interactable's InteractionAnchor to screen coordinates.
    /// Visually belongs to the object without clipping or intersecting 3D meshes.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/UI/Object Interaction Prompt")]
    public sealed class ObjectInteractionPrompt : MonoBehaviour, IValidatable
    {
        [Header("References")]
        [Tooltip("RectTransform of the root canvas containing this prompt.")]
        [SerializeField] private RectTransform canvasRect;

        [Tooltip("CanvasGroup controlling prompt alpha visibility.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Visual root transform used for pop/scale animation.")]
        [SerializeField] private Transform visualRoot;

        [Tooltip("TMP label showing the action key badge (e.g. '[ E ]').")]
        [SerializeField] private TMP_Text keyBadgeLabel;

        [Tooltip("TMP label showing the action description (e.g. 'Use Espresso Machine').")]
        [SerializeField] private TMP_Text actionTextLabel;

        [Header("Animation Tuning")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;
        [SerializeField] private float glideDuration = 0.18f;
        [SerializeField] private float screenFloatOffset = 18f; // pixels

        private RectTransform _rectTransform;
        private IInteractable _currentFocus;
        private Camera _mainCamera;
        private Sequence _animSequence;
        private Tween _glideTween;
        private Vector3 _baseScale = Vector3.one;
        private bool _isShowing;

        private void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            visualRoot = transform.Find("VisualRoot") ?? transform;
            var parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null) canvasRect = parentCanvas.GetComponent<RectTransform>();
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (canvasRect == null)
            {
                var parentCanvas = GetComponentInParent<Canvas>();
                if (parentCanvas != null) canvasRect = parentCanvas.GetComponent<RectTransform>();
            }

            if (visualRoot == null) visualRoot = transform;
            _baseScale = visualRoot.localScale;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            _mainCamera = Camera.main;
        }

        private void OnDisable()
        {
            _animSequence?.Kill();
            _glideTween?.Kill();
            _isShowing = false;
        }

        public void Show(IInteractable interactable, string bindingKey, Camera camera = null)
        {
            if (interactable == null)
            {
                Hide();
                return;
            }

            if (camera != null) _mainCamera = camera;
            else if (_mainCamera == null) _mainCamera = Camera.main;

            _currentFocus = interactable;
            UpdateLabels(bindingKey);

            Vector2 targetCanvasPos = CalculateCanvasPosition(_currentFocus.GetPromptWorldPosition());

            _animSequence?.Kill();
            _glideTween?.Kill();

            _rectTransform.anchoredPosition = targetCanvasPos - new Vector2(0f, screenFloatOffset);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (visualRoot != null) visualRoot.localScale = _baseScale * 0.85f;

            _animSequence = DOTween.Sequence();
            _animSequence.Join(DOTween.To(() => _rectTransform.anchoredPosition, p => _rectTransform.anchoredPosition = p, targetCanvasPos, fadeInDuration).SetEase(Ease.OutQuad));
            if (canvasGroup != null)
            {
                _animSequence.Join(DOTween.To(() => canvasGroup.alpha, a => canvasGroup.alpha = a, 1f, fadeInDuration).SetEase(Ease.OutQuad));
            }

            if (visualRoot != null)
            {
                // Soft 0.85 -> 1.05 -> 1.0 pop
                _animSequence.Join(DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, _baseScale * 1.05f, fadeInDuration * 0.65f).SetEase(Ease.OutQuad).SetTarget(visualRoot));
                _animSequence.Append(DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, _baseScale, fadeInDuration * 0.35f).SetEase(Ease.InOutQuad).SetTarget(visualRoot));
            }

            _isShowing = true;
        }

        public void TransferTo(IInteractable newInteractable, string bindingKey, Camera camera = null)
        {
            if (newInteractable == null)
            {
                Hide();
                return;
            }

            if (camera != null) _mainCamera = camera;
            else if (_mainCamera == null) _mainCamera = Camera.main;

            _currentFocus = newInteractable;
            UpdateLabels(bindingKey);

            Vector2 targetCanvasPos = CalculateCanvasPosition(_currentFocus.GetPromptWorldPosition());

            _glideTween?.Kill();
            _glideTween = DOTween.To(() => _rectTransform.anchoredPosition, p => _rectTransform.anchoredPosition = p, targetCanvasPos, glideDuration).SetEase(Ease.OutCubic);

            // Subtle gentle micro-pulse on focus switch
            if (visualRoot != null)
            {
                DOTween.Kill(visualRoot);
                visualRoot.localScale = _baseScale * 0.95f;
                DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, _baseScale, glideDuration).SetEase(Ease.OutBack).SetTarget(visualRoot);
            }

            if (canvasGroup != null && canvasGroup.alpha < 0.95f)
            {
                DOTween.To(() => canvasGroup.alpha, a => canvasGroup.alpha = a, 1f, 0.12f);
            }

            _isShowing = true;
        }

        public void Hide()
        {
            if (!_isShowing && (canvasGroup == null || canvasGroup.alpha <= 0.01f))
            {
                _currentFocus = null;
                return;
            }

            _isShowing = false;
            _currentFocus = null;

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

        public void UpdateTracking(Camera camera = null)
        {
            if (!_isShowing || _currentFocus == null)
            {
                return;
            }

            if (camera != null) _mainCamera = camera;
            else if (_mainCamera == null) _mainCamera = Camera.main;

            if (_mainCamera == null || canvasRect == null)
            {
                return;
            }

            // If not actively gliding between objects, lock anchored position to screen projection
            if (_glideTween == null || !_glideTween.IsActive())
            {
                _rectTransform.anchoredPosition = CalculateCanvasPosition(_currentFocus.GetPromptWorldPosition());
            }
        }

        private Vector2 CalculateCanvasPosition(Vector3 worldPos)
        {
            if (_mainCamera == null || canvasRect == null)
            {
                return Vector2.zero;
            }

            Vector3 screenPoint = _mainCamera.WorldToScreenPoint(worldPos);

            // If behind camera plane, hide or clamp
            if (screenPoint.z < 0)
            {
                return new Vector2(-10000f, -10000f);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);
            return localPoint;
        }

        private void UpdateLabels(string bindingKey)
        {
            if (_currentFocus == null) return;

            string key = string.IsNullOrEmpty(bindingKey) ? "E" : bindingKey;
            if (keyBadgeLabel != null)
            {
                keyBadgeLabel.text = $"[ {key} ]";
            }

            if (actionTextLabel != null)
            {
                actionTextLabel.text = $"{_currentFocus.ActionName} {_currentFocus.PromptText}";
            }
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (canvasGroup == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UI",
                    "ObjectInteractionPrompt is missing a CanvasGroup reference.",
                    this));
            }

            if (actionTextLabel == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UI",
                    "ObjectInteractionPrompt actionTextLabel is unassigned.",
                    this));
            }
        }
    }
}
