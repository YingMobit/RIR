using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "GroundedCheckTrigger",menuName = "GAS/Triggers/Charactor/GroundedCheckTrigger",order = 0)]
public class GroundedCheckTrigger : AbilityTriggerUnit {
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float checkDistance;

    public override AbilityTriggerUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) {
        if(Physics.Raycast((abilityComponentContext.Controllers[ControllerTypeEnum.Transform] as ITransformController).LogicPosition + Vector3.up * checkDistance,Vector3.down,checkDistance * 2,groundLayer)) {
            return TaskStatus.Suceeded;
        } else {
            return TaskStatus.Failed;
        }
    }
}