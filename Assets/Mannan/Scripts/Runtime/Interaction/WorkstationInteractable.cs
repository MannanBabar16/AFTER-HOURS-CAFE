using System.Collections.Generic;
using Mannan.Core.Logging;
using Mannan.Core.Validation;
using Mannan.Player;
using Mannan.World;
using UnityEngine;
using UnityEngine.Events;

namespace Mannan.Interaction
{
    /// <summary>
    /// Contextual workstation interactable (e.g. coffee brewing station, register, pastry shelf).
    /// Transitions camera to an authored close-up view and constrains player movement while active.
    /// </summary>
    [AddComponentMenu("Mannan/Interaction/Workstation Interactable")]
    public sealed class WorkstationInteractable : InteractableBase
    {
        [Header("Workstation Configuration")]
        [Tooltip("Authored camera pose transform when viewing this workstation in close-up.")]
        [SerializeField] private Transform cameraAnchor;

        [Tooltip("Duration of the camera blend transition into the close-up.")]
        [SerializeField] private float transitionDuration = 0.75f;

        [Header("Workstation Events")]
        [SerializeField] private UnityEvent onWorkstationEntered;
        [SerializeField] private UnityEvent onWorkstationExited;

        public static event System.Action<WorkstationInteractable> OnWorkstationEnteredGlobal;
        public static event System.Action<WorkstationInteractable> OnWorkstationExitedGlobal;

        public bool IsInWorkstation { get; private set; }
        public Transform CameraAnchor => cameraAnchor;

        private PlayerController _activePlayerController;
        private PlayerInputReader _activeInputReader;

        private void OnDisable()
        {
            if (_activeInputReader != null)
            {
                _activeInputReader.OnCancelPressed -= HandleCancelPressed;
            }
        }

        private void Update()
        {
            if (IsInWorkstation && _activeInputReader != null)
            {
                // Check if user requested exit via cancel key (Q / Escape / Gamepad B)
                if (_activeInputReader.ConsumeCancelTriggered())
                {
                    ExitWorkstation();
                }
            }
        }

        private void HandleCancelPressed()
        {
            if (IsInWorkstation)
            {
                ExitWorkstation();
            }
        }

        public override void Interact(GameObject interactor)
        {
            if (IsInWorkstation)
            {
                return;
            }

            base.Interact(interactor);

            _activePlayerController = interactor.GetComponent<PlayerController>();
            _activeInputReader = interactor.GetComponent<PlayerInputReader>();

            EnterWorkstation();
        }

        public void EnterWorkstation()
        {
            if (cameraAnchor == null)
            {
                CafeLogger.LogError("Workstation", $"Workstation '{name}' cannot be entered: cameraAnchor is missing.", this);
                return;
            }

            IsInWorkstation = true;

            if (_activeInputReader != null)
            {
                _activeInputReader.OnCancelPressed += HandleCancelPressed;
            }

            // Lock player movement during workstation interaction
            if (_activePlayerController != null)
            {
                _activePlayerController.SetMovementLocked(true);
            }

            // Blend camera to authored close-up anchor
            if (CafeCameraRig.Instance != null)
            {
                CafeCameraRig.Instance.TransitionToAnchor(cameraAnchor, transitionDuration);
            }

            CafeLogger.Log("Workstation", $"Entered workstation '{PromptText}'. Press [Q] to exit.");
            onWorkstationEntered?.Invoke();
            OnWorkstationEnteredGlobal?.Invoke(this);
        }

        public void ExitWorkstation()
        {
            if (!IsInWorkstation)
            {
                return;
            }

            IsInWorkstation = false;

            if (_activeInputReader != null)
            {
                _activeInputReader.OnCancelPressed -= HandleCancelPressed;
            }

            // Cache reference so the asynchronous camera completion callback restores movement on the correct controller
            var playerToUnlock = _activePlayerController;

            // Return camera back to 3/4 gameplay view
            if (CafeCameraRig.Instance != null)
            {
                CafeCameraRig.Instance.ReturnToGameplay(transitionDuration, () =>
                {
                    // Restore player movement once camera returns
                    if (playerToUnlock != null)
                    {
                        playerToUnlock.SetMovementLocked(false);
                    }
                });
            }
            else if (playerToUnlock != null)
            {
                playerToUnlock.SetMovementLocked(false);
            }

            CafeLogger.Log("Workstation", $"Exited workstation '{PromptText}'. Returning to 3/4 gameplay.");
            onWorkstationExited?.Invoke();
            OnWorkstationExitedGlobal?.Invoke(this);

            _activePlayerController = null;
            _activeInputReader = null;
        }

        public override void Validate(List<ValidationIssue> issues)
        {
            base.Validate(issues);

            if (cameraAnchor == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "Workstation",
                    $"WorkstationInteractable on '{name}' does not have a cameraAnchor assigned.",
                    this));
            }
        }
    }
}
