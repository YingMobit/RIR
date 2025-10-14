using GAS;
using InputSystemNameSpace;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorFallAction",menuName ="GAS/Action/Charactor/Fall",order =0)]
public class CharactorFallAction : AbilityActionUnit {
    [Header("Animation Params")]
    [SerializeField] string AnimationParam_FallDown;
    [Header("Attribute Config")]
    [SerializeField] string InAirSpeedAttributeName;
    [Header("Smooth Config")]
    [SerializeField] float InAirSpeedSmoothTime;
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

        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        var walkSpeedAttribute = abilityRuntimeContext.AbilityComponentContext.AttributeSet[InAirSpeedAttributeName];
        var velocity = new Vector3(moveDir.x * walkSpeedAttribute.Float(),transformController.Velocity.y,moveDir.y * walkSpeedAttribute.Float());

        transformController.VelocityTo(velocity,InAirSpeedSmoothTime);
        return TaskStatus.Running;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        IAnimationController animationController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Animation] as IAnimationController;
        animationController.SetBool(AnimationParam_FallDown,false);
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