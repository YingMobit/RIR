using System;
using UnityEngine;

public abstract class CharactorStateBase : ScriptableObject, IState {
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void OnUpdate();
    public abstract void OnFixedUpdate();
    public abstract IStateMachine stateMachine { get; set; }
    public virtual void OnInit(IStateMachine stateMachine) {
        this.stateMachine = stateMachine;
    }
    public abstract CharactorStateBase Clone();
}

public static class CharactorFSMStateDataConfig {
    public const string StateDataMenuPathRoot = "ScriptableObject/CharactorFSMStateData/";
}
