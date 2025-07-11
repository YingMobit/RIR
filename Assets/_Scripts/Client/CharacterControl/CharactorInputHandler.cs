using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
public class CharactorInputHandler : MonoBehaviour, InputHandleProvider {
    [Header("InputConfig")]
    [SerializeField] private InputActionAsset m_PlayerInputActionConfig;
    [SerializeField] private string m_PlayerMovementActionName;
    [SerializeField] private string m_PlayerJumpActionName;
    [SerializeField] private string m_PlayerSwitchMovementMethodActionName;

    private InputAction m_PlayerMovementAction;
    private InputAction m_PlayerJumpAction;
    private InputAction m_PlayerSwitchMovementMethodAction;

    public event Action<Vector2> MoveDirInputEvent;
    public event Action JumpInputEvent;
    public event Action MovementMethodSwitchEvent;

    void Awake() {
        GetInputActions();
        RegistInputEvent();
    }

    void OnEnable() {
        m_PlayerMovementAction?.Enable();
        m_PlayerJumpAction?.Enable();
        m_PlayerSwitchMovementMethodAction?.Enable();
    }

    void GetInputActions() {
        m_PlayerMovementAction = m_PlayerInputActionConfig[m_PlayerMovementActionName];
        m_PlayerJumpAction = m_PlayerInputActionConfig[m_PlayerJumpActionName];
        m_PlayerSwitchMovementMethodAction = m_PlayerInputActionConfig[m_PlayerSwitchMovementMethodActionName];
    }

    void RegistInputEvent() {
        m_PlayerMovementAction.performed += OnMoveDirInput;
        m_PlayerMovementAction.canceled += OnMoveDirInput;
        m_PlayerJumpAction.performed += OnJumpInput;
        m_PlayerSwitchMovementMethodAction.performed += OnSwitchMoveMethod;
    }

    #region InputHandle
    void OnMoveDirInput(CallbackContext callBackContext) {
        var dir = callBackContext.ReadValue<Vector2>();
        MoveDirInputEvent?.Invoke(dir);
    }

    void OnJumpInput(CallbackContext callbackContext) {
        var jumped = callbackContext.ReadValue<float>() > 0;
        if(jumped)
            JumpInputEvent?.Invoke();
    }

    void OnSwitchMoveMethod(CallbackContext callbackContext) {
        MovementMethodSwitchEvent?.Invoke();
    }
    #endregion

    void OnDisable() {
        m_PlayerMovementAction?.Disable();
        m_PlayerJumpAction?.Disable();
        m_PlayerSwitchMovementMethodAction?.Disable();
    }

    public bool MoveKeyPressing => MoveDirInput != Vector2.zero;
    public Vector2 MoveDirInput => m_PlayerMovementAction.ReadValue<Vector2>();
    public bool JumpPressed => m_PlayerJumpAction.IsPressed();
}

public interface InputHandleProvider {
    public bool MoveKeyPressing { get; }
    public Vector2 MoveDirInput { get; }
    public bool JumpPressed { get; }
    public event Action<Vector2> MoveDirInputEvent;
    public event Action JumpInputEvent;
    public event Action MovementMethodSwitchEvent;
}
