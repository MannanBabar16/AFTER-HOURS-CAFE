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

        public Vector2 MoveInput { get; private set; }
        public bool InteractTriggered { get; private set; }
        public bool CancelTriggered { get; private set; }

        public event System.Action OnInteractPressed;
        public event System.Action OnCancelPressed;

        public InputActionAsset ActionsAsset => actionsAsset;

        private InputActionMap _actionMap;
        private InputAction _moveAction;
        private InputAction _interactAction;
        private InputAction _cancelAction;
        private bool _ownsActionMap;

        private void Awake()
        {
            InitializeActions();
        }

        private void OnEnable()
        {
            if (_actionMap == null)
            {
                InitializeActions();
            }

            _actionMap?.Enable();
        }

        private void OnDisable()
        {
            _actionMap?.Disable();
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
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _moveAction.AddBinding("<Gamepad>/leftStick");

            _interactAction = _actionMap.AddAction("Interact", InputActionType.Button);
            _interactAction.AddBinding("<Keyboard>/e");
            _interactAction.AddBinding("<Gamepad>/buttonSouth");

            _cancelAction = _actionMap.AddAction("Cancel", InputActionType.Button);
            _cancelAction.AddBinding("<Keyboard>/q");
            _cancelAction.AddBinding("<Keyboard>/escape");
            _cancelAction.AddBinding("<Gamepad>/buttonEast");
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
            InteractTriggered = true;
            OnInteractPressed?.Invoke();
        }

        private void HandleCancelPerformed(InputAction.CallbackContext context)
        {
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
        /// Returns the active binding display string for the Interact action (e.g. "E", "A").
        /// </summary>
        public string GetInteractBindingDisplayString()
        {
            if (_interactAction != null)
            {
                string display = _interactAction.GetBindingDisplayString();
                if (!string.IsNullOrEmpty(display))
                {
                    return display;
                }
            }
            return "E";
        }

        /// <summary>
        /// Returns the active binding display string for the Cancel action (e.g. "Q", "B").
        /// </summary>
        public string GetCancelBindingDisplayString()
        {
            if (_cancelAction != null)
            {
                string display = _cancelAction.GetBindingDisplayString();
                if (!string.IsNullOrEmpty(display))
                {
                    return display;
                }
            }
            return "Q";
        }
    }
}
