using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class CharactorActionFSM : MonoBehaviour, IStateMachine {
    public GameObject Entity => gameObject;
    [SerializeField] private CharactorStateBase currentState;
    [Header("States Config")]
    [SerializeField] private CharactorActionState initialStateType;
    [SerializeField] private SerializableDictionary<CharactorActionState,CharactorStateBase> allStates = new();

    #region Runtime Data
    private CharactorStateBase defualtState;
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
        if(currentState != null) {
            currentState.OnExit();
        }
        currentState = newState;
        currentState.OnInit(this);
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
        // Init();
    }

    void Init() {
        Type stateType;
        foreach(var state in allStates.Keys) {
            stateType = allStates[state].GetType();
            allStates[state] = ScriptableObject.CreateInstance(stateType) as CharactorStateBase;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

}

public enum CharactorActionState {
    Attack,
    Reload
}
