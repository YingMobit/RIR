using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "Sequence ",menuName = "GAS/Composite/Sequence",order =0)]
public class Sequence : AbilityCompositeUnit {

    #region Runtime
    int unitIndex = 0;
    #endregion

    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var currentUnit = Childs[unitIndex];
        var status = currentUnit.OnExcute(abilityRuntimeContext);
        if(status == TaskStatus.Suceeded) {
            unitIndex++;
            if(unitIndex == Childs.Count) {
                return TaskStatus.Suceeded;
            } else {
                Childs[unitIndex].OnTriggered(abilityRuntimeContext);
                return TaskStatus.Running;
            }
        } else if(status == TaskStatus.Failed) { 
            return TaskStatus.Failed;
        } else { 
            return TaskStatus.Running;
        }
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        var currentUnit = Childs[unitIndex];
        var status = currentUnit.OnExit(abilityRuntimeContext, allEffectFinished);
        if(status == TaskStatus.Suceeded) {
            unitIndex++;
            if(unitIndex == Childs.Count) {
                return TaskStatus.Suceeded;
            } else {
                return TaskStatus.Running;
            }
        } else if(status == TaskStatus.Failed) {
            return TaskStatus.Failed;
        } else {
            return TaskStatus.Running;
        }
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        var status = Childs[unitIndex].OnInterrupt(interuptionContext);
        if(status == TaskStatus.Running) {
            return TaskStatus.Running;
        } else if(status == TaskStatus.Suceeded) {
            return TaskStatus.Suceeded;
        } else {
            return TaskStatus.Failed;
        }
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        unitIndex = 0;
        Child.OnTriggered(abilityRuntimeContext);
    }
}
