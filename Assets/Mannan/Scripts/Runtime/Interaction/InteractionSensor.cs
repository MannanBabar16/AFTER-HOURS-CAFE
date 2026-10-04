using System;
using System.Collections.Generic;
using Mannan.Core.Validation;
using Mannan.Player;
using UnityEngine;

namespace Mannan.Interaction
{
    /// <summary>
    /// Player-facing proximity and orientation sensor for 3/4 diorama perspective.
    /// Finds the best interactable candidate based on distance and facing angle without scene raycasts.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/Interaction/Interaction Sensor")]
    public sealed class InteractionSensor : MonoBehaviour, IValidatable
    {
        [Header("Detection Settings")]
        [Tooltip("Maximum distance to detect interactable objects.")]
        [SerializeField] private float detectionRadius = 2.2f;

        [Tooltip("LayerMask used to query interactable colliders.")]
        [SerializeField] private LayerMask interactableLayer = ~0;

        [Range(0f, 1f)]
        [Tooltip("Weight given to the player's facing direction relative to the object (0 = ignore facing, 1 = strict facing).")]
        [SerializeField] private float forwardWeight = 0.55f;

        [Range(0f, 1f)]
        [Tooltip("Weight given to distance proximity.")]
        [SerializeField] private float distanceWeight = 0.45f;

        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerInputReader inputReader;

        public event Action<IInteractable> OnFocusChanged;
        public event Action<IInteractable> OnInteractionExecuted;

        public IInteractable CurrentFocus { get; private set; }
        public float DetectionRadius => detectionRadius;
        public PlayerInputReader InputReader => inputReader;

        private readonly Collider[] _overlapBuffer = new Collider[16];
        private readonly List<IInteractable> _candidateCache = new List<IInteractable>(8);

        private void Reset()
        {
            playerController = GetComponent<PlayerController>();
            inputReader = GetComponent<PlayerInputReader>();
        }

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.OnInteractPressed += HandleInteractPressed;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.OnInteractPressed -= HandleInteractPressed;
            }
        }

        private void Update()
        {
            UpdateSensor();
            HandleInteractionInput();
        }

        private void UpdateSensor()
        {
            if (playerController != null && !playerController.CanMove)
            {
                // When player movement is locked (e.g. inside a workstation), clear focus
                if (CurrentFocus != null)
                {
                    SetCurrentFocus(null);
                }
                return;
            }

            Vector3 sensorOrigin = transform.position + Vector3.up * 0.5f;
            Vector3 playerForward = playerController != null ? playerController.FacingDirection : transform.forward;

            int hitCount = Physics.OverlapSphereNonAlloc(sensorOrigin, detectionRadius, _overlapBuffer, interactableLayer, QueryTriggerInteraction.Collide);

            _candidateCache.Clear();
            IInteractable bestCandidate = null;
            float bestScore = -1f;

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _overlapBuffer[i];
                if (col == null || col.isTrigger && col.gameObject == gameObject)
                {
                    continue;
                }

                // Check component on hit or in parents
                IInteractable interactable = col.GetComponentInParent<IInteractable>();
                if (interactable == null || _candidateCache.Contains(interactable))
                {
                    continue;
                }

                _candidateCache.Add(interactable);

                if (!interactable.CanInteract(gameObject))
                {
                    continue;
                }

                Vector3 toCandidate = interactable.Transform.position - sensorOrigin;
                toCandidate.y = 0f;
                float distance = toCandidate.magnitude;

                if (distance > detectionRadius)
                {
                    continue;
                }

                Vector3 directionToCandidate = distance > 0.01f ? toCandidate / distance : playerForward;
                float dot = Vector3.Dot(playerForward, directionToCandidate);

                // Ignore objects distinctly behind the player (beyond 110 degrees)
                if (dot < -0.35f)
                {
                    continue;
                }

                // Normalise factors: distance score (1 at origin, 0 at radius), facing score (1 at front, 0 at sides/back)
                float normDistance = 1f - Mathf.Clamp01(distance / detectionRadius);
                float normFacing = Mathf.Clamp01((dot + 0.35f) / 1.35f);

                float score = (normFacing * forwardWeight) + (normDistance * distanceWeight);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = interactable;
                }
            }

            if (bestCandidate != CurrentFocus)
            {
                SetCurrentFocus(bestCandidate);
            }
        }

        private void SetCurrentFocus(IInteractable newFocus)
        {
            if (CurrentFocus != null)
            {
                CurrentFocus.OnFocusExit(gameObject);
            }

            CurrentFocus = newFocus;

            if (CurrentFocus != null)
            {
                CurrentFocus.OnFocusEnter(gameObject);
            }

            OnFocusChanged?.Invoke(CurrentFocus);
        }

        private int _lastInteractFrame = -1;

        private void HandleInteractPressed()
        {
            TryExecuteInteraction();
        }

        private void HandleInteractionInput()
        {
            if (inputReader != null && inputReader.ConsumeInteractTriggered())
            {
                TryExecuteInteraction();
            }
        }

        private void TryExecuteInteraction()
        {
            if (Time.frameCount == _lastInteractFrame)
            {
                return;
            }

            if (CurrentFocus != null && CurrentFocus.CanInteract(gameObject))
            {
                _lastInteractFrame = Time.frameCount;
                IInteractable target = CurrentFocus;
                target.Interact(gameObject);
                OnInteractionExecuted?.Invoke(target);
            }
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (detectionRadius < 0.5f)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Interaction",
                    $"InteractionSensor radius ({detectionRadius}) is unusually small.",
                    this));
            }

            if (inputReader == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "Interaction",
                    "InteractionSensor is missing a PlayerInputReader reference.",
                    this));
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.9f, 0.7f, 0.2f, 0.25f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, detectionRadius);
        }
    }
}
