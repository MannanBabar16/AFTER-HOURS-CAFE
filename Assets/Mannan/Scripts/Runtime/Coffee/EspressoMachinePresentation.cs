using System.Collections.Generic;
using DG.Tweening;
using Mannan.Core.Validation;
using UnityEngine;

namespace Mannan.Coffee
{
    /// <summary>
    /// Presentation layer for the espresso machine.
    /// Handles VFX (espresso pour stream, rising steam), audio cues, and DOTween mechanical animations.
    /// Fully decoupled from brewing logic by listening exclusively to EspressoWorkstation events.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EspressoWorkstation))]
    [AddComponentMenu("Mannan/Coffee/Espresso Machine Presentation")]
    public sealed class EspressoMachinePresentation : MonoBehaviour, IValidatable
    {
        [Header("VFX References")]
        [Tooltip("Particle system representing the liquid espresso stream pouring into the cup.")]
        [SerializeField] private ParticleSystem pourStreamVfx;

        [Tooltip("Particle system representing the warm steam rising from the fresh espresso.")]
        [SerializeField] private ParticleSystem steamVfx;

        [Header("Mechanical Animation")]
        [Tooltip("Transform of the brew button or portafilter lever for tactile press animation.")]
        [SerializeField] private Transform brewButtonTransform;

        [Tooltip("Visual root of the espresso machine used for subtle vibration during extraction.")]
        [SerializeField] private Transform machineVisualRoot;

        [Header("Audio Hooks")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip cupPlaceSfx;
        [SerializeField] private AudioClip brewButtonSfx;
        [SerializeField] private AudioClip extractionLoopSfx;
        [SerializeField] private AudioClip extractionCompleteSfx;

        private EspressoWorkstation _workstation;
        private Tween _buttonTween;
        private Tween _vibrationTween;
        private Tween _steamTween;
        private Vector3 _buttonBasePos;
        private Vector3 _machineBasePos;

        private void Awake()
        {
            _workstation = GetComponent<EspressoWorkstation>();

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (brewButtonTransform != null)
            {
                _buttonBasePos = brewButtonTransform.localPosition;
            }

            if (machineVisualRoot != null)
            {
                _machineBasePos = machineVisualRoot.localPosition;
            }

            // Ensure VFX are stopped on awake
            StopPourVfx();
            StopSteamVfx();
        }

        private void OnEnable()
        {
            if (_workstation != null)
            {
                _workstation.OnCupPlaced += HandleCupPlaced;
                _workstation.OnExtractionStarted += HandleExtractionStarted;
                _workstation.OnExtractionCompleted += HandleExtractionCompleted;
                _workstation.OnWorkstationReset += HandleWorkstationReset;
            }
        }

        private void OnDisable()
        {
            if (_workstation != null)
            {
                _workstation.OnCupPlaced -= HandleCupPlaced;
                _workstation.OnExtractionStarted -= HandleExtractionStarted;
                _workstation.OnExtractionCompleted -= HandleExtractionCompleted;
                _workstation.OnWorkstationReset -= HandleWorkstationReset;
            }

            _buttonTween?.Kill();
            _vibrationTween?.Kill();
            _steamTween?.Kill();
            StopPourVfx();
            StopSteamVfx();
        }

        private void HandleCupPlaced()
        {
            PlaySound(cupPlaceSfx, 0.7f, 1.0f);
        }

        private void HandleExtractionStarted(DrinkDefinition drink)
        {
            PlaySound(brewButtonSfx, 0.8f, 1.05f);

            // Button tactile mechanical press animation
            if (brewButtonTransform != null)
            {
                _buttonTween?.Kill();
                brewButtonTransform.localPosition = _buttonBasePos;
                Vector3 pressedPos = _buttonBasePos - new Vector3(0f, 0.008f, 0f);

                _buttonTween = DOTween.Sequence()
                    .Append(DOTween.To(() => brewButtonTransform.localPosition, p => brewButtonTransform.localPosition = p, pressedPos, 0.08f).SetEase(Ease.OutQuad))
                    .Append(DOTween.To(() => brewButtonTransform.localPosition, p => brewButtonTransform.localPosition = p, _buttonBasePos, 0.12f).SetEase(Ease.OutBounce))
                    .SetTarget(brewButtonTransform);
            }

            // Subtle machine vibration
            if (machineVisualRoot != null)
            {
                _vibrationTween?.Kill();
                machineVisualRoot.localPosition = _machineBasePos;
                _vibrationTween = DOTween.To(() => 0f, t =>
                {
                    float jitterX = Mathf.Sin(t * 80f) * 0.0006f;
                    float jitterY = Mathf.Cos(t * 90f) * 0.0004f;
                    machineVisualRoot.localPosition = _machineBasePos + new Vector3(jitterX, jitterY, 0f);
                }, 1f, drink != null ? drink.ExtractionDuration : 3.5f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    machineVisualRoot.localPosition = _machineBasePos;
                });
            }

            // Start VFX
            StartPourVfx();
            StartSteamVfx();

            // Start extraction audio loop
            if (extractionLoopSfx != null && audioSource != null)
            {
                audioSource.clip = extractionLoopSfx;
                audioSource.loop = true;
                audioSource.volume = 0.65f;
                audioSource.pitch = 1.0f;
                audioSource.Play();
            }
        }

        private void HandleExtractionCompleted(DrinkDefinition drink)
        {
            StopPourVfx();

            // Stop extraction loop audio
            if (audioSource != null && audioSource.loop)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }

            PlaySound(extractionCompleteSfx, 0.9f, 1.0f);

            // Stop vibration
            _vibrationTween?.Kill();
            if (machineVisualRoot != null)
            {
                machineVisualRoot.localPosition = _machineBasePos;
            }

            // Let steam linger for 3 seconds then stop
            _steamTween?.Kill();
            _steamTween = DOTween.Sequence()
                .AppendInterval(3.0f)
                .AppendCallback(StopSteamVfx)
                .SetTarget(this);
        }

        private void HandleWorkstationReset()
        {
            StopPourVfx();
            StopSteamVfx();
            _steamTween?.Kill();
            _vibrationTween?.Kill();
            if (machineVisualRoot != null)
            {
                machineVisualRoot.localPosition = _machineBasePos;
            }
        }

        private void StartPourVfx()
        {
            if (pourStreamVfx != null)
            {
                pourStreamVfx.Play(true);
            }
        }

        private void StopPourVfx()
        {
            if (pourStreamVfx != null)
            {
                pourStreamVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void StartSteamVfx()
        {
            if (steamVfx != null)
            {
                steamVfx.Play(true);
            }
        }

        private void StopSteamVfx()
        {
            if (steamVfx != null)
            {
                steamVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void PlaySound(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            if (clip == null || audioSource == null) return;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip, volume);
        }

        public void Validate(List<ValidationIssue> issues)
        {
            if (pourStreamVfx == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Coffee",
                    "EspressoMachinePresentation pourStreamVfx is unassigned.",
                    this));
            }

            if (steamVfx == null)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "Coffee",
                    "EspressoMachinePresentation steamVfx is unassigned.",
                    this));
            }
        }
    }
}
