using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "GroundedCheckTrigger",menuName = "GAS/Triggers/Charactor/GroundedCheckTrigger",order = 0)]
public class GroundedCheckTrigger : AbilityTriggerUnit {
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float checkDistance = 0.1f;

    public override AbilityTriggerUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) {
        if(Physics.Raycast((abilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController).Position,Vector3.down,checkDistance,groundLayer)) {
            return TaskStatus.Suceeded;
        } else {
            return TaskStatus.Failed;
        }
    }
}