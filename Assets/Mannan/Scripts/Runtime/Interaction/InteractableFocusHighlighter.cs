using DG.Tweening;
using UnityEngine;

namespace Mannan.Interaction
{
    /// <summary>
    /// Optional, presentation-only component that provides cozy, subtle focus feedback on interactables.
    /// Modifies material emission or scale using DOTween without modifying vendor asset originals.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/Interaction/Interactable Focus Highlighter")]
    public sealed class InteractableFocusHighlighter : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The InteractableBase to observe. If null, resolved from this object or parents.")]
        [SerializeField] private InteractableBase interactable;

        [Tooltip("The Renderer to highlight. If null, resolved from this object.")]
        [SerializeField] private Renderer targetRenderer;

        [Header("Tuning")]
        [Tooltip("Subtle highlight color applied to emission on focus.")]
        [SerializeField] private Color highlightColor = new Color(0.22f, 0.17f, 0.10f, 1f); // warm cozy coffee tone

        [SerializeField] private float transitionDuration = 0.2f;

        private MaterialPropertyBlock _propBlock;
        private static readonly int EmissionColorProp = Shader.PropertyToID("_EmissionColor");
        private Tween _colorTween;
        private Color _currentColor = Color.black;

        private void Reset()
        {
            interactable = GetComponent<InteractableBase>() ?? GetComponentInParent<InteractableBase>();
            targetRenderer = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        }

        private void Awake()
        {
            if (interactable == null) interactable = GetComponent<InteractableBase>() ?? GetComponentInParent<InteractableBase>();
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
            _propBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (interactable != null)
            {
                interactable.OnFocusedEvent += HandleFocused;
                interactable.OnUnfocusedEvent += HandleUnfocused;
                interactable.OnInteractedEvent += HandleInteracted;
            }
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.OnFocusedEvent -= HandleFocused;
                interactable.OnUnfocusedEvent -= HandleUnfocused;
                interactable.OnInteractedEvent -= HandleInteracted;
            }

            _colorTween?.Kill();
            ApplyColor(Color.black);
        }

        private void HandleFocused() => Highlight(true);
        private void HandleUnfocused() => Highlight(false);

        public void Highlight(bool enable)
        {
            if (targetRenderer == null) return;

            Color targetColor = enable ? highlightColor : Color.black;

            _colorTween?.Kill();
            _colorTween = DOTween.To(() => _currentColor, c =>
            {
                _currentColor = c;
                ApplyColor(_currentColor);
            }, targetColor, transitionDuration).SetEase(Ease.OutQuad);
        }

        private void HandleInteracted(GameObject interactor)
        {
            // Subtle tiny punch on interaction
            if (targetRenderer != null)
            {
                DOTween.Punch(() => targetRenderer.transform.localScale, s => targetRenderer.transform.localScale = s, Vector3.one * 0.04f, 0.25f, 5, 0.5f).SetTarget(targetRenderer);
            }
        }

        private void ApplyColor(Color color)
        {
            if (targetRenderer == null) return;
            targetRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(EmissionColorProp, color);
            targetRenderer.SetPropertyBlock(_propBlock);
        }
    }
}
