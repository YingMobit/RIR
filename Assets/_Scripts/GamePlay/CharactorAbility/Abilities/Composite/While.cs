using GAS;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "While ",menuName = "GAS/Composite/While",order = 0)]
public class While : AbilityCompositeUnit {
    private const int CONDITION_EXIT_STATUS_ID = 0;
    private const int DO_ACTION_EXIT_STATUS_ID = 1;

    public override AbilityBehaviorUnit Clone() {
        return new While();
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var condition = Childs[0];
        var doAction = Childs[1];
        var conditionStatus = condition.OnExcute(abilityRuntimeContext);
        if(conditionStatus == TaskStatus.Suceeded) {
            doAction.OnExcute(abilityRuntimeContext);
            return TaskStatus.Running;
        } else if(conditionStatus == TaskStatus.Failed) {
            return TaskStatus.Suceeded;
        } else if(conditionStatus == TaskStatus.Running) {
            Debug.LogError($"Condition Should Finish In One Frame");
            return TaskStatus.Failed;
        }
        return TaskStatus.Failed;
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        var blackboard = abilityRuntimeContext.GetBlackBoard(RuntimeToken); 
        if(!blackboard.Get<TaskStatus>(CONDITION_EXIT_STATUS_ID).IsFinished()) 
            blackboard.Set(CONDITION_EXIT_STATUS_ID,Childs[0].OnExit(abilityRuntimeContext,allEffectFinished));
        if(!blackboard.Get<TaskStatus>(DO_ACTION_EXIT_STATUS_ID).IsFinished()) 
            blackboard.Set(DO_ACTION_EXIT_STATUS_ID,Childs[1].OnExit(abilityRuntimeContext,allEffectFinished));
        if(blackboard.Get<TaskStatus>(CONDITION_EXIT_STATUS_ID) == TaskStatus.Suceeded &&
           blackboard.Get<TaskStatus>(DO_ACTION_EXIT_STATUS_ID) == TaskStatus.Suceeded) {
            return TaskStatus.Suceeded;
        } else if(blackboard.Get<TaskStatus>(CONDITION_EXIT_STATUS_ID) == TaskStatus.Running || 
                  blackboard.Get<TaskStatus>(DO_ACTION_EXIT_STATUS_ID) == TaskStatus.Running) {
            return TaskStatus.Running;
        } else {
            return TaskStatus.Failed;
        }
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        foreach(var unit in Childs) {
            unit.OnInterrupt(interuptionContext);
        }
        return TaskStatus.Suceeded;
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        Childs[0].OnTriggered(abilityRuntimeContext);
        Childs[1].OnTriggered(abilityRuntimeContext);
        var blackboard = abilityRuntimeContext.GetBlackBoard(RuntimeToken); 
        blackboard.Set(CONDITION_EXIT_STATUS_ID,TaskStatus.UnStart);
        blackboard.Set(DO_ACTION_EXIT_STATUS_ID,TaskStatus.UnStart);
    }
}