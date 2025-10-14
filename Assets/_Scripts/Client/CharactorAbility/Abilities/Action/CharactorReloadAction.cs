using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorReloadAction",menuName = "GAS/Action/Charactor/Reload",order = 0)]
public class CharactorReloadAction : AbilityActionUnit {
    public override AbilityBehaviorUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        return TaskStatus.Running;  
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        return TaskStatus.Suceeded; 
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        return TaskStatus.Suceeded;
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        
    }
}