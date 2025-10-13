using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "OrTriggerDecorator",menuName = "GAS/TriggerDecorators/OrTriggerDecorator",order = 0)]
public class OrTriggerDecorator : AbilityTriggerDecorateUnit {
    public override AbilityTriggerUnit Clone() {
        return new OrTriggerDecorator();
    }

    public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) {
        foreach(var child in triggerUnits) {
            if(child.TryTrigger(abilityComponentContext) == TaskStatus.Suceeded) {
                return TaskStatus.Suceeded;
            }
        }

        return TaskStatus.Failed;
    }
}