using System.Collections.Generic;
using Mannan.Core.Validation;
using UnityEngine;

namespace Mannan.Coffee
{
    /// <summary>
    /// Presentation component managing the liquid level and appearance inside a CoffeeCup.
    /// Uses local scale/position and MaterialPropertyBlock for zero-allocation stylized liquid filling.
    /// Completely separated from coffee brewing logic.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/Coffee/Cup Liquid Visual")]
    public sealed class CupLiquidVisual : MonoBehaviour, IValidatable
    {
        [Header("References")]
        [Tooltip("The MeshRenderer representing the liquid surface / body.")]
        [SerializeField] private MeshRenderer liquidRenderer;

        [Tooltip("Transform of the liquid mesh inside the cup.")]
        [SerializeField] private Transform liquidTransform;

        [Header("Level Range")]
        [Tooltip("Local position of the liquid surface when empty (bottom of cup).")]
        [SerializeField] private Vector3 emptyLocalPosition = new Vector3(0f, 0.02f, 0f);

        [Tooltip("Local position of the liquid surface when full.")]
        [SerializeField] private Vector3 fullLocalPosition = new Vector3(0f, 0.09f, 0f);

        [Tooltip("Local scale of the liquid mesh when empty.")]
        [SerializeField] private Vector3 emptyLocalScale = new Vector3(0.7f, 0.001f, 0.7f);

        [Tooltip("Local scale of the liquid mesh when full.")]
        [SerializeField] private Vector3 fullLocalScale = new Vector3(0.95f, 1.0f, 0.95f);

        private MaterialPropertyBlock _propBlock;
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProp = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();
            if (liquidTransform == null && liquidRenderer != null)
            {
                liquidTransform = liquidRenderer.transform;
            }

            ResetVisual();
        }

        public void SetFill(float normalizedFill)
        {
            normalizedFill = Mathf.Clamp01(normalizedFill);

            if (liquidRenderer == null || liquidTransform == null)
            {
                return;
            }

            if (normalizedFill <= 0.001f)
            {
                liquidRenderer.enabled = false;
                liquidTransform.localPosition = emptyLocalPosition;
                liquidTransform.localScale = emptyLocalScale;
                return;
            }

            liquidRenderer.enabled = true;
            liquidTransform.localPosition = Vector3.Lerp(emptyLocalPosition, fullLocalPosition, normalizedFill);
            liquidTransform.localScale = Vector3.Lerp(emptyLocalScale, fullLocalScale, normalizedFill);
        }

        public void SetColor(Color color)
        {
            if (liquidRenderer == null) return;
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            liquidRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(BaseColorProp, color);
            _propBlock.SetColor(ColorProp, color);
            liquidRenderer.SetPropertyBlock(_propBlock);
        }

        public void ResetVisual()
        {
            SetFill(0f);
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (liquidRenderer == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Coffee",
                    "CupLiquidVisual is missing a liquidRenderer reference.",
                    this));
            }
        }
    }
}
