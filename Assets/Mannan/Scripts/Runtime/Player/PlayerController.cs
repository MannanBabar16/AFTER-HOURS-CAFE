using System.Collections.Generic;
using Mannan.Core.Validation;
using Mannan.World;
using UnityEngine;

namespace Mannan.Player
{
    /// <summary>
    /// Player movement controller for After Hours Café.
    /// Handles camera-relative WASD movement, smooth character orientation, and movement locking.
    /// Independent of specific visual meshes or character rigs.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Mannan/Player/Player Controller")]
    public sealed class PlayerController : MonoBehaviour, IValidatable
    {
        [Header("References")]
        [Tooltip("The CharacterController handling physical collision and motion.")]
        [SerializeField] private CharacterController characterController;

        [Tooltip("Input reader providing movement and action triggers.")]
        [SerializeField] private PlayerInputReader inputReader;

        [Tooltip("Visual root transform containing the character model. Rotates independently toward movement direction.")]
        [SerializeField] private Transform visualRoot;

        [Header("Movement Tuning")]
        [Tooltip("Base walk speed in meters per second.")]
        [SerializeField] private float walkSpeed = 3.8f;

        [Tooltip("Acceleration rate when starting movement.")]
        [SerializeField] private float acceleration = 12f;

        [Tooltip("Deceleration rate when stopping.")]
        [SerializeField] private float deceleration = 16f;

        [Tooltip("Smooth rotation speed in degrees per second.")]
        [SerializeField] private float turnSpeed = 720f;

        [Tooltip("Downward gravity force applied to keep character grounded.")]
        [SerializeField] private float gravity = 18f;

        [Header("State")]
        [SerializeField] private bool canMove = true;

        private Vector3 _currentVelocity;
        private float _verticalVelocity;

        public bool CanMove => canMove;
        public bool IsMoving => _currentVelocity.sqrMagnitude > 0.05f;
        public float CurrentSpeed => _currentVelocity.magnitude;
        public Vector3 MovementVelocity => _currentVelocity;
        public Transform VisualRoot => visualRoot;
        public Vector3 FacingDirection => visualRoot != null ? visualRoot.forward : transform.forward;

        private void Reset()
        {
            characterController = GetComponent<CharacterController>();
            inputReader = GetComponent<PlayerInputReader>();
        }

        private void Awake()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }
        }

        private void Update()
        {
            UpdateMovement();
        }

        public void SetMovementLocked(bool locked)
        {
            canMove = !locked;
            if (locked)
            {
                _currentVelocity = Vector3.zero;
            }
        }

        private void UpdateMovement()
        {
            Vector2 rawInput = (canMove && inputReader != null) ? inputReader.MoveInput : Vector2.zero;

            // Compute camera-relative world direction
            Vector3 cameraForward = Vector3.forward;
            Vector3 cameraRight = Vector3.right;

            if (CafeCameraRig.Instance != null)
            {
                cameraForward = CafeCameraRig.Instance.GetPlanarForward();
                cameraRight = CafeCameraRig.Instance.GetPlanarRight();
            }
            else if (Camera.main != null)
            {
                Vector3 camFwd = Camera.main.transform.forward;
                camFwd.y = 0f;
                cameraForward = camFwd.normalized;

                Vector3 camRight = Camera.main.transform.right;
                camRight.y = 0f;
                cameraRight = camRight.normalized;
            }

            Vector3 targetDirection = (cameraForward * rawInput.y + cameraRight * rawInput.x);
            if (targetDirection.sqrMagnitude > 1f)
            {
                targetDirection.Normalize();
            }

            Vector3 targetVelocity = targetDirection * walkSpeed;
            float rate = targetVelocity.sqrMagnitude > 0.01f ? acceleration : deceleration;
            _currentVelocity = Vector3.MoveTowards(_currentVelocity, targetVelocity, rate * Time.deltaTime);

            // Handle gravity and grounding
            if (characterController.isGrounded)
            {
                _verticalVelocity = -2f; // Slight downward snap for slopes and stairs
            }
            else
            {
                _verticalVelocity -= gravity * Time.deltaTime;
            }

            Vector3 motion = _currentVelocity;
            motion.y = _verticalVelocity;

            characterController.Move(motion * Time.deltaTime);

            // Rotate visual root toward movement direction
            if (visualRoot != null && targetDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                visualRoot.rotation = Quaternion.RotateTowards(visualRoot.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (characterController == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "Player",
                    "PlayerController is missing a CharacterController reference.",
                    this));
            }

            if (visualRoot == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Player",
                    "PlayerController visualRoot is unassigned. The character model will not rotate visually toward movement.",
                    this));
            }
        }
    }
}
