using System.Collections.Generic;
using Mannan.Core.Logging;
using Mannan.Core.Validation;
using UnityEngine;

namespace Mannan.Core.Bootstrap
{
    /// <summary>
    /// Lightweight scene/session composition root.
    /// Explicitly triggers lifecycle setup for registered services without functioning as a god object.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mannan/Core/Cafe Bootstrap")]
    public sealed class CafeBootstrap : MonoBehaviour, IValidatable
    {
        [Header("Configuration")]
        [Tooltip("If true, automatically initialises registered providers on Awake.")]
        [SerializeField] private bool initializeOnAwake = true;

        [Header("Scene Providers")]
        [Tooltip("Optional explicit MonoBehaviour references implementing IInitializable.")]
        [SerializeField] private List<MonoBehaviour> initializables = new List<MonoBehaviour>();

        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (initializeOnAwake)
            {
                InitializeServices();
            }
        }

        private void OnDestroy()
        {
            ShutdownServices();
        }

        public void InitializeServices()
        {
            if (IsInitialized)
            {
                return;
            }

            CafeLogger.Log("Bootstrap", "Initialising core cafe session providers...");

            for (int i = 0; i < initializables.Count; i++)
            {
                MonoBehaviour entry = initializables[i];
                if (entry is IInitializable initializable)
                {
                    initializable.Initialize();
                }
                else if (entry != null)
                {
                    CafeLogger.LogWarning("Bootstrap", $"Assigned provider '{entry.name}' does not implement IInitializable.", this);
                }
            }

            IsInitialized = true;
            CafeLogger.Log("Bootstrap", "Cafe bootstrap initialisation completed.");
        }

        public void ShutdownServices()
        {
            if (!IsInitialized)
            {
                return;
            }

            for (int i = initializables.Count - 1; i >= 0; i--)
            {
                MonoBehaviour entry = initializables[i];
                if (entry is IInitializable initializable)
                {
                    initializable.Shutdown();
                }
            }

            IsInitialized = false;
            CafeLogger.Log("Bootstrap", "Cafe bootstrap shutdown completed.");
        }

        public void Validate(List<ValidationIssue> issues)
        {
            for (int i = 0; i < initializables.Count; i++)
            {
                MonoBehaviour entry = initializables[i];
                if (entry == null)
                {
                    issues.Add(new ValidationIssue(
                        ValidationSeverity.Warning,
                        "Bootstrap",
                        $"Null entry at index {i} in CafeBootstrap initializables list.",
                        this));
                }
                else if (entry is not IInitializable)
                {
                    issues.Add(new ValidationIssue(
                        ValidationSeverity.Warning,
                        "Bootstrap",
                        $"Target '{entry.name}' in initializables does not implement IInitializable.",
                        this));
                }
            }
        }
    }
}
