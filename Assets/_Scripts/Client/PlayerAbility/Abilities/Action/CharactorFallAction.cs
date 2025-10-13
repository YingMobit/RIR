using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorFallAction",menuName ="GAS/Action/Charactor/Fall",order =0)]
public class CharactorFallAction : AbilityActionUnit {
    [Header("Animation Params")]
    [SerializeField] string AnimationParam_FallDown;
    [Header("Attribute Config")]
    [SerializeField] string FallGravityAttributeName;
    [SerializeField] string InAirSpeedAttributeName;
    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
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