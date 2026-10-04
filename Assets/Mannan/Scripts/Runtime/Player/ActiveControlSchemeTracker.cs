using System.Collections.Generic;
using Mannan.Core.Validation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Mannan.Player
{
    /// <summary>
    /// Identifies the player's active input control scheme.
    /// </summary>
    public enum ActiveControlScheme
    {
        KeyboardMouse,
        Gamepad
    }

    /// <summary>
    /// Reusable service/component that tracks the player's last meaningful input device.
    /// Differentiates Keyboard/Mouse vs Gamepad while filtering out stick drift and sensor jitter.
    /// Exposes notifications when the active scheme changes so UI and presentation stay in sync.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-110)]
    [AddComponentMenu("Mannan/Player/Active Control Scheme Tracker")]
    public sealed class ActiveControlSchemeTracker : MonoBehaviour, IValidatable
    {
        [Header("Thresholds")]
        [Tooltip("Minimum magnitude for stick input to be considered deliberate (filters drift).")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float stickDeadzone = 0.20f;

        [Tooltip("Minimum press amount for analog triggers to count as deliberate.")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float triggerDeadzone = 0.20f;

        [Tooltip("Minimum mouse movement per frame (in pixels) to trigger switch to Keyboard/Mouse.")]
        [Range(1f, 20f)]
        [SerializeField] private float mouseDeltaThreshold = 3.0f;

        public ActiveControlScheme CurrentControlScheme { get; private set; } = ActiveControlScheme.KeyboardMouse;

        public event System.Action<ActiveControlScheme> OnControlSchemeChanged;

        private float _stickDeadzoneSqr;
        private float _mouseDeltaThresholdSqr;

        private void Awake()
        {
            UpdateThresholdSquares();
        }

        private void OnValidate()
        {
            UpdateThresholdSquares();
        }

        private void UpdateThresholdSquares()
        {
            _stickDeadzoneSqr = stickDeadzone * stickDeadzone;
            _mouseDeltaThresholdSqr = mouseDeltaThreshold * mouseDeltaThreshold;
        }

        private void OnEnable()
        {
            InputSystem.onEvent += HandleInputEvent;
        }

        private void OnDisable()
        {
            InputSystem.onEvent -= HandleInputEvent;
        }

        /// <summary>
        /// Explicitly sets the active control scheme if different, notifying listeners.
        /// </summary>
        public void SetControlScheme(ActiveControlScheme scheme)
        {
            if (scheme == CurrentControlScheme)
            {
                return;
            }

            CurrentControlScheme = scheme;
            OnControlSchemeChanged?.Invoke(scheme);
        }

        /// <summary>
        /// External notification (e.g. from an InputAction callback) that a specific device was used.
        /// </summary>
        public void NotifyDeviceUsed(ActiveControlScheme scheme)
        {
            SetControlScheme(scheme);
        }

        private void HandleInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (device == null)
            {
                return;
            }

            if (device is Keyboard keyboard)
            {
                if (keyboard.anyKey.isPressed)
                {
                    SetControlScheme(ActiveControlScheme.KeyboardMouse);
                }
            }
            else if (device is Mouse mouse)
            {
                if (mouse.leftButton.isPressed || mouse.rightButton.isPressed || mouse.middleButton.isPressed)
                {
                    SetControlScheme(ActiveControlScheme.KeyboardMouse);
                }
                else if (mouse.delta.ReadValue().sqrMagnitude >= _mouseDeltaThresholdSqr)
                {
                    SetControlScheme(ActiveControlScheme.KeyboardMouse);
                }
            }
            else if (device is Gamepad gamepad)
            {
                if (IsGamepadMeaningfullyUsed(gamepad))
                {
                    SetControlScheme(ActiveControlScheme.Gamepad);
                }
            }
        }

        private bool IsGamepadMeaningfullyUsed(Gamepad gamepad)
        {
            // Direct button presses
            if (gamepad.buttonSouth.isPressed ||
                gamepad.buttonEast.isPressed ||
                gamepad.buttonWest.isPressed ||
                gamepad.buttonNorth.isPressed ||
                gamepad.leftShoulder.isPressed ||
                gamepad.rightShoulder.isPressed ||
                gamepad.startButton.isPressed ||
                gamepad.selectButton.isPressed ||
                gamepad.leftStickButton.isPressed ||
                gamepad.rightStickButton.isPressed ||
                gamepad.dpad.up.isPressed ||
                gamepad.dpad.down.isPressed ||
                gamepad.dpad.left.isPressed ||
                gamepad.dpad.right.isPressed)
            {
                return true;
            }

            // Analog triggers with deadzone
            if (gamepad.leftTrigger.ReadValue() >= triggerDeadzone ||
                gamepad.rightTrigger.ReadValue() >= triggerDeadzone)
            {
                return true;
            }

            // Analog sticks with deadzone (filters drift)
            if (gamepad.leftStick.ReadValue().sqrMagnitude >= _stickDeadzoneSqr ||
                gamepad.rightStick.ReadValue().sqrMagnitude >= _stickDeadzoneSqr)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resolves the clean display keycap for an InputAction matching the specified control scheme.
        /// </summary>
        public static string GetBindingDisplayForScheme(InputAction action, ActiveControlScheme scheme)
        {
            if (action == null)
            {
                return string.Empty;
            }

            string targetGroup = scheme == ActiveControlScheme.Gamepad ? "Gamepad" : "Keyboard&Mouse";

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.isComposite) continue;

                // Priority 1: Match by binding group
                if (!string.IsNullOrEmpty(binding.groups) && binding.groups.Contains(targetGroup))
                {
                    string display = action.GetBindingDisplayString(i);
                    if (!string.IsNullOrEmpty(display))
                    {
                        return FormatBindingDisplay(display);
                    }
                }
                // Priority 2: Fallback by device path if groups unassigned
                else if (string.IsNullOrEmpty(binding.groups) && !string.IsNullOrEmpty(binding.path))
                {
                    bool isGamepadBinding = binding.path.StartsWith("<Gamepad>", System.StringComparison.OrdinalIgnoreCase);
                    if ((scheme == ActiveControlScheme.Gamepad && isGamepadBinding) ||
                        (scheme == ActiveControlScheme.KeyboardMouse && !isGamepadBinding))
                    {
                        string display = action.GetBindingDisplayString(i);
                        if (!string.IsNullOrEmpty(display))
                        {
                            return FormatBindingDisplay(display);
                        }
                    }
                }
            }

            // Default safe fallback if no matching binding was configured
            return scheme == ActiveControlScheme.Gamepad ? "A" : "E";
        }

        /// <summary>
        /// Cleans verbose Unity binding names into concise readable badge text (e.g. "Button South" -> "A").
        /// </summary>
        public static string FormatBindingDisplay(string rawDisplay)
        {
            if (string.IsNullOrEmpty(rawDisplay)) return string.Empty;
            string trimmed = rawDisplay.Trim();

            // Match case-insensitive common names
            switch (trimmed.ToLowerInvariant())
            {
                case "button south":
                case "buttonsouth":
                    return "A";
                case "button east":
                case "buttoneast":
                    return "B";
                case "button west":
                case "buttonwest":
                    return "X";
                case "button north":
                case "buttonnorth":
                    return "Y";
                case "left shoulder":
                    return "LB";
                case "right shoulder":
                    return "RB";
                case "left trigger":
                    return "LT";
                case "right trigger":
                    return "RT";
                case "left stick":
                    return "LS";
                case "right stick":
                    return "RS";
                case "escape":
                    return "Esc";
                default:
                    return trimmed;
            }
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (stickDeadzone <= 0.01f)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Input",
                    "ActiveControlSchemeTracker stickDeadzone is very low and may suffer from analog drift.",
                    this));
            }
        }
    }
}
