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

        [Header("Anchors")]
        [Tooltip("Optional authored anchor where the world-space prompt should appear. If null, calculates from bounds.")]
        [SerializeField] private Transform interactionAnchor;

        [Tooltip("Additional vertical clearance offset in meters to ensure the prompt floats clearly above the object.")]
        [SerializeField] private float promptVerticalOffset = 0.25f;

        [Header("Presentation Hooks")]
        [SerializeField] private UnityEvent onFocused;
        [SerializeField] private UnityEvent onUnfocused;
        [SerializeField] private UnityEvent<GameObject> onInteracted;

        public event Action OnFocusedEvent;
        public event Action OnUnfocusedEvent;
        public event Action<GameObject> OnInteractedEvent;

        public string PromptText => promptText;
        public string ActionName => actionName;
        public Transform Transform => transform;
        public Transform InteractionAnchor => interactionAnchor;
        public float PromptVerticalOffset { get => promptVerticalOffset; set => promptVerticalOffset = value; }
        public bool IsInteractable { get => isInteractable; set => isInteractable = value; }

        /// <summary>
        /// Returns the world position where the interaction prompt should attach.
        /// Uses the interactionAnchor if assigned; otherwise calculates a sensible fallback from bounds.
        /// </summary>
        public virtual Vector3 GetPromptWorldPosition()
        {
            if (interactionAnchor != null)
            {
                return interactionAnchor.position + Vector3.up * promptVerticalOffset;
            }

            if (TryGetComponent<Collider>(out var col))
            {
                return col.bounds.center + Vector3.up * (col.bounds.extents.y + promptVerticalOffset);
            }

            if (TryGetComponent<Renderer>(out var rend))
            {
                return rend.bounds.center + Vector3.up * (rend.bounds.extents.y + promptVerticalOffset);
            }

            return transform.position + Vector3.up * (1.0f + promptVerticalOffset);
        }

        public virtual bool CanInteract(GameObject interactor)
        {
            return isInteractable && enabled && gameObject.activeInHierarchy;
        }

        public virtual void OnFocusEnter(GameObject interactor)
        {
            onFocused?.Invoke();
            OnFocusedEvent?.Invoke();
        }

        public virtual void OnFocusExit(GameObject interactor)
        {
            onUnfocused?.Invoke();
            OnUnfocusedEvent?.Invoke();
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
