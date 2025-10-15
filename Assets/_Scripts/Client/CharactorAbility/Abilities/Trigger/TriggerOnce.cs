using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "TriggerOnce",menuName = "GAS/Triggers/TriggerOnce",order =0)]
public class TriggerOnce : AbilityTriggerUnit {
    bool hasTriggered = false;
    public override AbilityTriggerUnit Clone() {
        return new TriggerOnce();
    }

    public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) {
        if(hasTriggered) {
            return TaskStatus.Failed;
        }
        hasTriggered = true;
        return TaskStatus.Suceeded;
    }
}