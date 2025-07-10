using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorJumpUp",menuName = CharactorFSMStateDataConfig.StateDataMenuPathRoot + "CharactorJumpUp",order = 0)]
public class CharactorJumpUp : CharactorStateBase {
    public override IStateMachine stateMachine { get; set; }

    public override CharactorStateBase Clone() {
        return Instantiate(this);
    }

    public override void OnEnter() {

    }

    public override void OnExit() {

    }

    public override void OnFixedUpdate() {

    }

    public override void OnUpdate() {

    }
}
