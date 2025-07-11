using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorFall",menuName = CharactorFSMStateDataConfig.StateDataMenuPathRoot + "CharactorFall",order = 0)]
public class CharactorFall : CharactorStateBase {
    public override IStateMachine stateMachine { get; set; }
    private Animator animator;
    private InputHandleProvider inputHandler;
    [SerializeField] string AnimationParam_Fall;
    [SerializeField] LayerMask GroundLayerMask;

    public override CharactorStateBase Clone() {
        return Instantiate(this);
    }

    public void Init(Animator _ani) {
        animator = _ani;
        inputHandler = stateMachine.Entity.GetComponent<InputHandleProvider>();
    }

    public override void OnEnter() {
        animator.SetBool(AnimationParam_Fall,true);
    }

    public override void OnExit() {
        animator.SetBool(AnimationParam_Fall,false);
    }

    public override void OnFixedUpdate() {
    }

    public override void OnUpdate() {
        Ray ray = new Ray(stateMachine.Entity.transform.position,Vector3.down);
        if(Physics.Raycast(ray,1,GroundLayerMask)) {
            if(!inputHandler.MoveKeyPressing)
                stateMachine.SwitchState((int)CharactorMoveState.Idle);
            else if(((CharactorMovementFSM)stateMachine).m_MovementIsWalk) {
                stateMachine.SwitchState((int)CharactorMoveState.Walk);
            } else {
                stateMachine.SwitchState((int)CharactorMoveState.Run);
            }
        }
    }
}
