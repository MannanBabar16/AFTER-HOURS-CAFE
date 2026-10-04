using System.Collections.Generic;
using DG.Tweening;
using Mannan.Core.Validation;
using Mannan.Interaction;
using Mannan.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mannan.Coffee
{
    /// <summary>
    /// Minimalist screen-space HUD for the espresso workstation.
    /// Displays contextual action prompts (e.g. "[E] Place Cup", "[E] Brew Espresso"),
    /// extraction progress bar, and completion feedback.
    /// Automatically reflects the player's active input device without hardcoded keys.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/Coffee/Espresso Workstation UI")]
    public sealed class EspressoWorkstationUI : MonoBehaviour, IValidatable
    {
        [Header("References")]
        [Tooltip("The workstation logic component to observe.")]
        [SerializeField] private EspressoWorkstation workstation;

        [Tooltip("CanvasGroup controlling HUD visibility.")]
        [SerializeField] private CanvasGroup rootCanvasGroup;

        [Header("Action Prompt")]
        [Tooltip("TMP label showing the interact action keycap badge (e.g. '[ E ]', '[ A ]').")]
        [SerializeField] private TMP_Text keyBadgeLabel;

        [Tooltip("TMP label showing the action text (e.g. 'Place Cup', 'Brew Espresso').")]
        [SerializeField] private TMP_Text actionTextLabel;

        [Header("Progress Display")]
        [Tooltip("CanvasGroup for the extraction progress container.")]
        [SerializeField] private CanvasGroup progressCanvasGroup;

        [Tooltip("UI Image fill for the extraction progress bar.")]
        [SerializeField] private Image progressFillBar;

        [Tooltip("TMP label showing extraction percentage or drink name.")]
        [SerializeField] private TMP_Text progressStatusLabel;

        [Header("Completion Feedback")]
        [Tooltip("CanvasGroup for completion celebration banner.")]
        [SerializeField] private CanvasGroup completionCanvasGroup;

        [Tooltip("TMP label for completion message.")]
        [SerializeField] private TMP_Text completionLabel;

        private PlayerInputReader _activeInputReader;
        private bool _isWorkstationOpen;

        private void Reset()
        {
            rootCanvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            if (rootCanvasGroup != null) rootCanvasGroup.alpha = 0f;
            if (progressCanvasGroup != null) progressCanvasGroup.alpha = 0f;
            if (completionCanvasGroup != null) completionCanvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            WorkstationInteractable.OnWorkstationEnteredGlobal += HandleWorkstationEntered;
            WorkstationInteractable.OnWorkstationExitedGlobal += HandleWorkstationExited;

            if (workstation != null)
            {
                workstation.OnStateChanged += HandleStateChanged;
                workstation.OnExtractionProgressChanged += HandleProgressChanged;
            }
        }

        private void OnDisable()
        {
            WorkstationInteractable.OnWorkstationEnteredGlobal -= HandleWorkstationEntered;
            WorkstationInteractable.OnWorkstationExitedGlobal -= HandleWorkstationExited;

            if (workstation != null)
            {
                workstation.OnStateChanged -= HandleStateChanged;
                workstation.OnExtractionProgressChanged -= HandleProgressChanged;
            }

            if (_activeInputReader != null)
            {
                _activeInputReader.OnControlSchemeChanged -= HandleControlSchemeChanged;
            }
        }

        private void HandleWorkstationEntered(WorkstationInteractable ws)
        {
            if (workstation == null || ws.gameObject != workstation.gameObject)
            {
                return;
            }

            _isWorkstationOpen = true;

            // Resolve player input reader
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                _activeInputReader = playerGo.GetComponent<PlayerInputReader>();
                if (_activeInputReader != null)
                {
                    _activeInputReader.OnControlSchemeChanged += HandleControlSchemeChanged;
                }
            }

            UpdateUIForState(workstation.CurrentState);

            // Fade in root HUD
            if (rootCanvasGroup != null)
            {
                DOTween.To(() => rootCanvasGroup.alpha, a => rootCanvasGroup.alpha = a, 1f, 0.25f);
            }
        }

        private void HandleWorkstationExited(WorkstationInteractable ws)
        {
            if (workstation == null || ws.gameObject != workstation.gameObject)
            {
                return;
            }

            _isWorkstationOpen = false;

            if (_activeInputReader != null)
            {
                _activeInputReader.OnControlSchemeChanged -= HandleControlSchemeChanged;
                _activeInputReader = null;
            }

            // Fade out root HUD
            if (rootCanvasGroup != null)
            {
                DOTween.To(() => rootCanvasGroup.alpha, a => rootCanvasGroup.alpha = a, 0f, 0.2f);
            }
        }

        private void HandleControlSchemeChanged(ActiveControlScheme scheme)
        {
            if (!_isWorkstationOpen || workstation == null) return;
            UpdateKeycapBadge();
        }

        private void HandleStateChanged(EspressoMachineState newState)
        {
            if (!_isWorkstationOpen) return;
            UpdateUIForState(newState);
        }

        private void HandleProgressChanged(float progress)
        {
            if (progressFillBar != null)
            {
                progressFillBar.fillAmount = progress;
            }

            if (progressStatusLabel != null)
            {
                progressStatusLabel.text = $"Extracting Espresso... {Mathf.RoundToInt(progress * 100f)}%";
            }
        }

        private void UpdateUIForState(EspressoMachineState state)
        {
            UpdateKeycapBadge();

            switch (state)
            {
                case EspressoMachineState.Idle:
                    SetActionPrompt(true, "Place Cup");
                    SetProgressVisible(false);
                    SetCompletionVisible(false);
                    break;

                case EspressoMachineState.CupReady:
                    SetActionPrompt(true, "Brew Espresso");
                    SetProgressVisible(false);
                    SetCompletionVisible(false);
                    break;

                case EspressoMachineState.Extracting:
                    SetActionPrompt(false, "");
                    SetProgressVisible(true);
                    SetCompletionVisible(false);
                    if (progressFillBar != null) progressFillBar.fillAmount = 0f;
                    break;

                case EspressoMachineState.Completed:
                    SetActionPrompt(true, "Take Espresso");
                    SetProgressVisible(false);
                    SetCompletionVisible(true);
                    break;
            }
        }

        private void UpdateKeycapBadge()
        {
            if (keyBadgeLabel == null) return;
            string key = _activeInputReader != null ? _activeInputReader.GetInteractBindingDisplayString() : "E";
            keyBadgeLabel.text = $"[ {key} ]";
        }

        private void SetActionPrompt(bool visible, string actionText)
        {
            if (keyBadgeLabel != null) keyBadgeLabel.gameObject.SetActive(visible);
            if (actionTextLabel != null)
            {
                actionTextLabel.gameObject.SetActive(visible);
                actionTextLabel.text = actionText;
            }
        }

        private void SetProgressVisible(bool visible)
        {
            if (progressCanvasGroup == null) return;
            DOTween.To(() => progressCanvasGroup.alpha, a => progressCanvasGroup.alpha = a, visible ? 1f : 0f, 0.15f);
        }

        private void SetCompletionVisible(bool visible)
        {
            if (completionCanvasGroup == null) return;
            DOTween.To(() => completionCanvasGroup.alpha, a => completionCanvasGroup.alpha = a, visible ? 1f : 0f, 0.2f);
            if (visible && completionLabel != null)
            {
                completionLabel.text = "Espresso Ready!";
                completionLabel.transform.localScale = Vector3.one * 0.9f;
                DOTween.To(() => completionLabel.transform.localScale, s => completionLabel.transform.localScale = s, Vector3.one, 0.25f).SetEase(Ease.OutBack);
            }
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (workstation == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Coffee",
                    "EspressoWorkstationUI workstation is unassigned.",
                    this));
            }

            if (keyBadgeLabel == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Coffee",
                    "EspressoWorkstationUI keyBadgeLabel is unassigned.",
                    this));
            }
        }
    }
}
