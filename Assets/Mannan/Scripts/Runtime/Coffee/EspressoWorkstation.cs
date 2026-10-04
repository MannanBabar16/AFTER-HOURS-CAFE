using System;
using System.Collections.Generic;
using DG.Tweening;
using Mannan.Core.Logging;
using Mannan.Core.Validation;
using Mannan.Interaction;
using Mannan.Player;
using UnityEngine;

namespace Mannan.Coffee
{
    /// <summary>
    /// Core logic and state authority for the espresso workstation.
    /// Manages cup positioning, extraction lifecycle, and valid state transitions.
    /// Exposes presentation hooks so audio, VFX, and UI remain decoupled.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(WorkstationInteractable))]
    [AddComponentMenu("Mannan/Coffee/Espresso Workstation")]
    public sealed class EspressoWorkstation : MonoBehaviour, IValidatable
    {
        [Header("Configuration")]
        [Tooltip("The drink recipe definition extracted by this machine.")]
        [SerializeField] private DrinkDefinition defaultDrinkDefinition;

        [Header("Sockets")]
        [Tooltip("Transform socket directly under the portafilter spouts where the cup rests during brewing.")]
        [SerializeField] private Transform cupSocket;

        [Tooltip("Transform socket on the counter where an empty/staged cup rests before brewing.")]
        [SerializeField] private Transform cupStagingSocket;

        [Header("Cup")]
        [Tooltip("The active CoffeeCup instance managed by this workstation.")]
        [SerializeField] private CoffeeCup activeCup;

        [Header("Tuning")]
        [Tooltip("Duration of the cup placement transition animation.")]
        [SerializeField] private float cupMoveDuration = 0.45f;

        [Tooltip("Arc height when moving the cup between staging and brewing socket.")]
        [SerializeField] private float cupMoveArcHeight = 0.08f;

        public EspressoMachineState CurrentState { get; private set; } = EspressoMachineState.Idle;
        public float CurrentProgress { get; private set; }
        public DrinkDefinition ActiveDrink => defaultDrinkDefinition;
        public CoffeeCup ActiveCup => activeCup;
        public bool IsCupInBrewSocket => CurrentState == EspressoMachineState.CupReady ||
                                         CurrentState == EspressoMachineState.Extracting ||
                                         CurrentState == EspressoMachineState.Completed;

        // Presentation Hooks / Events
        public event Action OnCupPlaced;
        public event Action OnCupRemoved;
        public event Action<DrinkDefinition> OnExtractionStarted;
        public event Action<float> OnExtractionProgressChanged;
        public event Action<DrinkDefinition> OnExtractionCompleted;
        public event Action OnWorkstationReset;
        public event Action<EspressoMachineState> OnStateChanged;

        private WorkstationInteractable _workstationInteractable;
        private PlayerInputReader _activeInputReader;
        private Tween _cupTween;
        private float _extractionTimer;

        private void Awake()
        {
            _workstationInteractable = GetComponent<WorkstationInteractable>();
            InitializeCupPlacement();
        }

        private void OnEnable()
        {
            WorkstationInteractable.OnWorkstationEnteredGlobal += HandleWorkstationEntered;
            WorkstationInteractable.OnWorkstationExitedGlobal += HandleWorkstationExited;
        }

        private void OnDisable()
        {
            WorkstationInteractable.OnWorkstationEnteredGlobal -= HandleWorkstationEntered;
            WorkstationInteractable.OnWorkstationExitedGlobal -= HandleWorkstationExited;
            _cupTween?.Kill();
            UnbindInputReader();
        }

        private void Update()
        {
            if (_workstationInteractable == null || !_workstationInteractable.IsInWorkstation)
            {
                return;
            }

            // In workstation mode, listen for contextual player interaction (E / A)
            if (_activeInputReader != null && _activeInputReader.ConsumeInteractTriggered())
            {
                HandleContextualInteract();
            }

            // Update extraction progress if brewing
            if (CurrentState == EspressoMachineState.Extracting)
            {
                UpdateExtraction(Time.deltaTime);
            }
        }

        private void InitializeCupPlacement()
        {
            if (activeCup == null)
            {
                return;
            }

            activeCup.ResetCup();
            if (cupStagingSocket != null)
            {
                activeCup.transform.position = cupStagingSocket.position;
                activeCup.transform.rotation = cupStagingSocket.rotation;
            }
            CurrentState = EspressoMachineState.Idle;
        }

        private void HandleWorkstationEntered(WorkstationInteractable ws)
        {
            if (ws != _workstationInteractable) return;

            // Resolve player input reader
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                _activeInputReader = playerGo.GetComponent<PlayerInputReader>();
            }

            BindInputReader();
        }

        private void HandleWorkstationExited(WorkstationInteractable ws)
        {
            if (ws != _workstationInteractable) return;
            UnbindInputReader();
        }

        private void BindInputReader()
        {
            if (_activeInputReader != null)
            {
                _activeInputReader.OnInteractPressed += HandleContextualInteract;
            }
        }

        private void UnbindInputReader()
        {
            if (_activeInputReader != null)
            {
                _activeInputReader.OnInteractPressed -= HandleContextualInteract;
                _activeInputReader = null;
            }
        }

        public void HandleContextualInteract()
        {
            if (_workstationInteractable == null || !_workstationInteractable.IsInWorkstation)
            {
                return;
            }

            switch (CurrentState)
            {
                case EspressoMachineState.Idle:
                    TryPlaceCup();
                    break;
                case EspressoMachineState.CupReady:
                    TryStartExtraction();
                    break;
                case EspressoMachineState.Extracting:
                    // Extraction in progress; ignore spam
                    break;
                case EspressoMachineState.Completed:
                    TryResetOrTakeCup();
                    break;
            }
        }

        public bool TryPlaceCup()
        {
            if (CurrentState != EspressoMachineState.Idle || activeCup == null || cupSocket == null)
            {
                return false;
            }

            _cupTween?.Kill();
            Vector3 startPos = activeCup.transform.position;
            Quaternion startRot = activeCup.transform.rotation;
            Vector3 targetPos = cupSocket.position;
            Quaternion targetRot = cupSocket.rotation;

            if (cupMoveDuration <= 0.001f)
            {
                activeCup.transform.position = targetPos;
                activeCup.transform.rotation = targetRot;
                activeCup.PlayPlacementReaction();
                SetState(EspressoMachineState.CupReady);
                OnCupPlaced?.Invoke();
                return true;
            }

            _cupTween = DOTween.To(() => 0f, t =>
            {
                float arc = Mathf.Sin(t * Mathf.PI) * cupMoveArcHeight;
                activeCup.transform.position = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * arc;
                activeCup.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            }, 1f, cupMoveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                activeCup.PlayPlacementReaction();
                SetState(EspressoMachineState.CupReady);
                OnCupPlaced?.Invoke();
            });

            return true;
        }

        public bool TryStartExtraction()
        {
            if (CurrentState != EspressoMachineState.CupReady || defaultDrinkDefinition == null || activeCup == null)
            {
                return false;
            }

            _extractionTimer = 0f;
            CurrentProgress = 0f;
            activeCup.SetDrink(defaultDrinkDefinition);
            activeCup.SetFillAmount(0f);

            SetState(EspressoMachineState.Extracting);
            OnExtractionStarted?.Invoke(defaultDrinkDefinition);
            return true;
        }

        public void UpdateExtraction(float deltaTime)
        {
            float duration = defaultDrinkDefinition != null ? defaultDrinkDefinition.ExtractionDuration : 3.5f;
            _extractionTimer += deltaTime;
            CurrentProgress = Mathf.Clamp01(_extractionTimer / duration);

            activeCup?.SetFillAmount(CurrentProgress);
            OnExtractionProgressChanged?.Invoke(CurrentProgress);

            if (CurrentProgress >= 1.0f)
            {
                CompleteExtraction();
            }
        }

        private void CompleteExtraction()
        {
            CurrentProgress = 1.0f;
            activeCup?.SetFillAmount(1.0f);
            activeCup?.PlayCompletionReaction();

            SetState(EspressoMachineState.Completed);
            OnExtractionCompleted?.Invoke(defaultDrinkDefinition);
            CafeLogger.Log("EspressoWorkstation", "Espresso extraction completed successfully!", this);
        }

        public bool TryResetOrTakeCup()
        {
            if (CurrentState != EspressoMachineState.Completed || activeCup == null)
            {
                return false;
            }

            _cupTween?.Kill();
            Vector3 startPos = activeCup.transform.position;
            Quaternion startRot = activeCup.transform.rotation;
            Vector3 targetPos = cupStagingSocket != null ? cupStagingSocket.position : startPos;
            Quaternion targetRot = cupStagingSocket != null ? cupStagingSocket.rotation : startRot;

            if (cupMoveDuration <= 0.001f)
            {
                activeCup.transform.position = targetPos;
                activeCup.transform.rotation = targetRot;
                activeCup.ResetCup();
                activeCup.PlayPlacementReaction();
                SetState(EspressoMachineState.Idle);
                OnWorkstationReset?.Invoke();
                OnCupRemoved?.Invoke();
                return true;
            }

            _cupTween = DOTween.To(() => 0f, t =>
            {
                float arc = Mathf.Sin(t * Mathf.PI) * cupMoveArcHeight;
                activeCup.transform.position = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * arc;
                activeCup.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            }, 1f, cupMoveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                activeCup.ResetCup();
                activeCup.PlayPlacementReaction();
                SetState(EspressoMachineState.Idle);
                OnWorkstationReset?.Invoke();
                OnCupRemoved?.Invoke();
            });

            return true;
        }

        private void SetState(EspressoMachineState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (defaultDrinkDefinition == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Coffee",
                    "EspressoWorkstation is missing a defaultDrinkDefinition reference.",
                    this));
            }

            if (cupSocket == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "Coffee",
                    "EspressoWorkstation is missing cupSocket transform under group head.",
                    this));
            }

            if (cupStagingSocket == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Coffee",
                    "EspressoWorkstation is missing cupStagingSocket transform.",
                    this));
            }

            if (activeCup == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Coffee",
                    "EspressoWorkstation has no activeCup assigned.",
                    this));
            }
        }
    }
}
