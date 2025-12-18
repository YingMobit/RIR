using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "And",menuName = "GAS/Decorator/And",order = 0)]
public class And : AbilityCompositeUnit {
    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        foreach(var unit in Childs) {
            unit.OnTriggered(abilityRuntimeContext);
        }
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        bool allSuccess = true;
        foreach(var unit in Childs) {
            var result = unit.OnExcute(abilityRuntimeContext);
            if(result == TaskStatus.Failed) {
                allSuccess = false;
            }
        }

        return allSuccess ? TaskStatus.Suceeded : TaskStatus.Failed;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        foreach(var unit in Childs) {
            unit.OnExit(abilityRuntimeContext,true);
        }
        return  TaskStatus.Suceeded;
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        foreach(var unit in Childs) {
            unit.OnInterrupt(interuptionContext);
        }
        return TaskStatus.Suceeded;
    }

    public override AbilityBehaviorUnit Clone() {
        return new And();
    }
}