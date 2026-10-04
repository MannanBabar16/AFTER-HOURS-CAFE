using System.Collections.Generic;
using Mannan.Core.Validation;
using UnityEngine;

namespace Mannan.Core.Data
{
    /// <summary>
    /// Base ScriptableObject for authoring definitions.
    /// Provides stable serialized ID, display name, description, and validation contract.
    /// Definitions are authored content and immutable at runtime.
    /// </summary>
    public abstract class DefinitionBase : ScriptableObject, IValidatable
    {
        [Header("Identity")]
        [Tooltip("Stable unique identifier used for save data and runtime cross-referencing.")]
        [SerializeField] private string id = string.Empty;

        [Tooltip("Human-readable name displayed in menus, UI, and inspectors.")]
        [SerializeField] private string displayName = string.Empty;

        [Header("Description")]
        [TextArea(2, 4)]
        [Tooltip("Flavor or descriptive text.")]
        [SerializeField] private string description = string.Empty;

        public string Id => id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;

        public virtual void Validate(List<ValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "Definitions",
                    $"Definition '{name}' has an empty or whitespace ID.",
                    this));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Info,
                    "Definitions",
                    $"Definition '{name}' does not have an explicit DisplayName assigned (will fall back to asset name).",
                    this));
            }
        }
    }
}
