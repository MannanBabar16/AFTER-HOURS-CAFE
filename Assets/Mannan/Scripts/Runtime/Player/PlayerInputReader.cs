using UnityEngine;
using UnityEngine.InputSystem;

namespace Mannan.Player
{
    /// <summary>
    /// Lightweight, zero-allocation input reader for After Hours Café.
    /// Supports direct keyboard (WASD / Arrows, E, Q) and gamepad input.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("Mannan/Player/Player Input Reader")]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool InteractTriggered { get; private set; }
        public bool CancelTriggered { get; private set; }

        public event System.Action OnInteractPressed;
        public event System.Action OnCancelPressed;

        public bool ConsumeInteractTriggered()
        {
            if (InteractTriggered)
            {
                InteractTriggered = false;
                return true;
            }
            return false;
        }

        public bool ConsumeCancelTriggered()
        {
            if (CancelTriggered)
            {
                CancelTriggered = false;
                return true;
            }
            return false;
        }

        private void Update()
        {
            // Reset triggers from previous frame before reading new frame input
            InteractTriggered = false;
            CancelTriggered = false;

            ReadMovement();
            ReadActions();
        }

        private void ReadMovement()
        {
            Vector2 move = Vector2.zero;

            // Keyboard input
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move.y -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move.x += 1f;
            }

            // Gamepad input
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.04f)
                {
                    move = stick;
                }
            }

            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            MoveInput = move;
        }

        private void ReadActions()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.eKey.wasPressedThisFrame)
                {
                    InteractTriggered = true;
                    OnInteractPressed?.Invoke();
                }

                if (keyboard.qKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                {
                    CancelTriggered = true;
                    OnCancelPressed?.Invoke();
                }
            }

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    InteractTriggered = true;
                    OnInteractPressed?.Invoke();
                }

                if (gamepad.buttonEast.wasPressedThisFrame)
                {
                    CancelTriggered = true;
                    OnCancelPressed?.Invoke();
                }
            }
        }
    }
}
