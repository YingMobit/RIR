using AbilitySystem.Editor.AbilityEditor;
using UnityEngine;

namespace AbilitySystem {
    [CreateAssetMenu(menuName = "Ability/Test/Action Unit", fileName = "TestActionUnit")]
    public class TestActionUnit : AbilityActionUnit {
        public UnitNodeRefrence Target;
        public UnitNodeRefrence aaa;
        public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) { }
        public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) => default;
        public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext, bool allEffectFinished) => default;
        public override TaskStatus OnInterrupt(IIntreruptionContext intreruptionContext) => default;
        public override AbilityBehaviorUnit Clone() {
            var inst = CreateInstance<TestActionUnit>();
            inst.HeadInfo = this.HeadInfo;
            return inst;
        }
    }
}
