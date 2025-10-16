using GAS;
using InputSystemNameSpace;
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
        var inputDir = inputQueue.PeekTail().MoveInput;
        var aimDir = CursorAimer.Instance.AimDirection;
        var moveDir = new Vector2();
        Quaternion rotation = Quaternion.FromToRotation(Vector2.up,new Vector2(aimDir.x,aimDir.z));
        moveDir = rotation * inputDir;

        ITransformController transformController = abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController;
        var walkSpeedAttribute = abilityRuntimeContext.AbilityComponentContext.AttributeSet[InAirSpeedAttributeID];
        var velocity = new Vector2(moveDir.x * walkSpeedAttribute.Float(),moveDir.y * walkSpeedAttribute.Float());

        transformController.HorizontalVelocityTo(velocity,InAirSpeedSmoothTime);
        abilityRuntimeContext.AbilityComponentContext.GlobalBlacboard.Set<bool>(AbilitySystem.ISFALLINGID_IN_GLOBALBLACKBORAD,true);
        return TaskStatus.Running;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        Debug.Log("Fall Action Exit");
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