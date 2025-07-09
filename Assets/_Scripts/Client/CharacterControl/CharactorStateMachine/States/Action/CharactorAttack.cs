using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorAttack",menuName = CharactorFSMStateDataConfig.StateDataMenuPathRoot + "CharactorAttack",order = 0)]
public class CharactorAttack : CharactorStateBase {
    public override IStateMachine stateMachine { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override CharactorStateBase Clone() {
        return Instantiate(this);
    }

    public override void OnEnter() {
        throw new NotImplementedException();
    }

    public override void OnExit() {
        throw new NotImplementedException();
    }

    public override void OnFixedUpdate() {
        throw new NotImplementedException();
    }

    public override void OnUpdate() {
        throw new NotImplementedException();
    }
}
