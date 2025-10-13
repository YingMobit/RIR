using GAS;
using InputSystemNameSpace;
using Unity.Physics;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorWalkAction",menuName = "GAS/Action/Charactor/Walk",order = 0)]
public class CharactorWalkAction : AbilityActionUnit {
    [Header("Animator config")]
    [SerializeField] string WalkStateName = "Walk";
    [SerializeField] int WalkAnimationLayer;
    [SerializeField] string AnimationParam_Dir_x;
    [SerializeField] string AnimationParam_Dir_z;
    [SerializeField] string AnimationParam_Walk;
    [SerializeField] string AnimationParam_Forward;
    [Header("SmoothConfig")]
    [SerializeField] float WalkSpeedSmoothTime = 0.1f;
    [Header("AttributeConfig")]
    [SerializeField] string WalkSpeedAttributeName = "WalkSpeed";
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
        animationController.SetFloatSmooth(AnimationParam_Dir_x,inputDir.x,WalkSpeedSmoothTime);
        animationController.SetFloatSmooth(AnimationParam_Dir_z,inputDir.y, WalkSpeedSmoothTime);
        animationController.SetFloatSmooth(AnimationParam_Forward,inputDir.y > 0 ? 1 : -1,WalkSpeedSmoothTime);

        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        var walkSpeedAttribute = abilityRuntimeContext.AbilityComponentContext.AttributeSet[WalkSpeedAttributeName];
        var velocity = new Vector3(moveDir.x * walkSpeedAttribute.Float(),transformController.Velocity.y,moveDir.y * walkSpeedAttribute.Float());
        
        transformController.VelocityTo(velocity,WalkSpeedSmoothTime);
        return TaskStatus.Running;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        
        //设置完平滑参数就可以退出了
        animationController.SetFloatSmooth(AnimationParam_Dir_x,0,WalkSpeedSmoothTime);
        animationController.SetFloatSmooth(AnimationParam_Dir_z,0,WalkSpeedSmoothTime);
        animationController.SetFloatSmooth(AnimationParam_Forward,transformController.Velocity.y > 0 ? 1 : -1,WalkSpeedSmoothTime);
        animationController.SetBool(AnimationParam_Walk,false);
        return TaskStatus.Suceeded;
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        return TaskStatus.Suceeded;
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetBool(AnimationParam_Walk,true);
    }
}