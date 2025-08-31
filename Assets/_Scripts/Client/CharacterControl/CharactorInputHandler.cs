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
    [SerializeField] private string m_PlayerAttackActionName;
    [SerializeField] private string m_PlayerReloadActionName;

    private InputAction m_PlayerMovementAction;
    private InputAction m_PlayerJumpAction;
    private InputAction m_PlayerSwitchMovementMethodAction;
    private InputAction m_PlayerAttackAction;
    private InputAction m_PlayerReloadAction;

    public event Action<Vector2> MoveDirInputEvent;
    public event Action JumpInputEvent;
    public event Action MovementMethodSwitchEvent;
    public event Action AttackInputEvent;
    public event Action ReloadInputEvent;

    void Awake() {
        GetInputActions();
        RegistInputEvent();
    }

    void OnEnable() {
        m_PlayerMovementAction?.Enable();
        m_PlayerJumpAction?.Enable();
        m_PlayerSwitchMovementMethodAction?.Enable();
        m_PlayerAttackAction?.Enable();
        m_PlayerReloadAction?.Enable();
    }

    void GetInputActions() {
        m_PlayerMovementAction = m_PlayerInputActionConfig[m_PlayerMovementActionName];
        m_PlayerJumpAction = m_PlayerInputActionConfig[m_PlayerJumpActionName];
        m_PlayerSwitchMovementMethodAction = m_PlayerInputActionConfig[m_PlayerSwitchMovementMethodActionName];
        m_PlayerAttackAction = m_PlayerInputActionConfig[m_PlayerAttackActionName];
        m_PlayerReloadAction = m_PlayerInputActionConfig[m_PlayerReloadActionName];
    }

    void RegistInputEvent() {
        m_PlayerMovementAction.performed += OnMoveDirInput;
        m_PlayerMovementAction.canceled += OnMoveDirInput;
        m_PlayerJumpAction.performed += OnJumpInput;
        m_PlayerSwitchMovementMethodAction.performed += OnSwitchMoveMethod;
        m_PlayerAttackAction.performed += OnAttack;
        m_PlayerReloadAction.performed += OnReload;
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

    void OnAttack(CallbackContext callbackContext) {
        AttackInputEvent?.Invoke();
    }

    void OnReload(CallbackContext callbackContext) {
        ReloadInputEvent?.Invoke();
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
    public event Action AttackInputEvent;
    public event Action ReloadInputEvent;
}
