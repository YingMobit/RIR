using UnityEngine;

namespace AbilitySystem {
    [CreateAssetMenu(menuName = "Ability/Test/Composite Unit", fileName = "TestCompositeUnit")]
    public class TestCompositeUnit : AbilityCompositeUnit {
        public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) { }
        public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) => default;
        public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext, bool allEffectFinished) => default;
        public override TaskStatus OnInterrupt(IIntreruptionContext intreruptionContext) => default;
        public override AbilityBehaviorUnit Clone() {
            var inst = CreateInstance<TestCompositeUnit>();
            inst.HeadInfo = this.HeadInfo;
            return inst;
        }
    }
}
