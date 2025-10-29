using GAS;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "While ",menuName = "GAS/Composite/While",order = 0)]
public class While : AbilityCompositeUnit {
    TaskStatus conditionExitStatus = TaskStatus.UnStart;
    TaskStatus doActionExitStatus = TaskStatus.UnStart;
    TaskStatus conditionInterruptStatus = TaskStatus.UnStart;
    TaskStatus doActionInterruptionStatus = TaskStatus.UnStart;

    public override AbilityBehaviorUnit Clone() {
        return new While();
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var condition = Childs[0];
        var doAction = Childs[1];
        var conditionStatus = condition.OnExcute(abilityRuntimeContext);
        if(conditionStatus == TaskStatus.Suceeded) {
            return doAction.OnExcute(abilityRuntimeContext);
        } else if(conditionStatus == TaskStatus.Failed) {
            return TaskStatus.Suceeded;
        } else if(conditionStatus == TaskStatus.Running) {
            Debug.LogError($"Condition Should Finish In One Frame");
            return TaskStatus.Failed;
        }
        return TaskStatus.Failed;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        if(conditionExitStatus != TaskStatus.Finished) conditionExitStatus = Childs[0].OnExit(abilityRuntimeContext,allEffectFinished);
        if(doActionExitStatus != TaskStatus.Finished) doActionExitStatus = Childs[1].OnExit(abilityRuntimeContext,allEffectFinished);
        if(conditionExitStatus == TaskStatus.Suceeded && doActionExitStatus == TaskStatus.Suceeded) {
            return TaskStatus.Suceeded;
        } else if(conditionExitStatus == TaskStatus.Running || doActionExitStatus == TaskStatus.Running) {
            return TaskStatus.Running;
        } else {
            return TaskStatus.Failed;
        }
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        if(conditionInterruptStatus != TaskStatus.Finished) conditionInterruptStatus = Childs[0].OnInterrupt(interuptionContext);
        if(doActionInterruptionStatus != TaskStatus.Finished) doActionInterruptionStatus = Childs[1].OnInterrupt(interuptionContext);
        if(conditionInterruptStatus == TaskStatus.Suceeded && doActionInterruptionStatus == TaskStatus.Suceeded) {
            return TaskStatus.Suceeded;
        } else if(conditionInterruptStatus == TaskStatus.Running || doActionInterruptionStatus == TaskStatus.Running) {
            return TaskStatus.Running;
        } else {
            return TaskStatus.Failed;
        }
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        Childs[0].OnTriggered(abilityRuntimeContext);
        Childs[1].OnTriggered(abilityRuntimeContext);
    }
}