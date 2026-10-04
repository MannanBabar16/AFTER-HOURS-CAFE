using System;
using System.Collections.Generic;
using Mannan.Core.Logging;
using Mannan.Core.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Mannan.Interaction
{
    /// <summary>
    /// Base component for interactable world objects.
    /// Provides inspector authoring, validation, and presentation event hooks.
    /// </summary>
    [AddComponentMenu("Mannan/Interaction/Interactable Base")]
    public class InteractableBase : MonoBehaviour, IInteractable, IValidatable
    {
        [Header("Authoring")]
        [Tooltip("Object name or description shown in prompt (e.g. 'Coffee Grinder', 'Counter').")]
        [SerializeField] private string promptText = "Interactable";

        [Tooltip("Action label (e.g. 'Interact', 'Brew', 'Inspect', 'Open').")]
        [SerializeField] private string actionName = "Interact";

        [Tooltip("If false, player cannot interact with this object currently.")]
        [SerializeField] private bool isInteractable = true;

        [Header("Presentation Hooks")]
        [SerializeField] private UnityEvent onFocused;
        [SerializeField] private UnityEvent onUnfocused;
        [SerializeField] private UnityEvent<GameObject> onInteracted;

        public event Action<GameObject> OnInteractedEvent;

        public string PromptText => promptText;
        public string ActionName => actionName;
        public Transform Transform => transform;
        public bool IsInteractable { get => isInteractable; set => isInteractable = value; }

        public virtual bool CanInteract(GameObject interactor)
        {
            return isInteractable && enabled && gameObject.activeInHierarchy;
        }

        public virtual void OnFocusEnter(GameObject interactor)
        {
            onFocused?.Invoke();
        }

        public virtual void OnFocusExit(GameObject interactor)
        {
            onUnfocused?.Invoke();
        }

        public virtual void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            CafeLogger.Log("Interaction", $"Interacted with '{name}' ({promptText})", this);
            onInteracted?.Invoke(interactor);
            OnInteractedEvent?.Invoke(interactor);
        }

        public virtual void Validate(List<ValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(promptText))
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Interaction",
                    $"Interactable on '{name}' has an empty PromptText.",
                    this));
            }

            if (GetComponentInChildren<Collider>() == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Interaction",
                    $"Interactable on '{name}' does not have any Collider. Proximity sensors will not detect it.",
                    this));
            }
        }
    }
}
