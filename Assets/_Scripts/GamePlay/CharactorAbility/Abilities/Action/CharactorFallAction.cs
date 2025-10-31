using GAS;
using InputSystemNameSpace;
using Lockstep.Math;
using LockStepLMath;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorFallAction",menuName ="GAS/Action/Charactor/Fall",order =0)]
public class CharactorFallAction : AbilityActionUnit {
    [Header("Animation Params")]
    [SerializeField] string AnimationParam_FallDown;
    [Header("Attribute Config")]
    [SerializeField] int InAirSpeedAttributeID;
    [Header("Smooth Config")]
    [SerializeField] float InAirSpeedSmoothTime;
    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var inputQueue = abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Get<InputQueue>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
        inputQueue.TryPeekTail(out var frameInputData);
        var inputDir = frameInputData.MoveInput;
        var aimDir = frameInputData.AimDirection;
        var moveDir = new LVector2();
        LQuaternion rotation = LQuaternion.FromToRotation(LVector2.up,new LVector2(aimDir.x,aimDir.z));
        moveDir = rotation * inputDir;

        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        var walkSpeedAttribute = abilityRuntimeContext.AbilityComponentContext.AttributeSet[InAirSpeedAttributeID];
        var velocity = new Vector2(moveDir.x.ToFloat() * walkSpeedAttribute.Float(),moveDir.y.ToFloat() * walkSpeedAttribute.Float());

        transformController.HorizontalVelocityTo(velocity,InAirSpeedSmoothTime);
        abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Set<bool>(AbilitySystem.ISFALLINGID_IN_GLOBALBLACKBORAD,true);
        return TaskStatus.Running;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetBool(AnimationParam_FallDown,false);
        abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Set<bool>(AbilitySystem.ISFALLINGID_IN_GLOBALBLACKBORAD,false);
        return TaskStatus.Suceeded;
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        return TaskStatus.Suceeded;
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetBool(AnimationParam_FallDown,true);
    }
}