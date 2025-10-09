using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactorWalkAbility",menuName = "GAS/Abilities/Charactor/Walk",order = 0)]
public class CharactorWalkAbility : AbilityBehaviorUnit {
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