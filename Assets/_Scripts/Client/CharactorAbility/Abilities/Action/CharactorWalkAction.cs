using GAS;
using InputSystemNameSpace;
using Lockstep.Math;
using LockStepLMath;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorWalkAction",menuName = "GAS/Action/Charactor/Walk",order = 0)]
public class CharactorWalkAction : AbilityActionUnit {
    [Header("Animator config")]
    [SerializeField] string AnimationParam_Dir_x;
    [SerializeField] string AnimationParam_Dir_z;
    [SerializeField] string AnimationParam_Walk;
    [Header("SmoothConfig")]
    [SerializeField] float WalkSpeedSmoothTime;
    [Header("AttributeConfig")]
    [SerializeField] int WalkSpeedAttributeID;
    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var inputQueue = abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Get<InputQueue>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
        var inputDir = inputQueue.PeekTail().MoveInput;
        var aimDir = CursorAimer.Instance.AimDirection;
        var moveDir = new LVector2();
        LQuaternion rotation = LQuaternion.FromToRotation(LVector2.up,new LVector2(aimDir.x,aimDir.z));
        moveDir = rotation * inputDir;
        
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetFloatSmooth(AnimationParam_Dir_x,inputDir.x.ToFloat(),WalkSpeedSmoothTime);
        animationController.SetFloatSmooth(AnimationParam_Dir_z,inputDir.y.ToFloat(), WalkSpeedSmoothTime);

        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        var walkSpeedAttribute = abilityRuntimeContext.AbilityComponentContext.AttributeSet[WalkSpeedAttributeID];
        var velocity = new Vector2(moveDir.x.ToFloat() * walkSpeedAttribute.Float(),moveDir.y.ToFloat() * walkSpeedAttribute.Float());
        
        transformController.HorizontalVelocityTo(velocity,WalkSpeedSmoothTime);
        return TaskStatus.Running;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        
        //设置完平滑参数就可以退出了
        animationController.SetFloatSmooth(AnimationParam_Dir_x,0,WalkSpeedSmoothTime);
        animationController.SetFloatSmooth(AnimationParam_Dir_z,0,WalkSpeedSmoothTime);
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