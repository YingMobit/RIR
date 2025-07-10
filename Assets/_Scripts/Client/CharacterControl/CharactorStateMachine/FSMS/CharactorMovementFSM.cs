using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

public class CharactorMovementFSM : MonoBehaviour, IStateMachine {
    public GameObject Entity => gameObject;
    [SerializeField] private CharactorMoveState currentStateType;
    [SerializeField] private CharactorStateBase currentState;

    [Header("States Config")]
    [SerializeField] private CharactorMoveState initialStateType;
    [SerializeField] private SerializableDictionary<CharactorMoveState,CharactorStateBase> allStates = new();
    [Tooltip("控制移动方式是走路还是奔跑")]
    [SerializeField] private bool m_MovementIsWalk;

    [Header("Components Needed")]
    Animator animator;
    new Rigidbody rigidbody;
    InputAction m_MovementAction;
    InputAction m_JumpAction;

    InputHandleProvider inputHandler;
    #region Runtime Data
    private CharactorStateBase defualtState;
    #endregion

    void Awake() {
        ComponentInit();
        RegistInputEvent();
        StatesInit();
        ReturnToDefualt();
    }

    void ComponentInit() {
        animator = GetComponent<Animator>();
        rigidbody = GetComponentInParent<Rigidbody>();
        inputHandler = GetComponent<InputHandleProvider>();
    }

    void StatesInit() {
        // 先收集所有键值对，避免在遍历时修改字典
        var stateEntries = new List<KeyValuePair<CharactorMoveState,CharactorStateBase>>();
        foreach(var pair in allStates) {
            stateEntries.Add(pair);
        }

        // 然后处理每个状态
        foreach(var pair in stateEntries) {
            var state = pair.Value.Clone();
            allStates[pair.Key] = state; // 现在可以安全地修改字典

            switch(pair.Key) {
                case CharactorMoveState.Walk:
                    ((CharactorWalk)state).Init(animator,rigidbody,inputHandler);
                    break;
                case CharactorMoveState.Run:
                    ((CharactorRun)state).Init(animator,rigidbody,inputHandler);
                    break;
                case CharactorMoveState.JumpUp:
                    break;
            }
            state.OnInit(this);
        }
    }

    void RegistInputEvent() {
        inputHandler.MoveDirInputEvent += HandleMoveDirInput;
        inputHandler.JumpInputEvent += HandleJumpInput;
        inputHandler.MovementMethodSwitchEvent += HandleMovementMethodSwitchInput;
    }

    #region Input Event Handle
    //这里处理输入导致的状态切换
    void HandleMoveDirInput(Vector2 dir) {
        if(currentStateType == CharactorMoveState.JumpUp || currentStateType == CharactorMoveState.Fall) {
            return;
        } else if(dir.magnitude == 0) {
            SwitchState((int)CharactorMoveState.Idle);
        } else if(m_MovementIsWalk && currentStateType != CharactorMoveState.Walk) {
            SwitchState((int)CharactorMoveState.Walk);
        } else if(currentStateType != CharactorMoveState.Run) {
            SwitchState((int)CharactorMoveState.Run);
        }
    }

    void HandleJumpInput() {
        if(currentStateType == CharactorMoveState.JumpUp) {
            return;
        }
        SwitchState((int)CharactorMoveState.JumpUp);
    }

    void HandleMovementMethodSwitchInput() {
        m_MovementIsWalk = !m_MovementIsWalk;
        if(currentStateType == CharactorMoveState.Run) {
            SwitchState((int)CharactorMoveState.Walk);
        } else if(currentStateType == CharactorMoveState.Walk) {
            SwitchState((int)CharactorMoveState.Run);
        }
    }
    #endregion

    public void SwitchState(int stateFlag) {
        CharactorMoveState stateType = (CharactorMoveState)stateFlag;
        if(allStates.ContainsKey(stateType)) {
            currentStateType = stateType;
            SwitchState(allStates[stateType]);
        } else {
            Debug.Log($"StateType int form doesn't exist: {stateFlag}");
        }
    }

    private void SwitchState(CharactorStateBase newState) {
        if(currentState != null) {
            currentState.OnExit();
        }
        currentState = newState;
        currentState.OnEnter();
    }

    public void ReturnToDefualt() {
        if(!defualtState) {
            foreach(var pair in allStates) {
                if(pair.Key == initialStateType) {
                    defualtState = pair.Value;
                    break;
                }
            }
        }
        SwitchState(defualtState);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        currentState.OnUpdate();
    }

    void FixedUpdate() {
        currentState.OnFixedUpdate();
    }

    public CharactorMovementFSM(InputAction _movementAction,InputAction _jumpAction) {
        m_MovementAction = _movementAction;
        m_JumpAction = _jumpAction;
    }
}

public enum CharactorMoveState {
    Idle,
    Walk,
    Run,
    JumpUp,
    Fall
}
