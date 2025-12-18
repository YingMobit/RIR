using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "Sequence ",menuName = "GAS/Composite/Sequence",order =0)]
public class Sequence : AbilityCompositeUnit {
    #region Runtime
    private int _lastRunUnitIndex = 0;
    private int _lastExitUnitIndex = 0;
    private bool _someUnitFailedOnExit = false;
    #endregion

    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        var currentUnit = Childs[_lastRunUnitIndex];
        var status = currentUnit.OnExcute(abilityRuntimeContext);
        if(status == TaskStatus.Suceeded) {
            if(_lastRunUnitIndex == Childs.Count - 1) {
                return TaskStatus.Suceeded;
            } else {
                _lastRunUnitIndex++;
                Childs[_lastRunUnitIndex].OnTriggered(abilityRuntimeContext);
                return TaskStatus.Running;
            }
        } else if(status == TaskStatus.Failed) { 
            return TaskStatus.Failed;
        } else { 
            return TaskStatus.Running;
        }
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        var currentUnit = Childs[_lastExitUnitIndex];
        var exitRes = currentUnit.OnExit(abilityRuntimeContext, allEffectFinished);
        if(exitRes.IsFinished()) {
            if(exitRes == TaskStatus.Failed) {
                _someUnitFailedOnExit = true;
            }

            if(_lastExitUnitIndex == _lastRunUnitIndex) {
                if(_someUnitFailedOnExit) {
                    return TaskStatus.Failed;
                } else {
                    return TaskStatus.Suceeded;
                }
            } else {
                _lastExitUnitIndex++;
                return TaskStatus.Running;
            }
        } else {
            return TaskStatus.Running;
        }
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        var status = Childs[_lastRunUnitIndex].OnInterrupt(interuptionContext);
        if(status == TaskStatus.Running) {
            return TaskStatus.Running;
        } else if(status == TaskStatus.Suceeded) {
            return TaskStatus.Suceeded;
        } else {
            return TaskStatus.Failed;
        }
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        _lastRunUnitIndex = 0;
        _lastExitUnitIndex = 0;
        _someUnitFailedOnExit = false;
        Child.OnTriggered(abilityRuntimeContext);
    }
}
