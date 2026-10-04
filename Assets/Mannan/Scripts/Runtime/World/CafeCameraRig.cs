using System;
using System.Collections.Generic;
using DG.Tweening;
using Mannan.Core.Logging;
using Mannan.Core.Validation;
using UnityEngine;

namespace Mannan.World
{
    /// <summary>
    /// 3/4 isometric-inspired perspective gameplay camera rig.
    /// Manages smooth follow tracking and contextual transitions to workstation close-up anchors.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Mannan/World/Cafe Camera Rig")]
    public sealed class CafeCameraRig : MonoBehaviour, IValidatable
    {
        public static CafeCameraRig Instance { get; private set; }

        public enum CameraMode
        {
            GameplayFollow = 0,
            WorkstationContext = 1,
            Transitioning = 2
        }

        [Header("Target & Follow")]
        [Tooltip("The player or target transform to follow.")]
        [SerializeField] private Transform followTarget;

        [Tooltip("Offset applied to the target position (e.g. centering at character chest height).")]
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);

        [Header("3/4 Perspective Tuning")]
        [Range(25f, 60f)]
        [Tooltip("Downward camera pitch angle in degrees (default 38-42 deg for diorama feel).")]
        [SerializeField] private float pitchAngle = 40f;

        [Range(0f, 360f)]
        [Tooltip("Yaw orbit angle around target (default 45 deg for classic 3/4 isometric diorama view).")]
        [SerializeField] private float yawAngle = 45f;

        [Range(5f, 20f)]
        [Tooltip("Distance from camera to follow target.")]
        [SerializeField] private float distance = 9.5f;

        [Range(20f, 60f)]
        [Tooltip("Field of view in degrees. Narrower FOV (~35-40 deg) provides diorama perspective with minimal distortion.")]
        [SerializeField] private float fieldOfView = 38f;

        [Range(0.01f, 0.5f)]
        [Tooltip("Smooth follow damping time in seconds.")]
        [SerializeField] private float followDamping = 0.12f;

        [Header("State")]
        [SerializeField] private CameraMode currentMode = CameraMode.GameplayFollow;

        private Camera _camera;
        private Vector3 _followVelocity;
        private Transform _activeAnchor;
        private Sequence _transitionSequence;

        public Camera UnityCamera => _camera;
        public CameraMode CurrentMode => currentMode;
        public bool IsInWorkstation => currentMode == CameraMode.WorkstationContext;
        public Transform FollowTarget => followTarget;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            _camera = GetComponent<Camera>();
            _camera.orthographic = false;
            _camera.fieldOfView = fieldOfView;
        }

        private void Start()
        {
            if (followTarget != null)
            {
                // Snap directly on start to avoid initial camera sweep
                transform.position = CalculateDesiredGameplayPosition();
                transform.rotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
            }
        }

        private void LateUpdate()
        {
            if (currentMode == CameraMode.GameplayFollow)
            {
                UpdateGameplayFollow();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            _transitionSequence?.Kill();
        }

        public void SetFollowTarget(Transform newTarget)
        {
            followTarget = newTarget;
        }

        private void UpdateGameplayFollow()
        {
            if (followTarget == null)
            {
                return;
            }

            Vector3 desiredPosition = CalculateDesiredGameplayPosition();
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _followVelocity, followDamping);
            transform.rotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
        }

        public Vector3 CalculateDesiredGameplayPosition()
        {
            if (followTarget == null)
            {
                return transform.position;
            }

            Vector3 focusPoint = followTarget.position + targetOffset;
            Quaternion orbitRotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
            Vector3 backwardOffset = orbitRotation * (Vector3.back * distance);
            return focusPoint + backwardOffset;
        }

        /// <summary>
        /// Smoothly transitions the camera to an authored workstation close-up anchor.
        /// </summary>
        public void TransitionToAnchor(Transform anchorTransform, float duration = 0.75f, Action onComplete = null)
        {
            if (anchorTransform == null)
            {
                CafeLogger.LogWarning("Camera", "Cannot transition to null workstation anchor.", this);
                return;
            }

            _activeAnchor = anchorTransform;
            currentMode = CameraMode.Transitioning;
            _transitionSequence?.Kill();

            CafeLogger.Log("Camera", $"Transitioning to workstation anchor '{anchorTransform.name}' over {duration:F2}s");

            _transitionSequence = DOTween.Sequence();
            _transitionSequence.Join(transform.DOMove(anchorTransform.position, duration).SetEase(Ease.OutCubic));
            _transitionSequence.Join(transform.DORotateQuaternion(anchorTransform.rotation, duration).SetEase(Ease.OutCubic));
            
            // If the anchor specifies custom FOV via a Camera component, match it, otherwise default to 32 deg for close-up
            float targetFov = 32f;
            if (anchorTransform.TryGetComponent<Camera>(out var anchorCam))
            {
                targetFov = anchorCam.fieldOfView;
            }
            _transitionSequence.Join(_camera.DOFieldOfView(targetFov, duration).SetEase(Ease.OutCubic));

            _transitionSequence.OnComplete(() =>
            {
                currentMode = CameraMode.WorkstationContext;
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Smoothly returns camera from workstation close-up to the standard 3/4 gameplay diorama follow view.
        /// </summary>
        public void ReturnToGameplay(float duration = 0.75f, Action onComplete = null)
        {
            currentMode = CameraMode.Transitioning;
            _activeAnchor = null;
            _transitionSequence?.Kill();

            Vector3 targetGameplayPos = CalculateDesiredGameplayPosition();
            Quaternion targetGameplayRot = Quaternion.Euler(pitchAngle, yawAngle, 0f);

            CafeLogger.Log("Camera", $"Returning camera to standard 3/4 gameplay view over {duration:F2}s");

            _transitionSequence = DOTween.Sequence();
            _transitionSequence.Join(transform.DOMove(targetGameplayPos, duration).SetEase(Ease.OutCubic));
            _transitionSequence.Join(transform.DORotateQuaternion(targetGameplayRot, duration).SetEase(Ease.OutCubic));
            _transitionSequence.Join(_camera.DOFieldOfView(fieldOfView, duration).SetEase(Ease.OutCubic));

            _transitionSequence.OnComplete(() =>
            {
                currentMode = CameraMode.GameplayFollow;
                _followVelocity = Vector3.zero;
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Planar camera forward vector projected on XZ plane, normalized.
        /// </summary>
        public Vector3 GetPlanarForward()
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        }

        /// <summary>
        /// Planar camera right vector projected on XZ plane, normalized.
        /// </summary>
        public Vector3 GetPlanarRight()
        {
            Vector3 right = transform.right;
            right.y = 0f;
            return right.sqrMagnitude > 0.001f ? right.normalized : Vector3.right;
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (followTarget == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Camera",
                    "CafeCameraRig does not have a Follow Target assigned.",
                    this));
            }

            if (distance < 3f)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Camera",
                    $"Camera distance ({distance}) is unusually close for a 3/4 diorama view.",
                    this));
            }
        }
    }
}
