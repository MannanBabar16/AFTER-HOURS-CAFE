using Mannan.Core.Validation;
using Mannan.Interaction;
using Mannan.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Mannan.UI
{
    /// <summary>
    /// Coordinates hybrid interaction UI presentation:
    /// - Normal Gameplay: Screen-space object-tracking prompt attached to focused interactables.
    /// - Workstation Mode: Safe-area screen control hint for workstation exit.
    /// Keeps gameplay systems completely decoupled from UI presentation.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/UI/Interaction Presentation Controller")]
    public sealed class InteractionPresentationController : MonoBehaviour, IValidatable
    {
        [Header("References")]
        [Tooltip("The player InteractionSensor to observe for focus changes.")]
        [SerializeField] private InteractionSensor sensor;

        [Tooltip("The player input reader used to query dynamic binding strings.")]
        [SerializeField] private PlayerInputReader inputReader;

        [Tooltip("The screen-space object-tracking interaction prompt.")]
        [SerializeField] private ObjectInteractionPrompt objectPrompt;

        [Tooltip("The safe-area workstation control hint.")]
        [SerializeField] private WorkstationControlHint workstationHint;

        [Tooltip("Camera used to project 3D world anchors to screen space. Defaults to Camera.main.")]
        [SerializeField] private Camera targetCamera;

        private IInteractable _previousFocus;
        private bool _isWorkstationActive;

        private void Reset()
        {
            objectPrompt = GetComponentInChildren<ObjectInteractionPrompt>();
            workstationHint = GetComponentInChildren<WorkstationControlHint>();
        }

        private void Awake()
        {
            if (objectPrompt == null) objectPrompt = GetComponentInChildren<ObjectInteractionPrompt>();
            if (workstationHint == null) workstationHint = GetComponentInChildren<WorkstationControlHint>();
            if (targetCamera == null) targetCamera = Camera.main;
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
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (!_isWorkstationActive && objectPrompt != null)
            {
                objectPrompt.UpdateTracking(targetCamera);
            }
        }

        public void BindPlayer(InteractionSensor targetSensor, PlayerInputReader targetInputReader = null)
        {
            if (sensor != null)
            {
                sensor.OnFocusChanged -= HandleFocusChanged;
            }

            sensor = targetSensor;
            if (targetInputReader != null)
            {
                inputReader = targetInputReader;
            }

            if (sensor != null && enabled)
            {
                sensor.OnFocusChanged += HandleFocusChanged;
                if (sensor.CurrentFocus != null)
                {
                    HandleFocusChanged(sensor.CurrentFocus);
                }
            }
        }

        private void HandleFocusChanged(IInteractable newFocus)
        {
            if (_isWorkstationActive)
            {
                return;
            }

            if (objectPrompt == null)
            {
                return;
            }

            string bindingKey = inputReader != null ? inputReader.GetInteractBindingDisplayString() : "E";

            if (newFocus != null)
            {
                if (_previousFocus != null && _previousFocus != newFocus)
                {
                    // Focus transferred between objects: smooth glide
                    objectPrompt.TransferTo(newFocus, bindingKey, targetCamera);
                }
                else
                {
                    // Fresh focus: pop/fade in
                    objectPrompt.Show(newFocus, bindingKey, targetCamera);
                }
            }
            else
            {
                objectPrompt.Hide();
            }

            _previousFocus = newFocus;
        }

        private void HandleWorkstationEntered(WorkstationInteractable ws)
        {
            _isWorkstationActive = true;
            _previousFocus = null;

            if (objectPrompt != null)
            {
                objectPrompt.Hide();
            }

            if (workstationHint != null)
            {
                string cancelKey = inputReader != null ? inputReader.GetCancelBindingDisplayString() : "Q";
                workstationHint.Show(cancelKey, "Back");
            }
        }

        private void HandleWorkstationExited(WorkstationInteractable ws)
        {
            _isWorkstationActive = false;

            if (workstationHint != null)
            {
                workstationHint.Hide();
            }

            // Restore object prompt if player is facing an interactable
            if (sensor != null && sensor.CurrentFocus != null)
            {
                HandleFocusChanged(sensor.CurrentFocus);
            }
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (objectPrompt == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UI",
                    "InteractionPresentationController has no ObjectInteractionPrompt assigned.",
                    this));
            }

            if (workstationHint == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UI",
                    "InteractionPresentationController has no WorkstationControlHint assigned.",
                    this));
            }
        }
    }
}
