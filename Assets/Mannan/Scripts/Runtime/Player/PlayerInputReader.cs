using UnityEngine;
using UnityEngine.InputSystem;

namespace Mannan.Player
{
    /// <summary>
    /// Action-based input reader using Unity's new Input System.
    /// Decouples gameplay systems (movement, interaction, workstations) from physical hardware keys.
    /// Supports rebindable Input Actions, gamepad and keyboard/mouse out of the box.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("Mannan/Player/Player Input Reader")]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Authored InputActionAsset. If left null, a programmatic fallback map with default bindings is generated.")]
        [SerializeField] private InputActionAsset actionsAsset;

        [Tooltip("Tracker component managing active control scheme. Auto-assigned if null.")]
        [SerializeField] private ActiveControlSchemeTracker schemeTracker;

        public Vector2 MoveInput { get; private set; }
        public bool InteractTriggered { get; private set; }
        public bool CancelTriggered { get; private set; }

        public ActiveControlScheme CurrentControlScheme => schemeTracker != null ? schemeTracker.CurrentControlScheme : ActiveControlScheme.KeyboardMouse;

        public event System.Action OnInteractPressed;
        public event System.Action OnCancelPressed;
        public event System.Action<ActiveControlScheme> OnControlSchemeChanged;

        public InputActionAsset ActionsAsset => actionsAsset;

        private InputActionMap _actionMap;
        private InputAction _moveAction;
        private InputAction _interactAction;
        private InputAction _cancelAction;
        private bool _ownsActionMap;

        private void Awake()
        {
            if (schemeTracker == null)
            {
                schemeTracker = GetComponent<ActiveControlSchemeTracker>();
                if (schemeTracker == null)
                {
                    schemeTracker = gameObject.AddComponent<ActiveControlSchemeTracker>();
                }
            }

            InitializeActions();
        }

        private void OnEnable()
        {
            if (schemeTracker != null)
            {
                schemeTracker.OnControlSchemeChanged += HandleSchemeChanged;
            }

            if (_actionMap == null)
            {
                InitializeActions();
            }

            _actionMap?.Enable();
        }

        private void OnDisable()
        {
            if (schemeTracker != null)
            {
                schemeTracker.OnControlSchemeChanged -= HandleSchemeChanged;
            }

            _actionMap?.Disable();
        }

        private void HandleSchemeChanged(ActiveControlScheme newScheme)
        {
            OnControlSchemeChanged?.Invoke(newScheme);
        }

        private void OnDestroy()
        {
            UnbindActionCallbacks();

            if (_ownsActionMap && _actionMap != null)
            {
                _actionMap.Dispose();
                _actionMap = null;
            }
        }

        private void InitializeActions()
        {
            if (_actionMap != null)
            {
                return;
            }

            if (actionsAsset != null)
            {
                _actionMap = actionsAsset.FindActionMap("Player", false);
                if (_actionMap != null)
                {
                    _moveAction = _actionMap.FindAction("Move", false);
                    _interactAction = _actionMap.FindAction("Interact", false);
                    _cancelAction = _actionMap.FindAction("Cancel", false);
                    _ownsActionMap = false;
                }
            }

            // Programmatic fallback if no asset or action map was resolved
            if (_moveAction == null || _interactAction == null || _cancelAction == null)
            {
                CreateFallbackActionMap();
            }

            BindActionCallbacks();
        }

        private void CreateFallbackActionMap()
        {
            _ownsActionMap = true;
            _actionMap = new InputActionMap("Player");

            _moveAction = _actionMap.AddAction("Move", InputActionType.Value);
            _moveAction.expectedControlType = "Vector2";
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w", groups: "Keyboard&Mouse")
                .With("Down", "<Keyboard>/s", groups: "Keyboard&Mouse")
                .With("Left", "<Keyboard>/a", groups: "Keyboard&Mouse")
                .With("Right", "<Keyboard>/d", groups: "Keyboard&Mouse");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow", groups: "Keyboard&Mouse")
                .With("Down", "<Keyboard>/downArrow", groups: "Keyboard&Mouse")
                .With("Left", "<Keyboard>/leftArrow", groups: "Keyboard&Mouse")
                .With("Right", "<Keyboard>/rightArrow", groups: "Keyboard&Mouse");
            _moveAction.AddBinding("<Gamepad>/leftStick", groups: "Gamepad");

            _interactAction = _actionMap.AddAction("Interact", InputActionType.Button);
            _interactAction.AddBinding("<Keyboard>/e", groups: "Keyboard&Mouse");
            _interactAction.AddBinding("<Gamepad>/buttonSouth", groups: "Gamepad");

            _cancelAction = _actionMap.AddAction("Cancel", InputActionType.Button);
            _cancelAction.AddBinding("<Keyboard>/q", groups: "Keyboard&Mouse");
            _cancelAction.AddBinding("<Keyboard>/escape", groups: "Keyboard&Mouse");
            _cancelAction.AddBinding("<Gamepad>/buttonEast", groups: "Gamepad");
        }

        private void BindActionCallbacks()
        {
            if (_interactAction != null)
            {
                _interactAction.performed += HandleInteractPerformed;
            }

            if (_cancelAction != null)
            {
                _cancelAction.performed += HandleCancelPerformed;
            }
        }

        private void UnbindActionCallbacks()
        {
            if (_interactAction != null)
            {
                _interactAction.performed -= HandleInteractPerformed;
            }

            if (_cancelAction != null)
            {
                _cancelAction.performed -= HandleCancelPerformed;
            }
        }

        private void HandleInteractPerformed(InputAction.CallbackContext context)
        {
            if (context.control != null)
            {
                if (context.control.device is Gamepad) schemeTracker?.NotifyDeviceUsed(ActiveControlScheme.Gamepad);
                else if (context.control.device is Keyboard || context.control.device is Mouse) schemeTracker?.NotifyDeviceUsed(ActiveControlScheme.KeyboardMouse);
            }

            InteractTriggered = true;
            OnInteractPressed?.Invoke();
        }

        private void HandleCancelPerformed(InputAction.CallbackContext context)
        {
            if (context.control != null)
            {
                if (context.control.device is Gamepad) schemeTracker?.NotifyDeviceUsed(ActiveControlScheme.Gamepad);
                else if (context.control.device is Keyboard || context.control.device is Mouse) schemeTracker?.NotifyDeviceUsed(ActiveControlScheme.KeyboardMouse);
            }

            CancelTriggered = true;
            OnCancelPressed?.Invoke();
        }

        private void Update()
        {
            // Reset triggers from previous frame before reading new frame state
            InteractTriggered = false;
            CancelTriggered = false;

            if (_moveAction != null)
            {
                MoveInput = _moveAction.ReadValue<Vector2>();
                if (MoveInput.sqrMagnitude > 1f)
                {
                    MoveInput = MoveInput.normalized;
                }

                if (MoveInput.sqrMagnitude > 0.04f && _moveAction.activeControl != null)
                {
                    if (_moveAction.activeControl.device is Gamepad) schemeTracker?.NotifyDeviceUsed(ActiveControlScheme.Gamepad);
                    else if (_moveAction.activeControl.device is Keyboard) schemeTracker?.NotifyDeviceUsed(ActiveControlScheme.KeyboardMouse);
                }
            }
            else
            {
                MoveInput = Vector2.zero;
            }
        }

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

        /// <summary>
        /// Returns the active binding display string for the Interact action for the currently active control scheme.
        /// </summary>
        public string GetInteractBindingDisplayString()
        {
            return GetInteractBindingDisplayString(CurrentControlScheme);
        }

        /// <summary>
        /// Returns the binding display string for the Interact action for a specific control scheme.
        /// </summary>
        public string GetInteractBindingDisplayString(ActiveControlScheme scheme)
        {
            if (_interactAction != null)
            {
                string display = ActiveControlSchemeTracker.GetBindingDisplayForScheme(_interactAction, scheme);
                if (!string.IsNullOrEmpty(display))
                {
                    return display;
                }
            }
            return scheme == ActiveControlScheme.Gamepad ? "A" : "E";
        }

        /// <summary>
        /// Returns the active binding display string for the Cancel action for the currently active control scheme.
        /// </summary>
        public string GetCancelBindingDisplayString()
        {
            return GetCancelBindingDisplayString(CurrentControlScheme);
        }

        /// <summary>
        /// Returns the binding display string for the Cancel action for a specific control scheme.
        /// </summary>
        public string GetCancelBindingDisplayString(ActiveControlScheme scheme)
        {
            if (_cancelAction != null)
            {
                string display = ActiveControlSchemeTracker.GetBindingDisplayForScheme(_cancelAction, scheme);
                if (!string.IsNullOrEmpty(display))
                {
                    return display;
                }
            }
            return scheme == ActiveControlScheme.Gamepad ? "B" : "Q";
        }
    }
}
