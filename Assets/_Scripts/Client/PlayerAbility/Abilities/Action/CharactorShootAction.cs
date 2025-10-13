using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorShootAction",menuName = "GAS/Action/Charactor/Shoot",order = 0)]
public class CharactorShootAction : AbilityActionUnit {
    public override AbilityBehaviorUnit Clone() {
        throw new System.NotImplementedException();
    }

    public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
        throw new System.NotImplementedException();
    }

    public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
        throw new System.NotImplementedException();
    }

    public override TaskStatus OnInterrupt(InteruptionContext interuptionContext) {
        throw new System.NotImplementedException();
    }

    public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
        throw new System.NotImplementedException();
    }
}