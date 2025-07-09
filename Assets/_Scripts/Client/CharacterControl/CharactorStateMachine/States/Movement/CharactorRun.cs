using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorRun",menuName = CharactorFSMStateDataConfig.StateDataMenuPathRoot + "CharactorRun",order = 0)]
public class CharactorRun : CharactorStateBase {
    private Animator animator;
    private Rigidbody rigidbody;
    [Header("Animator config")]
    [SerializeField] string RunStateName = "Run";
    [SerializeField] int RunAnimationLayer;
    [SerializeField] string AnimationParam_Dir_x;
    [SerializeField] string AnimationParam_Dir_z;
    [Header("Logic Config")]
    [SerializeField] float RunSpeed;

    #region Runtime Data
    public override IStateMachine stateMachine { get; set; }
    float Direction_x;
    float Direction_z;
    #endregion

    public void Init(Animator _animator,Rigidbody _rigidbody) {
        animator = _animator;
        rigidbody = _rigidbody;
    }

    public override void OnEnter() {
        Direction_x = Direction_z = 0;
        animator.Play(RunStateName,RunAnimationLayer);
    }

    public override void OnExit() {

    }

    public override void OnUpdate() {
        Direction_x = Input.GetAxis("Horizontal");
        Direction_z = Input.GetAxis("Vertical");
        var dir = new Vector3(Direction_x,0,Direction_z);
        Direction_x = dir.normalized.x;
        Direction_z = dir.normalized.z;

        animator.SetFloat(AnimationParam_Dir_x,Direction_x);
        animator.SetFloat(AnimationParam_Dir_z,Direction_z);

        var velocity = new Vector3(Direction_x + RunSpeed,rigidbody.linearVelocity.y,Direction_z * RunSpeed);
        rigidbody.linearVelocity = velocity;
    }

    public override void OnFixedUpdate() {

    }

    public override CharactorStateBase Clone() {
        return Instantiate(this);
    }
}
