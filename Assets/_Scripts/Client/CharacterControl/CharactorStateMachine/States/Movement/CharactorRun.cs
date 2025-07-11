using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "CharactorRun",menuName = CharactorFSMStateDataConfig.StateDataMenuPathRoot + "CharactorRun",order = 0)]
public class CharactorRun : CharactorStateBase {
    private Animator animator;
    private Rigidbody rigidbody;
    [Header("Animator config")]
    [SerializeField] string RunStateName = "Run";
    [SerializeField] int RunAnimationLayer;
    [SerializeField] string AnimationParam_Dir_x;
    [SerializeField] string AnimationParam_Dir_z;
    [SerializeField] string AnimationParam_Run;

    #region Runtime Data
    public override IStateMachine stateMachine { get; set; }
    InputHandleProvider inputHandler;
    AbilityValues<float> RunSpeed;
    #endregion

    public void Init(Animator _animator,Rigidbody _rigidbody,InputHandleProvider _inputHandler) {
        animator = _animator;
        rigidbody = _rigidbody;
        inputHandler = _inputHandler;
        RunSpeed = stateMachine.Entity.GetComponent<PlayerController>().playerRuntimeAbilityData.RunSpeed;
    }

    public override void OnEnter() {
        animator.SetBool(AnimationParam_Run,true);
    }

    public override void OnExit() {
        animator.SetBool(AnimationParam_Run,false);
    }

    public override void OnUpdate() {
        var inputDir = inputHandler.MoveDirInput;
        var aimDir = CursorAimer.Instance.AimDirection;
        var moveDir = new Vector2();
        Quaternion rotation = Quaternion.FromToRotation(Vector2.up,new Vector2(aimDir.x,aimDir.z));
        moveDir = rotation * inputDir;

        animator.SetFloat(AnimationParam_Dir_x,inputDir.x);
        animator.SetFloat(AnimationParam_Dir_z,inputDir.y);

        var velocity = new Vector3(moveDir.x * RunSpeed.CurrentValue,rigidbody.linearVelocity.y,moveDir.y * RunSpeed.CurrentValue);
        rigidbody.linearVelocity = velocity;
    }

    public override void OnFixedUpdate() {

    }

    public override CharactorStateBase Clone() {
        return Instantiate(this);
    }
}
