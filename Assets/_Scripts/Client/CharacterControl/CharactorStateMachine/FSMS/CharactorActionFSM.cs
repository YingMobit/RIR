using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Utility;

public class CharactorActionFSM : MonoBehaviour, IStateMachine {
    public GameObject GameObject => gameObject;
    [SerializeField] private CharactorActionState currentStateType;
    [SerializeField] private CharactorStateBase currentState;
    [Header("States Config")]
    [SerializeField] private CharactorActionState initialStateType;
    [SerializeField] private SerializableDictionary<CharactorActionState,CharactorStateBase> allStates = new();

    #region Runtime Data
    private CharactorStateBase defualtState;
    private Animator animator;
    private InputHandleProvider inputHandleProvider;
    #endregion

    public void SwitchState(int stateFlag) {
        CharactorActionState stateType = (CharactorActionState)stateFlag;
        if(allStates.ContainsKey(stateType)) {
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

    void Awake() {
        InitComponent();
        InputEventRegist();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        InitState();
        ReturnToDefualt();
    }

    void InitComponent() {
        animator = GetComponent<Animator>();
        inputHandleProvider = GetComponent<InputHandleProvider>();
    }

    void InitState() {
        var newDic = new SerializableDictionary<CharactorActionState,CharactorStateBase>();
        CharactorStateBase state;
        foreach(var pair in allStates) {
            state = pair.Value.Clone();
            state.OnInit(this);
            switch(pair.Key) {
                case CharactorActionState.Attack:
                    ((CharactorAttack)state).Init(GetComponent<PlayerController>().Weapon);
                    break;
                case CharactorActionState.Reload:
                    ((CharactorReload)state).Init();
                    break;
            }
            newDic.Add(pair.Key,state);
        }
        allStates = newDic;
    }

    void InputEventRegist() {
        inputHandleProvider.AttackInputEvent += HandleAttackInput;
        inputHandleProvider.ReloadInputEvent += HandleReloadInput;
    }

    #region Input Event Handler
    void HandleAttackInput() {
        if(currentStateType == CharactorActionState.Attack)
            return;
        SwitchState((int)CharactorActionState.Attack);

    }

    void HandleReloadInput() {
        if(currentStateType == CharactorActionState.Reload)
            return;
        SwitchState((int)CharactorActionState.Reload);
    }

    #endregion


    // Update is called once per frame
    void Update() {
        currentState.OnUpdate();
    }

    void FixedUpdate() {
        currentState.OnFixedUpdate();
    }
}

[Flags]
public enum CharactorActionState {
    Attack = 1,
    Reload = 1 << 1
}
