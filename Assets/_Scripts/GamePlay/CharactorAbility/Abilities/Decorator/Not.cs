using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "Not",menuName = "GAS/Decorator/Not",order = 0)]
public class Not : AbilityDecorateUnit {
    public override AbilityBehaviorUnit Clone() {
        return new Not();
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        if(Child.OnExcute(abilityRuntimeContext) == TaskStatus.Suceeded) {
            return TaskStatus.Failed;
        } else if(Child.OnExcute(abilityRuntimeContext) == TaskStatus.Failed) {
            return TaskStatus.Suceeded;
        } else {
            return TaskStatus.Failed;
        }
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        return Child.OnExit(abilityRuntimeContext,allEffectFinished);
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        return Child.OnInterrupt(interuptionContext);
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        Child.OnTriggered(abilityRuntimeContext);   
    }
}
