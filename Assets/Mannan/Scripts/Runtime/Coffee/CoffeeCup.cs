using System.Collections.Generic;
using DG.Tweening;
using Mannan.Core.Validation;
using UnityEngine;

namespace Mannan.Coffee
{
    /// <summary>
    /// Reusable runtime representation of a coffee cup.
    /// Tracks current drink definition, fill progress, and coordinates visual presentation.
    /// Independent from specific machines, orders, or player characters.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/Coffee/Coffee Cup")]
    public sealed class CoffeeCup : MonoBehaviour, IValidatable
    {
        [Header("References")]
        [Tooltip("The presentation component responsible for liquid level display.")]
        [SerializeField] private CupLiquidVisual liquidVisual;

        [Tooltip("Visual root transform used for placement/completion micro-animations.")]
        [SerializeField] private Transform visualRoot;

        [Header("State")]
        [SerializeField] private DrinkDefinition currentDrink;
        [Range(0f, 1f)]
        [SerializeField] private float normalizedFill;

        public DrinkDefinition CurrentDrink => currentDrink;
        public float NormalizedFill => normalizedFill;
        public bool IsFilled => normalizedFill >= 0.95f;

        private Vector3 _baseScale = Vector3.one;
        private Tween _reactionTween;

        private void Reset()
        {
            liquidVisual = GetComponentInChildren<CupLiquidVisual>();
            visualRoot = transform.Find("VisualRoot") ?? transform;
        }

        private void Awake()
        {
            if (visualRoot == null)
            {
                visualRoot = transform.Find("VisualRoot") ?? transform;
            }

            _baseScale = visualRoot.localScale;

            if (liquidVisual == null)
            {
                liquidVisual = GetComponentInChildren<CupLiquidVisual>();
            }

            if (currentDrink != null && normalizedFill > 0f)
            {
                liquidVisual?.SetColor(currentDrink.LiquidColor);
                liquidVisual?.SetFill(normalizedFill);
            }
            else
            {
                ResetCup();
            }
        }

        private void OnDisable()
        {
            _reactionTween?.Kill();
        }

        public void SetDrink(DrinkDefinition drink)
        {
            currentDrink = drink;
            if (currentDrink != null && liquidVisual != null)
            {
                liquidVisual.SetColor(currentDrink.LiquidColor);
            }
        }

        public void SetFillAmount(float amount)
        {
            normalizedFill = Mathf.Clamp01(amount);
            if (liquidVisual != null)
            {
                liquidVisual.SetFill(normalizedFill);
            }
        }

        public void ResetCup()
        {
            normalizedFill = 0f;
            currentDrink = null;
            if (liquidVisual != null)
            {
                liquidVisual.ResetVisual();
            }

            if (visualRoot != null)
            {
                _reactionTween?.Kill();
                visualRoot.localScale = _baseScale;
            }
        }

        /// <summary>
        /// Micro-animation when the cup is placed onto a surface or socket.
        /// </summary>
        public void PlayPlacementReaction()
        {
            if (visualRoot == null) return;
            _reactionTween?.Kill();
            visualRoot.localScale = _baseScale;

            // Subtle squash & stretch settle (Y squash, XZ slight flare)
            Vector3 squashed = new Vector3(_baseScale.x * 1.05f, _baseScale.y * 0.92f, _baseScale.z * 1.05f);
            _reactionTween = DOTween.Sequence()
                .Append(DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, squashed, 0.08f).SetEase(Ease.OutQuad))
                .Append(DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, _baseScale, 0.14f).SetEase(Ease.OutBack))
                .SetTarget(visualRoot);
        }

        /// <summary>
        /// Celebratory micro-reaction when extraction completes.
        /// </summary>
        public void PlayCompletionReaction()
        {
            if (visualRoot == null) return;
            _reactionTween?.Kill();
            visualRoot.localScale = _baseScale;

            // Subtle gentle bounce upward
            Vector3 stretched = new Vector3(_baseScale.x * 0.96f, _baseScale.y * 1.06f, _baseScale.z * 0.96f);
            _reactionTween = DOTween.Sequence()
                .Append(DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, stretched, 0.12f).SetEase(Ease.OutQuad))
                .Append(DOTween.To(() => visualRoot.localScale, s => visualRoot.localScale = s, _baseScale, 0.18f).SetEase(Ease.OutBounce))
                .SetTarget(visualRoot);
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (liquidVisual == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Coffee",
                    "CoffeeCup is missing a CupLiquidVisual reference.",
                    this));
            }
        }
    }
}
