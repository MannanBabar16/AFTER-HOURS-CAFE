using System.Collections.Generic;
using Mannan.Core.Validation;
using UnityEngine;

namespace Mannan.Coffee
{
    /// <summary>
    /// ScriptableObject defining an immutable authored coffee drink recipe.
    /// Used by coffee preparation workstations and future order evaluation systems.
    /// </summary>
    [CreateAssetMenu(fileName = "Drink_New", menuName = "Mannan/Coffee/Drink Definition", order = 10)]
    public class DrinkDefinition : ScriptableObject, IValidatable
    {
        [Header("Identity")]
        [Tooltip("Unique stable identifier (e.g. 'espresso', 'americano').")]
        [SerializeField] private string drinkId = "espresso";

        [Tooltip("Player-facing display name.")]
        [SerializeField] private string displayName = "Classic Espresso";

        [TextArea(2, 4)]
        [Tooltip("Flavor text and description.")]
        [SerializeField] private string description = "Rich, bold single shot of espresso topped with a velvety golden crema.";

        [Header("Preparation Parameters")]
        [Tooltip("Duration in seconds for espresso extraction.")]
        [Range(1.0f, 10.0f)]
        [SerializeField] private float extractionDuration = 3.5f;

        [Header("Visual Presentation")]
        [Tooltip("Base liquid body color (deep rich coffee).")]
        [SerializeField] private Color liquidColor = new Color(0.18f, 0.10f, 0.05f, 1.0f);

        [Tooltip("Surface / crema color (warm golden caramel).")]
        [SerializeField] private Color cremaColor = new Color(0.78f, 0.58f, 0.32f, 1.0f);

        [Tooltip("Optional presentation icon for UI and orders.")]
        [SerializeField] private Sprite icon;

        public string DrinkId => drinkId;
        public string DisplayName => displayName;
        public string Description => description;
        public float ExtractionDuration => extractionDuration;
        public Color LiquidColor => liquidColor;
        public Color CremaColor => cremaColor;
        public Sprite Icon => icon;

        public void Validate(List<ValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(drinkId))
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "Coffee",
                    "DrinkDefinition is missing a drinkId.",
                    this));
            }

            if (extractionDuration <= 0f)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "Coffee",
                    "DrinkDefinition extractionDuration must be greater than zero.",
                    this));
            }
        }
    }
}
