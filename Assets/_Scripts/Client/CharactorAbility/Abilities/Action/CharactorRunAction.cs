using GAS;
using InputSystemNameSpace;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorRunAction",menuName = "GAS/Action/Charactor/Run",order = 0)]
public class CharactorRunAction : AbilityActionUnit {
    [Header("Animator config")]
    [SerializeField] string AnimationParam_Dir_x;
    [SerializeField] string AnimationParam_Dir_z;
    [SerializeField] string AnimationParam_Run;
    [Header("SmoothConfig")]
    [SerializeField] float RunSpeedSmoothTime;
    [Header("Attribute Config")]
    [SerializeField] string RunSpeedAttributeName;
    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var inputQueue = abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Get<InputQueue>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
        var inputDir = inputQueue.PeekTail().MoveInput;
        var aimDir = inputQueue.PeekTail().AimDirection;
        var moveDir = new Vector2();
        Quaternion rotation = Quaternion.FromToRotation(Vector2.up,new Vector2(aimDir.x,aimDir.z));
        moveDir = rotation * inputDir;

        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetFloatSmooth(AnimationParam_Dir_x,inputDir.x,RunSpeedSmoothTime);
        animationController.SetFloatSmooth(AnimationParam_Dir_z,inputDir.y,RunSpeedSmoothTime);

        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        var walkSpeedAttribute = abilityRuntimeContext.AbilityComponentContext.AttributeSet[RunSpeedAttributeName];
        var velocity = new Vector3(moveDir.x * walkSpeedAttribute.Float(),transformController.Velocity.y,moveDir.y * walkSpeedAttribute.Float());

        transformController.VelocityTo(velocity,RunSpeedSmoothTime);
        return TaskStatus.Running;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;

        //设置完平滑参数就可以退出了
        animationController.SetFloatSmooth(AnimationParam_Dir_x,0,RunSpeedSmoothTime);
        animationController.SetFloatSmooth(AnimationParam_Dir_z,0,RunSpeedSmoothTime);
        animationController.SetBool(AnimationParam_Run,false);
        return TaskStatus.Suceeded;
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        return TaskStatus.Suceeded;
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetBool(AnimationParam_Run,true);
    }
}