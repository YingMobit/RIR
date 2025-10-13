using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "AndTriggerDecorator",menuName = "GAS/TriggerDecorators/AndTriggerDecorator",order =0)]
public class AndTriggerDecorator : AbilityTriggerDecorateUnit {
    public override AbilityTriggerUnit Clone() {
        return new AndTriggerDecorator();
    }

    public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) {
        foreach(var child in triggerUnits) {
            if(child.TryTrigger(abilityComponentContext) != TaskStatus.Suceeded) {
                return TaskStatus.Failed;
            }
        }

        return TaskStatus.Suceeded;
    }
}