using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "NotTriggerDecorator",menuName = "GAS/TriggerDecorators/NotTriggerDecorator",order = 0)]
public class NotTriggerDecorator : AbilityTriggerDecorateUnit {
    public override AbilityTriggerUnit Clone() {
        return new NotTriggerDecorator();
    }

    public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) {
        if(triggerUnits[0].TryTrigger(abilityComponentContext) == TaskStatus.Suceeded) {
            return TaskStatus.Failed;
        }
        return TaskStatus.Suceeded;
    }
}