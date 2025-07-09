using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorJump",menuName = CharactorFSMStateDataConfig.StateDataMenuPathRoot + "CharactorJump",order = 0)]
public class CharactorJump : CharactorStateBase {
    public override IStateMachine stateMachine { get; set; }

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
