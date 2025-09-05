using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "CharactorWalk",menuName = CharactorFSMStateDataConfig.StateDataMenuPathRoot + "CharactorWalk",order = 0)]
public class CharactorWalk : CharactorStateBase {
    private Animator animator;
    private Rigidbody rigidbody;
    [Header("Animator config")]
    [SerializeField] string WalkStateName = "Walk";
    [SerializeField] int WalkAnimationLayer;
    [SerializeField] string AnimationParam_Dir_x;
    [SerializeField] string AnimationParam_Dir_z;
    [SerializeField] string AnimationParam_Walk;
    [SerializeField] string AnimationParam_Forward;

    #region Runtime Data
    public override IStateMachine stateMachine { get; set; }
    InputHandleProvider inputHandler;
    AbilityValues<float> WalkSpeed;
    #endregion

    public void Init(Animator _animator,Rigidbody _rigidbody,InputHandleProvider _inputHandler) {
        animator = _animator;
        rigidbody = _rigidbody;
        inputHandler = _inputHandler;
        WalkSpeed = stateMachine.GameObject.GetComponent<PlayerController>().playerRuntimeAbilityData.WalkSpeed;
    }

    public override void OnEnter() {
        animator.SetBool(AnimationParam_Walk,true);
    }


    public override void OnUpdate() {
        var inputDir = inputHandler.MoveDirInput;
        var aimDir = CursorAimer.Instance.AimDirection;
        var moveDir = new Vector2();
        Quaternion rotation = Quaternion.FromToRotation(Vector2.up,new Vector2(aimDir.x,aimDir.z));
        moveDir = rotation * inputDir;

        animator.SetFloat(AnimationParam_Dir_x,inputDir.x);
        animator.SetFloat(AnimationParam_Dir_z,inputDir.y);
        animator.SetFloat(AnimationParam_Forward,inputDir.y > 0 ? 1 : -1);

        var velocity = new Vector3(moveDir.x * WalkSpeed.CurrentValue,rigidbody.linearVelocity.y,moveDir.y * WalkSpeed.CurrentValue);
        rigidbody.linearVelocity = velocity;
    }

    public override void OnFixedUpdate() {

    }

    public override void OnExit() {
        animator.SetBool(AnimationParam_Walk,false);
    }

    public override CharactorStateBase Clone() {
        return Instantiate(this);
    }
}
