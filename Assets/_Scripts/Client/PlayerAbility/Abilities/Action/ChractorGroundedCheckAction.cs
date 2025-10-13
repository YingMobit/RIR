using GAS;
using UnityEngine;
[CreateAssetMenu(fileName = "CharactorGoundedCheckAction",menuName = "GAS/Action/Charactor/GroundedCheck",order = 0)]
public class CharactorGoundedCheckAction : AbilityActionUnit {
    [SerializeField] LayerMask groundLayer; 
    [SerializeField] float checkDistance = 0.1f;

    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        if(Physics.Raycast((abilityRuntimeContext.AbilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController).Position,Vector3.down,checkDistance,groundLayer)) {
            return TaskStatus.Suceeded;
        } else {
            return TaskStatus.Failed;
        }
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        return TaskStatus.Suceeded;
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        return TaskStatus.Suceeded;
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
    }
}