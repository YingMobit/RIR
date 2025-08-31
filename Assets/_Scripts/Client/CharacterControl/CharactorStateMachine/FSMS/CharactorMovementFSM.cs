using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

public class CharactorMovementFSM : MonoBehaviour, IStateMachine {
    public GameObject GameObject => gameObject;
    [SerializeField] private CharactorMoveState currentStateType;
    [SerializeField] private CharactorStateBase currentState;

    [Header("States Config")]
    [SerializeField] private CharactorMoveState initialStateType;
    [SerializeField] private SerializableDictionary<CharactorMoveState,CharactorStateBase> allStates = new();
    [Tooltip("�����ƶ���ʽ����·���Ǳ���")]
    [SerializeField] public bool m_MovementIsWalk { get; private set; }

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
    }

    void Start() {
        StatesInit();
        ReturnToDefualt();
    }

    void ComponentInit() {
        animator = GetComponent<Animator>();
        rigidbody = GetComponentInParent<Rigidbody>();
        inputHandler = GetComponent<InputHandleProvider>();
    }

    void RegistInputEvent() {
        inputHandler.MoveDirInputEvent += HandleMoveDirInput;
        inputHandler.JumpInputEvent += HandleJumpInput;
        inputHandler.MovementMethodSwitchEvent += HandleMovementMethodSwitchInput;
    }

    void StatesInit() {
        // ���ռ����м�ֵ�ԣ������ڱ���ʱ�޸��ֵ�
        var stateEntries = new List<KeyValuePair<CharactorMoveState,CharactorStateBase>>();
        foreach(var pair in allStates) {
            stateEntries.Add(pair);
        }

        // Ȼ����ÿ��״̬
        foreach(var pair in stateEntries) {
            var state = pair.Value.Clone();
            allStates[pair.Key] = state; // ���ڿ��԰�ȫ���޸��ֵ�

            state.OnInit(this);
            switch(pair.Key) {
                case CharactorMoveState.Walk:
                    ((CharactorWalk)state).Init(animator,rigidbody,inputHandler);
                    break;
                case CharactorMoveState.Run:
                    ((CharactorRun)state).Init(animator,rigidbody,inputHandler);
                    break;
                case CharactorMoveState.JumpUp:
                    ((CharactorJumpUp)state).Init(animator,rigidbody);
                    break;
                case CharactorMoveState.Fall:
                    ((CharactorFall)state).Init(animator);
                    break;
            }
        }
    }

    #region Input Event Handle
    //���ﴦ�����뵼�µ�״̬�л�
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
        if(currentState == null) {
            currentState = newState;
            currentState.OnEnter();
        }
        if(!currentState.Interruptable || currentState.Priority > newState.Priority)
            return;
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

[Flags]
public enum CharactorMoveState {
    Idle = 1,
    Walk = 1 << 1,
    Run = 1 << 2,
    JumpUp = 1 << 3,
    Fall = 1 << 4
}
