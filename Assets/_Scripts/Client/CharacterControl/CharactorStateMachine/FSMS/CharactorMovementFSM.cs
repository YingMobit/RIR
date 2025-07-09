using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class CharactorMovementFSM : MonoBehaviour, IStateMachine {
    public GameObject Entity => gameObject;
    [SerializeField] private CharactorStateBase currentState;

    [Header("States Config")]
    [SerializeField] private CharactorMoveState initialStateType;
    [SerializeField] private SerializableDictionary<CharactorMoveState,CharactorStateBase> allStates = new();

    [Header("Components Needed")]
    Animator animator;
    new Rigidbody rigidbody;

    #region Runtime Data
    private CharactorStateBase defualtState;
    #endregion

    public void SwitchState(int stateFlag) {
        CharactorMoveState stateType = (CharactorMoveState)stateFlag;
        if(allStates.ContainsKey(stateType)) {
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

    void Awake() {
        ComponentInit();
        StatesInit();
        ReturnToDefualt();
    }

    void ComponentInit() {
        animator = GetComponent<Animator>();
        rigidbody = GetComponentInParent<Rigidbody>();
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
                    ((CharactorWalk)state).Init(animator,rigidbody);
                    break;
                case CharactorMoveState.Run:
                    ((CharactorRun)state).Init(animator,rigidbody);
                    break;
                case CharactorMoveState.Jump:
                    break;
            }
            state.OnInit(this);
        }
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
}

public enum CharactorMoveState {
    Walk,
    Run,
    Jump
}
