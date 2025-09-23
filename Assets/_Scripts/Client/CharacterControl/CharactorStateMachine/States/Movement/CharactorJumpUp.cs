using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorJumpUp",menuName = CharactorFSMStateDataConfig.StateDataMenuPathRoot + "CharactorJumpUp",order = 0)]
public class CharactorJumpUp : CharactorStateBase {
    public override IStateMachine stateMachine { get; set; }
    private Animator animator;
    private Rigidbody rigidbody;
    private Attribute<float> jumpHeight;
    private Attribute<float> jumpHorizontalImpulse;
    private float posYLastFrame;
    private InputHandleProvider inputHandler;

    [SerializeField] string AnimationParam_JumpUp;
    public override CharactorStateBase Clone() {
        return Instantiate(this);
    }

    public void Init(Animator _ani,Rigidbody _rig) {
        animator = _ani;
        rigidbody = _rig;
        jumpHeight = stateMachine.GameObject.GetComponent<PlayerController>().playerRuntimeAbilityData.JumpHeight;
        jumpHorizontalImpulse = stateMachine.GameObject.GetComponent<PlayerController>().playerRuntimeAbilityData.JumpVerticalImpulse;
        inputHandler = stateMachine.GameObject.GetComponent<InputHandleProvider>();
    }

    public override void OnEnter() {
        var inputDir = inputHandler.MoveDirInput;
        var aimDir = CursorAimer.Instance.AimDirection;
        var moveDir = new Vector2();
        Quaternion rotation = Quaternion.FromToRotation(Vector2.up,new Vector2(aimDir.x,aimDir.z));
        moveDir = rotation * inputDir;

        Vector3 verticalImpulse = jumpHeight.CurrentValue * Vector3.up;
        Vector3 horizontalImpulse = jumpHorizontalImpulse.CurrentValue * new Vector3(moveDir.x,0,moveDir.y).normalized;
        rigidbody.AddForce(verticalImpulse + horizontalImpulse,ForceMode.VelocityChange);
        animator.SetBool(AnimationParam_JumpUp,true);
        posYLastFrame = stateMachine.GameObject.transform.position.y;
    }

    public override void OnExit() {
        animator.SetBool(AnimationParam_JumpUp,false);
    }

    public override void OnFixedUpdate() {

    }

    public override void OnUpdate() {
        if(stateMachine.GameObject.transform.position.y - posYLastFrame < 0) {
            stateMachine.SwitchState((int)CharactorMoveState.Fall);
        }
        posYLastFrame = stateMachine.GameObject.transform.position.y;
    }
}
